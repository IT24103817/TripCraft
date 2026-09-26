using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Resources;
using TripCraft.Infrastructure.Persistence;
using TripCraft.Infrastructure.Resources;

namespace TripCraft.Tests.Resources;

/// <summary>An InMemory database with two guides, two vehicles and one Kandy hotel for service tests.</summary>
public sealed class ResourceTestDb : IDisposable
{
    public static readonly Guid EnglishGuide = Guid.NewGuid();
    public static readonly Guid GermanGuide = Guid.NewGuid();
    public static readonly Guid Van = Guid.NewGuid();
    public static readonly Guid Car = Guid.NewGuid();
    public static readonly Guid Kandy = Guid.NewGuid();
    public static readonly Guid KandyDouble = Guid.NewGuid();

    public AppDbContext Db { get; } = new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase($"resources-{Guid.NewGuid()}").Options);

    public ResourceRepository Repository => new(Db);

    public ResourceTestDb()
    {
        Db.Guides.AddRange(
            new Guide { Id = EnglishGuide, Name = "Nimal", Phone = "+94 77 1", DayRateLkr = 6000, MaxPax = 10,
                        Languages = [new GuideLanguage { GuideId = EnglishGuide, LanguageCode = "en" }] },
            new Guide { Id = GermanGuide, Name = "Kumari", Phone = "+94 77 2", DayRateLkr = 6500, MaxPax = 4,
                        Languages = [new GuideLanguage { GuideId = GermanGuide, LanguageCode = "de" },
                                     new GuideLanguage { GuideId = GermanGuide, LanguageCode = "en" }] });
        Db.Vehicles.AddRange(
            new Vehicle { Id = Van, RegistrationNo = "VAN-1", Type = "Van", Seats = 6, RatePerKmLkr = 120 },
            new Vehicle { Id = Car, RegistrationNo = "CAR-1", Type = "Car", Seats = 3, RatePerKmLkr = 100 });
        Db.Hotels.Add(new Hotel
        {
            Id = Kandy, Name = "Kandy Hills", City = "Kandy", StarRating = 4, Latitude = 7.29, Longitude = 80.63,
            RoomTypes = [new RoomType { Id = KandyDouble, HotelId = Kandy, Name = "Double", Capacity = 2, TotalRooms = 3, RatePerNightLkr = 12000 }]
        });
        Db.RateCards.Add(new RateCardEntry { MarginPct = 12m, EffectiveFrom = new DateOnly(2026, 1, 1) });
        Db.SaveChanges();
    }

    public void Hold(Application.Workflows.Ports.ResourceType type, Guid id, DateOnly from, DateOnly to, int quantity = 1,
        HoldStatus status = HoldStatus.Held)
    {
        Db.ResourceHolds.Add(new ResourceHold { ResourceType = type, ResourceId = id, FromDate = from, ToDate = to, Quantity = quantity, Status = status });
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
    }

    public void Dispose() => Db.Dispose();
}
