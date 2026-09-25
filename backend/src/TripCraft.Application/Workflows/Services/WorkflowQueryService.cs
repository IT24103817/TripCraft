using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Services;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Services;

/// <summary>
/// Read side of the workflow monitor. Tourists see only workflows of their own trips (checked here,
/// not only in the controller); Operations Managers and Admins see all of them.
/// </summary>
public class WorkflowQueryService(IAgentWorkflowRepository workflows, ITripRequestRepository trips) : IWorkflowQueryService
{
    public async Task<WorkflowDto> GetAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var workflow = await LoadForUserAsync(user, id, ct);
        var steps = await workflows.ListStepsAsync(id, ct);
        var end = workflow.FinishedAt ?? DateTime.UtcNow;

        return new WorkflowDto(
            workflow.Id, workflow.TripRequestId, workflow.Status.ToString(), workflow.CurrentStep,
            WorkflowJson.ToElement(workflow.Plan) ?? default, WorkflowJson.ToElement(workflow.ValidationResult),
            WorkflowJson.ToElement(workflow.FinalOutcome), workflow.ErrorSummary, workflow.StartedAt, workflow.FinishedAt,
            (long)(end - workflow.StartedAt).TotalMilliseconds, steps.Count, steps.Sum(s => (long)s.DurationMs));
    }

    public async Task<IReadOnlyList<AgentStepDto>> ListStepsAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        await LoadForUserAsync(user, id, ct);
        var steps = await workflows.ListStepsAsync(id, ct);
        return steps.Select(s => new AgentStepDto(
            s.Id, s.StepNo, s.AgentName, s.ToolName,
            WorkflowJson.ToElement(s.InputSummary) ?? default, WorkflowJson.ToElement(s.OutputSummary) ?? default,
            WorkflowJson.ToElement(s.ValidationResult) ?? default, s.DurationMs, s.Retries, s.Status, s.CreatedAt)).ToList();
    }

    public Task<PagedResult<WorkflowSummaryDto>> ListAsync(WorkflowListQuery query, CancellationToken ct)
    {
        var workflowsQuery = workflows.Query();
        if (query.Status is { } status)
            workflowsQuery = workflowsQuery.Where(w => w.Status == status);

        return workflowsQuery
            .OrderByDescending(w => w.StartedAt)
            .ToPagedResultAsync(query.Page, query.PageSize, w => new WorkflowSummaryDto(
                w.Id, w.TripRequestId, w.Status.ToString(), w.CurrentStep, w.StartedAt, w.FinishedAt, w.ErrorSummary), ct);
    }

    private async Task<AgentWorkflow> LoadForUserAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var workflow = await workflows.Query().FirstOrDefaultAsync(w => w.Id == id, ct)
                       ?? throw new NotFoundException("Workflow not found.");
        if (user.IsTourist)
        {
            var trip = await trips.GetByIdAsync(workflow.TripRequestId, ct)
                       ?? throw new NotFoundException("Trip request not found.");
            TripRequestService.EnsureCanAccess(user, trip);
        }
        return workflow;
    }
}
