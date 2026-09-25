using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Trips;

public class Attraction : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal EntryFeeLkr { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    /// <summary>
    /// Soft delete (DELETE /api/attractions/{id} is exposed). Deleted attractions stay in old
    /// itineraries, so there is no global query filter — list queries must filter on this themselves.
    /// </summary>
    public bool IsDeleted { get; set; }
}
