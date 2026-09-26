using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Trips;

public class ItineraryDay : BaseEntity
{
    public Guid ItineraryId { get; set; }
    public int DayNumber { get; set; }
    public string City { get; set; } = string.Empty;

    // FK to hotels(id), configured by Resource Management (Infrastructure/Resources/HotelConfiguration).
    public Guid? HotelId { get; set; }

    public string? Notes { get; set; }

    public List<ItineraryStop> Stops { get; set; } = [];
}
