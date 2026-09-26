using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

/// <summary>
/// Component B's read side for the agent workflow (IResourceCatalog): what is free, and at what rate.
/// Used by the internal availability tools of the Resource &amp; Action agent and by the C# proposal validator.
/// </summary>
public class ResourceCatalog(IResourceRepository resources) : IResourceCatalog
{
    public async Task<IReadOnlyList<GuideOption>> FindAvailableGuidesAsync(DateOnly from, DateOnly to, string language,
        int pax, CancellationToken ct)
    {
        var code = language.Trim().ToLower();
        var busy = HeldIds(ResourceType.Guide, from, to);
        var guides = await resources.Guides()
            .Where(g => g.IsActive && g.MaxPax >= pax && g.Languages.Any(l => l.LanguageCode == code) && !busy.Contains(g.Id))
            .OrderBy(g => g.DayRateLkr).ThenBy(g => g.Name)
            .ToListAsync(ct);
        return guides.Select(ToOption).ToList();
    }

    public async Task<IReadOnlyList<VehicleOption>> FindAvailableVehiclesAsync(DateOnly from, DateOnly to, int seats,
        CancellationToken ct)
    {
        var busy = HeldIds(ResourceType.Vehicle, from, to);
        var vehicles = await resources.Vehicles()
            .Where(v => v.IsActive && v.Seats >= seats && !busy.Contains(v.Id))
            .OrderBy(v => v.Seats).ThenBy(v => v.RatePerKmLkr)
            .ToListAsync(ct);
        return vehicles.Select(ToOption).ToList();
    }

    public async Task<IReadOnlyList<RoomOption>> FindAvailableRoomsAsync(string city, DateOnly night, int rooms,
        CancellationToken ct)
    {
        var cityName = city.Trim().ToLower();
        var roomTypes = await resources.RoomTypes()
            .Where(r => r.Hotel!.IsActive && r.Hotel.City.ToLower() == cityName)
            .ToListAsync(ct);
        var ids = roomTypes.Select(r => r.Id).ToList();
        var held = await resources.Holds()
            .Where(h => h.ResourceType == ResourceType.Room && h.Status == HoldStatus.Held && ids.Contains(h.ResourceId)
                        && h.FromDate <= night && night <= h.ToDate)
            .GroupBy(h => h.ResourceId).Select(g => new { g.Key, Rooms = g.Sum(h => h.Quantity) })
            .ToDictionaryAsync(x => x.Key, x => x.Rooms, ct);

        return roomTypes
            .Select(r => ToOption(r, AvailabilityRules.FreeRooms(r.TotalRooms, [held.GetValueOrDefault(r.Id)])))
            .Where(o => o.AvailableRooms >= rooms)
            .OrderBy(o => o.HotelName).ThenBy(o => o.RoomTypeName)
            .ToList();
    }

    public async Task<RateCard> GetRateCardAsync(CancellationToken ct)
    {
        var margin = await resources.CurrentMarginPctAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct);
        return new RateCard(margin,
            await resources.Guides().Where(g => g.IsActive).ToDictionaryAsync(g => g.Id, g => g.DayRateLkr, ct),
            await resources.Vehicles().Where(v => v.IsActive).ToDictionaryAsync(v => v.Id, v => v.RatePerKmLkr, ct),
            await resources.RoomTypes().Where(r => r.Hotel!.IsActive).ToDictionaryAsync(r => r.Id, r => r.RatePerNightLkr, ct));
    }

    public async Task<GuideOption?> GetGuideAsync(Guid id, CancellationToken ct) =>
        await resources.Guides().FirstOrDefaultAsync(g => g.Id == id && g.IsActive, ct) is { } g ? ToOption(g) : null;

    public async Task<VehicleOption?> GetVehicleAsync(Guid id, CancellationToken ct) =>
        await resources.Vehicles().FirstOrDefaultAsync(v => v.Id == id && v.IsActive, ct) is { } v ? ToOption(v) : null;

    public async Task<RoomOption?> GetRoomTypeAsync(Guid roomTypeId, CancellationToken ct) =>
        await resources.RoomTypes().FirstOrDefaultAsync(r => r.Id == roomTypeId && r.Hotel!.IsActive, ct) is { } r
            ? ToOption(r, 0) : null;

    public Task<bool> HasOverlappingHoldAsync(ResourceType type, Guid resourceId, DateOnly from, DateOnly to,
        CancellationToken ct) =>
        resources.Holds().AnyAsync(h => h.ResourceType == type && h.ResourceId == resourceId && h.Status == HoldStatus.Held
                                        && h.FromDate <= to && from <= h.ToDate, ct);

    /// <summary>Ids of resources of this type with a Held hold overlapping [from, to] (AvailabilityRules.Overlaps in SQL).</summary>
    private IQueryable<Guid> HeldIds(ResourceType type, DateOnly from, DateOnly to) =>
        resources.Holds()
            .Where(h => h.ResourceType == type && h.Status == HoldStatus.Held && h.FromDate <= to && from <= h.ToDate)
            .Select(h => h.ResourceId);

    private static GuideOption ToOption(Guide g) =>
        new(g.Id, g.Name, g.Languages.Select(l => l.LanguageCode).OrderBy(c => c).ToList(), g.MaxPax);

    private static VehicleOption ToOption(Vehicle v) => new(v.Id, v.RegistrationNo, v.Type, v.Seats);

    private static RoomOption ToOption(RoomType r, int free) =>
        new(r.HotelId, r.Hotel!.Name, r.Id, r.Name, r.Capacity, free);
}
