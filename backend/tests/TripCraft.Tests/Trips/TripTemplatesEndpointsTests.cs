using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Templates;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

/// <summary>Mood packages: seeded list with prices, detail with a valid itinerary, and "Book as is" (v1.1).</summary>
public class TripTemplatesEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Five_packages_are_listed_in_order_with_a_from_price()
    {
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        var list = await tourist.GetFromJsonAsync<List<TripTemplateDto>>("/api/trip-templates", TestJson.Options);

        list!.Select(t => t.Name).Should().Equal("Hill-country escape", "Beach and heritage", "Wildlife weekend",
            "Culture triangle", "Slow train journey");
        list.Should().OnlyContain(t => t.FromPriceLkr > 0 && t.FromPriceUsd > 0 && t.PricedForPax == 2
                                       && t.MoodTag != "" && t.Cities.Count > 0 && t.Itinerary == null);
    }

    [Fact]
    public async Task Every_package_day_uses_1_to_3_real_attractions_in_that_days_city()
    {
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var list = await tourist.GetFromJsonAsync<List<TripTemplateDto>>("/api/trip-templates", TestJson.Options);
        var cityOf = await factory.QueryDbAsync(db => db.Attractions.ToDictionaryAsync(a => a.Id, a => a.City));

        foreach (var summary in list!)
        {
            var detail = await tourist.GetFromJsonAsync<TripTemplateDto>($"/api/trip-templates/{summary.Id}", TestJson.Options);
            detail!.Itinerary.Should().HaveCount(detail.Days);
            foreach (var day in detail.Itinerary!)
            {
                day.Stops.Should().HaveCountGreaterThan(0).And.HaveCountLessThanOrEqualTo(3);
                day.Stops.Should().OnlyContain(s => s.AttractionId != null && cityOf[s.AttractionId.Value] == day.City
                                                    && s.Latitude != null, $"{detail.Name} day {day.Day}");
            }
        }
    }

    [Fact]
    public async Task Book_as_is_creates_a_trip_from_the_package_and_starts_planning()
    {
        var tourist = await factory.CreateClientAsAsync("tourist2@tripcraft.test");
        var template = (await tourist.GetFromJsonAsync<List<TripTemplateDto>>("/api/trip-templates", TestJson.Options))!
            .Single(t => t.Slug == "culture-triangle");
        var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(40);

        var response = await tourist.PostAsJsonAsync($"/api/trip-templates/{template.Id}/book",
            new BookTemplateRequest(start, 2, 900, "Germany", "C01X00T47"));

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var booked = (await response.Content.ReadFromJsonAsync<BookTemplateResponse>(TestJson.Options))!;
        booked.Trip.Objective.Should().Be(template.Objective);
        booked.Trip.Cities.Should().Equal("Sigiriya", "Kandy");
        booked.Trip.EndDate.Should().Be(start.AddDays(3));
        booked.Trip.Status.Should().Be("Planning");
        booked.Planning.Skeleton.Select(d => d.City).Distinct().Should().Equal("Sigiriya", "Kandy");
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == booked.Trip.Id))).Status
            .Should().Be(TripRequestStatus.Planning);
    }

    [Fact]
    public async Task Booking_needs_valid_details_and_a_tourist()
    {
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var id = (await tourist.GetFromJsonAsync<List<TripTemplateDto>>("/api/trip-templates", TestJson.Options))![0].Id;
        var ok = new BookTemplateRequest(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30), 2, 900, "Germany", "C01X00T47");

        (await tourist.PostAsJsonAsync($"/api/trip-templates/{id}/book", ok with { Pax = 0, PassportNumber = "x" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await tourist.PostAsJsonAsync($"/api/trip-templates/{Guid.NewGuid()}/book", ok)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsJsonAsync($"/api/trip-templates/{id}/book", ok)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await guide.GetAsync("/api/trip-templates")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
