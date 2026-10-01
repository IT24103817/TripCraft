using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Trips;

/// <summary>
/// Component A seed data (PLAN.md section 4, extended for the demo): 21 attractions in six cities — Colombo 2,
/// Kandy 4, Ella 4, Galle 5, Nuwara Eliya 3, Sigiriya 3 — so a 5-day trip never runs out of stops; a tourist
/// profile for each seeded Tourist user; and one completed sample trip for reports.
/// Safe to run on every start: it only adds attractions whose name is missing and profiles that are missing.
/// </summary>
public static class TripsSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        // IgnoreQueryFilters: a soft-deleted seed attraction counts as present, so it is not re-added.
        var existing = await db.Attractions.IgnoreQueryFilters().Select(a => a.Name).ToListAsync(ct);
        var missing = CreateAttractions().Where(a => !existing.Contains(a.Name)).ToList();
        db.Attractions.AddRange(missing);

        var tourists = await CreateTouristsAsync(db, ct);
        db.Tourists.AddRange(tourists);
        await db.SaveChangesAsync(ct);

        // The sample trip belongs to the first seeded tourist (tourist1), as before.
        var firstTourist = await (from t in db.Tourists
                                  join u in db.Users on t.UserId equals u.Id
                                  where u.Email.EndsWith("@tripcraft.test")
                                  orderby u.Email
                                  select t).FirstOrDefaultAsync(ct);
        var addSampleTrip = firstTourist is not null && !await db.TripRequests.AnyAsync(ct);
        if (addSampleTrip)
        {
            AddCompletedSampleTrip(db, firstTourist!, await db.Attractions.ToListAsync(ct));
            await db.SaveChangesAsync(ct);
        }

        if (missing.Count > 0 || tourists.Count > 0 || addSampleTrip)
            logger.LogInformation("Seeded {Attractions} attractions, {Tourists} tourists and {Trips} sample trip",
                missing.Count, tourists.Count, addSampleTrip ? 1 : 0);
    }

    private static List<Attraction> CreateAttractions() =>
    [
        new() { Name = "Gangaramaya Temple", City = "Colombo", Category = "Temple", DurationMinutes = 60, EntryFeeLkr = 500, Latitude = 6.9166, Longitude = 79.8563 },
        new() { Name = "Galle Face Green", City = "Colombo", Category = "Park", DurationMinutes = 60, EntryFeeLkr = 0, Latitude = 6.9271, Longitude = 79.8450 },
        new() { Name = "Temple of the Sacred Tooth Relic", City = "Kandy", Category = "Temple", DurationMinutes = 90, EntryFeeLkr = 2000, Latitude = 7.2936, Longitude = 80.6413 },
        new() { Name = "Royal Botanical Gardens, Peradeniya", City = "Kandy", Category = "Garden", DurationMinutes = 120, EntryFeeLkr = 3000, Latitude = 7.2685, Longitude = 80.5966 },
        new() { Name = "Kandy Lake", City = "Kandy", Category = "Park", DurationMinutes = 45, EntryFeeLkr = 0, Latitude = 7.2926, Longitude = 80.6424 },
        new() { Name = "Bahirawakanda Vihara Buddha Statue", City = "Kandy", Category = "Viewpoint", DurationMinutes = 60, EntryFeeLkr = 500, Latitude = 7.2956, Longitude = 80.6303 },
        new() { Name = "Nine Arches Bridge", City = "Ella", Category = "Viewpoint", DurationMinutes = 60, EntryFeeLkr = 0, Latitude = 6.8768, Longitude = 81.0608 },
        new() { Name = "Little Adam's Peak", City = "Ella", Category = "Hike", DurationMinutes = 150, EntryFeeLkr = 0, Latitude = 6.8691, Longitude = 81.0663 },
        new() { Name = "Ravana Falls", City = "Ella", Category = "Waterfall", DurationMinutes = 45, EntryFeeLkr = 0, Latitude = 6.8403, Longitude = 81.0536 },
        new() { Name = "Ella Rock", City = "Ella", Category = "Hike", DurationMinutes = 240, EntryFeeLkr = 0, Latitude = 6.8547, Longitude = 81.0419 },
        new() { Name = "Galle Fort", City = "Galle", Category = "Heritage", DurationMinutes = 120, EntryFeeLkr = 0, Latitude = 6.0269, Longitude = 80.2170 },
        new() { Name = "Jungle Beach, Unawatuna", City = "Galle", Category = "Beach", DurationMinutes = 120, EntryFeeLkr = 0, Latitude = 6.0183, Longitude = 80.2386 },
        new() { Name = "Galle Lighthouse", City = "Galle", Category = "Heritage", DurationMinutes = 30, EntryFeeLkr = 0, Latitude = 6.0247, Longitude = 80.2195 },
        new() { Name = "Japanese Peace Pagoda, Rumassala", City = "Galle", Category = "Temple", DurationMinutes = 45, EntryFeeLkr = 0, Latitude = 6.0165, Longitude = 80.2393 },
        new() { Name = "Koggala Sea Turtle Hatchery", City = "Galle", Category = "Wildlife", DurationMinutes = 60, EntryFeeLkr = 1500, Latitude = 5.9927, Longitude = 80.3287 },
        new() { Name = "Gregory Lake", City = "Nuwara Eliya", Category = "Park", DurationMinutes = 90, EntryFeeLkr = 400, Latitude = 6.9580, Longitude = 80.7780 },
        new() { Name = "Pedro Tea Estate", City = "Nuwara Eliya", Category = "Tea estate", DurationMinutes = 90, EntryFeeLkr = 750, Latitude = 6.9660, Longitude = 80.7860 },
        new() { Name = "Horton Plains and World's End", City = "Nuwara Eliya", Category = "Hike", DurationMinutes = 240, EntryFeeLkr = 6000, Latitude = 6.8020, Longitude = 80.8060 },
        new() { Name = "Sigiriya Rock Fortress", City = "Sigiriya", Category = "Heritage", DurationMinutes = 180, EntryFeeLkr = 9000, Latitude = 7.9570, Longitude = 80.7603 },
        new() { Name = "Pidurangala Rock", City = "Sigiriya", Category = "Hike", DurationMinutes = 120, EntryFeeLkr = 1000, Latitude = 7.9667, Longitude = 80.7597 },
        new() { Name = "Dambulla Cave Temple", City = "Sigiriya", Category = "Temple", DurationMinutes = 90, EntryFeeLkr = 2000, Latitude = 7.8567, Longitude = 80.6490 }
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
            Cities = TripRequest.JoinCities(["Kandy", "Ella"]),
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
