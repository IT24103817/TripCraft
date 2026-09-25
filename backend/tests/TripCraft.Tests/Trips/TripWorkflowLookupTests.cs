using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

/// <summary>GET /api/trip-requests/{id}/workflow: the mobile app only knows the trip id.</summary>
public class TripWorkflowLookupTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private async Task<(HttpClient Tourist, TripRequestDto Trip)> TripAsync()
    {
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var created = await tourist.PostAsJsonAsync("/api/trip-requests", TripRequestsEndpointsTests.NewTrip());
        return (tourist, (await created.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!);
    }

    [Fact]
    public async Task Owner_gets_the_latest_workflow_after_planning_starts()
    {
        var (tourist, trip) = await TripAsync();
        var started = await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        var workflowId = (await started.Content.ReadFromJsonAsync<StartPlanningResponse>(TestJson.Options))!.WorkflowId;

        var workflow = await tourist.GetFromJsonAsync<WorkflowDto>($"/api/trip-requests/{trip.Id}/workflow", TestJson.Options);

        workflow!.Id.Should().Be(workflowId);
        workflow.Status.Should().Be("Planning");
    }

    [Fact]
    public async Task Before_planning_returns_404()
    {
        var (tourist, trip) = await TripAsync();

        var response = await tourist.GetAsync($"/api/trip-requests/{trip.Id}/workflow");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Another_tourist_gets_403()
    {
        var (tourist, trip) = await TripAsync();
        await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        var other = await factory.CreateClientAsAsync("tourist2@tripcraft.test");

        (await other.GetAsync($"/api/trip-requests/{trip.Id}/workflow")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
