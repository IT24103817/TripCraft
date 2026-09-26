using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Quotations;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Quotations;

public class QuotationRepository(AppDbContext db) : IQuotationRepository
{
    public IQueryable<Quotation> Query() => db.Quotations.AsNoTracking().Include(q => q.Lines);

    public IQueryable<ApprovalDecision> Decisions() => db.ApprovalDecisions.AsNoTracking();

    public Task<Quotation?> FindAsync(Guid id, CancellationToken ct) =>
        db.Quotations.Include(q => q.Lines).FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task<int> LatestVersionAsync(Guid tripRequestId, CancellationToken ct)
    {
        var saved = await db.Quotations.Where(q => q.TripRequestId == tripRequestId)
            .Select(q => (int?)q.Version).MaxAsync(ct) ?? 0;
        // Versions staged in this request count too (a proposal adds at most one, but be safe).
        var staged = db.ChangeTracker.Entries<Quotation>()
            .Where(e => e.State == EntityState.Added && e.Entity.TripRequestId == tripRequestId)
            .Select(e => e.Entity.Version).DefaultIfEmpty(0).Max();
        return Math.Max(saved, staged);
    }

    public void Add<T>(T entity) where T : class => db.Add(entity);
    public void Remove<T>(T entity) where T : class => db.Remove(entity);
}
