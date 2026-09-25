namespace TripCraft.Application.Identity.Dtos;

public record UserDto(Guid Id, string Email, string FullName, string Role, bool IsActive)
{
    public static UserDto FromEntity(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString(), user.IsActive);
}
