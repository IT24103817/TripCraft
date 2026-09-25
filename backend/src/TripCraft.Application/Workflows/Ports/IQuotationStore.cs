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

    Task SetStatusAsync(Guid quotationId, QuotationDecision status, CancellationToken ct);

    void RecordDecision(Guid quotationId, Guid decidedBy, QuotationDecision decision, string? comment);
}

public enum QuotationDecision
{
    Approved,
    Rejected,
    RevisionRequested
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
    IReadOnlyList<QuotationDraftLine> Lines);

/// <summary>LineType is guide, vehicle, room or entry (quotation_lines.line_type).</summary>
public record QuotationDraftLine(string LineType, string Description, decimal Qty, decimal UnitLkr, decimal AmountLkr);

/// <summary>AwaitingDecision is true while the quotation is Pending (not yet approved, rejected or revised).</summary>
public record QuotationSummary(Guid Id, Guid TripRequestId, int Version, bool AwaitingDecision, decimal TotalLkr,
    decimal TotalUsd);
