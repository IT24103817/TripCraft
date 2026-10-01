using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface IHotelService
{
    Task<PagedResult<HotelDto>> ListAsync(HotelListQuery query, CancellationToken ct);
    Task<HotelDto> GetAsync(Guid id, CancellationToken ct);
    Task<HotelDto> CreateAsync(CurrentUser user, SaveHotelRequest request, CancellationToken ct);
    Task<HotelDto> UpdateAsync(CurrentUser user, Guid id, SaveHotelRequest request, CancellationToken ct);
    Task DeleteAsync(CurrentUser user, Guid id, CancellationToken ct);
    Task<RoomTypeDto> AddRoomTypeAsync(CurrentUser user, Guid hotelId, SaveRoomTypeRequest request, CancellationToken ct);
    Task<RoomTypeDto> UpdateRoomTypeAsync(CurrentUser user, Guid hotelId, Guid roomTypeId, SaveRoomTypeRequest request, CancellationToken ct);
    Task DeleteRoomTypeAsync(CurrentUser user, Guid hotelId, Guid roomTypeId, CancellationToken ct);
}

/// <summary>Hotels and their room types (Component B). Room types with upcoming holds cannot be removed or shrunk below them.</summary>
public class HotelService(IResourceRepository resources, IAuditLogger audit, IUnitOfWork unitOfWork) : IHotelService
{
    public static readonly IReadOnlyDictionary<string, Expression<Func<Hotel, object>>> SortableFields =
        new Dictionary<string, Expression<Func<Hotel, object>>>
        {
            ["name"] = h => h.Name,
            ["city"] = h => h.City,
            ["starRating"] = h => h.StarRating
        };

    public async Task<PagedResult<HotelDto>> ListAsync(HotelListQuery query, CancellationToken ct)
    {
        var q = resources.Hotels();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(h => h.Name.ToLower().Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(query.City))
            q = q.Where(h => h.City.ToLower() == query.City.Trim().ToLower());
        if (query.MinStars.HasValue)
            q = q.Where(h => h.StarRating >= query.MinStars.Value);

        return await q.ApplySort(query.Sort, SortableFields, "name")
            .ToPagedResultAsync(query.Page, query.PageSize, HotelDto.FromEntity, ct);
    }

    public async Task<HotelDto> GetAsync(Guid id, CancellationToken ct) => HotelDto.FromEntity(await LoadAsync(id, ct));

    public async Task<HotelDto> CreateAsync(CurrentUser user, SaveHotelRequest request, CancellationToken ct)
    {
        var hotel = new Hotel();
        Apply(hotel, request);
        hotel.RoomTypes = request.RoomTypes.Select(row => ToRoomType(hotel.Id, row)).ToList();
        resources.Add(hotel);
        audit.Record(user.Id, "HotelCreated", nameof(Hotel), hotel.Id, null, HotelDto.FromEntity(hotel));
        await unitOfWork.SaveChangesAsync(ct);
        return HotelDto.FromEntity(hotel);
    }

    public async Task<HotelDto> UpdateAsync(CurrentUser user, Guid id, SaveHotelRequest request, CancellationToken ct)
    {
        var hotel = await LoadAsync(id, ct);
        var before = HotelDto.FromEntity(hotel);
        Apply(hotel, request);
        await SyncRoomTypesAsync(hotel, request.RoomTypes, ct);
        audit.Record(user.Id, "HotelUpdated", nameof(Hotel), hotel.Id, before, HotelDto.FromEntity(hotel));
        await unitOfWork.SaveChangesAsync(ct);
        return HotelDto.FromEntity(hotel);
    }

    public async Task DeleteAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var hotel = await LoadAsync(id, ct);
        foreach (var roomType in hotel.RoomTypes)
            await EnsureNoUpcomingHoldsAsync(roomType.Id, ct);
        hotel.IsDeleted = true;
        hotel.IsActive = false;
        audit.Record(user.Id, "HotelDeleted", nameof(Hotel), hotel.Id, HotelDto.FromEntity(hotel), null);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<RoomTypeDto> AddRoomTypeAsync(CurrentUser user, Guid hotelId, SaveRoomTypeRequest request, CancellationToken ct)
    {
        var hotel = await LoadAsync(hotelId, ct);
        EnsureUniqueRoomTypeName(hotel, request.Name, null);
        var roomType = new RoomType { HotelId = hotel.Id };
        Apply(roomType, request);
        // Added explicitly (the Guid key is set, so EF would otherwise treat it as existing);
        // EF's relationship fix-up then puts it into hotel.RoomTypes.
        resources.Add(roomType);
        audit.Record(user.Id, "RoomTypeCreated", nameof(RoomType), roomType.Id, null, RoomTypeDto.FromEntity(roomType));
        await unitOfWork.SaveChangesAsync(ct);
        return RoomTypeDto.FromEntity(roomType);
    }

    public async Task<RoomTypeDto> UpdateRoomTypeAsync(CurrentUser user, Guid hotelId, Guid roomTypeId,
        SaveRoomTypeRequest request, CancellationToken ct)
    {
        var hotel = await LoadAsync(hotelId, ct);
        var roomType = hotel.RoomTypes.FirstOrDefault(r => r.Id == roomTypeId)
                       ?? throw new NotFoundException("Room type not found.");
        EnsureUniqueRoomTypeName(hotel, request.Name, roomTypeId);

        await EnsureTotalCoversHoldsAsync(roomTypeId, request.TotalRooms, ct);

        var before = RoomTypeDto.FromEntity(roomType);
        Apply(roomType, request);
        audit.Record(user.Id, "RoomTypeUpdated", nameof(RoomType), roomType.Id, before, RoomTypeDto.FromEntity(roomType));
        await unitOfWork.SaveChangesAsync(ct);
        return RoomTypeDto.FromEntity(roomType);
    }

