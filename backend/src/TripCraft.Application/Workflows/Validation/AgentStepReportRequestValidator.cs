using System.Text.Json;
using FluentValidation;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Validation;

public class AgentStepReportRequestValidator : AbstractValidator<AgentStepReportRequest>
{
    public static readonly string[] AgentNames = ["planner", "itinerary", "resources", "validation"];
    public static readonly string[] StepStatuses = ["Succeeded", "Failed"];

    /// <summary>Summaries are small by design; a big blob means someone is sending raw text.</summary>
    public const int MaxSummaryChars = 8000;

    public AgentStepReportRequestValidator()
    {
        RuleFor(r => r.AgentName).Must(n => AgentNames.Contains(n)).WithMessage("Unknown agent name.");
        RuleFor(r => r.Status).Must(s => StepStatuses.Contains(s)).WithMessage("Status must be Succeeded or Failed.");
        RuleFor(r => r.DurationMs).InclusiveBetween(0, 10 * 60 * 1000);
        RuleFor(r => r.Retries).InclusiveBetween(0, 10);
        RuleFor(r => r.ToolCalls).Must(c => c is null || c.Count <= 50).WithMessage("At most 50 tool calls.");
        RuleForEach(r => r.ToolCalls).ChildRules(call =>
            call.RuleFor(c => c.Tool).NotEmpty().MaximumLength(64));
        RuleFor(r => r.InputSummary).Must(Small).WithMessage($"input_summary is over {MaxSummaryChars} characters.");
        RuleFor(r => r.OutputSummary).Must(Small).WithMessage($"output_summary is over {MaxSummaryChars} characters.");
        RuleFor(r => r.ValidationResult).Must(Small).WithMessage($"validation_result is over {MaxSummaryChars} characters.");
    }

    private static bool Small(JsonElement? json) => json is null || json.Value.GetRawText().Length <= MaxSummaryChars;
}
