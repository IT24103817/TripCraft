using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Infrastructure.Persistence;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Shared.Database;

/// <summary>The real API on a real, already migrated PostgreSQL database (same fakes for agents and B/C ports).</summary>
public class PostgresWebApplicationFactory(string connectionString) : TestWebApplicationFactory
{
    protected override void ConfigureDatabase(IServiceCollection services) =>
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
}
