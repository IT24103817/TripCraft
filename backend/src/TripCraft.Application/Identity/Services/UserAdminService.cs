using Microsoft.AspNetCore.Identity;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Identity.Dtos;

namespace TripCraft.Application.Identity.Services;

public class UserAdminService(
    IUserRepository users,
    IPasswordHasher<User> passwordHasher) : IUserAdminService
{
    public async Task<List<UserDto>> ListAsync(CancellationToken ct)
    {
        var all = await users.ListAsync(ct);
        return all.Select(UserDto.FromEntity).ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        var email = AuthService.NormaliseEmail(request.Email);
        if (await users.EmailExistsAsync(email, ct))
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            Role = request.Role,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await users.AddAsync(user, ct);
        await users.SaveChangesAsync(ct);
        return UserDto.FromEntity(user);
    }

    public async Task DeactivateAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException("User not found.");
        user.IsActive = false;
        await users.SaveChangesAsync(ct);
    }
}
