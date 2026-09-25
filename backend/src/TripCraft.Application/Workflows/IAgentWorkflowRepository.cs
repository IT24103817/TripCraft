namespace TripCraft.Application.Workflows;

public interface IAgentWorkflowRepository
{
    void Add(AgentWorkflow workflow);

    /// <summary>Tracked, for updates.</summary>
    Task<AgentWorkflow?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>The newest workflow for a trip request, tracked. Null if planning never started.</summary>
    Task<AgentWorkflow?> GetLatestForTripAsync(Guid tripRequestId, CancellationToken ct);

    /// <summary>True while a workflow for this trip is still in Planning.</summary>
    Task<bool> HasActiveForTripAsync(Guid tripRequestId, CancellationToken ct);

    /// <summary>Read-only base query for list screens.</summary>
    IQueryable<AgentWorkflow> Query();

    void AddStep(AgentStep step);
    Task<int> NextStepNoAsync(Guid workflowId, CancellationToken ct);
    Task<List<AgentStep>> ListStepsAsync(Guid workflowId, CancellationToken ct);
}
