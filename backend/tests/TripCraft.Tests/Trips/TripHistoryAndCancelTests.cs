using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Trips;

/// <summary>Component A status workflow and history: POST /cancel and GET /history.</summary>
public class TripHistoryAndCancelTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private async Task<(HttpClient Tourist, TripRequestDto Trip)> SubmitAsync(string tourist = "tourist3@tripcraft.test")
    {
        var client = await factory.CreateClientAsAsync(tourist);
        var response = await client.PostAsJsonAsync("/api/trip-requests", TripRequestsEndpointsTests.NewTrip());
        response.EnsureSuccessStatusCode();
        return (client, (await response.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!);
    }

    [Fact]
    public async Task Owner_cancels_a_submitted_trip_and_it_can_no_longer_be_planned()
    {
        var (tourist, trip) = await SubmitAsync();

        var cancel = await tourist.PostAsync($"/api/trip-requests/{trip.Id}/cancel", null);

        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        (await cancel.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!.Status.Should().Be("Cancelled");
        var plan = await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        plan.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Cancelling_a_trip_that_is_planning_is_409()
    {
        var (trip, _) = await factory.StartPlanningAsync();
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        var response = await tourist.PostAsync($"/api/trip-requests/{trip.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Only a Submitted trip request can be cancelled");
    }

    [Fact]
    public async Task Another_tourist_cannot_cancel_or_read_the_history()
    {
        var (_, trip) = await SubmitAsync("tourist3@tripcraft.test");
        var other = await factory.CreateClientAsAsync("tourist2@tripcraft.test");

        (await other.PostAsync($"/api/trip-requests/{trip.Id}/cancel", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await other.GetAsync($"/api/trip-requests/{trip.Id}/history")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task History_lists_the_trip_and_workflow_events_in_order_with_status_changes()
    {
        var (trip, _) = await factory.StartPlanningAsync();
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        var history = await tourist.GetFromJsonAsync<List<TripHistoryEntryDto>>(
            $"/api/trip-requests/{trip.Id}/history", TestJson.Options);

        history!.Select(h => h.Action).Should().ContainInOrder(
            "TripRequestCreated", "TripRequestStatusChanged", "AgentWorkflowStarted");
        history.Should().BeInAscendingOrder(h => h.At);
        history.Should().Contain(h => h.Action == "TripRequestStatusChanged"
                                      && h.FromStatus == "Submitted" && h.ToStatus == "Planning"
                                      && h.Actor == "Tourist");
    }

    [Fact]
    public async Task Manager_sees_the_history_and_unknown_trip_is_404()
    {
        var (_, trip) = await SubmitAsync();
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        (await manager.GetAsync($"/api/trip-requests/{trip.Id}/history")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await manager.GetAsync($"/api/trip-requests/{Guid.NewGuid()}/history")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("""{"status":"Planning"}""", "Planning")]
    [InlineData("""{"Status":"Submitted","pax":2}""", "Submitted")]
    [InlineData("""{"pax":2}""", null)]
    [InlineData(null, null)]
    public void ReadStatus_takes_the_status_field_of_a_snapshot(string? json, string? expected)
    {
        TripHistoryEntryDto.ReadStatus(json).Should().Be(expected);
    }
}
