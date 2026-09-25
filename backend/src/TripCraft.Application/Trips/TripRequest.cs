using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Trips;

public class TripRequest : BaseEntity
{
    public Guid TouristId { get; set; }
    public Tourist? Tourist { get; set; }

    public string Objective { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Pax { get; set; }
    public decimal BudgetUsd { get; set; }

    /// <summary>Free-form JSON, e.g. {"language":"en","transport":"train"}. Stored as jsonb.</summary>
    public string Preferences { get; set; } = "{}";

    public TripRequestStatus Status { get; set; } = TripRequestStatus.Submitted;
}
