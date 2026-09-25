namespace TripCraft.Application.Workflows.Ports;

/// <summary>
/// Read-only view of Resource Management (Student B, PLAN.md section 3 Component B) that the
/// workflow needs. Student B implements it on top of the guides, vehicles, hotels, room_types,
/// resource_holds and rate tables. Nothing here creates a hold.
/// </summary>
public interface IResourceCatalog
{
    /// <summary>Active guides who speak the language, take at least pax people and have no hold in the range.</summary>
    Task<IReadOnlyList<GuideOption>> FindAvailableGuidesAsync(DateOnly from, DateOnly to, string language, int pax,
        CancellationToken ct);

    /// <summary>Active vehicles with at least `seats` seats and no hold in the range.</summary>
    Task<IReadOnlyList<VehicleOption>> FindAvailableVehiclesAsync(DateOnly from, DateOnly to, int seats,
        CancellationToken ct);

    /// <summary>Room types in the city with at least `rooms` rooms free that night.</summary>
    Task<IReadOnlyList<RoomOption>> FindAvailableRoomsAsync(string city, DateOnly night, int rooms,
        CancellationToken ct);

    Task<RateCard> GetRateCardAsync(CancellationToken ct);

    Task<GuideOption?> GetGuideAsync(Guid id, CancellationToken ct);
    Task<VehicleOption?> GetVehicleAsync(Guid id, CancellationToken ct);
    Task<RoomOption?> GetRoomTypeAsync(Guid roomTypeId, CancellationToken ct);

    /// <summary>True if a Held hold for this resource overlaps [from, to].</summary>
    Task<bool> HasOverlappingHoldAsync(ResourceType type, Guid resourceId, DateOnly from, DateOnly to,
        CancellationToken ct);
}

public enum ResourceType
{
    Guide,
    Vehicle,
    Room
}

public record GuideOption(Guid Id, string Name, IReadOnlyList<string> Languages, int MaxPax);

public record VehicleOption(Guid Id, string RegistrationNo, string Type, int Seats);

/// <summary>AvailableRooms is the free count for the night asked; 0 when looked up by id.</summary>
public record RoomOption(Guid HotelId, string HotelName, Guid RoomTypeId, string RoomTypeName, int Capacity,
    int AvailableRooms);

/// <summary>LKR rates keyed by resource id, plus the operator margin in percent.</summary>
public record RateCard(
    decimal MarginPct,
    IReadOnlyDictionary<Guid, decimal> GuideDayRates,
    IReadOnlyDictionary<Guid, decimal> VehicleKmRates,
    IReadOnlyDictionary<Guid, decimal> RoomNightRates);
