using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Trips;

/// <summary>
/// Component A seed data (PLAN.md section 4): 8 attractions across Colombo/Kandy/Ella/Galle,
/// a tourist profile for each seeded Tourist user, and one completed sample trip for reports.
/// </summary>
public static class TripsSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.Attractions.AnyAsync(ct))
            return;

        var attractions = CreateAttractions();
        db.Attractions.AddRange(attractions);

        var tourists = await CreateTouristsAsync(db, ct);
        db.Tourists.AddRange(tourists);

        if (tourists.Count > 0)
            AddCompletedSampleTrip(db, tourists[0], attractions);

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Attractions} attractions, {Tourists} tourists and 1 sample trip",
            attractions.Count, tourists.Count);
    }

    private static List<Attraction> CreateAttractions() =>
    [
        new() { Name = "Gangaramaya Temple", City = "Colombo", Category = "Temple", DurationMinutes = 60, EntryFeeLkr = 500, Latitude = 6.9166, Longitude = 79.8563 },
        new() { Name = "Galle Face Green", City = "Colombo", Category = "Park", DurationMinutes = 60, EntryFeeLkr = 0, Latitude = 6.9271, Longitude = 79.8450 },
        new() { Name = "Temple of the Sacred Tooth Relic", City = "Kandy", Category = "Temple", DurationMinutes = 90, EntryFeeLkr = 2000, Latitude = 7.2936, Longitude = 80.6413 },
        new() { Name = "Royal Botanical Gardens, Peradeniya", City = "Kandy", Category = "Garden", DurationMinutes = 120, EntryFeeLkr = 3000, Latitude = 7.2685, Longitude = 80.5966 },
        new() { Name = "Nine Arches Bridge", City = "Ella", Category = "Viewpoint", DurationMinutes = 60, EntryFeeLkr = 0, Latitude = 6.8768, Longitude = 81.0608 },
        new() { Name = "Little Adam's Peak", City = "Ella", Category = "Hike", DurationMinutes = 150, EntryFeeLkr = 0, Latitude = 6.8691, Longitude = 81.0663 },
        new() { Name = "Galle Fort", City = "Galle", Category = "Heritage", DurationMinutes = 120, EntryFeeLkr = 0, Latitude = 6.0269, Longitude = 80.2170 },
        new() { Name = "Jungle Beach, Unawatuna", City = "Galle", Category = "Beach", DurationMinutes = 120, EntryFeeLkr = 0, Latitude = 6.0183, Longitude = 80.2386 }
    ];

    /// <summary>One profile per seeded Tourist user that does not have one yet.</summary>
    private static async Task<List<Tourist>> CreateTouristsAsync(AppDbContext db, CancellationToken ct)
    {
        var touristUsers = await db.Users
            .Where(u => u.Role == UserRole.Tourist && u.Email.EndsWith("@tripcraft.test"))
            .Where(u => !db.Tourists.Any(t => t.UserId == u.Id))
            .OrderBy(u => u.Email)
            .ToListAsync(ct);

        string[] nationalities = ["United Kingdom", "Germany", "Australia"];
        return touristUsers.Select((user, i) => new Tourist
        {
            UserId = user.Id,
            Nationality = nationalities[i % nationalities.Length],
            PassportNumberMasked = $"****{4521 + i}"
        }).ToList();
    }

    /// <summary>A finished 3-day Kandy + Ella trip so the reports screens have data.</summary>
    private static void AddCompletedSampleTrip(AppDbContext db, Tourist tourist, List<Attraction> attractions)
    {
        Attraction Find(string name) => attractions.Single(a => a.Name == name);

        var trip = new TripRequest
        {
            TouristId = tourist.Id,
            Objective = "3 days for 2 people in Kandy and Ella, hill-country train, English-speaking guide.",
            StartDate = new DateOnly(2026, 8, 10),
            EndDate = new DateOnly(2026, 8, 12),
            Pax = 2,
            BudgetUsd = 900m,
            Preferences = """{"language":"en","transport":"train","pace":"relaxed"}""",
            Status = TripRequestStatus.Completed
        };

        // Hotels per day and the holds of this trip are added by Resources/ResourcesSeeder.
        var itinerary = new Itinerary
        {
            TripRequestId = trip.Id,
            Version = 1,
            GeneratedBy = ItinerarySource.Agent,
            Days =
            [
                new ItineraryDay
                {
                    DayNumber = 1, City = "Kandy", Notes = "Arrive from Colombo by road.",
                    Stops =
                    [
                        new ItineraryStop { AttractionId = Find("Royal Botanical Gardens, Peradeniya").Id, Sequence = 1, ArrivalTime = new TimeOnly(11, 0) },
                        new ItineraryStop { AttractionId = Find("Temple of the Sacred Tooth Relic").Id, Sequence = 2, ArrivalTime = new TimeOnly(16, 0) }
                    ]
                },
                new ItineraryDay
                {
                    DayNumber = 2, City = "Ella", Notes = "Morning train Kandy to Ella.",
                    Stops =
                    [
                        new ItineraryStop { AttractionId = Find("Nine Arches Bridge").Id, Sequence = 1, ArrivalTime = new TimeOnly(16, 30) }
                    ]
                },
                new ItineraryDay
                {
                    DayNumber = 3, City = "Ella", Notes = "Departure after lunch.",
                    Stops =
                    [
                        new ItineraryStop { AttractionId = Find("Little Adam's Peak").Id, Sequence = 1, ArrivalTime = new TimeOnly(7, 0) }
                    ]
                }
            ]
        };

        db.TripRequests.Add(trip);
        db.Itineraries.Add(itinerary);
    }
}
