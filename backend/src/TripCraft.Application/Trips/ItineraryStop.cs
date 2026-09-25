using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Trips;

public class ItineraryStop : BaseEntity
{
    public Guid ItineraryDayId { get; set; }

    public Guid AttractionId { get; set; }
    public Attraction? Attraction { get; set; }

    /// <summary>Visit order within the day, starting at 1.</summary>
    public int Sequence { get; set; }

    public TimeOnly? ArrivalTime { get; set; }
}
