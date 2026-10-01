using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Resources.Dtos;

public record RoomTypeDto(Guid Id, Guid HotelId, string Name, int Capacity, decimal RatePerNightLkr, int TotalRooms)
{
    public static RoomTypeDto FromEntity(RoomType r) => new(r.Id, r.HotelId, r.Name, r.Capacity, r.RatePerNightLkr, r.TotalRooms);
}

public record HotelDto(Guid Id, string Name, string City, int StarRating, double Latitude, double Longitude,
    bool IsActive, IReadOnlyList<RoomTypeDto> RoomTypes, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static HotelDto FromEntity(Hotel h) => new(h.Id, h.Name, h.City, h.StarRating, h.Latitude, h.Longitude,
        h.IsActive, h.RoomTypes.OrderBy(r => r.Name).Select(RoomTypeDto.FromEntity).ToList(), h.CreatedAt, h.UpdatedAt);
}

/// <summary>
/// POST /api/hotels and PUT /api/hotels/{id} (v1.1: with the room types in one form). On PUT a row with an Id updates
/// that room type, a row without one is added, and room types missing from the list are deleted.
/// </summary>
public record SaveHotelRequest(string Name, string City, int StarRating, double Latitude, double Longitude, bool IsActive,
    List<HotelRoomTypeRow> RoomTypes);

public record HotelRoomTypeRow(Guid? Id, string Name, int Capacity, decimal RatePerNightLkr, int TotalRooms);

public record SaveRoomTypeRequest(string Name, int Capacity, decimal RatePerNightLkr, int TotalRooms);

/// <summary>GET /api/hotels?search=&amp;city=&amp;minStars=&amp;sort=&amp;page=&amp;pageSize=</summary>
public class HotelListQuery : PagedQuery
{
    public string? City { get; set; }
    public int? MinStars { get; set; }
}
