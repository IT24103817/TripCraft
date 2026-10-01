using FluentValidation;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Dtos;

/// <summary>POST /api/trip-requests/{id}/guide-change-requests (the trip's guide).</summary>
public record CreateGuideChangeRequest(string Reason);

/// <summary>POST /api/guide-change-requests/{id}/resolve (manager): one of the request's candidates.</summary>
public record ResolveGuideChangeRequest(Guid ReplacementGuideId);

/// <summary>
/// One request for the manager's dashboard. Candidates are the guides who are free for the whole trip, speak the
/// trip's language and can take the party (empty once resolved).
/// </summary>
public record GuideChangeRequestDto(Guid Id, Guid TripRequestId, string TripObjective, DateOnly StartDate,
    DateOnly EndDate, int Pax, string Language, Guid GuideId, string GuideName, string Reason, string Status,
    Guid? ReplacementGuideId, string? ReplacementGuideName, DateTime CreatedAt, DateTime? ResolvedAt,
    IReadOnlyList<GuideOption> Candidates);

public class CreateGuideChangeRequestValidator : AbstractValidator<CreateGuideChangeRequest>
{
    public CreateGuideChangeRequestValidator() => RuleFor(r => r.Reason).NotEmpty().MaximumLength(500);
}

public class ResolveGuideChangeRequestValidator : AbstractValidator<ResolveGuideChangeRequest>
{
    public ResolveGuideChangeRequestValidator() => RuleFor(r => r.ReplacementGuideId).NotEmpty();
}
