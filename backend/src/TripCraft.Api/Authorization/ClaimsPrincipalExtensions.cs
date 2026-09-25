using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

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
}
