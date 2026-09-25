using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Workflows;

/// <summary>Every status code the workflow endpoints can return (201/400/401/403/404/409).</summary>
public class WorkflowStatusCodeTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private static readonly AgentStepReportRequest Step = new("planner", [], null, null, null, 10, 0, "Succeeded");

    [Fact]
    public async Task Step_report_on_an_existing_workflow_returns_201()
    {
        var (_, workflowId) = await factory.StartPlanningAsync();

        var response = await factory.CreateInternalClient().PostAsJsonAsync($"/api/internal/workflows/{workflowId}/steps", Step);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Unknown_workflow_returns_404_everywhere()
    {
        var missing = Guid.NewGuid();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var agent = factory.CreateInternalClient();

        (await manager.GetAsync($"/api/workflows/{missing}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.GetAsync($"/api/workflows/{missing}/steps")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await agent.PostAsJsonAsync($"/api/internal/workflows/{missing}/steps", Step)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await agent.PostAsJsonAsync($"/api/internal/workflows/{missing}/proposal",
            new AgentProposalRequest(null, null, null, null, null, "FailedSafely", 0, "x"))).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Invalid_step_or_proposal_returns_400()
    {
        var (_, workflowId) = await factory.StartPlanningAsync();
        var agent = factory.CreateInternalClient();

        (await agent.PostAsJsonAsync($"/api/internal/workflows/{workflowId}/steps", Step with { AgentName = "hacker" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await agent.PostAsJsonAsync($"/api/internal/workflows/{workflowId}/proposal",
            new AgentProposalRequest(null, null, null, null, null, "Approved", 0, null))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Missing_jwt_is_401_and_wrong_role_is_403()
    {
        var (_, workflowId) = await factory.StartPlanningAsync();
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        (await factory.CreateClient().GetAsync($"/api/workflows/{workflowId}")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await guide.GetAsync($"/api/workflows/{workflowId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await tourist.GetAsync("/api/workflows")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Starting_planning_while_a_workflow_runs_returns_409()
    {
        var (trip, _) = await factory.StartPlanningAsync();
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        (await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
