using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Resources;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Resources;

public class ResourceRepository(AppDbContext db) : IResourceRepository
{
    /// <summary>Used when no rate card has started yet (PLAN.md section 3C: 15 % operator margin).</summary>
    public const decimal DefaultMarginPct = 15m;

    public IQueryable<Guide> Guides() => db.Guides.AsNoTracking().Include(g => g.Languages).Where(g => !g.IsDeleted);
    public IQueryable<Vehicle> Vehicles() => db.Vehicles.AsNoTracking().Where(v => !v.IsDeleted);
    public IQueryable<Hotel> Hotels() => db.Hotels.AsNoTracking().Include(h => h.RoomTypes).Where(h => !h.IsDeleted);

    public IQueryable<RoomType> RoomTypes() =>
        db.RoomTypes.AsNoTracking().Include(r => r.Hotel).Where(r => !r.Hotel!.IsDeleted);

    public IQueryable<ResourceHold> Holds() => db.ResourceHolds.AsNoTracking();
    public IQueryable<StopCheckIn> CheckIns() => db.StopCheckIns.AsNoTracking();
    public IQueryable<GuideChangeRequest> GuideChangeRequests() => db.GuideChangeRequests.AsNoTracking();
    public IQueryable<GuideRating> GuideRatings() => db.GuideRatings.AsNoTracking();

    public Task<GuideChangeRequest?> FindGuideChangeRequestAsync(Guid id, CancellationToken ct) =>
        db.GuideChangeRequests.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Guide?> FindGuideAsync(Guid id, CancellationToken ct) =>
        db.Guides.Include(g => g.Languages).FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, ct);

    public Task<Guide?> FindGuideByUserAsync(Guid userId, CancellationToken ct) =>
        db.Guides.AsNoTracking().Include(g => g.Languages).FirstOrDefaultAsync(g => g.UserId == userId && !g.IsDeleted, ct);

    public Task<Vehicle?> FindVehicleAsync(Guid id, CancellationToken ct) =>
        db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, ct);

    public Task<Hotel?> FindHotelAsync(Guid id, CancellationToken ct) =>
        db.Hotels.Include(h => h.RoomTypes).FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted, ct);

    public Task<ResourceHold?> FindHoldAsync(Guid id, CancellationToken ct) =>
        db.ResourceHolds.FirstOrDefaultAsync(h => h.Id == id, ct);

    public Task<List<ResourceHold>> FindHeldHoldsOfTripAsync(Guid tripRequestId, CancellationToken ct) =>
        db.ResourceHolds.Where(h => h.TripRequestId == tripRequestId && h.Status == HoldStatus.Held).ToListAsync(ct);

    public async Task<int> HeldQuantityAsync(ResourceType type, Guid resourceId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var saved = await db.ResourceHolds
            .Where(h => h.ResourceType == type && h.ResourceId == resourceId && h.Status == HoldStatus.Held
                        && h.FromDate <= to && from <= h.ToDate)
            .SumAsync(h => h.Quantity, ct);
        var staged = StagedHolds()
            .Where(h => h.ResourceType == type && h.ResourceId == resourceId
                        && AvailabilityRules.Overlaps(h.FromDate, h.ToDate, from, to))
            .Sum(h => h.Quantity);
        return saved + staged;
    }

    public async Task LockRoomTypeAsync(Guid roomTypeId, CancellationToken ct)
    {
        // Row lock until the surrounding transaction ends. InMemory (tests) has no locks and no transactions.
        if (db.Database.IsNpgsql())
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM room_types WHERE id = {roomTypeId} FOR UPDATE", ct);
    }

    public async Task<decimal> CurrentMarginPctAsync(DateOnly today, CancellationToken ct) =>
        await db.RateCards.Where(r => r.EffectiveFrom <= today).OrderByDescending(r => r.EffectiveFrom)
            .Select(r => (decimal?)r.MarginPct).FirstOrDefaultAsync(ct) ?? DefaultMarginPct;

    public Task<StopLocation?> FindStopAsync(Guid stopId, CancellationToken ct) =>
        (from stop in db.ItineraryStops.AsNoTracking()
         join day in db.ItineraryDays on stop.ItineraryDayId equals day.Id
         join itinerary in db.Itineraries on day.ItineraryId equals itinerary.Id
         join attraction in db.Attractions on stop.AttractionId equals attraction.Id
         where stop.Id == stopId
         select new StopLocation(stop.Id, itinerary.TripRequestId, attraction.Name, attraction.Latitude, attraction.Longitude))
        .FirstOrDefaultAsync(ct);

    public Task<int> CountStopsOfTripAsync(Guid tripRequestId, CancellationToken ct) =>
        StopIdsOfTrip(tripRequestId).CountAsync(ct);

    public Task<int> CountCheckInsOfTripAsync(Guid tripRequestId, CancellationToken ct) =>
        db.StopCheckIns.CountAsync(c => StopIdsOfTrip(tripRequestId).Contains(c.ItineraryStopId), ct);

    public IReadOnlyList<ResourceHold> StagedHolds() =>
        db.ChangeTracker.Entries<ResourceHold>().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();

    public void Add<T>(T entity) where T : class => db.Add(entity);
    public void Remove<T>(T entity) where T : class => db.Remove(entity);

    private IQueryable<Guid> StopIdsOfTrip(Guid tripRequestId) =>
        from stop in db.ItineraryStops
        join day in db.ItineraryDays on stop.ItineraryDayId equals day.Id
        join itinerary in db.Itineraries on day.ItineraryId equals itinerary.Id
        where itinerary.TripRequestId == tripRequestId
        select stop.Id;
}
