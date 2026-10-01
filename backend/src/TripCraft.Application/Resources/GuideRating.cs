using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

/// <summary>The tourist's 1–5 star rating of the guide after a completed trip (v1.1). One per trip.</summary>
public class GuideRating : BaseEntity
{
    public Guid TripRequestId { get; set; }
    public Guid GuideId { get; set; }
    public Guid RatedByUserId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
}
