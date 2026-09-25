using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Workflows;

/// <summary>Component integration test: create (start planning) -> list with a status filter -> pagination fields.</summary>
public class WorkflowsListFlowTests
{
    [Fact]
    public async Task Started_workflows_are_listed_by_status_with_paging()
    {
        await using var factory = new TestWebApplicationFactory();
        var started = new List<Guid>();
        for (var i = 0; i < 3; i++)
            started.Add((await factory.StartPlanningAsync()).WorkflowId);
        await factory.RunToProposalAsync(); // one more, moved on to PendingApproval
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var page1 = await manager.GetFromJsonAsync<PagedResult<WorkflowSummaryDto>>(
            "/api/workflows?status=Planning&page=1&pageSize=2", TestJson.Options);
        var page2 = await manager.GetFromJsonAsync<PagedResult<WorkflowSummaryDto>>(
            "/api/workflows?status=Planning&page=2&pageSize=2", TestJson.Options);

        page1!.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);
        page1.Total.Should().Be(3);
        page1.Items.Should().HaveCount(2).And.OnlyContain(w => w.Status == "Planning");
        page2!.Items.Should().ContainSingle();
        page1.Items.Concat(page2.Items).Select(w => w.Id).Should().BeEquivalentTo(started);
        page1.Items[0].StartedAt.Should().BeOnOrAfter(page1.Items[1].StartedAt); // newest first
    }
}
