using TripCraft.Application.Trips.Planning;

namespace TripCraft.Application.Trips.Dtos;

/// <summary>
/// Returned with 202 Accepted. WorkflowStatus is "Planning" when the agent service accepted the job,
/// or "FailedSafely" (with ErrorSummary) when it could not be reached — the trip is then back to its
/// previous status so planning can be retried.
/// </summary>
public record StartPlanningResponse(
    Guid WorkflowId,
    Guid TripRequestId,
    string WorkflowStatus,
    string TripStatus,
    IReadOnlyList<SkeletonDay> Skeleton,
    string? ErrorSummary);
