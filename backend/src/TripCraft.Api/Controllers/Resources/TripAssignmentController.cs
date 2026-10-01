using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Api.Controllers.Resources;

/// <summary>The guide and vehicle booked for a trip, and the tourist's rating of the guide (v1.1).</summary>
[ApiController]
[Route("api/trip-requests/{id:guid}")]
[Authorize(Roles = Roles.TouristOrOperationsManager)]
public class TripAssignmentController(ITripAssignmentService assignments) : ControllerBase
{
    /// <summary>Guide name/phone and vehicle; nulls before the trip is confirmed. Owner tourist or a manager.</summary>
    [HttpGet("assignment")]
    public async Task<ActionResult<TripAssignmentDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await assignments.GetAsync(User.GetCurrentUser(), id, ct));

    /// <summary>404 until the tourist has rated the guide.</summary>
    [HttpGet("guide-rating")]
    public async Task<ActionResult<GuideRatingDto>> GetRating(Guid id, CancellationToken ct) =>
        Ok(await assignments.GetRatingAsync(User.GetCurrentUser(), id, ct));

    /// <summary>1–5 stars after a Completed trip, once. 409 before completion or when already rated.</summary>
    [HttpPost("guide-rating")]
    [Authorize(Roles = Roles.Tourist)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuideRatingDto>> Rate(Guid id, RateGuideRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await assignments.RateGuideAsync(User.GetCurrentUser(), id, request, ct));
}
