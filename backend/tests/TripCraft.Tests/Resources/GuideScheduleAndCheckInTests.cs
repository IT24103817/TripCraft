using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Resources;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Resources;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

/// <summary>
/// The guide's schedule and GPS check-in (Component B business operation). guide1 is linked to Nimal Perera by the
/// seed; each test builds a Confirmed trip with two stops that Nimal is held for.
/// </summary>
public class GuideScheduleAndCheckInTests
{
    private const double NineArchesLat = 6.8768, NineArchesLng = 81.0608;

    private static async Task<(TestWebApplicationFactory Factory, Guid TripId, Guid[] StopIds)> ConfirmedTripAsync()
    {
        var factory = new TestWebApplicationFactory();
        var ids = await factory.QueryDbAsync(async db =>
        {
            var tourist = await db.Tourists.FirstAsync();
            var bridge = await db.Attractions.SingleAsync(a => a.Name == "Nine Arches Bridge");
            var peak = await db.Attractions.SingleAsync(a => a.Name == "Little Adam's Peak");
            var start = DateOnly.FromDateTime(DateTime.UtcNow);
            var trip = new TripRequest
            {
                TouristId = tourist.Id, Objective = "2 days in Ella", StartDate = start, EndDate = start.AddDays(1), Pax = 2,
                BudgetUsd = 800, Preferences = "{}", Status = TripRequestStatus.Confirmed
            };
            var day = new ItineraryDay { DayNumber = 1, City = "Ella", HotelId = ResourcesSeeder.EllaHotel };
            day.Stops.Add(new ItineraryStop { ItineraryDayId = day.Id, AttractionId = bridge.Id, Sequence = 1 });
            day.Stops.Add(new ItineraryStop { ItineraryDayId = day.Id, AttractionId = peak.Id, Sequence = 2 });
            var itinerary = new Itinerary { TripRequestId = trip.Id, Days = [day] };
            db.AddRange(trip, itinerary);
            db.ResourceHolds.Add(new ResourceHold { ResourceType = ResourceType.Guide, ResourceId = ResourcesSeeder.NimalGuide,
                TripRequestId = trip.Id, FromDate = trip.StartDate, ToDate = trip.EndDate });
            await db.SaveChangesAsync();
            return (trip.Id, day.Stops.OrderBy(s => s.Sequence).Select(s => s.Id).ToArray());
        });
        return (factory, ids.Item1, ids.Item2);
    }

    [Fact]
    public async Task The_guide_sees_the_trip_with_its_stops_hotel_and_no_check_ins_yet()
    {
        var (factory, tripId, _) = await ConfirmedTripAsync();
        await using var _ = factory;
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        var schedule = await guide.GetFromJsonAsync<GuideScheduleDto>("/api/guides/me/schedule", TestJson.Options);

        schedule!.GuideName.Should().Be("Nimal Perera");
        var trip = schedule.Trips.Should().ContainSingle(t => t.TripRequestId == tripId).Subject;
        trip.Days.Should().ContainSingle().Which.HotelName.Should().Be("Ella Gap");
        trip.Days[0].Stops.Select(s => s.AttractionName).Should().Equal("Nine Arches Bridge", "Little Adam's Peak");
        trip.Days[0].Stops.Should().OnlyContain(s => s.CheckedInAt == null);
    }

    [Fact]
    public async Task Check_in_needs_the_guide_within_500_m()
    {
        var (factory, _, stops) = await ConfirmedTripAsync();
        await using var _ = factory;
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        var far = await guide.PostAsJsonAsync("/api/check-ins", new CheckInRequest(stops[0], NineArchesLat + 0.02, NineArchesLng));

        far.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await far.Content.ReadAsStringAsync()).Should().Contain("move within 500 m");
    }

    [Fact]
    public async Task First_check_in_starts_the_trip_and_the_last_one_completes_it()
    {
        var (factory, tripId, stops) = await ConfirmedTripAsync();
        await using var _ = factory;
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        var first = await guide.PostAsJsonAsync("/api/check-ins", new CheckInRequest(stops[0], NineArchesLat + 0.001, NineArchesLng));
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        (await first.Content.ReadFromJsonAsync<CheckInResultDto>(TestJson.Options))!.TripStatus.Should().Be("InProgress");

        (await guide.PostAsJsonAsync("/api/check-ins", new CheckInRequest(stops[0], NineArchesLat, NineArchesLng)))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);

        var last = await guide.PostAsJsonAsync("/api/check-ins", new CheckInRequest(stops[1], 6.8691, 81.0663));
        (await last.Content.ReadFromJsonAsync<CheckInResultDto>(TestJson.Options))!.TripStatus.Should().Be("Completed");

        var (status, changes) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == tripId)).Status,
            await db.AuditLogs.CountAsync(a => a.EntityId == tripId && a.Action == "TripRequestStatusChanged")));
        status.Should().Be(TripRequestStatus.Completed);
        changes.Should().Be(2);
    }

    [Fact]
    public async Task Another_guide_cannot_check_in_and_a_guide_without_profile_is_403()
    {
        var (factory, _, stops) = await ConfirmedTripAsync();
        await using var _ = factory;
        var otherGuide = await factory.CreateClientAsAsync("guide2@tripcraft.test");

        (await otherGuide.PostAsJsonAsync("/api/check-ins", new CheckInRequest(stops[0], NineArchesLat, NineArchesLng)))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        (await tourist.GetAsync("/api/guides/me/schedule")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
