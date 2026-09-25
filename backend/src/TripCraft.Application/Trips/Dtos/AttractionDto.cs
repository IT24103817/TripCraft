namespace TripCraft.Application.Trips.Dtos;

public record AttractionDto(
    Guid Id, string Name, string City, string Category, int DurationMinutes,
    decimal EntryFeeLkr, double Latitude, double Longitude)
{
    public static AttractionDto FromEntity(Attraction a) => new(
        a.Id, a.Name, a.City, a.Category, a.DurationMinutes, a.EntryFeeLkr, a.Latitude, a.Longitude);
}
