using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Quotations.Dashboard;
using TripCraft.Application.Trips;

namespace TripCraft.Api.Controllers.Dashboard;

/// <summary>The manager dashboard (v1.1): action counts first, then today's and tomorrow's trips.</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = Roles.OperationsManager)]
public class DashboardController(IDashboardService dashboard) : ControllerBase
{
    /// <summary>Accepted (to confirm), declined (needs a decision), needs operator, guide change requests, cancellations.</summary>
    [HttpGet("actions")]
    public async Task<ActionResult<DashboardActionsDto>> Actions(CancellationToken ct) => Ok(await dashboard.GetActionsAsync(ct));

    /// <summary>Trips waiting for the operator, with the decline reason or error summary. status = one of the three.</summary>
    [HttpGet("attention")]
    public async Task<ActionResult<IReadOnlyList<AttentionItemDto>>> Attention([FromQuery] TripRequestStatus? status,
        CancellationToken ct) =>
        Ok(await dashboard.GetAttentionAsync(status, ct));

    /// <summary>Confirmed or in-progress trips running today or tomorrow, with guide and vehicle.</summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyList<UpcomingTripDto>>> Upcoming(CancellationToken ct) =>
        Ok(await dashboard.GetUpcomingAsync(ct));
}
