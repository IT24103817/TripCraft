using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Application.Identity.Services;

namespace TripCraft.Api.Controllers.Admin;

/// <summary>Admin only: the audit trail of every business change (who, what, before/after, when).</summary>
[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Policy = Policies.AdminOnly)]
public class AdminAuditLogsController(IAuditLogQueryService auditLogs) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> List([FromQuery] AuditLogListQuery query, CancellationToken ct)
    {
        return Ok(await auditLogs.ListAsync(query, ct));
    }
}
