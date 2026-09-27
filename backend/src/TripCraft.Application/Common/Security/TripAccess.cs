using TripCraft.Application.Common.Exceptions;

namespace TripCraft.Application.Common.Security;

/// <summary>
/// Resource-based rule shared by every component: a tourist may only use their own trip request and what belongs
/// to it (workflow, quotation). Staff are limited by their role in the controllers instead.
/// </summary>
public static class TripAccess
{
    /// <summary>403 when a tourist asks for a trip owned by another user.</summary>
    public static void EnsureCanAccess(CurrentUser user, Guid? tripOwnerUserId)
    {
        if (user.IsTourist && tripOwnerUserId != user.Id)
            throw new ForbiddenException("You can only access your own trip requests.");
    }
}
