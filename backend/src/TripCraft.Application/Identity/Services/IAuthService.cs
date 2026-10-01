using TripCraft.Application.Identity.Dtos;

namespace TripCraft.Application.Identity.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct);
    Task<UserDto> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct);
}
