using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Workflows.External;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Workflows;

/// <summary>
/// Seeds city_distances, the fallback when OpenRouteService is down or has no key.
/// Approximate driving distances and times between every pair of the six seeded attraction cities.
/// Safe to run on every start: it only adds pairs that are missing (in either direction).
/// </summary>
public static class WorkflowsSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var existing = await db.CityDistances.Select(d => new { d.FromCity, d.ToCity }).ToListAsync(ct);
        var known = existing.Select(d => Key(d.FromCity, d.ToCity)).ToHashSet();
        var missing = Rows().Where(r => !known.Contains(Key(r.FromCity, r.ToCity))).ToList();
        if (missing.Count == 0)
            return;

        db.CityDistances.AddRange(missing);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} city distances", missing.Count);
    }

    private static CityDistance[] Rows() =>
    [
        Row("Colombo", "Kandy", 115, 180),
        Row("Colombo", "Galle", 126, 120),
        Row("Colombo", "Ella", 230, 330),
        Row("Kandy", "Ella", 140, 270),
        Row("Kandy", "Galle", 230, 270),
        Row("Ella", "Galle", 200, 300),
        Row("Colombo", "Nuwara Eliya", 170, 270),
        Row("Colombo", "Sigiriya", 175, 240),
        Row("Kandy", "Nuwara Eliya", 77, 150),
        Row("Kandy", "Sigiriya", 90, 150),
        Row("Ella", "Nuwara Eliya", 55, 120),
        Row("Ella", "Sigiriya", 200, 330),
        Row("Galle", "Nuwara Eliya", 230, 330),
        Row("Galle", "Sigiriya", 290, 330),
        Row("Nuwara Eliya", "Sigiriya", 160, 270)
    ];

    /// <summary>Order-independent key, so Kandy→Ella and Ella→Kandy are the same pair.</summary>
    private static string Key(string a, string b) =>
        string.CompareOrdinal(a.ToLowerInvariant(), b.ToLowerInvariant()) < 0
            ? $"{a.ToLowerInvariant()}|{b.ToLowerInvariant()}"
            : $"{b.ToLowerInvariant()}|{a.ToLowerInvariant()}";

    private static CityDistance Row(string from, string to, decimal km, int minutes) =>
        new() { FromCity = from, ToCity = to, DistanceKm = km, DurationMinutes = minutes };
}
