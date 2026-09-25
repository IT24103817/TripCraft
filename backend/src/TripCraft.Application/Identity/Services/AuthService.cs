using Microsoft.AspNetCore.Identity;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Identity.Dtos;

namespace TripCraft.Application.Identity.Services;

public class AuthService(
    IUserRepository users,
    IPasswordHasher<User> passwordHasher,
    ITokenService tokenService) : IAuthService
{
    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = NormaliseEmail(request.Email);
        if (await users.EmailExistsAsync(email, ct))
            throw new ConflictException("An account with this email already exists.");

        // Self-registration always creates a Tourist. Other roles are created by an Admin.
        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            Role = UserRole.Tourist,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await users.AddAsync(user, ct);
        await users.SaveChangesAsync(ct);
        return UserDto.FromEntity(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(NormaliseEmail(request.Email), ct);

        // Same message for every failure so the response does not reveal which emails exist.
        if (user is null || !user.IsActive)
            throw new AuthenticationFailedException("Invalid email or password.");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new AuthenticationFailedException("Invalid email or password.");

        var (token, expiresAt) = tokenService.CreateToken(user);
        return new AuthResponse(token, expiresAt, UserDto.FromEntity(user));
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException("User not found.");
        return UserDto.FromEntity(user);
    }

    public static string NormaliseEmail(string email) => email.Trim().ToLowerInvariant();
}
