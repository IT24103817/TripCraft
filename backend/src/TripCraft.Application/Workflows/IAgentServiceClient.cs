using TripCraft.Application.Trips.Planning;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Internal call to the LangGraph service (AGENT_SERVICE_URL, header X-Internal-Key).
/// Only the API talks to the agent service — never React or Flutter.
/// </summary>
public interface IAgentServiceClient
{
    /// <exception cref="AgentServiceException">The service is down, timed out or refused the request.</exception>
    Task StartWorkflowAsync(StartAgentWorkflowRequest request, CancellationToken ct);
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
