using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Identity;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Set for accounts created with a temporary password (guides, v1.1): the app forces a change.</summary>
    public bool MustChangePassword { get; set; }
}
