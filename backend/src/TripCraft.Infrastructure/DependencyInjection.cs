using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
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

        return services;
    }
}
