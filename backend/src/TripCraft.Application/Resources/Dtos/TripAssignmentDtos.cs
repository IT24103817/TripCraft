using FluentValidation;

namespace TripCraft.Application.Resources.Dtos;

/// <summary>
/// Who and what is booked for a trip (GET /api/trip-requests/{id}/assignment), for the tourist's "what's next" line,
/// e.g. "Guide: Nimal, starts 10 Oct". All fields are null before Confirm.
/// </summary>
public record TripAssignmentDto(Guid TripRequestId, Guid? GuideId, string? GuideName, string? GuidePhone,
    string? VehicleRegistrationNo, string? VehicleType, int? VehicleSeats);

/// <summary>POST /api/trip-requests/{id}/guide-rating.</summary>
public record RateGuideRequest(int Stars, string? Comment);

public record GuideRatingDto(Guid TripRequestId, Guid GuideId, string GuideName, int Stars, string? Comment, DateTime RatedAt);

public class RateGuideRequestValidator : AbstractValidator<RateGuideRequest>
{
    public RateGuideRequestValidator()
    {
        RuleFor(r => r.Stars).InclusiveBetween(1, 5);
        RuleFor(r => r.Comment).MaximumLength(500);
    }
}
