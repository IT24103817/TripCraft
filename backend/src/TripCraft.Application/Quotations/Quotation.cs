using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Quotations;

/// <summary>
/// One priced version of a trip (PLAN.md section 4). A revision adds version n+1; the manager decides on one
/// version. Amounts are LKR, converted to USD at FxRate (LKR per USD) as of FxAsOf; FxStale when the provider failed.
/// </summary>
public class Quotation : BaseEntity
{
    public Guid TripRequestId { get; set; }

    /// <summary>The agent workflow that proposed it; null for a quotation entered by hand (e.g. seed data).</summary>
    public Guid? WorkflowId { get; set; }

    public int Version { get; set; }
    public decimal SubtotalLkr { get; set; }
    public decimal MarginPct { get; set; }
    public decimal TotalLkr { get; set; }
    public decimal TotalUsd { get; set; }
    public decimal FxRate { get; set; }
    public DateTime FxAsOf { get; set; }
    public bool FxStale { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Pending;

    /// <summary>When the tourist accepted the approved price in the app.</summary>
    public DateTime? AcceptedAt { get; set; }

    public List<QuotationLine> Lines { get; set; } = [];
}

public enum QuotationStatus
{
    Pending,
    Approved,
    Rejected,
    RevisionRequested
}
