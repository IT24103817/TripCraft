using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

public class Hotel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    public List<RoomType> RoomTypes { get; set; } = [];
}
