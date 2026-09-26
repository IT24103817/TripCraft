using TripCraft.Application.Common.Paging;
using TripCraft.Application.Identity.Dtos;

namespace TripCraft.Application.Identity.Services;

public interface IAuditLogQueryService
{
    Task<PagedResult<AuditLogDto>> ListAsync(AuditLogListQuery query, CancellationToken ct);
}
