namespace TripCraft.Application.Identity.Dtos;

/// <summary>POST /api/auth/change-password: the signed-in user replaces their password (e.g. a temporary one).</summary>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
