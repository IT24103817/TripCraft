using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Workflows;

public class WorkflowsEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Owner_tourist_reads_workflow_with_validation_and_timings()
    {
        var (_, outcome) = await factory.RunToProposalAsync();
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        var workflow = await tourist.GetFromJsonAsync<WorkflowDto>($"/api/workflows/{outcome.WorkflowId}", TestJson.Options);

        workflow!.Status.Should().Be("PendingApproval");
        workflow.CurrentStep.Should().Be("awaiting-manager");
        workflow.ValidationResult!.Value.GetProperty("isValid").GetBoolean().Should().BeTrue();
        workflow.Plan.GetProperty("constraints").GetProperty("cities")[0].GetString().Should().Be("Kandy");
        workflow.ElapsedMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task The_proposed_guide_vehicle_and_rooms_come_with_display_names()
    {
        var (_, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var workflow = await manager.GetFromJsonAsync<WorkflowDto>($"/api/workflows/{outcome.WorkflowId}", TestJson.Options);

        workflow!.ResourceNames.Should().Contain(Fakes.FakeResourcesState.GuideEn.ToString(), "Nimal Perera")
            .And.Contain(Fakes.FakeResourcesState.VanSixSeats.ToString(), "Van CAB-1234")
            .And.Contain(Fakes.FakeResourcesState.KandyStandard.ToString(), "Kandy Hills — Standard Double");
    }

    [Fact]
    public async Task Steps_are_returned_in_step_order()
    {
        var (_, workflowId) = await factory.StartPlanningAsync();
        var agent = factory.CreateInternalClient();
        foreach (var name in new[] { "planner", "itinerary", "resources", "validation" })
        {
            var step = new AgentStepReportRequest(name, [], null, null, null, 10, 0, "Succeeded");
            (await agent.PostAsJsonAsync($"/api/internal/workflows/{workflowId}/steps", step)).EnsureSuccessStatusCode();
        }
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var steps = await manager.GetFromJsonAsync<List<AgentStepDto>>($"/api/workflows/{workflowId}/steps", TestJson.Options);

        steps!.Select(s => (s.StepNo, s.AgentName)).Should().Equal(
            (1, "planner"), (2, "itinerary"), (3, "resources"), (4, "validation"));
    }

    [Fact]
    public async Task Other_tourist_and_guide_get_403()
    {
        var (_, workflowId) = await factory.StartPlanningAsync();
        var otherTourist = await factory.CreateClientAsAsync(WorkflowFlow.OtherTourist);
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        (await otherTourist.GetAsync($"/api/workflows/{workflowId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await guide.GetAsync($"/api/workflows/{workflowId}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Staff_list_workflows_filtered_by_status_and_tourists_cannot()
    {
        await factory.RunToProposalAsync();
        var admin = await factory.CreateClientAsAsync("admin1@tripcraft.test");
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        var page = await admin.GetFromJsonAsync<PagedResult<WorkflowSummaryDto>>(
            "/api/workflows?status=PendingApproval&page=1&pageSize=5", TestJson.Options);

        page!.Items.Should().NotBeEmpty().And.OnlyContain(w => w.Status == "PendingApproval");
        (await tourist.GetAsync("/api/workflows")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await admin.GetAsync("/api/workflows?pageSize=500")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Staff_search_workflows_by_objective_and_sort_by_a_whitelisted_field()
    {
        await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        var found = await manager.GetFromJsonAsync<PagedResult<WorkflowSummaryDto>>(
            "/api/workflows?search=HILL-COUNTRY&sort=-startedAt", TestJson.Options);
        var none = await manager.GetFromJsonAsync<PagedResult<WorkflowSummaryDto>>(
            "/api/workflows?search=no-such-objective-xyz", TestJson.Options);

        found!.Items.Should().NotBeEmpty()
            .And.OnlyContain(w => w.Objective.Contains("hill-country", StringComparison.OrdinalIgnoreCase));
        found.Items.Select(w => w.StartedAt).Should().BeInDescendingOrder();
        none!.Items.Should().BeEmpty();
        (await manager.GetAsync("/api/workflows?sort=status")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await manager.GetAsync("/api/workflows?sort=objective")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
