using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources;

/// <summary>Data access for Resource Management. Query methods are read-only; Add stages a row, the caller commits.</summary>
public interface IResourceRepository
{
    IQueryable<Guide> Guides();          // not deleted, languages included
    IQueryable<Vehicle> Vehicles();      // not deleted
    IQueryable<Hotel> Hotels();          // not deleted, room types included
    IQueryable<RoomType> RoomTypes();    // of hotels that are not deleted, hotel included
    IQueryable<ResourceHold> Holds();    // every status
    IQueryable<StopCheckIn> CheckIns();

    Task<Guide?> FindGuideAsync(Guid id, CancellationToken ct);       // tracked
    Task<Guide?> FindGuideByUserAsync(Guid userId, CancellationToken ct);
    Task<Vehicle?> FindVehicleAsync(Guid id, CancellationToken ct);   // tracked
    Task<Hotel?> FindHotelAsync(Guid id, CancellationToken ct);       // tracked, with room types
    Task<ResourceHold?> FindHoldAsync(Guid id, CancellationToken ct); // tracked

    /// <summary>Held quantity of a resource in [from, to], counting rows staged in this request too.</summary>
    Task<int> HeldQuantityAsync(ResourceType type, Guid resourceId, DateOnly from, DateOnly to, CancellationToken ct);

    /// <summary>Locks the room type row until the transaction ends, so two approvals cannot oversell a night.</summary>
    Task LockRoomTypeAsync(Guid roomTypeId, CancellationToken ct);

    Task<decimal> CurrentMarginPctAsync(DateOnly today, CancellationToken ct);

    /// <summary>An itinerary stop with its trip and the attraction's position (for GPS check-in).</summary>
    Task<StopLocation?> FindStopAsync(Guid stopId, CancellationToken ct);

    /// <summary>Number of stops in the trip's saved itinerary.</summary>
    Task<int> CountStopsOfTripAsync(Guid tripRequestId, CancellationToken ct);

    /// <summary>Number of saved check-ins at the stops of the trip's itinerary.</summary>
    Task<int> CountCheckInsOfTripAsync(Guid tripRequestId, CancellationToken ct);

    /// <summary>Holds added in this request and not saved yet.</summary>
    IReadOnlyList<ResourceHold> StagedHolds();

    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
}

public record StopLocation(Guid StopId, Guid TripRequestId, string AttractionName, double Latitude, double Longitude);
