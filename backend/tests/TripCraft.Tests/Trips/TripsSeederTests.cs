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
    public async Task Seeds_eight_attractions_two_per_city()
    {
        await using var db = await SeededContextAsync();

        var perCity = await db.Attractions.GroupBy(a => a.City).Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

        perCity.Should().HaveCount(4);
        perCity.Select(c => c.Key).Should().BeEquivalentTo(new[] { "Colombo", "Kandy", "Ella", "Galle" });
        perCity.Should().OnlyContain(c => c.Count == 2);
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

        (await db.Attractions.CountAsync()).Should().Be(8);
        (await db.Tourists.CountAsync()).Should().Be(3);
        (await db.TripRequests.CountAsync()).Should().Be(1);
    }
}
