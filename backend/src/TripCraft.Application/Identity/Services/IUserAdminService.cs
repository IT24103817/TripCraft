using TripCraft.Application.Identity.Dtos;

namespace TripCraft.Application.Identity.Services;

public interface IUserAdminService
{
    Task<List<UserDto>> ListAsync(CancellationToken ct);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct);
    Task DeactivateAsync(Guid userId, CancellationToken ct);
}
