using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

public class TripRequestsEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private const string Manager = "manager1@tripcraft.test";

    private static readonly DateOnly Start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14);

    public static CreateTripRequestRequest NewTrip(string objective = "5 days in Kandy and Ella with the hill-country train", decimal budget = 1500) =>
        new(objective, Start, Start.AddDays(4), 4, budget,
            JsonDocument.Parse("""{"language":"en","transport":"train"}""").RootElement,
            "United Kingdom", "N1234567");

    private async Task<TripRequestDto> CreateTripAsync(string tourist, CreateTripRequestRequest? request = null)
    {
        var client = await factory.CreateClientAsAsync(tourist);
        var response = await client.PostAsJsonAsync("/api/trip-requests", request ?? NewTrip());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!;
    }

    [Fact]
    public async Task Tourist_creates_trip_request_and_gets_201_with_location()
    {
        var client = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        var response = await client.PostAsJsonAsync("/api/trip-requests", NewTrip());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options);
        response.Headers.Location!.AbsolutePath.Should().Be($"/api/trip-requests/{created!.Id}");
        created.Status.Should().Be("Submitted");
        created.Preferences.GetProperty("transport").GetString().Should().Be("train");
    }

    [Fact]
    public async Task Invalid_trip_request_returns_400_with_field_errors()
    {
        var client = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var bad = NewTrip() with { Pax = 0, EndDate = Start.AddDays(-1) };

        var response = await client.PostAsJsonAsync("/api/trip-requests", bad);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Pax").And.Contain("End date must be on or after the start date.");
    }

    [Fact]
    public async Task Manager_list_supports_status_filter_sort_and_paging()
    {
        await CreateTripAsync("tourist2@tripcraft.test", NewTrip(budget: 1000));
        await CreateTripAsync("tourist2@tripcraft.test", NewTrip(budget: 3000));
        await CreateTripAsync("tourist2@tripcraft.test", NewTrip(budget: 2000));
        var client = await factory.CreateClientAsAsync(Manager);

        var page = await client.GetFromJsonAsync<PagedResult<TripRequestDto>>(
            "/api/trip-requests?status=Submitted&sort=-budgetUsd&page=1&pageSize=2", TestJson.Options);

        page!.Items.Should().HaveCount(2);
        page.Page.Should().Be(1);
        page.PageSize.Should().Be(2);
        page.Total.Should().BeGreaterThanOrEqualTo(3);
        page.Items.Should().BeInDescendingOrder(t => t.BudgetUsd);
        page.Items.Should().OnlyContain(t => t.Status == "Submitted");
    }

    [Fact]
    public async Task Search_matches_words_in_the_objective()
    {
        await CreateTripAsync("tourist2@tripcraft.test", NewTrip("Whale watching in Galle for two days"));
        var client = await factory.CreateClientAsAsync(Manager);

        var page = await client.GetFromJsonAsync<PagedResult<TripRequestDto>>(
            "/api/trip-requests?search=WHALE", TestJson.Options);

        page!.Items.Should().NotBeEmpty().And.OnlyContain(t => t.Objective.Contains("Whale"));
    }

    [Fact]
    public async Task Tourist_list_only_contains_their_own_trips()
    {
        var others = await CreateTripAsync("tourist2@tripcraft.test");
        var client = await factory.CreateClientAsAsync("tourist3@tripcraft.test");

        var page = await client.GetFromJsonAsync<PagedResult<TripRequestDto>>("/api/trip-requests?pageSize=100", TestJson.Options);

        page!.Items.Should().NotContain(t => t.Id == others.Id);
    }

    [Fact]
    public async Task Tourist_reading_another_tourists_trip_returns_403()
    {
        var others = await CreateTripAsync("tourist2@tripcraft.test");
        var client = await factory.CreateClientAsAsync("tourist3@tripcraft.test");

        var response = await client.GetAsync($"/api/trip-requests/{others.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Unknown_trip_returns_404()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.GetAsync($"/api/trip-requests/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Sorting_by_a_field_outside_the_whitelist_returns_400()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.GetAsync("/api/trip-requests?sort=passwordHash");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Guide_cannot_use_trip_request_endpoints()
    {
        var client = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        var response = await client.GetAsync("/api/trip-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Manager_cannot_submit_a_trip_request()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.PostAsJsonAsync("/api/trip-requests", NewTrip());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Submitted_trip_can_be_updated_and_the_change_is_audited()
    {
        var trip = await CreateTripAsync("tourist1@tripcraft.test");
        var client = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var update = new UpdateTripRequestRequest(trip.Objective, trip.StartDate, trip.EndDate, 6, 2500, null);

        var response = await client.PutAsJsonAsync($"/api/trip-requests/{trip.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options);
        updated!.Pax.Should().Be(6);
        var auditActions = await factory.QueryDbAsync(db =>
            db.AuditLogs.Where(a => a.EntityId == trip.Id).Select(a => a.Action).ToListAsync());
        auditActions.Should().Contain("TripRequestUpdated");
    }

    [Fact]
    public async Task Start_planning_returns_202_and_saves_workflow_status_and_audit()
    {
        var trip = await CreateTripAsync("tourist1@tripcraft.test");
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<StartPlanningResponse>(TestJson.Options);
        response.Headers.Location!.ToString().Should().Be($"/api/workflows/{result!.WorkflowId}");
        result.WorkflowStatus.Should().Be("Planning");
        result.TripStatus.Should().Be("Planning");
        result.Skeleton.Select(d => d.City).Should().Equal("Kandy", "Kandy", "Kandy", "Ella", "Ella");

        var (tripStatus, workflowCount, auditActions) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            await db.AgentWorkflows.CountAsync(w => w.TripRequestId == trip.Id),
            await db.AuditLogs.Where(a => a.EntityId == trip.Id || a.EntityId == result.WorkflowId)
                .Select(a => a.Action).ToListAsync()));
        tripStatus.Should().Be(TripRequestStatus.Planning);
        workflowCount.Should().Be(1);
        auditActions.Should().Contain("TripRequestStatusChanged").And.Contain("AgentWorkflowStarted");
    }

    [Fact]
    public async Task Starting_planning_twice_returns_409()
    {
        var trip = await CreateTripAsync("tourist1@tripcraft.test");
        var client = await factory.CreateClientAsAsync(Manager);
        await client.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);

        var second = await client.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Editing_a_trip_that_is_being_planned_returns_409()
    {
        var trip = await CreateTripAsync("tourist1@tripcraft.test");
        var client = await factory.CreateClientAsAsync(Manager);
        await client.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        var update = new UpdateTripRequestRequest(trip.Objective, trip.StartDate, trip.EndDate, 2, 900, null);

        var response = await client.PutAsJsonAsync($"/api/trip-requests/{trip.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Start_planning_without_a_known_city_returns_400()
    {
        var trip = await CreateTripAsync("tourist1@tripcraft.test", NewTrip("Somewhere sunny with good food please"));
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Objective must mention at least one destination");
    }

    [Fact]
    public async Task Manager_reads_the_seeded_itinerary()
    {
        var seededTripId = await factory.QueryDbAsync(db =>
            db.TripRequests.Where(t => t.Status == TripRequestStatus.Completed).Select(t => t.Id).FirstAsync());
        var client = await factory.CreateClientAsAsync(Manager);

        var itinerary = await client.GetFromJsonAsync<ItineraryDto>($"/api/trip-requests/{seededTripId}/itinerary", TestJson.Options);

        itinerary!.Days.Select(d => d.DayNumber).Should().Equal(1, 2, 3);
        itinerary.Days[0].Stops[0].AttractionName.Should().Be("Royal Botanical Gardens, Peradeniya");
    }

    [Fact]
    public async Task Itinerary_for_a_trip_that_has_none_returns_404()
    {
        var trip = await CreateTripAsync("tourist1@tripcraft.test");
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.GetAsync($"/api/trip-requests/{trip.Id}/itinerary");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
