namespace TripCraft.Application.Common;

/// <summary>A real round trip to the database, for GET /health.</summary>
public interface IDatabaseHealth
{
    Task<bool> CanConnectAsync(CancellationToken ct);
}
