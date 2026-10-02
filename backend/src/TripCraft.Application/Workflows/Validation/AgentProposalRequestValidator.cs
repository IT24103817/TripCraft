using FluentValidation;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Validation;

/// <summary>
/// Only checks the envelope. The content (days, resources, quotation) is checked by ProposalValidator,
/// so an incomplete proposal ends the workflow safely instead of bouncing with a 400.
/// Status is the agents' own verdict (PLAN.md section 10: the over-budget golden case returns RevisionRequested). It is
/// not a trip or workflow status: the API reads it and decides (send, best price, or NeedsOperator).
/// </summary>
public class AgentProposalRequestValidator : AbstractValidator<AgentProposalRequest>
{
    public static readonly string[] AgentStatuses = ["PendingApproval", "RevisionRequested", "FailedSafely"];

    public AgentProposalRequestValidator()
    {
        RuleFor(r => r.Status).Must(s => AgentStatuses.Contains(s))
            .WithMessage("Status must be PendingApproval, RevisionRequested or FailedSafely.");
        RuleFor(r => r.Replans).InclusiveBetween(0, 10);
        RuleFor(r => r.ErrorSummary).MaximumLength(1000);
        RuleFor(r => r.Days).Must(d => d is null || d.Count <= 60).WithMessage("At most 60 days.");
    }
}
