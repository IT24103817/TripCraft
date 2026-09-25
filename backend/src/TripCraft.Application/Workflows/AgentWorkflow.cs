using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Workflows;

/// <summary>
/// agent_workflows from PLAN.md section 4. Owned by Component C; created by Component A for the
/// start-planning endpoint (PLAN.md section 14, Sunday). Stores state and summaries only —
/// never raw prompts or hidden reasoning.
/// </summary>
public class AgentWorkflow : BaseEntity
{
    public Guid TripRequestId { get; set; }
    public string Objective { get; set; } = string.Empty;

    /// <summary>jsonb. Starts as the itinerary skeleton; the Planner agent replaces it with its plan.</summary>
    public string Plan { get; set; } = "{}";

    public AgentWorkflowStatus Status { get; set; } = AgentWorkflowStatus.Planning;
    public string? CurrentStep { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    /// <summary>jsonb summary of the result, filled when the workflow ends.</summary>
    public string? FinalOutcome { get; set; }

    public string? ErrorSummary { get; set; }
}
