using FluentValidation;

namespace TripCraft.Application.Workflows.Dtos;

/// <summary>
/// PUT /api/trip-requests/{id}/proposal/resources: the manager swaps the guide, the vehicle and/or the hotel room
/// type of a city, picked from GET /api/availability. Null fields stay as they are.
/// </summary>
public record SwapResourcesRequest(Guid? GuideId, Guid? VehicleId, IReadOnlyList<RoomSwap>? Rooms);

/// <summary>Every night spent in City uses RoomTypeId (enough rooms for the party).</summary>
public record RoomSwap(string City, Guid RoomTypeId);

/// <summary>The proposal under review after an edit. Days and resources use the agents' snake_case shape.</summary>
public record EditableProposalDto(Guid WorkflowId, Guid TripRequestId, bool EditedSinceQuotation,
    IReadOnlyList<ProposalDay> Days, ProposalResources? Resources);

/// <summary>POST /api/quotations/{id}/calculate: the new version made from the (edited) proposal.</summary>
public record RepriceResponse(Guid QuotationId, int Version, decimal TotalLkr, decimal TotalUsd,
    decimal PreviousTotalLkr, decimal PreviousTotalUsd, string WorkflowStatus, ProposalValidationResult Validation);

public class SwapResourcesRequestValidator : AbstractValidator<SwapResourcesRequest>
{
    public SwapResourcesRequestValidator()
    {
        RuleFor(r => r).Must(r => r.GuideId is not null || r.VehicleId is not null || r.Rooms is { Count: > 0 })
            .WithMessage("Choose a guide, a vehicle or a room type to swap.").WithName("request");
        RuleFor(r => r.GuideId).NotEqual(Guid.Empty).When(r => r.GuideId is not null);
        RuleFor(r => r.VehicleId).NotEqual(Guid.Empty).When(r => r.VehicleId is not null);
        RuleForEach(r => r.Rooms).ChildRules(room =>
        {
            room.RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            room.RuleFor(x => x.RoomTypeId).NotEmpty();
        });
    }
}
