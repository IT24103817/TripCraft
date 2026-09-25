namespace TripCraft.Application.Workflows;

/// <summary>Workflow states used so far. Component C adds the approval-side states.</summary>
public enum AgentWorkflowStatus
{
    Planning,
    PendingApproval,
    Completed,
    FailedSafely
}
