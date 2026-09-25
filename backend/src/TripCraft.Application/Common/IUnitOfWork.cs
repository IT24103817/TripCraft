namespace TripCraft.Application.Common;

/// <summary>
/// Saves every change tracked in the current request in one database transaction.
/// Repositories only stage changes; services decide when to commit.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
