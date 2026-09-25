namespace TripCraft.Application.Trips.Dtos;

/// <summary>Body for both POST and PUT /api/attractions.</summary>
public record SaveAttractionRequest(
    string Name, string City, string Category, int DurationMinutes,
    decimal EntryFeeLkr, double Latitude, double Longitude);
