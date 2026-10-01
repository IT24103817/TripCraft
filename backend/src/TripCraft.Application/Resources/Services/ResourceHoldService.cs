using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

/// <summary>
/// Component B business operation (IResourceHoldService): check, then stage a hold on the current unit of work.
/// Runs inside the caller's transaction (the approval), so the hold commits or rolls back with it.
/// Guides and vehicles: no overlapping Held hold at all. Rooms: held + requested ≤ total rooms on every night.
/// PostgreSQL enforces the guide/vehicle rule again with an exclusion constraint, so two concurrent approvals
/// cannot both win.
/// </summary>
public class ResourceHoldService(IResourceRepository resources) : IResourceHoldService
{
    public async Task CreateHoldAsync(ResourceHoldRequest hold, CancellationToken ct)
    {
        if (hold.To < hold.From)
            throw new ConflictException("A hold must end on or after the day it starts.");

        if (hold.Type == ResourceType.Room)
            await CheckRoomsAsync(hold, ct);
        else
            await CheckSingleAsync(hold, ct);

        resources.Add(new ResourceHold
        {
            ResourceType = hold.Type,
            ResourceId = hold.ResourceId,
            TripRequestId = hold.TripRequestId == Guid.Empty ? null : hold.TripRequestId,
            FromDate = hold.From,
            ToDate = hold.To,
            Quantity = hold.Quantity,
            Status = HoldStatus.Held,
            Note = hold.Note
        });
    }

    public async Task<int> ReleaseTripHoldsAsync(Guid tripRequestId, ResourceType? onlyType, CancellationToken ct)
    {
        var held = await resources.FindHeldHoldsOfTripAsync(tripRequestId, ct);
        var released = held.Where(h => onlyType is null || h.ResourceType == onlyType).ToList();
        foreach (var hold in released)
            hold.Status = HoldStatus.Released;
        return released.Count;
    }

    private async Task CheckSingleAsync(ResourceHoldRequest hold, CancellationToken ct)
    {
        var exists = hold.Type == ResourceType.Guide
            ? await resources.FindGuideAsync(hold.ResourceId, ct) is { IsActive: true }
            : await resources.FindVehicleAsync(hold.ResourceId, ct) is { IsActive: true };
        if (!exists)
            throw new ConflictException($"{hold.Type} {hold.ResourceId} does not exist or is inactive.");

        if (await resources.HeldQuantityAsync(hold.Type, hold.ResourceId, hold.From, hold.To, ct) > 0)
            throw new ConflictException($"{hold.Type} is already held between {hold.From:yyyy-MM-dd} and {hold.To:yyyy-MM-dd}.");
    }

    private async Task CheckRoomsAsync(ResourceHoldRequest hold, CancellationToken ct)
    {
        var roomType = await resources.RoomTypes().FirstOrDefaultAsync(r => r.Id == hold.ResourceId, ct)
                       ?? throw new ConflictException($"Room type {hold.ResourceId} does not exist.");

        // Serialise holds on this room type until the transaction ends (SELECT ... FOR UPDATE in PostgreSQL).
        await resources.LockRoomTypeAsync(roomType.Id, ct);

        for (var night = hold.From; night <= hold.To; night = night.AddDays(1))
        {
            var held = await resources.HeldQuantityAsync(ResourceType.Room, roomType.Id, night, night, ct);
            if (AvailabilityRules.FreeRooms(roomType.TotalRooms, [held]) < hold.Quantity)
                throw new ConflictException(
                    $"Only {AvailabilityRules.FreeRooms(roomType.TotalRooms, [held])} {roomType.Name} rooms are free on {night:yyyy-MM-dd}.");
        }
    }
}
