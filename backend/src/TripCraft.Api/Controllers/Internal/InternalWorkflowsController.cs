using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Services;

namespace TripCraft.Api.Controllers.Internal;

/// <summary>Callbacks from the agent service: one step report per agent, then one final proposal.</summary>
[ApiController]
[Route("api/internal/workflows")]
[AllowAnonymous]
[TypeFilter(typeof(InternalKeyAuthFilter))]
public class InternalWorkflowsController(IWorkflowStepService steps, IWorkflowProposalService proposals) : ControllerBase
{
    [HttpPost("{id:guid}/steps")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<AgentStepCreatedResponse>> AddStep(Guid id, AgentStepReportRequest report, CancellationToken ct)
    {
        var created = await steps.RecordAsync(id, report, ct);
        return Created($"/api/workflows/{id}/steps", created);
    }

    /// <summary>Runs the deterministic ProposalValidator and moves the workflow to its next status.</summary>
    [HttpPost("{id:guid}/proposal")]
    public async Task<ActionResult<ProposalOutcomeResponse>> Proposal(Guid id, AgentProposalRequest proposal, CancellationToken ct) =>
        Ok(await proposals.ReceiveAsync(id, proposal, ct));
}
