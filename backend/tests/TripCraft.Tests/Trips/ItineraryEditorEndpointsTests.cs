using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

/// <summary>PUT /api/trip-requests/{id}/itinerary/days/{day}: the itinerary editor (Component A).</summary>
public class ItineraryEditorEndpointsTests
{
    private static async Task<(TestWebApplicationFactory Factory, Guid TripId)> TripAsync(
        TripRequestStatus status = TripRequestStatus.Confirmed)
    {
        var factory = new TestWebApplicationFactory();
        var tripId = await factory.QueryDbAsync(async db =>
        {
            var tourist = await db.Tourists.FirstAsync();
            var bridge = await db.Attractions.SingleAsync(a => a.Name == "Nine Arches Bridge");
            var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20);
            var trip = new TripRequest
            {
                TouristId = tourist.Id, Objective = "2 days in Ella", StartDate = start, EndDate = start.AddDays(1), Pax = 2,
                BudgetUsd = 800, Preferences = "{}", Status = status
            };
            var day = new ItineraryDay { DayNumber = 1, City = "Ella", Notes = "Arrive by train." };
            day.Stops.Add(new ItineraryStop { ItineraryDayId = day.Id, AttractionId = bridge.Id, Sequence = 1 });
            db.AddRange(trip, new Itinerary { TripRequestId = trip.Id, Days = [day] });
            await db.SaveChangesAsync();
            return trip.Id;
        });
        return (factory, tripId);
    }

    private static Task<Guid> IdAsync(TestWebApplicationFactory factory, string name) =>
        factory.QueryDbAsync(db => db.Attractions.Where(a => a.Name == name).Select(a => a.Id).SingleAsync());

    [Fact]
    public async Task A_manager_replaces_a_days_stops_and_notes_and_the_change_is_versioned_and_audited()
    {
        var (factory, tripId) = await TripAsync();
        await using var _ = factory;
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var rock = await IdAsync(factory, "Ella Rock");
        var falls = await IdAsync(factory, "Ravana Falls");

        var response = await manager.PutAsJsonAsync($"/api/trip-requests/{tripId}/itinerary/days/1",
            new EditItineraryDayRequest([falls, rock], "Early start for Ella Rock."));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var itinerary = (await response.Content.ReadFromJsonAsync<ItineraryDto>(TestJson.Options))!;
        itinerary.Version.Should().Be(2);
        itinerary.GeneratedBy.Should().Be("Manual");
        itinerary.Days[0].Stops.Select(s => s.AttractionName).Should().Equal("Ravana Falls", "Ella Rock");
        itinerary.Days[0].Notes.Should().Be("Early start for Ella Rock.");
        (await factory.QueryDbAsync(db => db.AuditLogs.AnyAsync(a => a.EntityId == tripId && a.Action == "ItineraryDayEdited")))
            .Should().BeTrue();
        (await factory.QueryDbAsync(db => db.ItineraryStops.CountAsync(s => s.AttractionId == rock || s.AttractionId == falls)))
            .Should().Be(2); // the old stop was deleted, not left behind
    }

    [Fact]
    public async Task Stops_must_be_1_to_3_distinct_attractions_in_the_days_city()
    {
        var (factory, tripId) = await TripAsync();
        await using var _ = factory;
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var temple = await IdAsync(factory, "Temple of the Sacred Tooth Relic"); // Kandy, not Ella
        var rock = await IdAsync(factory, "Ella Rock");
        var url = $"/api/trip-requests/{tripId}/itinerary/days/1";

        var wrongCity = await manager.PutAsJsonAsync(url, new EditItineraryDayRequest([temple], null));
        var none = await manager.PutAsJsonAsync(url, new EditItineraryDayRequest([], null));
        var twice = await manager.PutAsJsonAsync(url, new EditItineraryDayRequest([rock, rock], null));
        var unknown = await manager.PutAsJsonAsync(url, new EditItineraryDayRequest([Guid.NewGuid()], null));

        wrongCity.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await wrongCity.Content.ReadAsStringAsync()).Should().Contain("is in Kandy, not Ella");
        none.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        twice.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        unknown.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await manager.PutAsJsonAsync($"/api/trip-requests/{tripId}/itinerary/days/9", new EditItineraryDayRequest([rock], null)))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Only_a_manager_edits_and_only_while_the_trip_is_confirmed()
    {
        var (factory, tripId) = await TripAsync(TripRequestStatus.InProgress);
        await using var _ = factory;
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var rock = await IdAsync(factory, "Ella Rock");
        var body = new EditItineraryDayRequest([rock], null);

        var started = await manager.PutAsJsonAsync($"/api/trip-requests/{tripId}/itinerary/days/1", body);
        var asTourist = await tourist.PutAsJsonAsync($"/api/trip-requests/{tripId}/itinerary/days/1", body);

        started.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await started.Content.ReadAsStringAsync()).Should().Contain("only be edited while the trip is Confirmed");
        asTourist.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
