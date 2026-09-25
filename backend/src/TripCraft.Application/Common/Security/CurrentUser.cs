using TripCraft.Application.Identity;

namespace TripCraft.Application.Common.Security;

/// <summary>The caller, taken from the JWT by the controller and passed into services.</summary>
public record CurrentUser(Guid Id, UserRole Role)
{
    public bool IsTourist => Role == UserRole.Tourist;
    public bool IsOperationsManager => Role == UserRole.OperationsManager;
}
