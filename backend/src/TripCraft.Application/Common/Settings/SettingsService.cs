using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources;

namespace TripCraft.Application.Common.Settings;

public interface ISettingsService
{
    Task<SettingsDto> GetAsync(CancellationToken ct);
    Task<SettingsDto> SaveAsync(CurrentUser user, SaveSettingsRequest request, CancellationToken ct);
}

/// <summary>
/// The Admin Settings page (v1.1). Saving writes the app_settings row and, when the margin changed, today's rate card
/// (quotations made before keep the margin they were priced with). Audited.
/// </summary>
public class SettingsService(
    ISettingsRepository settings,
    IResourceRepository resources,
    TripSettings current,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : ISettingsService
{
    public async Task<SettingsDto> GetAsync(CancellationToken ct)
    {
        var row = await settings.FindAsync(ct);
        return new SettingsDto(row?.LlmProvider ?? current.LlmProvider ?? "ollama", current.CancellationCutoffDays,
            await resources.CurrentMarginPctAsync(current.Today(), ct), current.DepositPct, current.OperatorContact,
            row?.UpdatedAt);
    }

    public async Task<SettingsDto> SaveAsync(CurrentUser user, SaveSettingsRequest request, CancellationToken ct)
    {
        var before = await GetAsync(ct);
        var row = await settings.FindAsync(ct);
        if (row is null)
        {
            row = new AppSettings();
            settings.Add(row);
        }
        row.LlmProvider = request.LlmProvider;
        row.CancellationCutoffDays = request.CancellationCutoffDays;
        row.DepositPct = request.DepositPct;
        row.OperatorContact = request.OperatorContact.Trim();

        if (request.MarginPct != before.MarginPct)
        {
            var today = current.Today();
            var card = await settings.FindRateCardAsync(today, ct);
            if (card is null)
                settings.Add(new RateCardEntry { MarginPct = request.MarginPct, EffectiveFrom = today });
            else
                card.MarginPct = request.MarginPct;
        }

        audit.Record(user.Id, "SettingsUpdated", nameof(AppSettings), row.Id, before, request);
        await unitOfWork.SaveChangesAsync(ct);
        return new SettingsDto(row.LlmProvider, row.CancellationCutoffDays, request.MarginPct, row.DepositPct,
            row.OperatorContact, row.UpdatedAt);
    }
}
