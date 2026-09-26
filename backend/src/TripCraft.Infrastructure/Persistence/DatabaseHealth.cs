using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;

namespace TripCraft.Infrastructure.Persistence;

public class DatabaseHealth(AppDbContext db) : IDatabaseHealth
{
    /// <summary>
    /// Runs SELECT 1 on a pooled connection. Never throws; a slow or failing database is "fail".
    /// Not Database.CanConnectAsync: on PostgreSQL that opens a new, unpooled connection every time, which under
    /// load (k6 db-response.js) used up the machine's ports and made healthy checks report 503.
    /// </summary>
    public async Task<bool> CanConnectAsync(CancellationToken ct)
    {
        try
        {
            if (!db.Database.IsRelational())
                return await db.Database.CanConnectAsync(ct); // EF InMemory in tests
            await db.Database.ExecuteSqlRawAsync("SELECT 1", ct);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
