using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Api.Controllers.Resources;

/// <summary>Component B — guides (Operations Manager CRUD, per action) and the signed-in guide's own schedule.</summary>
[ApiController]
[Route("api/guides")]
[Authorize(Roles = Roles.GuideOrOperationsManager)]
public class GuidesController(IGuideService guides, IGuideScheduleService schedules) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<PagedResult<GuideDto>>> List([FromQuery] GuideListQuery query, CancellationToken ct) =>
        Ok(await guides.ListAsync(query, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<GuideDto>> GetById(Guid id, CancellationToken ct) => Ok(await guides.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuideAccountDto>> Create(CreateGuideRequest request, CancellationToken ct)
    {
        // The guide and their login are created together; the temporary password is in this response only.
        var created = await guides.CreateAsync(User.GetCurrentUser(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Guide.Id }, created);
    }

    /// <summary>New one-time temporary password; the guide must change it at the next login.</summary>
    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuideAccountDto>> ResetPassword(Guid id, CancellationToken ct) =>
        Ok(await guides.ResetPasswordAsync(User.GetCurrentUser(), id, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GuideDto>> Update(Guid id, SaveGuideRequest request, CancellationToken ct) =>
        Ok(await guides.UpdateAsync(User.GetCurrentUser(), id, request, ct));

    /// <summary>Soft delete. 409 while the guide is held for an upcoming trip.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await guides.DeleteAsync(User.GetCurrentUser(), id, ct);
        return NoContent();
    }

    /// <summary>A guide's held trips with days, stops and check-ins (manager view).</summary>
    [HttpGet("{id:guid}/schedule")]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<GuideScheduleDto>> Schedule(Guid id, CancellationToken ct) =>
        Ok(await schedules.GetScheduleAsync(id, ct));

    /// <summary>The signed-in guide's own schedule (Flutter). 403 if no guide profile is linked to the login.</summary>
    [HttpGet("me/schedule")]
    [Authorize(Roles = Roles.Guide)]
    public async Task<ActionResult<GuideScheduleDto>> MySchedule(CancellationToken ct) =>
        Ok(await schedules.GetMyScheduleAsync(User.GetCurrentUser(), ct));
}
