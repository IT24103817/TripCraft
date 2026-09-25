using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Services;

namespace TripCraft.Api.Controllers.Workflows;

/// <summary>
/// Agent workflow monitor. Tourists may read workflows of their own trips (checked in the service);
/// Operations Managers and Admins read all of them. Guides get 403.
/// </summary>
[ApiController]
[Route("api/workflows")]
[Authorize(Roles = Roles.TouristOrStaff)]
public class WorkflowsController(IWorkflowQueryService workflows) : ControllerBase
{
    /// <summary>Status, plan, validation result, current step, final outcome and timings.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkflowDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await workflows.GetAsync(User.GetCurrentUser(), id, ct));

    /// <summary>Agent steps in order (step_no).</summary>
    [HttpGet("{id:guid}/steps")]
    public async Task<ActionResult<IReadOnlyList<AgentStepDto>>> Steps(Guid id, CancellationToken ct) =>
        Ok(await workflows.ListStepsAsync(User.GetCurrentUser(), id, ct));

    [HttpGet]
    [Authorize(Roles = Roles.Staff)]
    public async Task<ActionResult<PagedResult<WorkflowSummaryDto>>> List([FromQuery] WorkflowListQuery query, CancellationToken ct) =>
        Ok(await workflows.ListAsync(query, ct));
}
