using TripCraft.Application.Common;

namespace TripCraft.Infrastructure.Persistence;

public class DatabaseHealth(AppDbContext db) : IDatabaseHealth
{
    /// <summary>Opens a connection and runs a trivial query. Never throws; a slow or failing database is "fail".</summary>
    public async Task<bool> CanConnectAsync(CancellationToken ct)
    {
        try
        {
            return await db.Database.CanConnectAsync(ct);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
