namespace TripCraft.Application.Identity.Dtos;

/// <summary>Admin-only: create a user with any role.</summary>
public record CreateUserRequest(string Email, string Password, string FullName, UserRole Role);
