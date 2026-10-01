using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Loads the database facts ProposalValidator needs, so the validator stays a pure function. Used when the agents
/// send a proposal and again when the manager re-prices an edited one.
/// </summary>
public class ProposalFactsLoader(IAttractionRepository attractions, IResourceCatalog resources)
{
    /// <summary>Loads only the rows the proposal refers to. Unparseable ids are left for the validator to flag.</summary>
    public async Task<ProposalFacts> LoadAsync(AgentProposalRequest proposal, TripRequest trip, CancellationToken ct)
    {
        var attractionIds = (proposal.Days ?? [])
            .SelectMany(d => d.Stops ?? [])
            .Select(s => ProposalValidator.ParseId(s.AttractionId))
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var entryFees = await attractions.QueryActive()
            .Where(a => attractionIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.EntryFeeLkr, ct);

        var guideId = ProposalValidator.ParseId(proposal.Resources?.GuideId);
        var vehicleId = ProposalValidator.ParseId(proposal.Resources?.VehicleId);
        var guide = guideId is null ? null : await resources.GetGuideAsync(guideId.Value, ct);
        var vehicle = vehicleId is null ? null : await resources.GetVehicleAsync(vehicleId.Value, ct);

        var roomTypes = new Dictionary<Guid, RoomOption>();
        var roomTypeIds = (proposal.Resources?.Rooms ?? [])
            .Select(r => ProposalValidator.ParseId(r.RoomTypeId)).OfType<Guid>().Distinct();
        foreach (var id in roomTypeIds)
        {
            if (await resources.GetRoomTypeAsync(id, ct) is { } roomType)
                roomTypes[id] = roomType;
        }

        var guideOverlaps = guide is not null && await resources.HasOverlappingHoldAsync(
            ResourceType.Guide, guide.Id, trip.StartDate, trip.EndDate, ct);
        var vehicleOverlaps = vehicle is not null && await resources.HasOverlappingHoldAsync(
            ResourceType.Vehicle, vehicle.Id, trip.StartDate, trip.EndDate, ct);

        return new ProposalFacts(entryFees, guide, vehicle, roomTypes, guideOverlaps, vehicleOverlaps,
            await resources.GetRateCardAsync(ct));
    }
}
