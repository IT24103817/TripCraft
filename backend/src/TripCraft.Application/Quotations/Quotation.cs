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

    /// <summary>When the tourist accepted the price in the app.</summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// The itinerary and resources this version priced (JSON of the stored proposal), so the review page can show
    /// version 1 and version 2 side by side after a revision or a direct edit.
    /// </summary>
    public string? ProposalSnapshot { get; set; }

    public List<QuotationLine> Lines { get; set; } = [];
}

/// <summary>
/// Pending = waiting for the manager; Approved = sent to the tourist; Declined = the tourist said no;
/// RevisionRequested / Superseded = replaced by a newer version; Rejected = the operator turned the trip down.
/// </summary>
public enum QuotationStatus
{
    Pending,
    Approved,
    Rejected,
    RevisionRequested,
    Declined,
    Superseded
}
