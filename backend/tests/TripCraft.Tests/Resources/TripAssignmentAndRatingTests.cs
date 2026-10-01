using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Trips;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

/// <summary>The booked guide and vehicle for the tourist, and "Rate your guide" after a completed trip (v1.1).</summary>
public class TripAssignmentAndRatingTests
{
    [Fact]
    public async Task The_tourist_sees_the_booked_guide_and_vehicle_and_others_cannot()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory, startInDays: 3);
        var owner = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var other = await factory.CreateClientAsAsync("tourist2@tripcraft.test");

        var assignment = await owner.GetFromJsonAsync<TripAssignmentDto>($"/api/trip-requests/{trip.TripId}/assignment", TestJson.Options);

        assignment!.GuideName.Should().Be("Nimal Perera");
        assignment.GuidePhone.Should().NotBeNullOrEmpty();
        assignment.VehicleRegistrationNo.Should().Be("CAB-1234");
        (await other.GetAsync($"/api/trip-requests/{trip.TripId}/assignment")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_guide_can_be_rated_once_and_only_after_the_trip_is_completed()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory);
        var owner = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var rate = new RateGuideRequest(5, "Knew every tea estate by name");

        var tooEarly = await owner.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-rating", rate);
        await factory.QueryDbAsync(async db =>
        {
            (await db.TripRequests.SingleAsync(t => t.Id == trip.TripId)).Status = TripRequestStatus.Completed;
            return await db.SaveChangesAsync();
        });
        var noRatingYet = await owner.GetAsync($"/api/trip-requests/{trip.TripId}/guide-rating");
        var badStars = await owner.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-rating", rate with { Stars = 6 });
        var rated = await owner.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-rating", rate);
        var again = await owner.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-rating", rate);

        tooEarly.StatusCode.Should().Be(HttpStatusCode.Conflict);
        noRatingYet.StatusCode.Should().Be(HttpStatusCode.NotFound);
        badStars.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        rated.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = (await rated.Content.ReadFromJsonAsync<GuideRatingDto>(TestJson.Options))!;
        dto.GuideName.Should().Be("Nimal Perera");
        dto.Stars.Should().Be(5);
        again.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await owner.GetFromJsonAsync<GuideRatingDto>($"/api/trip-requests/{trip.TripId}/guide-rating", TestJson.Options))!
            .Comment.Should().Be("Knew every tea estate by name");
    }

    [Fact]
    public async Task A_manager_reads_but_cannot_rate()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory);
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        (await manager.GetAsync($"/api/trip-requests/{trip.TripId}/assignment")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await manager.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-rating", new RateGuideRequest(4, null)))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
