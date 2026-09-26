using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface IAvailabilityService
{
    Task<IReadOnlyList<AvailableResourceDto>> SearchAsync(AvailabilityQuery query, CancellationToken ct);
}

/// <summary>
/// Component B business operation for the manager: which guides, vehicles or rooms are free for a date range.
/// Same rules as the agents use (Component B's own ResourceCatalog), plus the rate so the manager can compare options.
/// </summary>
public class AvailabilityService(ResourceCatalog catalog) : IAvailabilityService
{
    public async Task<IReadOnlyList<AvailableResourceDto>> SearchAsync(AvailabilityQuery q, CancellationToken ct)
    {
        var rates = await catalog.GetRateCardAsync(ct);
        return q.Type switch
        {
            ResourceType.Guide => (await catalog.FindAvailableGuidesAsync(q.From, q.To, q.Language!, q.Pax!.Value, ct))
                .Select(g => new AvailableResourceDto(ResourceType.Guide, g.Id, g.Name,
                    $"Speaks {string.Join(", ", g.Languages)} · up to {g.MaxPax} people",
                    rates.GuideDayRates.GetValueOrDefault(g.Id), null))
                .ToList(),
            ResourceType.Vehicle => (await catalog.FindAvailableVehiclesAsync(q.From, q.To, q.Seats!.Value, ct))
                .Select(v => new AvailableResourceDto(ResourceType.Vehicle, v.Id, v.RegistrationNo,
                    $"{v.Type} · {v.Seats} seats", rates.VehicleKmRates.GetValueOrDefault(v.Id), null))
                .ToList(),
            _ => await RoomsAsync(q, rates, ct)
        };
    }

    /// <summary>A room type is listed only if it has enough free rooms on every night asked; FreeRooms is the lowest.</summary>
    private async Task<IReadOnlyList<AvailableResourceDto>> RoomsAsync(AvailabilityQuery q, RateCard rates, CancellationToken ct)
    {
        Dictionary<Guid, RoomOption>? common = null;
        for (var night = q.From; night <= q.To; night = night.AddDays(1))
        {
            var free = (await catalog.FindAvailableRoomsAsync(q.City!, night, q.Rooms!.Value, ct)).ToDictionary(r => r.RoomTypeId);
            common = common is null
                ? free
                : common.Where(c => free.ContainsKey(c.Key))
                    .ToDictionary(c => c.Key, c => c.Value with { AvailableRooms = Math.Min(c.Value.AvailableRooms, free[c.Key].AvailableRooms) });
        }

        return (common ?? []).Values
            .Select(r => new AvailableResourceDto(ResourceType.Room, r.RoomTypeId, $"{r.HotelName} — {r.RoomTypeName}",
                $"Sleeps {r.Capacity}", rates.RoomNightRates.GetValueOrDefault(r.RoomTypeId), r.AvailableRooms))
            .OrderBy(r => r.Name)
            .ToList();
    }
}
