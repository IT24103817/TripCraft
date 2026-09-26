using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

/// <summary>A guide's GPS check-in at an itinerary stop. One per stop; the distance is stored as evidence.</summary>
public class StopCheckIn : BaseEntity
{
    public Guid ItineraryStopId { get; set; }
    public Guid GuideId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int DistanceMeters { get; set; }
    public DateTime CheckedInAt { get; set; }
}
