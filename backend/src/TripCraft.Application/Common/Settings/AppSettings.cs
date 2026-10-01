using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Common.Settings;

/// <summary>
/// The one row of operator settings an Admin edits on the Settings page (table app_settings). The margin is not
/// here: it lives in rate_cards (effective from a date), so old quotations keep the margin they were priced with.
/// </summary>
public class AppSettings : BaseEntity
{
    /// <summary>"ollama" or "groq", sent to the agent service with each new workflow.</summary>
    public string LlmProvider { get; set; } = "ollama";

    public int CancellationCutoffDays { get; set; } = TripSettings.DefaultCancellationCutoffDays;
    public decimal DepositPct { get; set; } = TripSettings.DefaultDepositPct;
    public string OperatorContact { get; set; } = TripSettings.DefaultOperatorContact;

    /// <summary>The settings for one request: the configuration defaults with this row on top.</summary>
    public TripSettings ApplyTo(TripSettings defaults) => defaults with
    {
        CancellationCutoffDays = CancellationCutoffDays,
        OperatorContact = OperatorContact,
        DepositPct = DepositPct,
        LlmProvider = LlmProvider
    };
}
