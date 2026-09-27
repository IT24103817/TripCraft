using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;
using TripCraft.Infrastructure.Persistence.Auditing;
using TripCraft.Infrastructure.Trips;

namespace TripCraft.Tests.Shared.Database;

/// <summary>
/// The itinerary editor deletes a day's stops and inserts new ones with the same sequence numbers in one save;
/// PostgreSQL's unique (itinerary_day_id, sequence) index must not reject that.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ItineraryEditPostgresTests(PostgresFixture postgres) : IAsyncLifetime
{
    private string _connectionString = string.Empty;

    public async Task InitializeAsync() => _connectionString = await postgres.CreateMigratedDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Replacing_the_stops_of_a_day_commits_in_postgres()
    {
        Guid tripId, a, b, c;
        await using (var db = PostgresFixture.CreateContext(_connectionString))
        {
            var user = new User { Email = "edit@tripcraft.test", FullName = "Edit", PasswordHash = "hash", Role = UserRole.Tourist };
            var tourist = new Tourist { UserId = user.Id, Nationality = "UK", PassportNumberMasked = "****1234" };
            var attractions = new[] { "One", "Two", "Three" }.Select(n => new Attraction
                { Name = n, City = "Ella", Category = "Viewpoint", DurationMinutes = 60, Latitude = 6.87, Longitude = 81.06 }).ToList();
            var start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
            var trip = new TripRequest { TouristId = tourist.Id, Objective = "Ella", StartDate = start, EndDate = start.AddDays(1),
                Pax = 2, BudgetUsd = 500, Preferences = "{}", Status = TripRequestStatus.Confirmed };
            var day = new ItineraryDay { DayNumber = 1, City = "Ella" };
            day.Stops.Add(new ItineraryStop { ItineraryDayId = day.Id, AttractionId = attractions[0].Id, Sequence = 1 });
            day.Stops.Add(new ItineraryStop { ItineraryDayId = day.Id, AttractionId = attractions[1].Id, Sequence = 2 });
            db.AddRange(user, tourist, trip, new Itinerary { TripRequestId = trip.Id, Days = [day] });
            db.Attractions.AddRange(attractions);
            await db.SaveChangesAsync();
            (tripId, a, b, c) = (trip.Id, attractions[0].Id, attractions[1].Id, attractions[2].Id);
        }

        await using (var db = PostgresFixture.CreateContext(_connectionString))
        {
            var service = new ItineraryEditService(new TripRequestRepository(db), new AttractionRepository(db),
                new AuditLogger(db), db);
            var result = await service.EditDayAsync(Guid.NewGuid(), tripId, 1, new EditItineraryDayRequest([c, b, a], null),
                CancellationToken.None);

            result.Days[0].Stops.Select(s => s.AttractionName).Should().Equal("Three", "Two", "One");
            result.Version.Should().Be(2);
        }

        await using var check = PostgresFixture.CreateContext(_connectionString);
        (await check.ItineraryStops.CountAsync()).Should().Be(3);
    }
}
