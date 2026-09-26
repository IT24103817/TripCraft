using TripCraft.Application.Common.Entities;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources;

/// <summary>
/// A resource reserved for a trip over [FromDate, ToDate] (inclusive). Guides and vehicles: quantity 1 and no
/// overlapping Held hold (also enforced by a PostgreSQL exclusion constraint). Rooms: one row per room type and
/// night, Quantity rooms, never more than the room type's TotalRooms per night.
/// </summary>
public class ResourceHold : BaseEntity
{
    public ResourceType ResourceType { get; set; }
    public Guid ResourceId { get; set; }

    /// <summary>The trip it is for; null for a manual block (e.g. vehicle maintenance).</summary>
    public Guid? TripRequestId { get; set; }

    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public int Quantity { get; set; } = 1;
    public HoldStatus Status { get; set; } = HoldStatus.Held;
    public string? Note { get; set; }
}

public enum HoldStatus
{
    Held,
    Released
}
