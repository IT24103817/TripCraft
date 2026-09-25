using Microsoft.AspNetCore.Authorization;

namespace TripCraft.Api.Authorization;

/// <summary>Named policies. Use [Authorize(Policy = Policies.AdminOnly)] on controllers or actions.</summary>
public static class Policies
{
    public const string TouristOnly = nameof(TouristOnly);
    public const string GuideOnly = nameof(GuideOnly);
    public const string OperationsManagerOnly = nameof(OperationsManagerOnly);
    public const string AdminOnly = nameof(AdminOnly);

    public static void Register(AuthorizationOptions options)
    {
        // Every endpoint needs a valid JWT unless it is marked [AllowAnonymous].
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

        options.AddPolicy(TouristOnly, p => p.RequireRole(Roles.Tourist));
        options.AddPolicy(GuideOnly, p => p.RequireRole(Roles.Guide));
        options.AddPolicy(OperationsManagerOnly, p => p.RequireRole(Roles.OperationsManager));
        options.AddPolicy(AdminOnly, p => p.RequireRole(Roles.Admin));
    }
}
