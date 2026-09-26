using Microsoft.OpenApi.Models;

namespace TripCraft.Api.Setup;

public static class SwaggerSetup
{
    /// <summary>Swagger with an "Authorize" button that takes a JWT bearer token.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "TripCraft API", Version = "v1" });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the accessToken from POST /api/auth/login.",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };
            options.AddSecurityDefinition("Bearer", scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });

            // The internal API (agents only) uses a shared key instead of a JWT.
            options.AddSecurityDefinition("InternalKey", new OpenApiSecurityScheme
            {
                Name = "X-Internal-Key",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "INTERNAL_AGENT_KEY — only for /api/internal/* (the agent service)."
            });
            options.OperationFilter<ProblemDetailsResponsesFilter>();
        });
        return services;
    }
}
