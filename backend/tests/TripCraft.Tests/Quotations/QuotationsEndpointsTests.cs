using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Quotations;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Quotations.Reports;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Quotations;

/// <summary>
/// Component C over HTTP with the real quotation store and Resource Management: list and detail, re-pricing,
/// the tourist accepting, and the reports. Each test uses its own app (approvals hold resources).
/// </summary>
public class QuotationsEndpointsTests
{
    private const string Tourist = "tourist1@tripcraft.test";

    [Fact]
    public async Task A_proposal_becomes_a_quotation_the_manager_can_list_and_the_owner_can_read()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var page = await manager.GetFromJsonAsync<PagedResult<QuotationDto>>(
            "/api/quotations?status=Pending&search=kandy&sort=-totalLkr", TestJson.Options);
        var listed = page!.Items.Should().ContainSingle(q => q.Id == outcome.QuotationId).Subject;
        listed.Version.Should().Be(1);
        listed.TotalLkr.Should().Be(TestProposals.GoldenTotalLkr);

        var owner = await factory.CreateClientAsAsync(Tourist);
        var detail = await owner.GetFromJsonAsync<QuotationDto>($"/api/quotations/{outcome.QuotationId}", TestJson.Options);
        detail!.TripRequestId.Should().Be(trip.Id);
        detail.Lines.Should().NotBeEmpty();

        var other = await factory.CreateClientAsAsync("tourist2@tripcraft.test");
        (await other.GetAsync($"/api/quotations/{outcome.QuotationId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await owner.GetAsync("/api/quotations")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await manager.GetAsync("/api/quotations?sort=secret")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Reprice_makes_version_2_with_named_lines_and_supersedes_version_1()
    {
        await using var factory = new RealComponentsFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/calculate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = (await response.Content.ReadFromJsonAsync<RepriceResponse>(TestJson.Options))!;
        result.Version.Should().Be(2);
        result.TotalLkr.Should().Be(TestProposals.GoldenTotalLkr, "the seeded rates did not change");
        result.PreviousTotalLkr.Should().Be(TestProposals.GoldenTotalLkr);
        result.Validation.IsValid.Should().BeTrue();
        var v1 = await manager.GetFromJsonAsync<QuotationDto>($"/api/quotations/{outcome.QuotationId}", TestJson.Options);
        var v2 = await manager.GetFromJsonAsync<QuotationDto>($"/api/quotations/{result.QuotationId}", TestJson.Options);
        v1!.Status.Should().Be("Superseded");
        v2!.Status.Should().Be("Pending");
        v2.Lines.Should().Contain(l => l.LineType == "guide" && l.Description.StartsWith("Guide Nimal Perera"));
        v2.Lines.Should().Contain(l => l.LineType == "room" && l.Description.Contains("Kandy Hills"));
        v1.ProposalSnapshot.Should().NotBeNull("the review page compares the versions side by side");
        v2.ProposalSnapshot!.Value.GetProperty("days").GetArrayLength().Should().Be(5);
        (await factory.QueryDbAsync(db => db.AuditLogs.AnyAsync(a => a.Action == "QuotationRepriced"))).Should().BeTrue();
        // Only the newest version can be decided.
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await manager.PostAsync($"/api/quotations/{result.QuotationId}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Accept_is_only_possible_after_the_quotation_was_sent_and_only_once()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var owner = await factory.CreateClientAsAsync(Tourist);

        (await owner.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null)).StatusCode
            .Should().Be(HttpStatusCode.Conflict, "the manager has not sent it yet");
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var accepted = await owner.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null);
        accepted.StatusCode.Should().Be(HttpStatusCode.OK);
        (await accepted.Content.ReadFromJsonAsync<QuotationDecisionResponse>(TestJson.Options))!.TripStatus.Should().Be("ClientAccepted");
        (await owner.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/calculate", null)).StatusCode
            .Should().Be(HttpStatusCode.Conflict, "an accepted trip is no longer in review");

        var detail = await manager.GetFromJsonAsync<QuotationDto>($"/api/quotations/{outcome.QuotationId}", TestJson.Options);
        detail!.Status.Should().Be(nameof(QuotationStatus.Approved));
        detail.AcceptedAt.Should().NotBeNull();
        detail.Decisions.Select(d => d.Decision).Should().Equal("Approved", "Accepted");

        // Confirm with the real Resource Management: holds in resource_holds, vouchers issued.
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await factory.QueryDbAsync(db => db.ResourceHolds.CountAsync(h => h.TripRequestId == trip.Id))).Should().Be(6);
    }

    [Fact]
    public async Task Reports_show_revenue_utilisation_and_trips_by_status()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, _) = await factory.RunToConfirmedAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var range = $"from={today.AddDays(-1):yyyy-MM-dd}&to={trip.EndDate:yyyy-MM-dd}";

        var revenue = await manager.GetFromJsonAsync<List<RevenueMonthDto>>($"/api/reports/revenue?{range}", TestJson.Options);
        revenue!.Sum(r => r.TotalLkr).Should().Be(TestProposals.GoldenTotalLkr);

        var utilisation = await manager.GetFromJsonAsync<List<UtilisationDto>>($"/api/reports/utilisation?{range}", TestJson.Options);
        utilisation!.Should().Contain(u => u.Name == "Nimal Perera" && u.HeldDays == 5 && u.UtilisationPct > 0);
        utilisation.Should().Contain(u => u.Name == "Kumari Silva" && u.HeldDays == 0);

        var statuses = await manager.GetFromJsonAsync<List<StatusCountDto>>($"/api/reports/trips-by-status?{range}", TestJson.Options);
        statuses!.Should().Contain(s => s.Status == "Confirmed" && s.Count >= 1);

        var sample = await manager.GetFromJsonAsync<List<RevenueMonthDto>>("/api/reports/revenue?from=2026-08-01&to=2026-08-31", TestJson.Options);
        sample!.Should().ContainSingle(r => r.Month == "2026-08", "the seeded sample trip was approved in August");
        (await manager.GetAsync("/api/reports/revenue?from=2026-01-01&to=2027-06-01")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await (await factory.CreateClientAsAsync("admin1@tripcraft.test")).GetAsync($"/api/reports/revenue?{range}"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
