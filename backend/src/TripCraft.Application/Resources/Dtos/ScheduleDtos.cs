namespace TripCraft.Application.Resources.Dtos;

/// <summary>GET /api/guides/me/schedule and /api/guides/{id}/schedule: the guide's held trips with their days.</summary>
public record GuideScheduleDto(Guid GuideId, string GuideName, IReadOnlyList<GuideTripDto> Trips);

/// <summary>VehicleType and VehicleSeats let the guide look up the trip's vehicle on the phone.</summary>
public record GuideTripDto(Guid TripRequestId, string Objective, DateOnly StartDate, DateOnly EndDate, int Pax,
    string Status, string? VehicleRegistrationNo, IReadOnlyList<GuideDayDto> Days, string? VehicleType = null,
    int? VehicleSeats = null);

public record GuideDayDto(int DayNumber, DateOnly Date, string City, string? HotelName, IReadOnlyList<GuideStopDto> Stops);

public record GuideStopDto(Guid StopId, int Sequence, string AttractionName, double Latitude, double Longitude,
    DateTime? CheckedInAt);

/// <summary>POST /api/check-ins: the guide's GPS position at a stop.</summary>
/// <summary>
/// POST /api/check-ins. Either VoucherCode (the guide scanned the tourist's trip voucher; TripRequestId optionally
/// names the trip the guide expects) or the GPS fields ItineraryStopId + Latitude + Longitude.
/// </summary>
public record CheckInRequest(Guid? ItineraryStopId, double? Latitude, double? Longitude, string? VoucherCode = null,
    Guid? TripRequestId = null);

/// <summary>Method is "Gps" or "Voucher"; DistanceMeters is only set for GPS.</summary>
public record CheckInResultDto(Guid StopId, int? DistanceMeters, DateTime CheckedInAt, string TripStatus, string Method,
    string StopName);
