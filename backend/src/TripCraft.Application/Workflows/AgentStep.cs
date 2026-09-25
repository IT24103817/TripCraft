using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Workflows;

/// <summary>
/// agent_steps from PLAN.md section 4: one row per agent run, posted by the agent service.
/// Observability evidence only — summaries, never prompts or model text.
/// </summary>
public class AgentStep : BaseEntity
{
    public Guid WorkflowId { get; set; }

    /// <summary>1, 2, 3... in the order the steps arrived for this workflow.</summary>
    public int StepNo { get; set; }

    public string AgentName { get; set; } = string.Empty;

    /// <summary>Comma-separated names of the tools the agent called, e.g. "get_attractions,get_distance".</summary>
    public string? ToolName { get; set; }

    public string InputSummary { get; set; } = "{}";
    public string OutputSummary { get; set; } = "{}";
    public string ValidationResult { get; set; } = "{}";
    public int DurationMs { get; set; }
    public int Retries { get; set; }
    public string Status { get; set; } = string.Empty;
}
