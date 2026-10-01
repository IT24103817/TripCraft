namespace TripCraft.Application.Identity.Dtos;

/// <summary>MustChangePassword: a guide's temporary password must be changed before using the app (v1.1).</summary>
public record UserDto(Guid Id, string Email, string FullName, string Role, bool IsActive, bool MustChangePassword = false)
{
    public static UserDto FromEntity(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString(), user.IsActive, user.MustChangePassword);
}
