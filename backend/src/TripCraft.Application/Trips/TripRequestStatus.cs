namespace TripCraft.Application.Trips;

/// <summary>Status workflow from PLAN.md section 3 (Component A). Stored as text.</summary>
public enum TripRequestStatus
{
    Submitted,
    Planning,
    PendingApproval,
    Approved,
    Rejected,
    RevisionRequested,
    Confirmed,
    InProgress,
    Completed,
    Cancelled
}
