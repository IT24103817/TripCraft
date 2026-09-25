using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TripCraft.Application.Common.Paging;

public static class QueryableExtensions
{
    /// <summary>
    /// Sorts by a whitelisted field. The validator has already rejected unknown fields,
    /// so an unknown key here falls back to the default instead of throwing.
    /// </summary>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sort,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortableFields,
        string defaultSort)
    {
        var requested = string.IsNullOrWhiteSpace(sort) ? defaultSort : sort;
        var descending = requested.StartsWith('-');
        var key = requested.TrimStart('-');

        if (!sortableFields.TryGetValue(key, out var keySelector))
        {
            descending = defaultSort.StartsWith('-');
            keySelector = sortableFields[defaultSort.TrimStart('-')];
        }

        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }

    public static async Task<PagedResult<TResult>> ToPagedResultAsync<T, TResult>(
        this IQueryable<T> query, int page, int pageSize, Func<T, TResult> map, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<TResult>(items.Select(map).ToList(), page, pageSize, total);
    }

    /// <summary>True when the sort string names a whitelisted field (with or without "-").</summary>
    public static bool IsValidSort(string? sort, IEnumerable<string> allowedFields) =>
        string.IsNullOrWhiteSpace(sort) || allowedFields.Contains(sort.TrimStart('-'));
}
