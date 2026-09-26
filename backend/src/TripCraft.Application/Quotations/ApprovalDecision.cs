using TripCraft.Application.Common.Entities;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations;

/// <summary>The human approval gate's record: who decided what on which quotation version, and why.</summary>
public class ApprovalDecision : BaseEntity
{
    public Guid QuotationId { get; set; }
    public Guid DecidedBy { get; set; }
    public QuotationDecision Decision { get; set; }
    public string? Comment { get; set; }
    public DateTime DecidedAt { get; set; }
}
