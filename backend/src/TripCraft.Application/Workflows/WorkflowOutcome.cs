using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Shape of agent_workflows.final_outcome: the last proposal, then the manager's decision.
/// Summaries only: days, resource ids, quotation numbers — never prompts or model text.
/// </summary>
public record WorkflowOutcome(StoredProposal Proposal, WorkflowDecision? Decision);

public record StoredProposal(
    List<ProposalDay>? Days,
    ProposalResources? Resources,
    ProposalQuotation? Quotation,
    List<ProposalViolation>? AgentViolations,
    int Replans,
    Guid? QuotationId);

public record WorkflowDecision(
    string Decision, Guid QuotationId, Guid DecidedBy, DateTime DecidedAt, string? Comment,
    IReadOnlyList<ResourceHoldRequest> Holds);
