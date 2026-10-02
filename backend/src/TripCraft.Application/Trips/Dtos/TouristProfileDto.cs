namespace TripCraft.Application.Trips.Dtos;

/// <summary>GET /api/tourists/me. Empty values until the tourist submits a first trip or uploads a photo.</summary>
public record TouristProfileDto(string Nationality, string PassportNumberMasked, bool HasPassportPhoto);
