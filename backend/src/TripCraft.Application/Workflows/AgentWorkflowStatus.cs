namespace TripCraft.Application.Workflows;

/// <summary>agent_workflows.status (PLAN.md sections 4–6). Stored as text.</summary>
public enum AgentWorkflowStatus
{
    Planning,
    PendingApproval,
    RevisionRequested,
    Approved,
    Rejected,
    Completed,
    FailedSafely
}
