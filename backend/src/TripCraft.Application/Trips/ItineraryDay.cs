using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Trips;

public class ItineraryDay : BaseEntity
{
    public Guid ItineraryId { get; set; }
    public int DayNumber { get; set; }
    public string City { get; set; } = string.Empty;

    // TODO(Component B): hotels table is owned by Resource Management. Add the FK to hotels(id)
    // once the Hotel entity exists; until then this is a plain nullable uuid column.
    public Guid? HotelId { get; set; }

    public string? Notes { get; set; }

    public List<ItineraryStop> Stops { get; set; } = [];
}
