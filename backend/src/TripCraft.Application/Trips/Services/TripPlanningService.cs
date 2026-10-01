using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Planning;
using TripCraft.Application.Workflows;

namespace TripCraft.Application.Trips.Services;

/// <summary>
/// Component A business operation. Steps:
/// 1. load the trip and check the caller may use it;
/// 2. check the status allows planning (TripStatusMachine: Submitted or FailedSafely → Planning; 409 otherwise);
/// 3. run the pure passport/date rules and build the day-by-day skeleton (400 on failure);
/// 4. save the agent_workflows row + trip status + audit rows in one SaveChanges (one transaction);
/// 5. call the agent service. If it fails, the trip becomes FailedSafely and "Try again" starts planning again.
/// </summary>
public class TripPlanningService(
    ITripRequestRepository trips,
    IAttractionRepository attractions,
    IAgentWorkflowRepository workflows,
    IAgentServiceClient agentService,
    IAuditLogger audit,
    IUnitOfWork unitOfWork,
    ILogger<TripPlanningService> logger) : ITripPlanningService
{
    public async Task<StartPlanningResponse> StartPlanningAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        // 1. Load and authorise.
        var trip = await trips.GetByIdAsync(tripRequestId, ct)
                   ?? throw new NotFoundException("Trip request not found.");
        TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);

        // 2. Status must allow planning.
        TripStatusMachine.EnsureCanMove(trip.Status, TripRequestStatus.Planning);
        if (await workflows.HasActiveForTripAsync(trip.Id, ct))
            throw new ConflictException("An agent workflow is already running for this trip request.");

        // 3. Pure business rules.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var errors = TripPlanningRules.ValidateForPlanning(trip, trip.Tourist!, today);

        // v1.1: the cities the tourist picked from the list. Trips made before v1.1 fall back to the objective text.
        var knownCities = await attractions.ListActiveCitiesAsync(ct);
        var cities = trip.CityList.Count > 0
            ? trip.CityList.ToList()
            : TripPlanningRules.ExtractCities(trip.Objective, knownCities);
        var tripDays = TripPlanningRules.TripDays(trip.StartDate, trip.EndDate);
        if (cities.Count == 0)
            errors.Add($"Choose at least one destination we cover: {string.Join(", ", knownCities)}.");
        else if (cities.Count > tripDays)
            errors.Add($"Objective mentions {cities.Count} cities but the trip is only {tripDays} day(s).");

        if (errors.Count > 0)
            throw new ValidationException(errors.Select(e => new ValidationFailure("tripRequest", e)));

        var skeleton = TripPlanningRules.BuildSkeleton(
            trip.StartDate, trip.EndDate, cities, TripPlanningRules.ReadPace(trip.Preferences));

        // 4. Workflow row + status change + audit, committed together.
        var workflow = new AgentWorkflow
        {
            TripRequestId = trip.Id,
            Objective = trip.Objective,
            Plan = JsonSerializer.Serialize(new { skeleton }),
            Status = AgentWorkflowStatus.Planning,
            CurrentStep = "planner",
            StartedAt = DateTime.UtcNow
        };
        workflows.Add(workflow);
        TripStatusMachine.Move(trip, TripRequestStatus.Planning, user.Id, "Planning started.", audit);
        audit.Record(user.Id, "AgentWorkflowStarted", nameof(AgentWorkflow), workflow.Id,
            null, new { workflow.TripRequestId, Status = workflow.Status.ToString(), Days = skeleton.Count });
        await unitOfWork.SaveChangesAsync(ct);

        // 5. Hand over to the agents. Never hold a DB transaction open during an HTTP call.
        // The client never throws: on failure it has already set the workflow to FailedSafely.
        var started = await agentService.StartAsync(workflow, new StartAgentWorkflowRequest(
            workflow.Id, trip.Id, trip.Objective, trip.StartDate, trip.EndDate,
            trip.Pax, trip.BudgetUsd, trip.Preferences, skeleton, null, cities), ct);
        if (!started)
        {
            logger.LogWarning("Agent service failed for workflow {WorkflowId}", workflow.Id);
            await RecordSafeFailureAsync(user, trip, workflow, ct);
        }

        return new StartPlanningResponse(
            workflow.Id, trip.Id, workflow.Status.ToString(), trip.Status.ToString(), skeleton, workflow.ErrorSummary);
    }

    /// <summary>
    /// PLAN.md section 5 safe failure: the agent client already set the workflow to FailedSafely with a
    /// summary; the trip becomes FailedSafely too (it can be edited and planned again), and both are audited.
    /// </summary>
    private async Task RecordSafeFailureAsync(CurrentUser user, TripRequest trip, AgentWorkflow workflow, CancellationToken ct)
    {
        audit.Record(user.Id, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), workflow.Id,
            new { Status = AgentWorkflowStatus.Planning.ToString() },
            new { Status = workflow.Status.ToString(), workflow.ErrorSummary });
        TripStatusMachine.Move(trip, TripRequestStatus.FailedSafely, user.Id,
            workflow.ErrorSummary ?? "The agent service could not be reached.", audit);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
