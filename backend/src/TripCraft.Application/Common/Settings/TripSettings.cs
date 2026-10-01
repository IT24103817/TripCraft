namespace TripCraft.Application.Common.Settings;

/// <summary>
/// Operator settings read from configuration (v1.1):
/// CANCELLATION_CUTOFF_DAYS (default 3) — a tourist may cancel until this many days before the start;
/// OPERATOR_CONTACT (default "operations@tripcraft.test") — shown when cancellation is closed;
/// OPERATOR_TIME_ZONE (default "Asia/Colombo") — "today" for the cut-off and the voucher day check is the operator's
/// date, not the server's UTC date (at 01:00 in Colombo it is still yesterday in UTC).
/// </summary>
public record TripSettings(int CancellationCutoffDays, string OperatorContact, string TimeZoneId = TripSettings.DefaultTimeZoneId)
{
    public const int DefaultCancellationCutoffDays = 3;
    public const string DefaultOperatorContact = "operations@tripcraft.test";
    public const string DefaultTimeZoneId = "Asia/Colombo";

    public static TripSettings Default { get; } = new(DefaultCancellationCutoffDays, DefaultOperatorContact);

    /// <summary>Today's date where the tours run.</summary>
    public DateOnly Today() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, TimeZoneId));

    /// <summary>The last day the tourist may cancel a trip that starts on <paramref name="start"/>.</summary>
    public DateOnly CancelUntil(DateOnly start) => start.AddDays(-CancellationCutoffDays);
}
