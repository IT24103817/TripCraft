using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Settings;
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

    private static CancelTripRequest Because(string reason = "Change of plans") => new(reason);

    [Fact]
    public async Task Owner_cancels_a_submitted_trip_with_a_reason_and_it_can_no_longer_be_planned()
    {
        var (tourist, trip) = await SubmitAsync();

        var noReason = await tourist.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/cancel", Because(""));
        var cancel = await tourist.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/cancel", Because("Family emergency"));

        noReason.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        (await cancel.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!.Status.Should().Be("Cancelled");
        var plan = await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        plan.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var history = await tourist.GetFromJsonAsync<List<TripHistoryEntryDto>>($"/api/trip-requests/{trip.Id}/history", TestJson.Options);
        history!.Should().Contain(h => h.ToStatus == "Cancelled" && h.Reason == "Cancelled by the client: Family emergency");
        (await factory.QueryDbAsync(db => db.Notifications.AnyAsync(n => n.Type == "TripCancelled" && n.TripRequestId == trip.Id)))
            .Should().BeTrue("the managers are told");
    }

    [Fact]
    public async Task Cancelling_a_trip_that_is_planning_is_409_from_the_state_machine()
    {
        var (trip, _) = await factory.StartPlanningAsync();
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        var response = await tourist.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/cancel", Because());

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("A trip that is planning cannot be cancelled");
    }

    [Fact]
    public async Task Tourist_cannot_cancel_inside_the_cutoff_but_is_told_whom_to_contact_and_a_manager_can()
    {
        var tourist = await factory.CreateClientAsAsync("tourist3@tripcraft.test");
        var soon = TripRequestsEndpointsTests.NewTrip() with
        {
            StartDate = TripSettings.Default.Today().AddDays(2),
            EndDate = TripSettings.Default.Today().AddDays(4)
        };
        var trip = (await (await tourist.PostAsJsonAsync("/api/trip-requests", soon)).Content
            .ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!;

        var info = await tourist.GetFromJsonAsync<CancellationInfoDto>($"/api/trip-requests/{trip.Id}/cancellation", TestJson.Options);
        var refused = await tourist.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/cancel", Because());

        info!.CanCancel.Should().BeFalse();
        info.CutoffDays.Should().Be(3);
        info.CancelUntil.Should().Be(trip.StartDate.AddDays(-3));
        info.OperatorContact.Should().Be("operations@tripcraft.test");
        info.ClosedReason.Should().Contain("Cancellation closed").And.Contain("operations@tripcraft.test");
        refused.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await refused.Content.ReadAsStringAsync()).Should().Contain("contact the operator");

        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        (await manager.GetFromJsonAsync<CancellationInfoDto>($"/api/trip-requests/{trip.Id}/cancellation", TestJson.Options))!
            .CanCancel.Should().BeTrue();
        (await manager.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/cancel", Because("Operator closed"))).StatusCode
            .Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cancelling_a_confirmed_trip_releases_its_holds_in_the_same_transaction()
    {
        await using var app = new TestWebApplicationFactory();
        var (trip, _) = await app.RunToConfirmedAsync();
        app.State<Workflows.Fakes.FakeResourcesState>().Holds.Should().HaveCount(6);
        var tourist = await app.CreateClientAsAsync(WorkflowFlow.Tourist);

        var response = await tourist.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/cancel", Because("Flight cancelled"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        app.State<Workflows.Fakes.FakeResourcesState>().Holds.Should().BeEmpty();
        var audit = await app.QueryDbAsync(db => db.AuditLogs.SingleAsync(a => a.Action == "TripRequestCancelled"));
        audit.After.Should().Contain("\"holdsReleased\":6");
    }

    [Fact]
    public async Task Another_tourist_cannot_cancel_or_read_the_history()
    {
        var (_, trip) = await SubmitAsync("tourist3@tripcraft.test");
        var other = await factory.CreateClientAsAsync("tourist2@tripcraft.test");

        (await other.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/cancel", Because())).StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
