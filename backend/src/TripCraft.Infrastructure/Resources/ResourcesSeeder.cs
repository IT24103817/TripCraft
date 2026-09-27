using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Identity;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Resources;

/// <summary>
/// PLAN.md section 4 seed: 4 guides, 3 vehicles, a 15 % rate card, and 6 hotels with 2 room types each — one per
/// attraction city (Kandy, Ella, Colombo, Galle, Nuwara Eliya, Sigiriya), so every city a trip can visit has rooms.
/// Ids are fixed so demos, agent fixtures and tests can refer to them. Guides, vehicles and the rate card are
/// seeded only when the guides table is empty; hotels missing by id are added on every start.
/// Also links the completed sample trip (TripsSeeder) to its hotels and holds, so reports have data.
/// </summary>
public static class ResourcesSeeder
{
    public static readonly Guid NimalGuide = Guid.Parse("00000000-0000-0000-0000-00000000a001");
    public static readonly Guid VanSixSeats = Guid.Parse("00000000-0000-0000-0000-00000000b001");
    public static readonly Guid KandyHotel = Guid.Parse("00000000-0000-0000-0000-00000000c001");
    public static readonly Guid KandyStandard = Guid.Parse("00000000-0000-0000-0000-00000000c011");
    public static readonly Guid EllaHotel = Guid.Parse("00000000-0000-0000-0000-00000000c002");
    public static readonly Guid EllaStandard = Guid.Parse("00000000-0000-0000-0000-00000000c021");

    /// <remarks>Nimal (guide1's login) is the cheapest English guide, so the demo trip is visible on the guide's phone.</remarks>
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var firstRun = !await db.Guides.AnyAsync(ct);
        if (firstRun)
            await SeedGuidesVehiclesAndRatesAsync(db, ct);

        var hotelsAdded = await AddMissingHotelsAsync(db, ct);

