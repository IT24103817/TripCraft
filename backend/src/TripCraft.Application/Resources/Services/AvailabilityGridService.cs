using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Identity;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface IAvailabilityGridService
{
    Task<AvailabilityGridDto> GetAsync(AvailabilityGridQuery query, CancellationToken ct);
}

/// <summary>
/// The manager's availability grid (v1.1): one row per guide, vehicle and hotel room type, one cell per day, each
/// cell Free, Held (trip not confirmed yet), Confirmed (booked trip) or Blocked (manual hold: leave, maintenance).
/// Rooms can be partly held, so their cells also give the free count.
/// </summary>
public class AvailabilityGridService(
    IResourceRepository resources,
    ITripRequestRepository trips,
    IUserRepository users) : IAvailabilityGridService
{
    private static readonly TripRequestStatus[] Booked =
        [TripRequestStatus.Confirmed, TripRequestStatus.InProgress, TripRequestStatus.Completed];

    public async Task<AvailabilityGridDto> GetAsync(AvailabilityGridQuery query, CancellationToken ct)
    {
        var days = Enumerable.Range(0, query.To.DayNumber - query.From.DayNumber + 1).Select(query.From.AddDays).ToList();
        var resourceRows = await ResourceRowsAsync(query, ct);
        var ids = resourceRows.Select(r => r.Id).ToList();
        var holds = await resources.Holds()
            .Where(h => h.Status == HoldStatus.Held && ids.Contains(h.ResourceId) && h.FromDate <= query.To && query.From <= h.ToDate)
            .ToListAsync(ct);
        var tripInfo = await TripInfoAsync(holds, ct);

        var rows = resourceRows.Select(r => new AvailabilityRowDto(r.Type, r.Id, r.Name, r.Detail, r.Capacity,
            days.Select(day => Cell(day, r.Capacity, holds.Where(h => h.ResourceId == r.Id && h.FromDate <= day && day <= h.ToDate).ToList(),
                tripInfo)).ToList())).ToList();
        return new AvailabilityGridDto(days, rows);
    }

    /// <summary>The state of one resource on one day from the holds covering it.</summary>
    public static AvailabilityCellDto Cell(DateOnly day, int capacity, IReadOnlyList<ResourceHold> covering,
        IReadOnlyDictionary<Guid, (string TouristName, TripRequestStatus Status)> tripInfo)
    {
        if (covering.Count == 0)
            return new AvailabilityCellDto(day, "Free", null, null, null, null, null, 0, capacity);

        var main = covering.OrderByDescending(h => h.Quantity).First();
        var held = covering.Sum(h => h.Quantity);
        var trip = main.TripRequestId is { } tid && tripInfo.TryGetValue(tid, out var info) ? info : ((string, TripRequestStatus)?)null;
        var state = main.TripRequestId is null ? "Blocked"
            : trip is { } t && Booked.Contains(t.Item2) ? "Confirmed"
            : "Held";
        return new AvailabilityCellDto(day, state, main.Id, main.TripRequestId, trip?.Item1, trip?.Item2.ToString(), main.Note,
            held, Math.Max(0, capacity - held));
    }

    private record ResourceRow(ResourceType Type, Guid Id, string Name, string Detail, int Capacity);

    private async Task<List<ResourceRow>> ResourceRowsAsync(AvailabilityGridQuery query, CancellationToken ct)
    {
        var rows = new List<ResourceRow>();
        if (query.Type is null or ResourceType.Guide)
        {
            var guides = resources.Guides().Where(g => g.IsActive);
            if (!string.IsNullOrWhiteSpace(query.Language))
                guides = guides.Where(g => g.Languages.Any(l => l.LanguageCode == query.Language.ToLower()));
            rows.AddRange((await guides.OrderBy(g => g.Name).ToListAsync(ct)).Select(g => new ResourceRow(ResourceType.Guide, g.Id,
                g.Name, string.Join(", ", g.Languages.Select(l => l.LanguageCode).OrderBy(c => c)) + $" · up to {g.MaxPax}", 1)));
        }
        if (query.Type is null or ResourceType.Vehicle)
        {
            var vehicles = resources.Vehicles().Where(v => v.IsActive);
            if (query.Seats is { } seats)
                vehicles = vehicles.Where(v => v.Seats >= seats);
            rows.AddRange((await vehicles.OrderBy(v => v.RegistrationNo).ToListAsync(ct)).Select(v => new ResourceRow(
                ResourceType.Vehicle, v.Id, v.RegistrationNo, $"{v.Type} · {v.Seats} seats", 1)));
        }
        if (query.Type is null or ResourceType.Room)
        {
            var rooms = resources.RoomTypes().Where(r => r.Hotel!.IsActive);
            if (!string.IsNullOrWhiteSpace(query.City))
                rooms = rooms.Where(r => r.Hotel!.City.ToLower() == query.City.Trim().ToLower());
            rows.AddRange((await rooms.OrderBy(r => r.Hotel!.City).ThenBy(r => r.Hotel!.Name).ThenBy(r => r.Name).ToListAsync(ct))
                .Select(r => new ResourceRow(ResourceType.Room, r.Id, $"{r.Hotel!.Name} — {r.Name}",
                    $"{r.Hotel.City} · sleeps {r.Capacity}", r.TotalRooms)));
        }
        return rows;
    }

    private async Task<Dictionary<Guid, (string TouristName, TripRequestStatus Status)>> TripInfoAsync(
        List<ResourceHold> holds, CancellationToken ct)
    {
        var tripIds = holds.Where(h => h.TripRequestId != null).Select(h => h.TripRequestId!.Value).Distinct().ToList();
        var tripRows = await trips.Query().Where(t => tripIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Status, UserId = t.Tourist!.UserId }).ToListAsync(ct);
        var names = (await users.ListAsync(ct)).ToDictionary(u => u.Id, u => u.FullName);
        return tripRows.ToDictionary(t => t.Id, t => (names.GetValueOrDefault(t.UserId, "Tourist"), t.Status));
    }
}
