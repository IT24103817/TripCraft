namespace TripCraft.Application.Resources.Dtos;

/// <summary>GET /api/guides/me/schedule and /api/guides/{id}/schedule: the guide's held trips with their days.</summary>
public record GuideScheduleDto(Guid GuideId, string GuideName, IReadOnlyList<GuideTripDto> Trips);

public record GuideTripDto(Guid TripRequestId, string Objective, DateOnly StartDate, DateOnly EndDate, int Pax,
    string Status, string? VehicleRegistrationNo, IReadOnlyList<GuideDayDto> Days);

public record GuideDayDto(int DayNumber, DateOnly Date, string City, string? HotelName, IReadOnlyList<GuideStopDto> Stops);

public record GuideStopDto(Guid StopId, int Sequence, string AttractionName, double Latitude, double Longitude,
    DateTime? CheckedInAt);

/// <summary>POST /api/check-ins: the guide's GPS position at a stop.</summary>
public record CheckInRequest(Guid ItineraryStopId, double Latitude, double Longitude);

public record CheckInResultDto(Guid StopId, int DistanceMeters, DateTime CheckedInAt, string TripStatus);
