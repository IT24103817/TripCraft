using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Identity;

/// <summary>GET /api/admin/audit-logs: Admin only, with filter, search, sort and paging.</summary>
public class AdminAuditLogsEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private const string Url = "/api/admin/audit-logs";

    [Fact]
    public async Task Admin_filters_by_entity_and_action_newest_first_with_the_actor()
    {
        await factory.StartPlanningAsync();
        await factory.StartPlanningAsync();
        var admin = await factory.CreateClientAsAsync("admin1@tripcraft.test");

        var page = await admin.GetFromJsonAsync<PagedResult<AuditLogDto>>(
            $"{Url}?entity=TripRequest&action=TripRequestStatusChanged&page=1&pageSize=1", TestJson.Options);

        page!.Items.Should().ContainSingle();
        page.Total.Should().BeGreaterThanOrEqualTo(2);
        var row = page.Items[0];
        row.Entity.Should().Be("TripRequest");
        row.ActorEmail.Should().Be("tourist1@tripcraft.test");
        row.ActorRole.Should().Be("Tourist");
        row.After.Should().Contain("Planning");
    }

    [Fact]
    public async Task Search_and_sort_work_and_an_unknown_sort_is_400()
    {
        await factory.StartPlanningAsync();
        var admin = await factory.CreateClientAsAsync("admin1@tripcraft.test");

        var page = await admin.GetFromJsonAsync<PagedResult<AuditLogDto>>(
            $"{Url}?search=workflowstarted&sort=at", TestJson.Options);
        page!.Items.Should().NotBeEmpty().And.OnlyContain(a => a.Action == "AgentWorkflowStarted");
        page.Items.Should().BeInAscendingOrder(a => a.At);

        (await admin.GetAsync($"{Url}?sort=password")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("manager1@tripcraft.test")]
    [InlineData("tourist1@tripcraft.test")]
    [InlineData("guide1@tripcraft.test")]
    public async Task Only_admins_may_read_the_audit_log(string email)
    {
        var client = await factory.CreateClientAsAsync(email);

        (await client.GetAsync(Url)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
