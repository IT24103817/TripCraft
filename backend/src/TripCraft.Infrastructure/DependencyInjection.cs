using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Identity;
using TripCraft.Infrastructure.Identity;
using TripCraft.Infrastructure.Persistence;
using TripCraft.Infrastructure.Persistence.Repositories;
using TripCraft.Infrastructure.Persistence.Seeding;

namespace TripCraft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, JwtSettings jwtSettings)
    {
        var databaseUrl = configuration["DATABASE_URL"];
        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(databaseUrl))
                throw new InvalidOperationException(
                    "DATABASE_URL is not set. Add it with 'dotnet user-secrets set DATABASE_URL ...' or an environment variable.");

            options.UseNpgsql(ConnectionStringParser.ToNpgsql(databaseUrl))
                   .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<DataSeeder>();

        services.AddSingleton(jwtSettings);
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
