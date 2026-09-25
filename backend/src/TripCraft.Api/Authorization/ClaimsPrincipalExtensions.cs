using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Identity;

namespace TripCraft.Api.Authorization;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the user id from the JWT "sub" claim.</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(sub, out var id)
            ? id
            : throw new InvalidOperationException("Token has no valid 'sub' claim.");
    }

    /// <summary>Id and role from the JWT, in the form services expect.</summary>
    public static CurrentUser GetCurrentUser(this ClaimsPrincipal user)
    {
        var role = Enum.Parse<UserRole>(user.FindFirstValue("role")!);
        return new CurrentUser(user.GetUserId(), role);
    }
}
