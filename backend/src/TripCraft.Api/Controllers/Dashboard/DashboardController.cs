using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Quotations.Dashboard;

namespace TripCraft.Api.Controllers.Dashboard;

/// <summary>The manager dashboard (v1.1): action counts first, then today's and tomorrow's trips.</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = Roles.OperationsManager)]
public class DashboardController(IDashboardService dashboard) : ControllerBase
{
    /// <summary>Proposals to review, client-accepted trips to confirm, guide change requests, cancellations, declines.</summary>
    [HttpGet("actions")]
    public async Task<ActionResult<DashboardActionsDto>> Actions(CancellationToken ct) => Ok(await dashboard.GetActionsAsync(ct));

    /// <summary>Confirmed or in-progress trips running today or tomorrow, with guide and vehicle.</summary>
    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyList<UpcomingTripDto>>> Upcoming(CancellationToken ct) =>
        Ok(await dashboard.GetUpcomingAsync(ct));
}
