using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

/// <summary>
/// A guide's check-in at an itinerary stop. One per stop. Gps check-ins store the position and distance as
/// evidence; Voucher check-ins (v1.1, the guide scanned the tourist's signed trip voucher) store the voucher id.
/// </summary>
public class StopCheckIn : BaseEntity
{
    public Guid ItineraryStopId { get; set; }
    public Guid GuideId { get; set; }
    public CheckInMethod Method { get; set; } = CheckInMethod.Gps;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? DistanceMeters { get; set; }
    public Guid? VoucherId { get; set; }
    public DateTime CheckedInAt { get; set; }
}

public enum CheckInMethod
{
    Gps,
    Voucher
}
