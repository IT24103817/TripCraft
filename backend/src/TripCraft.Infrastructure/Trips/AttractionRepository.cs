using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Trips;

public class AttractionRepository(AppDbContext db) : IAttractionRepository
{
    public IQueryable<Attraction> QueryActive() =>
        db.Attractions.AsNoTracking().Where(a => !a.IsDeleted);

    public Task<Attraction?> GetActiveByIdAsync(Guid id, CancellationToken ct) =>
        db.Attractions.FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);

    public Task<bool> NameExistsInCityAsync(string name, string city, Guid? excludeId, CancellationToken ct) =>
        db.Attractions.AnyAsync(a =>
            !a.IsDeleted &&
            a.Name.ToLower() == name.ToLower() &&
            a.City.ToLower() == city.ToLower() &&
            (excludeId == null || a.Id != excludeId), ct);

    public Task<List<string>> ListActiveCitiesAsync(CancellationToken ct) =>
        db.Attractions.Where(a => !a.IsDeleted).Select(a => a.City).Distinct().OrderBy(c => c).ToListAsync(ct);

    public void Add(Attraction attraction) => db.Attractions.Add(attraction);
}
