using TripCraft.Application.Resources;

namespace TripCraft.Application.Common.Settings;

public interface ISettingsRepository
{
    /// <summary>The settings row, tracked; null until an Admin first saves the Settings page.</summary>
    Task<AppSettings?> FindAsync(CancellationToken ct);

    /// <summary>The rate card that starts on this date, tracked.</summary>
    Task<RateCardEntry?> FindRateCardAsync(DateOnly effectiveFrom, CancellationToken ct);

    void Add<T>(T entity) where T : class;
}
