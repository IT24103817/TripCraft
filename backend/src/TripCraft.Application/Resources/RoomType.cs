using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

/// <summary>A kind of room in a hotel. TotalRooms is how many exist; holds reserve some of them per night.</summary>
public class RoomType : BaseEntity
{
    public Guid HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal RatePerNightLkr { get; set; }
    public int TotalRooms { get; set; }
}
