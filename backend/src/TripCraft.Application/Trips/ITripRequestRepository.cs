namespace TripCraft.Application.Trips;

public interface ITripRequestRepository
{
    /// <summary>Base query for list screens. Includes the tourist so ownership can be filtered.</summary>
    IQueryable<TripRequest> Query();

    /// <summary>Loads a trip request with its tourist, tracked for updates.</summary>
    Task<TripRequest?> GetByIdAsync(Guid id, CancellationToken ct);

    void Add(TripRequest tripRequest);

    Task<Tourist?> GetTouristByUserIdAsync(Guid userId, CancellationToken ct);
    void AddTourist(Tourist tourist);

    /// <summary>True when the trip already has a saved itinerary.</summary>
    Task<bool> HasItineraryAsync(Guid tripRequestId, CancellationToken ct);

    /// <summary>Stages a new saved itinerary (with days and stops); the caller commits.</summary>
    void AddItinerary(Itinerary itinerary);

    /// <summary>Loads the itinerary with days, stops and attractions, ordered for display.</summary>
    Task<Itinerary?> GetItineraryAsync(Guid tripRequestId, CancellationToken ct);

    /// <summary>Loads the itinerary with its days and stops, tracked, for the itinerary editor.</summary>
    Task<Itinerary?> GetItineraryForUpdateAsync(Guid tripRequestId, CancellationToken ct);

    /// <summary>Stages a new stop of an existing itinerary day (it has a client-side id, so it must be added).</summary>
    void AddItineraryStop(ItineraryStop stop);
}
