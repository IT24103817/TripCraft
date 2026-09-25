namespace TripCraft.Application.Identity.Dtos;

public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);
