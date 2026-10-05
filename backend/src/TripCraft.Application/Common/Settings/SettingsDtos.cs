using FluentValidation;

namespace TripCraft.Application.Common.Settings;

/// <summary>GET/PUT /api/admin/settings. MarginPct is today's rate card margin.</summary>
public record SettingsDto(string LlmProvider, int CancellationCutoffDays, decimal MarginPct, decimal DepositPct,
    string OperatorContact, DateTime? UpdatedAt);

public record SaveSettingsRequest(string LlmProvider, int CancellationCutoffDays, decimal MarginPct, decimal DepositPct,
    string OperatorContact);

public class SaveSettingsRequestValidator : AbstractValidator<SaveSettingsRequest>
{
    public SaveSettingsRequestValidator()
    {
        RuleFor(x => x.LlmProvider).Must(TripSettings.IsLlmProvider).WithMessage("Choose ollama, gemini or groq.");
        RuleFor(x => x.CancellationCutoffDays).InclusiveBetween(0, 30);
        RuleFor(x => x.MarginPct).InclusiveBetween(0, 100);
        RuleFor(x => x.DepositPct).InclusiveBetween(0, 100);
        RuleFor(x => x.OperatorContact).NotEmpty().MaximumLength(200);
    }
}
