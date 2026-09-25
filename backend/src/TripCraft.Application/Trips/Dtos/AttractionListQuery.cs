using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Trips.Dtos;

/// <summary>GET /api/attractions?city=&amp;category=&amp;search=&amp;sort=&amp;page=&amp;pageSize=</summary>
public class AttractionListQuery : PagedQuery
{
    public string? City { get; set; }
    public string? Category { get; set; }
}
