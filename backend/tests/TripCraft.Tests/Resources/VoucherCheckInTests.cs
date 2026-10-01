using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Resources;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Trips;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

/// <summary>POST /api/check-ins with a scanned trip voucher (v1.1): signature, trip, day and guide are checked.</summary>
public class VoucherCheckInTests
{
    private static CheckInRequest Scan(string code, Guid? tripId = null) => new(null, null, null, code, tripId);

    [Fact]
    public async Task Scanning_the_trip_voucher_checks_in_todays_stops_in_order_and_moves_the_trip_on()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory);
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        var first = await guide.PostAsJsonAsync("/api/check-ins", Scan(trip.TripVoucherCode, trip.TripId));
        var second = await guide.PostAsJsonAsync("/api/check-ins", Scan(trip.TripVoucherCode));
        var third = await guide.PostAsJsonAsync("/api/check-ins", Scan(trip.TripVoucherCode));

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var a = (await first.Content.ReadFromJsonAsync<CheckInResultDto>(TestJson.Options))!;
        a.StopId.Should().Be(trip.StopIds[0]);
        a.StopName.Should().Be("Nine Arches Bridge");
        a.Method.Should().Be("Voucher");
        a.DistanceMeters.Should().BeNull();
        a.TripStatus.Should().Be("InProgress");
        (await second.Content.ReadFromJsonAsync<CheckInResultDto>(TestJson.Options))!.TripStatus.Should().Be("Completed");
        third.StatusCode.Should().Be(HttpStatusCode.Conflict, "the trip is completed");
        var checkIns = await factory.QueryDbAsync(db => db.StopCheckIns.Where(c => trip.StopIds.Contains(c.ItineraryStopId)).ToListAsync());
        checkIns.Should().HaveCount(2).And.OnlyContain(c => c.Method == CheckInMethod.Voucher && c.VoucherId != null);
        var history = await factory.QueryDbAsync(db => db.AuditLogs
            .Where(l => l.EntityId == trip.TripId && l.Action == "TripRequestStatusChanged").Select(l => l.After).ToListAsync());
        history.Should().Contain(h => h!.Contains("InProgress") && h.Contains("First check-in"));
    }

    [Fact]
    public async Task A_forged_code_a_hotel_voucher_and_another_trip_are_400()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory);
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");
        var forged = trip.TripVoucherCode[..^1] + (trip.TripVoucherCode[^1] == 'A' ? 'B' : 'A');

        var badSignature = await guide.PostAsJsonAsync("/api/check-ins", Scan(forged));
        var hotel = await guide.PostAsJsonAsync("/api/check-ins", Scan(trip.HotelVoucherCode));
        var otherTrip = await guide.PostAsJsonAsync("/api/check-ins", Scan(trip.TripVoucherCode, Guid.NewGuid()));

        badSignature.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await badSignature.Content.ReadAsStringAsync()).Should().Contain("signature does not match");
        hotel.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await hotel.Content.ReadAsStringAsync()).Should().Contain("hotel voucher");
        otherTrip.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await otherTrip.Content.ReadAsStringAsync()).Should().Contain("another trip");
        (await factory.QueryDbAsync(db => db.StopCheckIns.CountAsync(c => trip.StopIds.Contains(c.ItineraryStopId)))).Should().Be(0);
    }

    [Fact]
    public async Task Another_guide_gets_403()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory);
        var otherGuide = await factory.CreateClientAsAsync("guide2@tripcraft.test");

        var response = await otherGuide.PostAsJsonAsync("/api/check-ins", Scan(trip.TripVoucherCode));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_voucher_for_a_trip_that_has_not_started_is_409()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory, startInDays: 5);
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        var response = await guide.PostAsJsonAsync("/api/check-ins", Scan(trip.TripVoucherCode));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("This voucher is for");
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.TripId))).Status
            .Should().Be(TripRequestStatus.Confirmed);
    }

    [Fact]
    public async Task GPS_check_in_still_works_as_the_alternative()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory);
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        var response = await guide.PostAsJsonAsync("/api/check-ins", new CheckInRequest(trip.StopIds[1], 6.8691, 81.0663));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = (await response.Content.ReadFromJsonAsync<CheckInResultDto>(TestJson.Options))!;
        result.Method.Should().Be("Gps");
        result.DistanceMeters.Should().NotBeNull();
    }
}
