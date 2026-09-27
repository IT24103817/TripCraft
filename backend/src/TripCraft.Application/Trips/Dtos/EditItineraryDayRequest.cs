namespace TripCraft.Application.Trips.Dtos;

/// <summary>PUT /api/trip-requests/{id}/itinerary/days/{dayNumber}: the day's stops in visiting order, and notes.</summary>
public record EditItineraryDayRequest(IReadOnlyList<Guid> AttractionIds, string? Notes);