    public async Task DeleteRoomTypeAsync(CurrentUser user, Guid hotelId, Guid roomTypeId, CancellationToken ct)
    {
        var hotel = await LoadAsync(hotelId, ct);
        var roomType = hotel.RoomTypes.FirstOrDefault(r => r.Id == roomTypeId)
                       ?? throw new NotFoundException("Room type not found.");
        await EnsureCanDeleteAsync(roomTypeId, ct);

        hotel.RoomTypes.Remove(roomType);
        resources.Remove(roomType);
        audit.Record(user.Id, "RoomTypeDeleted", nameof(RoomType), roomType.Id, RoomTypeDto.FromEntity(roomType), null);
        await unitOfWork.SaveChangesAsync(ct);
    }

    /// <summary>
    /// The hotel form's room-types table (v1.1): rows with an id update, rows without one are added, missing ones are
    /// deleted — each with the same rules as the single room-type endpoints.
    /// </summary>
    private async Task SyncRoomTypesAsync(Hotel hotel, List<HotelRoomTypeRow> rows, CancellationToken ct)
    {
        foreach (var removed in hotel.RoomTypes.Where(r => rows.All(row => row.Id != r.Id)).ToList())
        {
            await EnsureCanDeleteAsync(removed.Id, ct);
            hotel.RoomTypes.Remove(removed);
            resources.Remove(removed);
        }
        foreach (var row in rows)
        {
            var existing = row.Id is { } id ? hotel.RoomTypes.FirstOrDefault(r => r.Id == id) : null;
            if (row.Id is not null && existing is null)
                throw new NotFoundException($"Room type {row.Id} is not part of {hotel.Name}.");
            if (existing is null)
            {
                resources.Add(ToRoomType(hotel.Id, row)); // EF fix-up adds it to hotel.RoomTypes
                continue;
            }
            await EnsureTotalCoversHoldsAsync(existing.Id, row.TotalRooms, ct);
            Apply(existing, new SaveRoomTypeRequest(row.Name, row.Capacity, row.RatePerNightLkr, row.TotalRooms));
        }
    }

    /// <summary>Business rule: never fewer rooms than are already held on any upcoming night.</summary>
    private async Task EnsureTotalCoversHoldsAsync(Guid roomTypeId, int totalRooms, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var busiestNight = await resources.Holds()
            .Where(h => h.ResourceType == ResourceType.Room && h.ResourceId == roomTypeId
                        && h.Status == HoldStatus.Held && h.ToDate >= today)
            .GroupBy(h => h.FromDate).Select(g => g.Sum(h => h.Quantity))
            .OrderByDescending(n => n).FirstOrDefaultAsync(ct);
        if (totalRooms < busiestNight)
            throw new ConflictException($"{busiestNight} rooms of this type are already held on one night; total cannot go below that.");
    }

    private async Task EnsureCanDeleteAsync(Guid roomTypeId, CancellationToken ct)
    {
        await EnsureNoUpcomingHoldsAsync(roomTypeId, ct);
        if (await resources.Holds().AnyAsync(h => h.ResourceType == ResourceType.Room && h.ResourceId == roomTypeId, ct))
            throw new ConflictException("This room type has booking history; set the hotel inactive instead of deleting it.");
    }

    private static RoomType ToRoomType(Guid hotelId, HotelRoomTypeRow row)
    {
        var roomType = new RoomType { HotelId = hotelId };
        Apply(roomType, new SaveRoomTypeRequest(row.Name, row.Capacity, row.RatePerNightLkr, row.TotalRooms));
        return roomType;
    }

    private async Task<Hotel> LoadAsync(Guid id, CancellationToken ct) =>
        await resources.FindHotelAsync(id, ct) ?? throw new NotFoundException("Hotel not found.");

    private async Task EnsureNoUpcomingHoldsAsync(Guid roomTypeId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (await resources.Holds().AnyAsync(h => h.ResourceType == ResourceType.Room && h.ResourceId == roomTypeId
                                                  && h.Status == HoldStatus.Held && h.ToDate >= today, ct))
            throw new ConflictException("Rooms of this type are held for an upcoming trip.");
    }

    private static void EnsureUniqueRoomTypeName(Hotel hotel, string name, Guid? excludeId)
    {
        if (hotel.RoomTypes.Any(r => r.Id != excludeId && string.Equals(r.Name, name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"{hotel.Name} already has a room type called {name.Trim()}.");
    }

    private static void Apply(Hotel hotel, SaveHotelRequest request)
    {
        hotel.Name = request.Name.Trim();
        hotel.City = request.City.Trim();
        hotel.StarRating = request.StarRating;
        hotel.Latitude = request.Latitude;
        hotel.Longitude = request.Longitude;
        hotel.IsActive = request.IsActive;
    }

    private static void Apply(RoomType roomType, SaveRoomTypeRequest request)
    {
        roomType.Name = request.Name.Trim();
        roomType.Capacity = request.Capacity;
        roomType.RatePerNightLkr = request.RatePerNightLkr;
        roomType.TotalRooms = request.TotalRooms;
    }
}
