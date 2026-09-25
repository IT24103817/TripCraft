using TripCraft.Application.Trips.Planning;

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
    IReadOnlyList<SkeletonDay> Skeleton);
