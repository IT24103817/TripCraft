using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;

namespace TripCraft.Application.Trips;

/// <summary>
/// The only place that knows which trip status may follow which (v1.1 lifecycle). Every service changes a trip's
/// status through <see cref="Move"/>: an illegal move is a 409, a legal one is written to the trip history
/// (audit_logs, action TripRequestStatusChanged) with the actor and the reason.
/// </summary>
public static class TripStatusMachine
{
    private static readonly Dictionary<TripRequestStatus, TripRequestStatus[]> Allowed = new()
    {
        [TripRequestStatus.Submitted] = [TripRequestStatus.Planning, TripRequestStatus.Cancelled],
        // Planning ends with a proposal for review, or a safe failure. The tourist cannot cancel while agents run.
        [TripRequestStatus.Planning] = [TripRequestStatus.PendingReview, TripRequestStatus.FailedSafely],
        [TripRequestStatus.FailedSafely] = [TripRequestStatus.Planning, TripRequestStatus.Cancelled],
        // The manager sends the quotation, asks the agents to re-plan, or the trip is rejected/cancelled.
        [TripRequestStatus.PendingReview] =
            [TripRequestStatus.QuotationSent, TripRequestStatus.RevisionRequested, TripRequestStatus.Cancelled],
        [TripRequestStatus.RevisionRequested] =
            [TripRequestStatus.PendingReview, TripRequestStatus.FailedSafely, TripRequestStatus.Cancelled],
        // The tourist accepts or declines (declined goes back to the manager's review).
        [TripRequestStatus.QuotationSent] =
            [TripRequestStatus.ClientAccepted, TripRequestStatus.PendingReview, TripRequestStatus.Cancelled],
        // The manager confirms (holds, vouchers), or reopens the review when a resource is no longer free.
        [TripRequestStatus.ClientAccepted] =
            [TripRequestStatus.Confirmed, TripRequestStatus.PendingReview, TripRequestStatus.Cancelled],
        [TripRequestStatus.Confirmed] = [TripRequestStatus.InProgress, TripRequestStatus.Cancelled],
        [TripRequestStatus.InProgress] = [TripRequestStatus.Completed],
        [TripRequestStatus.Completed] = [],
        [TripRequestStatus.Cancelled] = []
    };

    /// <summary>The statuses a trip may move to next.</summary>
    public static IReadOnlyList<TripRequestStatus> NextOf(TripRequestStatus from) => Allowed[from];

    public static bool CanMove(TripRequestStatus from, TripRequestStatus to) => Allowed[from].Contains(to);

    /// <summary>409 Conflict with a readable message when the move is not allowed.</summary>
    public static void EnsureCanMove(TripRequestStatus from, TripRequestStatus to)
    {
        if (!CanMove(from, to))
            throw new ConflictException($"A trip that is {Describe(from)} cannot become {Describe(to)}.");
    }

    /// <summary>Checks the move, changes the status and records it in the trip history. The caller commits.</summary>
    public static void Move(TripRequest trip, TripRequestStatus to, Guid? actorId, string reason, IAuditLogger audit)
    {
        EnsureCanMove(trip.Status, to);
        var from = trip.Status;
        trip.Status = to;
        audit.Record(actorId, "TripRequestStatusChanged", nameof(TripRequest), trip.Id,
            new { Status = from.ToString() }, new { Status = to.ToString(), Reason = reason });
    }

    /// <summary>"PendingReview" → "pending review", for messages.</summary>
    public static string Describe(TripRequestStatus status) =>
        string.Concat(status.ToString().Select((c, i) => i > 0 && char.IsUpper(c) ? " " + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
}
