using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Common;

/// <summary>
/// Runs the real API in memory. PostgreSQL is swapped for EF Core InMemory so tests need no database.
/// Each factory gets its own database, so test classes do not see each other's data.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"tripcraft-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("JWT_SECRET", "test-secret-that-is-at-least-32-bytes-long!");
        builder.UseSetting("JWT_ISSUER", "tripcraft-tests");
        builder.UseSetting("DATABASE_URL", "Host=unused");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}

internal static class ServiceCollectionExtensions
{
    public static void RemoveAll<T>(this IServiceCollection services)
    {
        var matches = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in matches)
            services.Remove(descriptor);
    }
}
