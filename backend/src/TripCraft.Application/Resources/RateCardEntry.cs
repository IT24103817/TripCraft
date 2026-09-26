using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

/// <summary>Operator margin in percent, effective from a date. The newest card that has started applies.</summary>
public class RateCardEntry : BaseEntity
{
    public decimal MarginPct { get; set; }
    public DateOnly EffectiveFrom { get; set; }
}
