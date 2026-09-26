using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations.Services;

/// <summary>
/// Component C's persistence for the workflow (IQuotationStore): a new version per proposal, its status and the
/// manager's decision. Every method only stages changes; the proposal or approval service commits them.
/// </summary>
public class QuotationStore(IQuotationRepository quotations) : IQuotationStore
{
    public async Task<Guid> AddVersionAsync(QuotationDraft draft, CancellationToken ct)
    {
        var quotation = new Quotation
        {
            TripRequestId = draft.TripRequestId,
            WorkflowId = draft.WorkflowId,
            Version = await quotations.LatestVersionAsync(draft.TripRequestId, ct) + 1,
            SubtotalLkr = draft.SubtotalLkr,
            MarginPct = draft.MarginPct,
            TotalLkr = draft.TotalLkr,
            TotalUsd = draft.TotalUsd,
            FxRate = draft.FxRate,
            FxAsOf = draft.FxAsOf,
            FxStale = draft.FxStale,
            Status = QuotationStatus.Pending
        };
        quotation.Lines = draft.Lines.Select(l => new QuotationLine
        {
            QuotationId = quotation.Id, LineType = l.LineType, Description = l.Description, Qty = l.Qty,
            UnitLkr = l.UnitLkr, AmountLkr = l.AmountLkr
        }).ToList();
        quotations.Add(quotation);
        return quotation.Id;
    }

    public async Task<QuotationSummary?> GetAsync(Guid quotationId, CancellationToken ct) =>
        await quotations.FindAsync(quotationId, ct) is { } q
            ? new QuotationSummary(q.Id, q.TripRequestId, q.Version, q.Status == QuotationStatus.Pending, q.TotalLkr, q.TotalUsd)
            : null;

    public async Task SetStatusAsync(Guid quotationId, QuotationDecision status, CancellationToken ct)
    {
        var quotation = await quotations.FindAsync(quotationId, ct)
                        ?? throw new InvalidOperationException($"Quotation {quotationId} not found.");
        quotation.Status = status switch
        {
            QuotationDecision.Approved => QuotationStatus.Approved,
            QuotationDecision.Rejected => QuotationStatus.Rejected,
            _ => QuotationStatus.RevisionRequested
        };
    }

    public void RecordDecision(Guid quotationId, Guid decidedBy, QuotationDecision decision, string? comment) =>
        quotations.Add(new ApprovalDecision
        {
            QuotationId = quotationId, DecidedBy = decidedBy, Decision = decision, Comment = comment?.Trim(),
            DecidedAt = DateTime.UtcNow
        });
}
