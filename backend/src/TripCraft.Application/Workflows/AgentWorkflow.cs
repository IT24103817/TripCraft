using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Workflows;

/// <summary>
/// agent_workflows from PLAN.md section 4. Stores state and summaries only —
/// never raw prompts, model text or hidden reasoning.
/// </summary>
public class AgentWorkflow : BaseEntity
{
    public Guid TripRequestId { get; set; }
    public string Objective { get; set; } = string.Empty;

    /// <summary>jsonb. Starts as the itinerary skeleton; replaced by the Planner agent's plan.</summary>
    public string Plan { get; set; } = "{}";

    public AgentWorkflowStatus Status { get; set; } = AgentWorkflowStatus.Planning;
    public string? CurrentStep { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    /// <summary>jsonb. The last proposal from the agents (days, resources, quotation), then the approval result.</summary>
    public string? FinalOutcome { get; set; }

    /// <summary>jsonb. Result of the C# ProposalValidator for the last proposal.</summary>
    public string? ValidationResult { get; set; }

    public string? ErrorSummary { get; set; }

    /// <summary>True while the agents are still working on this workflow.</summary>
    public bool IsActive => Status is AgentWorkflowStatus.Planning;
}
