using TripCraft.Application.Identity;

namespace TripCraft.Api.Authorization;

/// <summary>Role names as strings, for use in [Authorize(Roles = ...)].</summary>
public static class Roles
{
    public const string Tourist = nameof(UserRole.Tourist);
    public const string Guide = nameof(UserRole.Guide);
    public const string OperationsManager = nameof(UserRole.OperationsManager);
    public const string Admin = nameof(UserRole.Admin);

    // Comma-separated = "any of these roles".
    public const string TouristOrOperationsManager = Tourist + "," + OperationsManager;
    public const string Staff = OperationsManager + "," + Admin;
    public const string TouristOrStaff = Tourist + "," + OperationsManager + "," + Admin;
    public const string GuideOrOperationsManager = Guide + "," + OperationsManager;
}
