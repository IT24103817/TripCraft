using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Workflows.Fakes;

/// <summary>
/// Stand-in for Resource Management (Student B) until it is merged. One shared state per test factory.
/// Holds are only "committed" when the real DbContext saves, like an EF entity would be.
/// </summary>
public class FakeResourcesState
{
    public static readonly Guid GuideEn = Guid.Parse("00000000-0000-0000-0000-00000000a001");
    public static readonly Guid VanSixSeats = Guid.Parse("00000000-0000-0000-0000-00000000b001");
    public static readonly Guid CarThreeSeats = Guid.Parse("00000000-0000-0000-0000-00000000b002");
    public static readonly Guid KandyHotel = Guid.Parse("00000000-0000-0000-0000-00000000c001");
    public static readonly Guid KandyStandard = Guid.Parse("00000000-0000-0000-0000-00000000c011");
    public static readonly Guid EllaHotel = Guid.Parse("00000000-0000-0000-0000-00000000c002");
    public static readonly Guid EllaStandard = Guid.Parse("00000000-0000-0000-0000-00000000c021");

    public List<GuideOption> Guides { get; } = [new(GuideEn, "Nimal Perera", ["en", "si"], 10)];

    public List<VehicleOption> Vehicles { get; } =
    [
        new(VanSixSeats, "CAB-1234", "Van", 6),
        new(CarThreeSeats, "CAR-9876", "Car", 3)
    ];

    public List<(string City, RoomOption Room)> Rooms { get; } =
    [
        ("Kandy", new(KandyHotel, "Kandy Hills", KandyStandard, "Standard Double", 2, 5)),
        ("Ella", new(EllaHotel, "Ella Gap", EllaStandard, "Standard Double", 2, 5))
    ];

    public RateCard RateCard { get; } = new(15m,
        new Dictionary<Guid, decimal> { [GuideEn] = 6000m },
        new Dictionary<Guid, decimal> { [VanSixSeats] = 120m, [CarThreeSeats] = 100m },
        new Dictionary<Guid, decimal> { [KandyStandard] = 12000m, [EllaStandard] = 12000m });

    /// <summary>Committed holds.</summary>
    public List<ResourceHoldRequest> Holds { get; } = [];

    public bool Overlaps(ResourceType type, Guid id, DateOnly from, DateOnly to, IEnumerable<ResourceHoldRequest>? extra = null) =>
        Holds.Concat(extra ?? []).Any(h => h.Type == type && h.ResourceId == id && h.From <= to && from <= h.To
                                          && type != ResourceType.Room);
}

public class FakeResourceCatalog(FakeResourcesState state) : IResourceCatalog
{
    public Task<IReadOnlyList<GuideOption>> FindAvailableGuidesAsync(DateOnly from, DateOnly to, string language, int pax,
        CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<GuideOption>>(state.Guides
            .Where(g => g.Languages.Contains(language) && g.MaxPax >= pax && !state.Overlaps(ResourceType.Guide, g.Id, from, to))
            .ToList());

    public Task<IReadOnlyList<VehicleOption>> FindAvailableVehiclesAsync(DateOnly from, DateOnly to, int seats,
        CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<VehicleOption>>(state.Vehicles
            .Where(v => v.Seats >= seats && !state.Overlaps(ResourceType.Vehicle, v.Id, from, to)).ToList());

    public Task<IReadOnlyList<RoomOption>> FindAvailableRoomsAsync(string city, DateOnly night, int rooms, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<RoomOption>>(state.Rooms
            .Where(r => string.Equals(r.City, city, StringComparison.OrdinalIgnoreCase) && r.Room.AvailableRooms >= rooms)
            .Select(r => r.Room).ToList());

    public Task<RateCard> GetRateCardAsync(CancellationToken ct) => Task.FromResult(state.RateCard);

    public Task<GuideOption?> GetGuideAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(state.Guides.FirstOrDefault(g => g.Id == id));

    public Task<VehicleOption?> GetVehicleAsync(Guid id, CancellationToken ct) =>
        Task.FromResult(state.Vehicles.FirstOrDefault(v => v.Id == id));

    public Task<RoomOption?> GetRoomTypeAsync(Guid roomTypeId, CancellationToken ct) =>
        Task.FromResult(state.Rooms.Select(r => r.Room).FirstOrDefault(r => r.RoomTypeId == roomTypeId));

    public Task<bool> HasOverlappingHoldAsync(ResourceType type, Guid resourceId, DateOnly from, DateOnly to,
        CancellationToken ct) => Task.FromResult(state.Overlaps(type, resourceId, from, to));
}

/// <summary>Stages holds; they become visible in the shared state only when the DbContext saves.</summary>
public class FakeResourceHoldService : IResourceHoldService
{
    private readonly FakeResourcesState _state;
    private readonly List<ResourceHoldRequest> _pending = [];

    public FakeResourceHoldService(FakeResourcesState state, AppDbContext db)
    {
        _state = state;
        db.SavedChanges += (_, _) =>
        {
            _state.Holds.AddRange(_pending);
            _pending.Clear();
        };
    }

    public Task CreateHoldAsync(ResourceHoldRequest hold, CancellationToken ct)
    {
        if (_state.Overlaps(hold.Type, hold.ResourceId, hold.From, hold.To, _pending))
            throw new ConflictException($"{hold.Type} {hold.ResourceId} is already held between {hold.From} and {hold.To}.");
        _pending.Add(hold);
        return Task.CompletedTask;
    }
}
