using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;

namespace TripCraft.Application.Trips;

/// <summary>
/// The only place that knows which trip status may follow which (v1.1 lifecycle: quotations go straight to the
/// client; the human approval gate is Confirm). Every service changes a trip's
/// status through <see cref="Move"/>: an illegal move is a 409, a legal one is written to the trip history
/// (audit_logs, action TripRequestStatusChanged) with the actor and the reason.
/// </summary>
public static class TripStatusMachine
{
    private static readonly Dictionary<TripRequestStatus, TripRequestStatus[]> Allowed = new()
    {
        [TripRequestStatus.Submitted] = [TripRequestStatus.Planning, TripRequestStatus.Cancelled],
        // Planning ends with an auto-sent quotation (validation passed, or only over budget after the re-plans),
        // or NeedsOperator (agents failed safely or a Hard rule failed). The tourist cannot cancel while agents run.
        [TripRequestStatus.Planning] = [TripRequestStatus.QuotationSent, TripRequestStatus.NeedsOperator],
        // The operator retries planning, edits and sends a quotation by hand, or cancels.
        [TripRequestStatus.NeedsOperator] =
            [TripRequestStatus.Planning, TripRequestStatus.QuotationSent, TripRequestStatus.Cancelled],
        // The tourist accepts or declines (with a reason).
        [TripRequestStatus.QuotationSent] =
            [TripRequestStatus.ClientAccepted, TripRequestStatus.ClientDeclined, TripRequestStatus.Cancelled],
        // The manager confirms (the approval gate: holds, vouchers), or edits and resends (the tourist accepts again).
        [TripRequestStatus.ClientAccepted] =
            [TripRequestStatus.Confirmed, TripRequestStatus.QuotationSent, TripRequestStatus.Cancelled],
        // The manager replans with a note (the agents get the note and the client's reason), or cancels.
        [TripRequestStatus.ClientDeclined] = [TripRequestStatus.Planning, TripRequestStatus.Cancelled],
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

    /// <summary>"ClientAccepted" → "client accepted", for messages.</summary>
    public static string Describe(TripRequestStatus status) =>
        string.Concat(status.ToString().Select((c, i) => i > 0 && char.IsUpper(c) ? " " + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
}
