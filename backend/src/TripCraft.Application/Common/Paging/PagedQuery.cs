namespace TripCraft.Application.Common.Paging;

/// <summary>Query-string fields shared by every list endpoint.</summary>
public abstract class PagedQuery
{
    public const int MaxPageSize = 100;

    public string? Search { get; set; }

    /// <summary>A sortable field name; prefix with "-" for descending, e.g. "-startDate".</summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
