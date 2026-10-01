using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Shape of agent_workflows.final_outcome: the last proposal, then the manager's decision.
/// Summaries only: days, resource ids, quotation numbers — never prompts or model text.
/// EditedSinceQuotation is true after the manager edited a day or swapped a resource: the quotation must be
/// re-priced before it can be sent to the client.
/// </summary>
public record WorkflowOutcome(StoredProposal Proposal, WorkflowDecision? Decision, bool EditedSinceQuotation = false);

/// <summary>What one quotation version priced (quotations.proposal_snapshot), for the v1/v2 comparison.</summary>
public record ProposalSnapshot(List<ProposalDay>? Days, ProposalResources? Resources)
{
    public static string Serialize(StoredProposal proposal) =>
        WorkflowJson.Serialize(new ProposalSnapshot(proposal.Days, proposal.Resources));
}

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
