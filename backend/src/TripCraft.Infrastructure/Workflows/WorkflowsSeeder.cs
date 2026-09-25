using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Workflows.External;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Workflows;

/// <summary>
/// Seeds city_distances, the fallback when OpenRouteService is down or has no key.
/// Approximate driving distances and times between the seeded attraction cities.
/// </summary>
public static class WorkflowsSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.CityDistances.AnyAsync(ct))
            return;

        db.CityDistances.AddRange(
            Row("Colombo", "Kandy", 115, 180),
            Row("Colombo", "Galle", 126, 120),
            Row("Colombo", "Ella", 230, 330),
            Row("Kandy", "Ella", 140, 270),
            Row("Kandy", "Galle", 230, 270),
            Row("Ella", "Galle", 200, 300));
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} city distances", 6);
    }

    private static CityDistance Row(string from, string to, decimal km, int minutes) =>
        new() { FromCity = from, ToCity = to, DistanceKm = km, DurationMinutes = minutes };
}
