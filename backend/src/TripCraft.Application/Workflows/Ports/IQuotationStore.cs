namespace TripCraft.Application.Workflows.Ports;

/// <summary>
/// Persistence for quotations, quotation_lines and approval_decisions (Student C, PLAN.md section 4).
/// Methods only stage changes on the current unit of work; the caller commits.
/// </summary>
public interface IQuotationStore
{
    /// <summary>Stages a new quotation version for the trip (version = previous + 1) with its lines.</summary>
    Task<Guid> AddVersionAsync(QuotationDraft draft, CancellationToken ct);

    Task<QuotationSummary?> GetAsync(Guid quotationId, CancellationToken ct);

    /// <summary>The highest version of the trip's quotations, or null.</summary>
    Task<QuotationSummary?> GetLatestForTripAsync(Guid tripRequestId, CancellationToken ct);

    /// <summary>
    /// Approved / Rejected / RevisionRequested / Declined / Superseded set that status; Accepted sets AcceptedAt
    /// (the quotation stays Approved); Confirmed changes nothing (the decision row is the record).
    /// </summary>
    Task SetStatusAsync(Guid quotationId, QuotationDecision status, CancellationToken ct);

    void RecordDecision(Guid quotationId, Guid decidedBy, QuotationDecision decision, string? comment);
}

/// <summary>Decisions recorded in approval_decisions: the manager's and, since v1.1, the tourist's.</summary>
public enum QuotationDecision
{
    Approved,
    Rejected,
    RevisionRequested,
    Declined,
    Accepted,
    Confirmed,

    /// <summary>Status only: a newer version replaced this one (re-price after an edit). Never recorded as a decision.</summary>
    Superseded
}

public record QuotationDraft(
    Guid TripRequestId,
    Guid WorkflowId,
    decimal SubtotalLkr,
    decimal MarginPct,
    decimal TotalLkr,
    decimal TotalUsd,
    decimal FxRate,
    DateTime FxAsOf,
    bool FxStale,
    IReadOnlyList<QuotationDraftLine> Lines,
    string? ProposalSnapshot = null);

/// <summary>LineType is guide, vehicle, room or entry (quotation_lines.line_type).</summary>
public record QuotationDraftLine(string LineType, string Description, decimal Qty, decimal UnitLkr, decimal AmountLkr);

/// <summary>Status is the QuotationStatus name; AwaitingDecision is true while it is Pending (waiting for the manager).</summary>
public record QuotationSummary(Guid Id, Guid TripRequestId, int Version, string Status, decimal TotalLkr,
    decimal TotalUsd, DateTime? AcceptedAt)
{
    public bool AwaitingDecision => Status == "Pending";
}
