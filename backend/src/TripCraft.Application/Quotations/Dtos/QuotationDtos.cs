using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Quotations.Dtos;

public record QuotationLineDto(string LineType, string Description, decimal Qty, decimal UnitLkr, decimal AmountLkr);

public record ApprovalDecisionDto(string Decision, string? Comment, DateTime DecidedAt);

public record QuotationDto(Guid Id, Guid TripRequestId, Guid? WorkflowId, int Version, string Status,
    decimal SubtotalLkr, decimal MarginPct, decimal MarginLkr, decimal TotalLkr, decimal TotalUsd, decimal FxRate,
    DateTime FxAsOf, bool FxStale, DateTime? AcceptedAt, IReadOnlyList<QuotationLineDto> Lines,
    IReadOnlyList<ApprovalDecisionDto> Decisions, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static QuotationDto FromEntity(Quotation q, IEnumerable<ApprovalDecision>? decisions = null) => new(
        q.Id, q.TripRequestId, q.WorkflowId, q.Version, q.Status.ToString(), q.SubtotalLkr, q.MarginPct,
        q.TotalLkr - q.SubtotalLkr, q.TotalLkr, q.TotalUsd, q.FxRate, q.FxAsOf, q.FxStale, q.AcceptedAt,
        q.Lines.Select(l => new QuotationLineDto(l.LineType, l.Description, l.Qty, l.UnitLkr, l.AmountLkr)).ToList(),
        (decisions ?? []).OrderBy(d => d.DecidedAt)
            .Select(d => new ApprovalDecisionDto(d.Decision.ToString(), d.Comment, d.DecidedAt)).ToList(),
        q.CreatedAt, q.UpdatedAt);
}

/// <summary>GET /api/quotations?status=&amp;tripRequestId=&amp;from=&amp;to=&amp;minTotalUsd=&amp;sort=&amp;page=&amp;pageSize=</summary>
public class QuotationListQuery : PagedQuery
{
    public QuotationStatus? Status { get; set; }
    public Guid? TripRequestId { get; set; }

    /// <summary>Created on or after (UTC date).</summary>
    public DateOnly? From { get; set; }

    /// <summary>Created on or before (UTC date).</summary>
    public DateOnly? To { get; set; }

    public decimal? MinTotalUsd { get; set; }
}

/// <summary>Result of POST /api/quotations/{id}/calculate: the re-priced quotation and what changed.</summary>
public record RecalculationDto(QuotationDto Quotation, decimal PreviousTotalLkr, decimal PreviousTotalUsd, bool Changed);
