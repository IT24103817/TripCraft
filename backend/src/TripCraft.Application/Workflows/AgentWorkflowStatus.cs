namespace TripCraft.Application.Workflows;

/// <summary>
/// agent_workflows.status (PLAN.md sections 4–6). Stored as text. Planning = the agents are working (also while they
/// re-plan after a declined quote); PendingApproval = re-priced by a manager, not sent yet; Approved = the quotation
/// was sent to the client; Completed = the trip was confirmed; FailedSafely = the agents failed or a Hard rule
/// failed, so the trip needs the operator. RevisionRequested is no longer a workflow status (v1.1).
/// </summary>
public enum AgentWorkflowStatus
{
    Planning,
    PendingApproval,
    Approved,
    Rejected,
    Completed,
    FailedSafely
}
