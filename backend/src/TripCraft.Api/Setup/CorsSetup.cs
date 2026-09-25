namespace TripCraft.Api.Setup;

public static class CorsSetup
{
    public const string PolicyName = "frontend";

    /// <summary>ALLOWED_ORIGINS is a comma-separated list, e.g. "http://localhost:5173,https://tripcraft.vercel.app".</summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = (configuration["ALLOWED_ORIGINS"] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options => options.AddPolicy(PolicyName, policy =>
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
        return services;
    }
}
