using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Templates;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Trips;

public class TripTemplateRepository(AppDbContext db) : ITripTemplateRepository
{
    public Task<List<TripTemplate>> ListActiveAsync(CancellationToken ct) =>
        db.TripTemplates.AsNoTracking().Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync(ct);

    public Task<TripTemplate?> GetActiveAsync(Guid id, CancellationToken ct) =>
        db.TripTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && t.IsActive, ct);
}
