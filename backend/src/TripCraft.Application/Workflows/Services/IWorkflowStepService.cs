using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Services;

public interface IWorkflowStepService
{
    Task<AgentStepCreatedResponse> RecordAsync(Guid workflowId, AgentStepReportRequest report, CancellationToken ct);
}
