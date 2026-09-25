using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Everything ProposalValidator needs from the database, loaded beforehand by WorkflowProposalService
/// so the validator itself stays a pure function (no database, clock or HTTP).
/// </summary>
public record ProposalFacts(
    IReadOnlyDictionary<Guid, decimal> AttractionEntryFeesLkr,
    GuideOption? Guide,
    VehicleOption? Vehicle,
    IReadOnlyDictionary<Guid, RoomOption> RoomTypes,
    bool GuideHoldOverlaps,
    bool VehicleHoldOverlaps,
    RateCard? RateCard);
