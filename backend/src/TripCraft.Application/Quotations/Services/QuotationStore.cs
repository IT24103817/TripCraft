using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations.Services;

/// <summary>
/// Component C's persistence for the workflow (IQuotationStore): a new version per proposal, its status and the
/// manager's decision. Every method only stages changes; the proposal or approval service commits them.
/// </summary>
public class QuotationStore(IQuotationRepository quotations, TripSettings settings) : IQuotationStore
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
            ProposalSnapshot = draft.ProposalSnapshot,
            DepositPct = settings.DepositPct,
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
        await quotations.FindAsync(quotationId, ct) is { } q ? Summary(q) : null;

    public async Task<QuotationSummary?> GetLatestForTripAsync(Guid tripRequestId, CancellationToken ct)
    {
        var latest = await quotations.Query().Where(q => q.TripRequestId == tripRequestId)
            .OrderByDescending(q => q.Version).FirstOrDefaultAsync(ct);
        return latest is null ? null : Summary(latest);
    }

    public async Task SetStatusAsync(Guid quotationId, QuotationDecision status, CancellationToken ct)
    {
        var quotation = await quotations.FindAsync(quotationId, ct)
                        ?? throw new InvalidOperationException($"Quotation {quotationId} not found.");
        switch (status)
        {
            case QuotationDecision.Approved: quotation.Status = QuotationStatus.Approved; break;
            case QuotationDecision.Rejected: quotation.Status = QuotationStatus.Rejected; break;
            case QuotationDecision.RevisionRequested: quotation.Status = QuotationStatus.RevisionRequested; break;
            case QuotationDecision.Declined: quotation.Status = QuotationStatus.Declined; break;
            case QuotationDecision.Superseded: quotation.Status = QuotationStatus.Superseded; break;
            case QuotationDecision.Accepted: quotation.AcceptedAt = DateTime.UtcNow; break; // stays Approved
            case QuotationDecision.Confirmed: break; // the approval_decisions row is the record
        }
    }

    private static QuotationSummary Summary(Quotation q) =>
        new(q.Id, q.TripRequestId, q.Version, q.Status.ToString(), q.TotalLkr, q.TotalUsd, q.AcceptedAt);

    public void RecordDecision(Guid quotationId, Guid decidedBy, QuotationDecision decision, string? comment) =>
        quotations.Add(new ApprovalDecision
        {
            QuotationId = quotationId, DecidedBy = decidedBy, Decision = decision, Comment = comment?.Trim(),
            DecidedAt = DateTime.UtcNow
        });
}
