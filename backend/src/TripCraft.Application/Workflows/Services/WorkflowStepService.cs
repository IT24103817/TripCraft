using TripCraft.Application.Common;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Services;

/// <summary>Stores one agent_steps row per step report from the agent service (observability, PLAN.md section 5).</summary>
public class WorkflowStepService(IAgentWorkflowRepository workflows, IUnitOfWork unitOfWork) : IWorkflowStepService
{
    private const int MaxToolNameLength = 200;

    public async Task<AgentStepCreatedResponse> RecordAsync(Guid workflowId, AgentStepReportRequest report, CancellationToken ct)
    {
        var workflow = await workflows.GetByIdAsync(workflowId, ct)
                       ?? throw new NotFoundException("Workflow not found.");
        if (workflow.Status is AgentWorkflowStatus.Approved or AgentWorkflowStatus.Rejected or AgentWorkflowStatus.Completed)
            throw new ConflictException($"Workflow is {workflow.Status}; it no longer accepts steps.");

        var toolNames = string.Join(",", (report.ToolCalls ?? []).Select(c => c.Tool).Distinct());
        var step = new AgentStep
        {
            WorkflowId = workflow.Id,
            StepNo = await workflows.NextStepNoAsync(workflow.Id, ct),
            AgentName = report.AgentName,
            ToolName = toolNames.Length == 0 ? null : toolNames[..Math.Min(toolNames.Length, MaxToolNameLength)],
            InputSummary = WorkflowJson.Serialize(new { summary = report.InputSummary, toolCalls = report.ToolCalls }),
            OutputSummary = WorkflowJson.Raw(report.OutputSummary),
            ValidationResult = WorkflowJson.Raw(report.ValidationResult),
            DurationMs = report.DurationMs,
            Retries = report.Retries,
            Status = report.Status
        };
        workflows.AddStep(step);
        workflow.CurrentStep = report.AgentName;

        await unitOfWork.SaveChangesAsync(ct);
        return new AgentStepCreatedResponse(step.Id, step.StepNo);
    }
}
