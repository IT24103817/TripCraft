using TripCraft.Application.Common.Paging;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Dtos;

/// <summary>
/// GET /api/availability?type=guide|vehicle|room&amp;from=&amp;to=&amp;language=&amp;pax=&amp;seats=&amp;city=&amp;rooms=
/// Guides need language and pax; vehicles need seats; rooms need city and rooms (checked for every night from..to).
/// </summary>
public class AvailabilityQuery
{
    public ResourceType Type { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public string? Language { get; set; }
    public int? Pax { get; set; }
    public int? Seats { get; set; }
    public string? City { get; set; }
    public int? Rooms { get; set; }
}

/// <summary>One free resource. FreeRooms is the smallest free count over the nights asked (rooms only).</summary>
public record AvailableResourceDto(ResourceType Type, Guid Id, string Name, string Detail, decimal RateLkr, int? FreeRooms);

public record HoldDto(Guid Id, ResourceType ResourceType, Guid ResourceId, string ResourceName, Guid? TripRequestId,
    DateOnly FromDate, DateOnly ToDate, int Quantity, string Status, string? Note);

/// <summary>POST /api/resource-holds: a manual block (e.g. vehicle maintenance), not linked to a trip.</summary>
public record CreateHoldRequest(ResourceType ResourceType, Guid ResourceId, DateOnly FromDate, DateOnly ToDate,
    int Quantity, string? Note);

/// <summary>GET /api/resource-holds?from=&amp;to=&amp;type=&amp;status=&amp;page=&amp;pageSize= (the availability calendar).</summary>
public class HoldListQuery : PagedQuery
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public ResourceType? Type { get; set; }
    public HoldStatus? Status { get; set; }
}
