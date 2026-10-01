namespace TripCraft.Application.Trips.Templates;

public interface ITripTemplateRepository
{
    /// <summary>Active packages in display order, read-only.</summary>
    Task<List<TripTemplate>> ListActiveAsync(CancellationToken ct);

    Task<TripTemplate?> GetActiveAsync(Guid id, CancellationToken ct);
}
