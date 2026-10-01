namespace TripCraft.Application.Trips.Dtos;

public record ItineraryDto(Guid Id, Guid TripRequestId, int Version, string GeneratedBy, IReadOnlyList<ItineraryDayDto> Days)
{
    public static ItineraryDto FromEntity(Itinerary i) => new(
        i.Id, i.TripRequestId, i.Version, i.GeneratedBy.ToString(),
        i.Days.OrderBy(d => d.DayNumber).Select(ItineraryDayDto.FromEntity).ToList());
}

public record ItineraryDayDto(int DayNumber, string City, Guid? HotelId, string? Notes, IReadOnlyList<ItineraryStopDto> Stops)
{
    public static ItineraryDayDto FromEntity(ItineraryDay d) => new(
        d.DayNumber, d.City, d.HotelId, d.Notes,
        d.Stops.OrderBy(s => s.Sequence).Select(ItineraryStopDto.FromEntity).ToList());
}

/// <summary>Latitude/Longitude (v1.1) let the app draw the route line between the day's stops.</summary>
public record ItineraryStopDto(int Sequence, TimeOnly? ArrivalTime, Guid AttractionId, string AttractionName, int DurationMinutes,
    double? Latitude = null, double? Longitude = null)
{
    public static ItineraryStopDto FromEntity(ItineraryStop s) => new(
        s.Sequence, s.ArrivalTime, s.AttractionId, s.Attraction?.Name ?? "", s.Attraction?.DurationMinutes ?? 0,
        s.Attraction?.Latitude, s.Attraction?.Longitude);
}
