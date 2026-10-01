using TripCraft.Application.Trips.Planning;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Internal call to the LangGraph service (AGENT_SERVICE_URL, header X-Internal-Key).
/// Only the API talks to the agent service — never React or Flutter.
/// Never throws for service problems: on failure it sets the workflow to FailedSafely with an
/// error summary and returns false. The caller saves the change.
/// </summary>
public interface IAgentServiceClient
{
    Task<bool> StartAsync(AgentWorkflow workflow, StartAgentWorkflowRequest request, CancellationToken ct);

    Task<bool> ReplanAsync(AgentWorkflow workflow, StartAgentWorkflowRequest request, string managerComment,
        CancellationToken ct);
}

/// <summary>Payload for the Planner agent. Matches the Planner input contract in PLAN.md section 5.</summary>
public record StartAgentWorkflowRequest(
    Guid WorkflowId,
    Guid TripRequestId,
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    int Pax,
    decimal BudgetUsd,
    string PreferencesJson,
    IReadOnlyList<SkeletonDay> Skeleton,
    IReadOnlyList<PreviousViolation>? PreviousViolations = null,
    IReadOnlyList<string>? Cities = null)
{
    /// <summary>A re-plan of the trip: same trip details and cities, plus the violations the manager sent back.</summary>
    public static StartAgentWorkflowRequest ForReplan(AgentWorkflow workflow, Trips.TripRequest trip,
        IReadOnlyList<PreviousViolation> previousViolations) =>
        new(workflow.Id, trip.Id, trip.Objective, trip.StartDate, trip.EndDate, trip.Pax, trip.BudgetUsd, trip.Preferences,
            [], previousViolations, trip.CityList);
}

/// <summary>A violation of the proposal a manager sent back; the Planner re-plans for it (e.g. OVER_BUDGET).</summary>
public record PreviousViolation(string Code, string Message)
{
    /// <summary>Reads the violations from a workflow's stored ValidationResult JSON. Empty when there is none.</summary>
    public static IReadOnlyList<PreviousViolation> FromValidationJson(string? validationResultJson)
    {
        var result = WorkflowJson.Deserialize<ProposalValidationResult>(validationResultJson);
        return result?.Violations.Select(v => new PreviousViolation(v.Code, v.Message)).ToList() ?? [];
    }
}
