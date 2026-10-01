namespace TripCraft.Application.Common.Settings;

/// <summary>
/// The operator settings in force for this request (v1.1). Defaults come from configuration:
/// CANCELLATION_CUTOFF_DAYS (default 3) — a tourist may cancel until this many days before the start;
/// OPERATOR_CONTACT (default "operations@tripcraft.test") — shown when cancellation is closed;
/// OPERATOR_TIME_ZONE (default "Asia/Colombo") — "today" for the cut-off and the voucher day check is the operator's
/// date, not the server's UTC date (at 01:00 in Colombo it is still yesterday in UTC).
/// Once an Admin saves the Settings page, the app_settings row overrides the cut-off, the contact, the deposit % and
/// the LLM provider; it is read again on every request (TripSettings is scoped).
/// LlmProvider null = let the agent service use its own LLM_PROVIDER.
/// </summary>
public record TripSettings(int CancellationCutoffDays, string OperatorContact, string TimeZoneId = TripSettings.DefaultTimeZoneId,
    decimal DepositPct = TripSettings.DefaultDepositPct, string? LlmProvider = null)
{
    public const int DefaultCancellationCutoffDays = 3;
    public const string DefaultOperatorContact = "operations@tripcraft.test";
    public const string DefaultTimeZoneId = "Asia/Colombo";
    public const decimal DefaultDepositPct = 30m;

    public static TripSettings Default { get; } = new(DefaultCancellationCutoffDays, DefaultOperatorContact);

    /// <summary>Today's date where the tours run.</summary>
    public DateOnly Today() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, TimeZoneId));

    /// <summary>The last day the tourist may cancel a trip that starts on <paramref name="start"/>.</summary>
    public DateOnly CancelUntil(DateOnly start) => start.AddDays(-CancellationCutoffDays);
}
