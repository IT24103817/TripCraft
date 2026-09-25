using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Workflows.External;

/// <summary>
/// city_distances: the static fallback table for IDistanceService (PLAN.md section 9).
/// One row per city pair; the lookup works in both directions.
/// </summary>
public class CityDistance : BaseEntity
{
    public string FromCity { get; set; } = string.Empty;
    public string ToCity { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
}
