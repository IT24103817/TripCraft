using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TripCraft.Api.Controllers.Internal;

/// <summary>
/// Protects the internal API used only by the agent service: the X-Internal-Key header must equal
/// INTERNAL_AGENT_KEY, otherwise 401. If INTERNAL_AGENT_KEY is not set, every request is refused.
/// Use together with [AllowAnonymous], because these callers have no JWT.
/// </summary>
public class InternalKeyAuthFilter(IConfiguration configuration) : IAuthorizationFilter
{
    public const string HeaderName = "X-Internal-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var expected = configuration["INTERNAL_AGENT_KEY"];
        var given = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (string.IsNullOrEmpty(expected) || !KeysMatch(given, expected))
            context.Result = new UnauthorizedResult();
    }

    /// <summary>Constant-time comparison so the key cannot be guessed from response timing.</summary>
    private static bool KeysMatch(string given, string expected) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(given), Encoding.UTF8.GetBytes(expected));
}
