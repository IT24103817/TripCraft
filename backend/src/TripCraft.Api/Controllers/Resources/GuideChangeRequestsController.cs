using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Api.Controllers.Resources;

/// <summary>A guide asks to be replaced on a confirmed trip; a manager picks the replacement (v1.1).</summary>
[ApiController]
public class GuideChangeRequestsController(IGuideChangeService changes) : ControllerBase
{
    /// <summary>The trip's guide only (403 otherwise); 409 unless the trip is Confirmed or a request is already open.</summary>
    [HttpPost("api/trip-requests/{id:guid}/guide-change-requests")]
    [Authorize(Roles = Roles.Guide)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuideChangeRequestDto>> Create(Guid id, CreateGuideChangeRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await changes.RequestAsync(User.GetCurrentUser(), id, request.Reason, ct));

    /// <summary>Open requests with the replacement candidates (same language, free for the whole trip).</summary>
    [HttpGet("api/guide-change-requests")]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<IReadOnlyList<GuideChangeRequestDto>>> ListOpen(CancellationToken ct) =>
        Ok(await changes.ListOpenAsync(ct));

    /// <summary>Swaps the guide holds in one transaction and notifies both guides and the tourist.</summary>
    [HttpPost("api/guide-change-requests/{id:guid}/resolve")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuideChangeRequestDto>> Resolve(Guid id, ResolveGuideChangeRequest request, CancellationToken ct) =>
        Ok(await changes.ResolveAsync(User.GetCurrentUser(), id, request.ReplacementGuideId, ct));
}
