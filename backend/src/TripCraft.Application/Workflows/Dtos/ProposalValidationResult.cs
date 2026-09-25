namespace TripCraft.Application.Workflows.Dtos;

/// <summary>Output of ProposalValidator. Stored as jsonb on agent_workflows.validation_result.</summary>
public record ProposalValidationResult(bool IsValid, IReadOnlyList<ProposalRuleViolation> Violations)
{
    public bool HasHard => Violations.Any(v => v.Severity == ViolationSeverity.Hard);
    public bool HasSoft => Violations.Any(v => v.Severity == ViolationSeverity.Soft);
}

public record ProposalRuleViolation(string Code, string Message, ViolationSeverity Severity);

/// <summary>Hard: the proposal cannot go to a manager (FailedSafely). Soft: it needs a revision (over budget).</summary>
public enum ViolationSeverity
{
    Hard,
    Soft
}
