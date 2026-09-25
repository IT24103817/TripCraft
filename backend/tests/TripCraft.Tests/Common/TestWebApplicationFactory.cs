using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.External;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Persistence;
using TripCraft.Tests.Workflows.Fakes;

namespace TripCraft.Tests.Common;

/// <summary>
/// Runs the real API in memory. PostgreSQL is swapped for EF Core InMemory so tests need no database.
/// Each factory gets its own database, so test classes do not see each other's data.
/// The agent service, the third-party APIs and the not-yet-merged Resource (B) and Quotation (C)
/// components are replaced by fakes, so no test touches the network.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string InternalKey = "test-internal-key";

    private readonly string _databaseName = $"tripcraft-tests-{Guid.NewGuid()}";

    /// <summary>Private folder for uploaded passport photos, one per factory.</summary>
    public string UploadsDir { get; } = Path.Combine(Path.GetTempPath(), "tripcraft-tests-uploads", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("JWT_SECRET", "test-secret-that-is-at-least-32-bytes-long!");
        builder.UseSetting("JWT_ISSUER", "tripcraft-tests");
        builder.UseSetting("DATABASE_URL", "Host=unused");
        builder.UseSetting("INTERNAL_AGENT_KEY", InternalKey);
        builder.UseSetting("UPLOADS_DIR", UploadsDir);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            ConfigureDatabase(services);

            services.RemoveAll<IAgentServiceClient>();
            services.AddSingleton<FakeAgentState>();
            services.AddScoped<IAgentServiceClient, FakeAgentServiceClient>();

            services.RemoveAll<IExchangeRateService>();
            services.RemoveAll<IDistanceService>();
            services.RemoveAll<IWeatherService>();
            services.AddScoped<IExchangeRateService, FakeExchangeRateService>();
            services.AddScoped<IDistanceService, FakeDistanceService>();
            services.AddScoped<IWeatherService, FakeWeatherService>();

            services.RemoveAll<IResourceCatalog>();
            services.RemoveAll<IResourceHoldService>();
            services.RemoveAll<IQuotationStore>();
            services.AddSingleton<FakeResourcesState>();
            services.AddSingleton<FakeQuotationsState>();
            services.AddScoped<IResourceCatalog, FakeResourceCatalog>();
            services.AddScoped<IResourceHoldService, FakeResourceHoldService>();
            services.AddScoped<IQuotationStore, FakeQuotationStore>();
        });
    }

    /// <summary>
    /// EF Core InMemory by default. InMemory has no real transactions; the approval flow still works because it
    /// saves once at the end. PostgresWebApplicationFactory overrides this with a real database.
    /// </summary>
    protected virtual void ConfigureDatabase(IServiceCollection services) =>
        services.AddDbContext<AppDbContext>(options => options
            .UseInMemoryDatabase(_databaseName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
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
