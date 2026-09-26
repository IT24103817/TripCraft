namespace TripCraft.Application.Quotations;

/// <summary>Data access for quotations (Component C). Query is read-only; Add stages a row, the caller commits.</summary>
public interface IQuotationRepository
{
    /// <summary>Quotations with their lines.</summary>
    IQueryable<Quotation> Query();

    IQueryable<ApprovalDecision> Decisions();

    /// <summary>Tracked, with lines.</summary>
    Task<Quotation?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>Highest version for the trip, or 0.</summary>
    Task<int> LatestVersionAsync(Guid tripRequestId, CancellationToken ct);

    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
}
