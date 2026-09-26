using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Quotations;

/// <summary>LineType is guide, vehicle, room or entry. AmountLkr = Qty × UnitLkr, rounded to 2 decimals.</summary>
public class QuotationLine : BaseEntity
{
    public Guid QuotationId { get; set; }
    public string LineType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal UnitLkr { get; set; }
    public decimal AmountLkr { get; set; }
}
