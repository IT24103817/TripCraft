using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Trips;

/// <summary>One itinerary per trip request. Re-planning bumps Version instead of adding a row.</summary>
public class Itinerary : BaseEntity
{
    public Guid TripRequestId { get; set; }
    public TripRequest? TripRequest { get; set; }

    public int Version { get; set; } = 1;
    public ItinerarySource GeneratedBy { get; set; }

    public List<ItineraryDay> Days { get; set; } = [];
}
