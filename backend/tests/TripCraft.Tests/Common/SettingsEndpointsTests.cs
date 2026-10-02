using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Tests.Trips;
using TripCraft.Tests.Workflows;
using TripCraft.Tests.Workflows.Fakes;

namespace TripCraft.Tests.Common;

/// <summary>The Admin Settings page (v1.1): stored in the database and read on every request.</summary>
public class SettingsEndpointsTests
{
    private static SaveSettingsRequest Saved(string provider = "groq", decimal marginPct = 20) =>
        new(provider, 10, marginPct, 25, "desk@tripcraft.test");

    [Fact]
    public async Task Defaults_come_from_configuration_until_an_admin_saves()
    {
        await using var factory = new TestWebApplicationFactory();
        var admin = await factory.CreateClientAsAsync("admin1@tripcraft.test");

        var settings = await admin.GetFromJsonAsync<SettingsDto>("/api/admin/settings", TestJson.Options);

        settings!.CancellationCutoffDays.Should().Be(3);
        settings.DepositPct.Should().Be(30);
        settings.MarginPct.Should().Be(15);
        settings.OperatorContact.Should().Be("operations@tripcraft.test");
        settings.LlmProvider.Should().Be("ollama");
        settings.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task Saved_settings_apply_to_the_next_requests()
    {
        await using var factory = new TestWebApplicationFactory();
        var admin = await factory.CreateClientAsAsync("admin1@tripcraft.test");

        var saved = await admin.PutAsJsonAsync("/api/admin/settings", Saved());
        saved.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cancellation notice: a trip starting in 8 days is now inside the 10-day cut-off.
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var soon = TripRequestsEndpointsTests.NewTrip() with { StartDate = TripSettings.Default.Today().AddDays(8), EndDate = TripSettings.Default.Today().AddDays(10) };
        var trip = (await (await tourist.PostAsJsonAsync("/api/trip-requests", soon)).Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!;
        var info = await tourist.GetFromJsonAsync<CancellationInfoDto>($"/api/trip-requests/{trip.Id}/cancellation", TestJson.Options);
        info!.CutoffDays.Should().Be(10);
        info.CanCancel.Should().BeFalse();
        info.OperatorContact.Should().Be("desk@tripcraft.test");

        // LLM provider: sent to the agent service with the new workflow.
        await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        factory.State<FakeAgentState>().Requests.Last().LlmProvider.Should().Be("groq");

        // Margin: today's rate card; audit row written.
        var (margin, audited) = await factory.QueryDbAsync(async db => (
            await db.RateCards.OrderByDescending(r => r.EffectiveFrom).Select(r => r.MarginPct).FirstAsync(),
            await db.AuditLogs.AnyAsync(a => a.Action == "SettingsUpdated")));
        margin.Should().Be(20);
        audited.Should().BeTrue();
        // A saved margin applies at once to pricing (same UTC-dated rate card the catalog reads).
        (await admin.GetFromJsonAsync<SettingsDto>("/api/admin/settings", TestJson.Options))!.MarginPct.Should().Be(20);
        (await admin.GetFromJsonAsync<SettingsDto>("/api/admin/settings", TestJson.Options))!.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task The_deposit_setting_is_stored_on_new_quotation_versions()
    {
        await using var factory = new RealComponentsFactory();
        var admin = await factory.CreateClientAsAsync("admin1@tripcraft.test");
        // Margin unchanged (15 %): the golden proposal is priced at 15 %, so a new margin would reject it.
        await admin.PutAsJsonAsync("/api/admin/settings", Saved("ollama", marginPct: 15));
        var (_, outcome) = await factory.RunToProposalAsync();
        outcome.QuotationId.Should().NotBeNull();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var quotation = await manager.GetFromJsonAsync<QuotationDto>($"/api/quotations/{outcome.QuotationId}", TestJson.Options);

        quotation!.DepositPct.Should().Be(25);
        quotation.DepositLkr.Should().Be(Math.Round(quotation.TotalLkr * 0.25m, 2));
        quotation.DepositPaid.Should().BeFalse();
    }

    [Theory]
    [InlineData("openai", 3, 15, 30)]
    [InlineData("groq", 31, 15, 30)]
    [InlineData("groq", 3, 101, 30)]
    [InlineData("groq", 3, 15, -1)]
    public async Task Invalid_settings_are_400(string provider, int cutoff, decimal margin, decimal deposit)
    {
        await using var factory = new TestWebApplicationFactory();
        var admin = await factory.CreateClientAsAsync("admin1@tripcraft.test");

        var response = await admin.PutAsJsonAsync("/api/admin/settings", new SaveSettingsRequest(provider, cutoff, margin, deposit, "x@y.test"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("manager1@tripcraft.test")]
    [InlineData("tourist1@tripcraft.test")]
    public async Task Only_an_admin_reads_or_changes_settings(string email)
    {
        await using var factory = new TestWebApplicationFactory();
        var client = await factory.CreateClientAsAsync(email);

        (await client.GetAsync("/api/admin/settings")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PutAsJsonAsync("/api/admin/settings", Saved())).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
