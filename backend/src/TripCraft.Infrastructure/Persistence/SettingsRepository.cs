using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Persistence;

public class SettingsRepository(AppDbContext db) : ISettingsRepository
{
    public Task<AppSettings?> FindAsync(CancellationToken ct) => db.AppSettings.OrderBy(s => s.CreatedAt).FirstOrDefaultAsync(ct);

    public Task<RateCardEntry?> FindRateCardAsync(DateOnly effectiveFrom, CancellationToken ct) =>
        db.RateCards.FirstOrDefaultAsync(r => r.EffectiveFrom == effectiveFrom, ct);

    public void Add<T>(T entity) where T : class => db.Add(entity);

    /// <summary>
    /// The settings for one request: configuration defaults, then the app_settings row if an Admin saved one.
    /// Synchronous because DI factories cannot await; it is one small primary-key-sized read per request.
    /// </summary>
    public static TripSettings Load(AppDbContext db, TripSettings defaults)
    {
        var row = db.AppSettings.AsNoTracking().OrderBy(s => s.CreatedAt).FirstOrDefault();
        return row is null ? defaults : row.ApplyTo(defaults);
    }
}
