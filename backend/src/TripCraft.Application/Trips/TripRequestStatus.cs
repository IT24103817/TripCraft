namespace TripCraft.Application.Trips;

/// <summary>
/// Trip lifecycle v1.1 (stored as text). Main path:
/// Submitted → Planning → PendingReview → QuotationSent → ClientAccepted → Confirmed → InProgress → Completed.
/// Side states: RevisionRequested (manager asked the agents to re-plan), FailedSafely (planning could not finish),
/// Cancelled (by the tourist before the cut-off, or by the operator). Allowed moves: <see cref="TripStatusMachine"/>.
/// </summary>
public enum TripRequestStatus
{
    Submitted,
    Planning,
    PendingReview,
    QuotationSent,
    ClientAccepted,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    RevisionRequested,
    FailedSafely
}
