using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Persistence.Repositories;

public class TripRequestRepository(AppDbContext db) : ITripRequestRepository
{
    public IQueryable<TripRequest> Query() =>
        db.TripRequests.AsNoTracking().Include(t => t.Tourist);

    public Task<TripRequest?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.TripRequests.Include(t => t.Tourist).FirstOrDefaultAsync(t => t.Id == id, ct);

    public void Add(TripRequest tripRequest) => db.TripRequests.Add(tripRequest);

    public Task<Tourist?> GetTouristByUserIdAsync(Guid userId, CancellationToken ct) =>
        db.Tourists.FirstOrDefaultAsync(t => t.UserId == userId, ct);

    public void AddTourist(Tourist tourist) => db.Tourists.Add(tourist);

    public Task<Itinerary?> GetItineraryAsync(Guid tripRequestId, CancellationToken ct) =>
        db.Itineraries
            .AsNoTracking()
            .Include(i => i.Days).ThenInclude(d => d.Stops).ThenInclude(s => s.Attraction)
            .FirstOrDefaultAsync(i => i.TripRequestId == tripRequestId, ct);
}
