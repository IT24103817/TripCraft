namespace TripCraft.Application.Trips;

public interface IAttractionRepository
{
    /// <summary>Attractions that are not soft-deleted.</summary>
    IQueryable<Attraction> QueryActive();

    Task<Attraction?> GetActiveByIdAsync(Guid id, CancellationToken ct);

    /// <summary>True if another active attraction already has this name in this city.</summary>
    Task<bool> NameExistsInCityAsync(string name, string city, Guid? excludeId, CancellationToken ct);

    Task<List<string>> ListActiveCitiesAsync(CancellationToken ct);

    void Add(Attraction attraction);
}
