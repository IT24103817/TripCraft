using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Infrastructure.Persistence;
using TripCraft.Infrastructure.Persistence.Seeding;

namespace TripCraft.Tests.Trips;

public class TripsSeederTests
{
    private static async Task<AppDbContext> SeededContextAsync()
    {
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var seeder = new DataSeeder(db, new PasswordHasher<User>(), NullLogger<DataSeeder>.Instance);
        await seeder.SeedAsync();
        return db;
    }

    [Fact]
    public async Task Seeds_twenty_one_attractions_in_six_cities_so_a_five_day_trip_never_runs_out()
    {
        await using var db = await SeededContextAsync();

        var perCity = await db.Attractions.GroupBy(a => a.City).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

        perCity.ToDictionary(c => c.Key, c => c.Count).Should().BeEquivalentTo(new Dictionary<string, int>
        {
            ["Colombo"] = 2, ["Kandy"] = 4, ["Ella"] = 4, ["Galle"] = 5, ["Nuwara Eliya"] = 3, ["Sigiriya"] = 3
        });
        // A 5-day trip needs 5 stops at the least (1 per day): Kandy + Ella alone now have 8.
        perCity.Where(c => c.Key is "Kandy" or "Ella").Sum(c => c.Count).Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task Every_attraction_city_has_a_hotel_and_a_distance_to_every_other_city()
    {
        await using var db = await SeededContextAsync();

        var cities = await db.Attractions.Select(a => a.City).Distinct().ToListAsync();
        var hotelCities = await db.Hotels.Select(h => h.City).Distinct().ToListAsync();
        var pairs = await db.CityDistances.Select(d => new { d.FromCity, d.ToCity }).ToListAsync();

        hotelCities.Should().Contain(cities);
        foreach (var a in cities)
        foreach (var b in cities.Where(b => string.CompareOrdinal(a, b) < 0))
            pairs.Should().Contain(p => (p.FromCity == a && p.ToCity == b) || (p.FromCity == b && p.ToCity == a),
                $"the itinerary agent needs the distance {a} - {b}");
        pairs.Should().HaveCount(15);
    }

    [Fact]
    public async Task An_existing_database_gets_the_missing_attractions_hotels_and_distances_on_the_next_start()
    {
        await using var db = await SeededContextAsync();
        // Simulate a database seeded before the new cities existed.
        db.Attractions.RemoveRange(db.Attractions.Where(a => a.City == "Sigiriya"));
        db.RoomTypes.RemoveRange(db.RoomTypes.Where(r => db.Hotels.Any(h => h.Id == r.HotelId && h.City == "Sigiriya")));
        db.Hotels.RemoveRange(db.Hotels.Where(h => h.City == "Sigiriya"));
        db.CityDistances.RemoveRange(db.CityDistances.Where(d => d.FromCity == "Sigiriya" || d.ToCity == "Sigiriya"));
        await db.SaveChangesAsync();

        await new DataSeeder(db, new PasswordHasher<User>(), NullLogger<DataSeeder>.Instance).SeedAsync();

        (await db.Attractions.CountAsync(a => a.City == "Sigiriya")).Should().Be(3);
        (await db.Hotels.CountAsync(h => h.City == "Sigiriya")).Should().Be(1);
        (await db.CityDistances.CountAsync()).Should().Be(15);
        (await db.TripRequests.CountAsync()).Should().Be(1); // the sample trip is not added twice
    }

    [Fact]
    public async Task Seeds_a_masked_profile_for_each_tourist_user()
    {
        await using var db = await SeededContextAsync();

        var tourists = await db.Tourists.ToListAsync();

        tourists.Should().HaveCount(3);
        tourists.Should().OnlyContain(t => t.PassportNumberMasked.StartsWith("****") && t.PassportNumberMasked.Length == 8);
    }

    [Fact]
    public async Task Seeds_one_completed_trip_with_a_valid_itinerary()
    {
        await using var db = await SeededContextAsync();

        var trip = await db.TripRequests.SingleAsync();
        var itinerary = await db.Itineraries
            .Include(i => i.Days).ThenInclude(d => d.Stops)
            .SingleAsync(i => i.TripRequestId == trip.Id);

        trip.Status.Should().Be(TripRequestStatus.Completed);
        trip.EndDate.Should().BeOnOrAfter(trip.StartDate);
        itinerary.Days.Select(d => d.DayNumber).Should().BeEquivalentTo(new[] { 1, 2, 3 });
        // Operator rule from PLAN.md section 5: every day has 1–3 stops.
        itinerary.Days.Should().OnlyContain(d => d.Stops.Count >= 1 && d.Stops.Count <= 3);
    }

    [Fact]
    public async Task Running_the_seeder_twice_adds_nothing_new()
    {
        await using var db = await SeededContextAsync();

        await new DataSeeder(db, new PasswordHasher<User>(), NullLogger<DataSeeder>.Instance).SeedAsync();

        (await db.Attractions.CountAsync()).Should().Be(21);
        (await db.Hotels.CountAsync()).Should().Be(6);
        (await db.CityDistances.CountAsync()).Should().Be(15);
        (await db.Tourists.CountAsync()).Should().Be(3);
        (await db.TripRequests.CountAsync()).Should().Be(1);
    }
}
