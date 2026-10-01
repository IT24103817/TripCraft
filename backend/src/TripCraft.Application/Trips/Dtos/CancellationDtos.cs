using FluentValidation;

namespace TripCraft.Application.Trips.Dtos;

/// <summary>POST /api/trip-requests/{id}/cancel. The reason is required and goes into the trip history.</summary>
public record CancelTripRequest(string Reason);

/// <summary>
/// GET /api/trip-requests/{id}/cancellation: whether the caller may cancel now. When not, ClosedReason explains why
/// and the app offers OperatorContact instead.
/// </summary>
public record CancellationInfoDto(bool CanCancel, DateOnly CancelUntil, int CutoffDays, string? ClosedReason,
    string OperatorContact);

public class CancelTripRequestValidator : AbstractValidator<CancelTripRequest>
{
    public CancelTripRequestValidator() => RuleFor(r => r.Reason).NotEmpty().MaximumLength(500);
}
