using System.Linq.Expressions;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Identity.Dtos;

namespace TripCraft.Application.Identity.Services;

/// <summary>Admin audit screen: search, filter, sort and page audit_logs (read-only).</summary>
public class AuditLogQueryService(IAuditLogReader auditLogs) : IAuditLogQueryService
{
    public static readonly IReadOnlyDictionary<string, Expression<Func<AuditLogView, object>>> SortableFields =
        new Dictionary<string, Expression<Func<AuditLogView, object>>>
        {
            ["at"] = a => a.At,
            ["action"] = a => a.Action,
            ["entity"] = a => a.Entity
        };

    public async Task<PagedResult<AuditLogDto>> ListAsync(AuditLogListQuery query, CancellationToken ct)
    {
        var q = auditLogs.Query();

        if (!string.IsNullOrWhiteSpace(query.Entity))
            q = q.Where(a => a.Entity == query.Entity);
        if (!string.IsNullOrWhiteSpace(query.Action))
            q = q.Where(a => a.Action == query.Action);
        if (query.From.HasValue)
        {
            var from = query.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            q = q.Where(a => a.At >= from);
        }
        if (query.To.HasValue)
        {
            var toExclusive = query.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            q = q.Where(a => a.At < toExclusive);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(a => a.Action.ToLower().Contains(term)
                             || a.Entity.ToLower().Contains(term)
                             || (a.ActorEmail != null && a.ActorEmail.ToLower().Contains(term)));
        }

        return await q
            .ApplySort(query.Sort, SortableFields, "-at")
            .ToPagedResultAsync(query.Page, query.PageSize, AuditLogDto.FromView, ct);
    }
}
