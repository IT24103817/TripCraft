namespace TripCraft.Application.Common;

/// <summary>
/// Saves every change tracked in the current request in one database transaction.
/// Repositories only stage changes; services decide when to commit.
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);

    /// <summary>
    /// Explicit transaction for read-check-write operations (e.g. hold overlap check + approval).
    /// Disposing it without CommitAsync rolls everything back.
    /// </summary>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct);

    /// <summary>Forgets every staged change, so nothing from a failed operation is saved later.</summary>
    void DiscardChanges();
}

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}