        if (firstRun)
            await LinkSampleTripAsync(db, ct);
        if (firstRun || hotelsAdded > 0)
            logger.LogInformation("Seeded resources: first run {FirstRun}, {Hotels} hotels added", firstRun, hotelsAdded);
    }

    private static async Task SeedGuidesVehiclesAndRatesAsync(AppDbContext db, CancellationToken ct)
    {
        var guideUsers = await db.Users.Where(u => u.Role == UserRole.Guide && u.Email.EndsWith("@tripcraft.test"))
            .OrderBy(u => u.Email).Select(u => u.Id).ToListAsync(ct);
        Guid? UserAt(int i) => i < guideUsers.Count ? guideUsers[i] : null;

        db.Guides.AddRange(
            Guide(NimalGuide, "Nimal Perera", "+94 77 123 4567", 6000, 10, UserAt(0), "en", "si"),
            Guide(Id("a002"), "Kumari Silva", "+94 71 222 3344", 6500, 8, UserAt(1), "en", "de"),
            Guide(Id("a003"), "Ruwan Fernando", "+94 76 555 0101", 7000, 12, UserAt(2), "en", "fr", "ja"),
            Guide(Id("a004"), "Anjali Jayasinghe", "+94 70 888 9090", 7500, 6, null, "en", "zh"));

        db.Vehicles.AddRange(
            new Vehicle { Id = VanSixSeats, RegistrationNo = "CAB-1234", Type = "Van", Seats = 6, RatePerKmLkr = 120 },
            new Vehicle { Id = Id("b002"), RegistrationNo = "CAR-9876", Type = "Car", Seats = 3, RatePerKmLkr = 100 },
            new Vehicle { Id = Id("b003"), RegistrationNo = "NC-4455", Type = "Coach", Seats = 15, RatePerKmLkr = 180 });

        db.RateCards.Add(new RateCardEntry { MarginPct = 15m, EffectiveFrom = new DateOnly(2026, 1, 1) });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Adds the seed hotels whose id is not in the table (soft-deleted ones count as present).</summary>
    private static async Task<int> AddMissingHotelsAsync(AppDbContext db, CancellationToken ct)
    {
        var existing = await db.Hotels.IgnoreQueryFilters().Select(h => h.Id).ToListAsync(ct);
        var missing = Hotels().Where(h => !existing.Contains(h.Id)).ToList();
        if (missing.Count == 0)
            return 0;
        db.Hotels.AddRange(missing);
        await db.SaveChangesAsync(ct);
        return missing.Count;
    }

    private static Hotel[] Hotels() =>
    [
            Hotel(KandyHotel, "Kandy Hills", "Kandy", 4, 7.2906, 80.6337,
                Room(KandyStandard, "Standard Double", 2, 12000, 5), Room(Id("c012"), "Family Room", 4, 20000, 3)),
            Hotel(EllaHotel, "Ella Gap", "Ella", 3, 6.8667, 81.0466,
                Room(EllaStandard, "Standard Double", 2, 12000, 5), Room(Id("c022"), "Deluxe Double", 2, 18000, 3)),
            Hotel(Id("c003"), "Galle Face Residence", "Colombo", 4, 6.9271, 79.8612,
                Room(Id("c031"), "Standard Double", 2, 15000, 8), Room(Id("c032"), "Suite", 3, 30000, 2)),
            Hotel(Id("c004"), "Fort Bay Hotel", "Galle", 4, 6.0269, 80.2170,
                Room(Id("c041"), "Standard Double", 2, 14000, 6), Room(Id("c042"), "Family Room", 4, 22000, 2)),
            Hotel(Id("c005"), "Highland Mist Inn", "Nuwara Eliya", 3, 6.9497, 80.7891,
                Room(Id("c051"), "Standard Double", 2, 13000, 5), Room(Id("c052"), "Family Room", 4, 21000, 2)),
            Hotel(Id("c006"), "Lion Rock Lodge", "Sigiriya", 3, 7.9496, 80.7512,
                Room(Id("c061"), "Standard Double", 2, 12500, 6), Room(Id("c062"), "Family Room", 4, 20000, 2))
    ];

    /// <summary>The completed sample trip: hotel per day and the holds it used (guide, van, rooms).</summary>
    private static async Task LinkSampleTripAsync(AppDbContext db, CancellationToken ct)
    {
        var trip = await db.TripRequests.FirstOrDefaultAsync(t => t.Status == TripRequestStatus.Completed, ct);
        if (trip is null)
            return;
        var days = await db.ItineraryDays.Where(d => db.Itineraries.Any(i => i.Id == d.ItineraryId && i.TripRequestId == trip.Id))
            .OrderBy(d => d.DayNumber).ToListAsync(ct);
        foreach (var day in days.Where(d => d.DayNumber < days.Count))
            day.HotelId = day.City == "Kandy" ? KandyHotel : EllaHotel;

        db.ResourceHolds.Add(Hold(ResourceType.Guide, NimalGuide, trip, trip.StartDate, trip.EndDate, 1));
        db.ResourceHolds.Add(Hold(ResourceType.Vehicle, VanSixSeats, trip, trip.StartDate, trip.EndDate, 1));
        foreach (var day in days.Where(d => d.HotelId is not null))
        {
            var night = trip.StartDate.AddDays(day.DayNumber - 1);
            db.ResourceHolds.Add(Hold(ResourceType.Room, day.City == "Kandy" ? KandyStandard : EllaStandard, trip, night, night, 1));
        }
        await db.SaveChangesAsync(ct);
    }

    private static Guid Id(string suffix) => Guid.Parse($"00000000-0000-0000-0000-00000000{suffix}");

    private static Guide Guide(Guid id, string name, string phone, decimal rate, int maxPax, Guid? userId, params string[] languages) =>
        new()
        {
            Id = id, Name = name, Phone = phone, DayRateLkr = rate, MaxPax = maxPax, UserId = userId,
            Languages = languages.Select(code => new GuideLanguage { GuideId = id, LanguageCode = code }).ToList()
        };

    private static Hotel Hotel(Guid id, string name, string city, int stars, double lat, double lng, params RoomType[] rooms) =>
        new()
        {
            Id = id, Name = name, City = city, StarRating = stars, Latitude = lat, Longitude = lng,
            RoomTypes = rooms.Select(r => { r.HotelId = id; return r; }).ToList()
        };

    private static RoomType Room(Guid id, string name, int capacity, decimal rate, int total) =>
        new() { Id = id, Name = name, Capacity = capacity, RatePerNightLkr = rate, TotalRooms = total };

    private static ResourceHold Hold(ResourceType type, Guid id, TripRequest trip, DateOnly from, DateOnly to, int quantity) =>
        new() { ResourceType = type, ResourceId = id, TripRequestId = trip.Id, FromDate = from, ToDate = to, Quantity = quantity };
}
