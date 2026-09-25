namespace TripCraft.Application.Identity.Dtos;

public record RegisterRequest(string Email, string Password, string FullName);
