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
/// 2. check the status allows planning (409 otherwise);
/// 3. run the pure passport/date rules and build the day-by-day skeleton (400 on failure);
/// 4. save the agent_workflows row + trip status + audit rows in one SaveChanges (one transaction);
/// 5. call the agent service. If it fails, record a safe failure and put the trip back.
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
    private static readonly TripRequestStatus[] PlannableStatuses =
        [TripRequestStatus.Submitted, TripRequestStatus.RevisionRequested];

    public async Task<StartPlanningResponse> StartPlanningAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        // 1. Load and authorise.
        var trip = await trips.GetByIdAsync(tripRequestId, ct)
                   ?? throw new NotFoundException("Trip request not found.");
        TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);

        // 2. Status must allow planning.
        if (!PlannableStatuses.Contains(trip.Status))
            throw new ConflictException($"Planning cannot start while the trip request is {trip.Status}.");
        if (await workflows.HasActiveForTripAsync(trip.Id, ct))
            throw new ConflictException("An agent workflow is already running for this trip request.");

        // 3. Pure business rules.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var errors = TripPlanningRules.ValidateForPlanning(trip, trip.Tourist!, today);

        var knownCities = await attractions.ListActiveCitiesAsync(ct);
        var cities = TripPlanningRules.ExtractCities(trip.Objective, knownCities);
        var tripDays = TripPlanningRules.TripDays(trip.StartDate, trip.EndDate);
        if (cities.Count == 0)
            errors.Add($"Objective must mention at least one destination we cover: {string.Join(", ", knownCities)}.");
        else if (cities.Count > tripDays)
            errors.Add($"Objective mentions {cities.Count} cities but the trip is only {tripDays} day(s).");

        if (errors.Count > 0)
            throw new ValidationException(errors.Select(e => new ValidationFailure("tripRequest", e)));

        var skeleton = TripPlanningRules.BuildSkeleton(
            trip.StartDate, trip.EndDate, cities, TripPlanningRules.ReadPace(trip.Preferences));

        // 4. Workflow row + status change + audit, committed together.
        var previousStatus = trip.Status;
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
        trip.Status = TripRequestStatus.Planning;

        audit.Record(user.Id, "TripRequestStatusChanged", nameof(TripRequest), trip.Id,
            new { Status = previousStatus.ToString() }, new { Status = trip.Status.ToString() });
        audit.Record(user.Id, "AgentWorkflowStarted", nameof(AgentWorkflow), workflow.Id,
            null, new { workflow.TripRequestId, Status = workflow.Status.ToString(), Days = skeleton.Count });
        await unitOfWork.SaveChangesAsync(ct);

        // 5. Hand over to the agents. Never hold a DB transaction open during an HTTP call.
        // The client never throws: on failure it has already set the workflow to FailedSafely.
        var started = await agentService.StartAsync(workflow, new StartAgentWorkflowRequest(
            workflow.Id, trip.Id, trip.Objective, trip.StartDate, trip.EndDate,
            trip.Pax, trip.BudgetUsd, trip.Preferences, skeleton), ct);
        if (!started)
        {
            logger.LogWarning("Agent service failed for workflow {WorkflowId}", workflow.Id);
            await RecordSafeFailureAsync(user, trip, workflow, previousStatus, ct);
        }

        return new StartPlanningResponse(
            workflow.Id, trip.Id, workflow.Status.ToString(), trip.Status.ToString(), skeleton, workflow.ErrorSummary);
    }

    /// <summary>
    /// PLAN.md section 5 safe failure: the agent client already set the workflow to FailedSafely with a
    /// summary; here the trip goes back to its previous status so it can be retried, and both are audited.
    /// </summary>
    private async Task RecordSafeFailureAsync(
        CurrentUser user, TripRequest trip, AgentWorkflow workflow, TripRequestStatus previousStatus,
        CancellationToken ct)
    {
        trip.Status = previousStatus;

        audit.Record(user.Id, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), workflow.Id,
            new { Status = AgentWorkflowStatus.Planning.ToString() },
            new { Status = workflow.Status.ToString(), workflow.ErrorSummary });
        audit.Record(user.Id, "TripRequestStatusChanged", nameof(TripRequest), trip.Id,
            new { Status = TripRequestStatus.Planning.ToString() }, new { Status = trip.Status.ToString() });
        await unitOfWork.SaveChangesAsync(ct);
    }
}
