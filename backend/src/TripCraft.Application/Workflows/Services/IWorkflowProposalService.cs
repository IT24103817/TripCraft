using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Services;

public interface IWorkflowProposalService
{
    Task<ProposalOutcomeResponse> ReceiveAsync(Guid workflowId, AgentProposalRequest proposal, CancellationToken ct);
}
