using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Trips.Dtos;

/// <summary>GET /api/trip-requests?status=&amp;from=&amp;to=&amp;search=&amp;sort=&amp;page=&amp;pageSize=</summary>
public class TripRequestListQuery : PagedQuery
{
    public TripRequestStatus? Status { get; set; }

    /// <summary>Only trips starting on or after this date.</summary>
    public DateOnly? From { get; set; }

    /// <summary>Only trips starting on or before this date.</summary>
    public DateOnly? To { get; set; }
}
