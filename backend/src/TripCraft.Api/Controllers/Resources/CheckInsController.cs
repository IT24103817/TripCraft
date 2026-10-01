using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Api.Controllers.Resources;

/// <summary>
/// Component B business operation for guides: check-in by scanning the tourist's signed trip voucher (v1.1), or
/// GPS check-in at a stop within 500 m.
/// </summary>
[ApiController]
[Route("api/check-ins")]
[Authorize(Roles = Roles.Guide)]
public class CheckInsController(IGuideScheduleService schedules) : ControllerBase
{
    /// <summary>
    /// Body {voucherCode} or {itineraryStopId, latitude, longitude}. 400 for a forged/hotel voucher or when too far
    /// away, 403 for another guide's trip, 409 when not a day of the trip, already checked in or the trip is not running.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CheckInResultDto>> CheckIn(CheckInRequest request, CancellationToken ct) =>
        Ok(await schedules.CheckInAsync(User.GetCurrentUser(), request, ct));
}
