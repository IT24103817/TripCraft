using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows.Services;

/// <summary>
/// Read side of the workflow monitor. Tourists see only workflows of their own trips (checked here,
/// not only in the controller); Operations Managers and Admins see all of them.
/// </summary>
public class WorkflowQueryService(IAgentWorkflowRepository workflows, ITripRequestRepository trips, IResourceCatalog resources)
    : IWorkflowQueryService
{
    /// <summary>Whitelisted sort fields of GET /api/workflows.</summary>
    public static readonly IReadOnlyDictionary<string, Expression<Func<AgentWorkflow, object>>> SortableFields =
        new Dictionary<string, Expression<Func<AgentWorkflow, object>>>
        {
            ["startedAt"] = w => w.StartedAt,
            ["finishedAt"] = w => w.FinishedAt!,
            ["status"] = w => w.Status
        };

    public async Task<WorkflowDto> GetAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var workflow = await LoadForUserAsync(user, id, ct);
        var steps = await workflows.ListStepsAsync(id, ct);
        var end = workflow.FinishedAt ?? DateTime.UtcNow;

        return new WorkflowDto(
            workflow.Id, workflow.TripRequestId, workflow.Status.ToString(), workflow.CurrentStep,
            WorkflowJson.ToElement(workflow.Plan) ?? default, WorkflowJson.ToElement(workflow.ValidationResult),
            WorkflowJson.ToElement(workflow.FinalOutcome), workflow.ErrorSummary, workflow.StartedAt, workflow.FinishedAt,
            (long)(end - workflow.StartedAt).TotalMilliseconds, steps.Count, steps.Sum(s => (long)s.DurationMs),
            await ResourceNamesAsync(workflow, ct));
    }

    /// <summary>Guide name, "Van CAB-1234" and "Hotel — Room type" for the ids in the proposal (unknown ids are left out).</summary>
    private async Task<IReadOnlyDictionary<string, string>> ResourceNamesAsync(AgentWorkflow workflow, CancellationToken ct)
    {
        var names = new Dictionary<string, string>();
        var proposed = WorkflowJson.Deserialize<WorkflowOutcome>(workflow.FinalOutcome)?.Proposal.Resources;
        if (proposed is null)
            return names;
        if (ProposalValidator.ParseId(proposed.GuideId) is { } guideId && await resources.GetGuideAsync(guideId, ct) is { } guide)
            names[proposed.GuideId!] = guide.Name;
        if (ProposalValidator.ParseId(proposed.VehicleId) is { } vehicleId && await resources.GetVehicleAsync(vehicleId, ct) is { } vehicle)
            names[proposed.VehicleId!] = $"{vehicle.Type} {vehicle.RegistrationNo}";
        foreach (var roomTypeId in (proposed.Rooms ?? []).Select(r => r.RoomTypeId).Distinct())
        {
            if (ProposalValidator.ParseId(roomTypeId) is { } id && await resources.GetRoomTypeAsync(id, ct) is { } room)
                names[roomTypeId] = $"{room.HotelName} — {room.RoomTypeName}";
        }
        return names;
    }

    public async Task<WorkflowDto> GetLatestForTripAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);
        var workflowId = await workflows.Query()
                             .Where(w => w.TripRequestId == tripRequestId)
                             .OrderByDescending(w => w.StartedAt)
                             .Select(w => (Guid?)w.Id)
                             .FirstOrDefaultAsync(ct)
                         ?? throw new NotFoundException("Planning has not started for this trip request.");
        return await GetAsync(user, workflowId, ct);
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
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            workflowsQuery = workflowsQuery.Where(w => w.Objective.ToLower().Contains(search));
        }

        return workflowsQuery
            .ApplySort(query.Sort, SortableFields, "-startedAt")
            .ToPagedResultAsync(query.Page, query.PageSize, w => new WorkflowSummaryDto(
                w.Id, w.TripRequestId, w.Status.ToString(), w.CurrentStep, w.StartedAt, w.FinishedAt, w.ErrorSummary,
                w.Objective), ct);
    }

    private async Task<AgentWorkflow> LoadForUserAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var workflow = await workflows.Query().FirstOrDefaultAsync(w => w.Id == id, ct)
                       ?? throw new NotFoundException("Workflow not found.");
        if (user.IsTourist)
        {
            var trip = await trips.GetByIdAsync(workflow.TripRequestId, ct)
                       ?? throw new NotFoundException("Trip request not found.");
            TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);
        }
        return workflow;
    }
}
