namespace TripCraft.Application.Common.Paging;

/// <summary>Shape of every list response: {items, page, pageSize, total}.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);
