using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Workflows.External;

namespace TripCraft.Infrastructure.External;

public static class ExternalServicesSetup
{
    /// <summary>PLAN.md section 9: 5 s per try, one retry, then each service's own fallback.</summary>
    public static readonly TimeSpan PerTryTimeout = TimeSpan.FromSeconds(5);

    public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddHttpClient<IExchangeRateService, ExchangeRateService>(c =>
                c.BaseAddress = BaseUrl(configuration, "FX_API_BASE_URL", "https://open.er-api.com/"))
            .AddRetryAndTimeout(PerTryTimeout);

        services.AddHttpClient<IDistanceService, DistanceService>(c =>
                c.BaseAddress = BaseUrl(configuration, "ORS_API_BASE_URL", "https://api.openrouteservice.org/"))
            .AddRetryAndTimeout(PerTryTimeout);

        services.AddHttpClient<IWeatherService, WeatherService>(c =>
                c.BaseAddress = BaseUrl(configuration, "OWM_API_BASE_URL", "https://api.openweathermap.org/"))
            .AddRetryAndTimeout(PerTryTimeout)
            .RemoveAllLoggers(); // OWM needs the key in the URL; the default logger would print it

        return services;
    }

    /// <summary>
    /// The provider's real URL unless an override is set (FX_API_BASE_URL, ORS_API_BASE_URL, OWM_API_BASE_URL).
    /// Overrides exist so the fallbacks can be tested by pointing a provider at an unreachable host.
    /// </summary>
    public static Uri BaseUrl(IConfiguration configuration, string name, string defaultUrl)
    {
        var configured = configuration[name];
        return new Uri(string.IsNullOrWhiteSpace(configured) ? defaultUrl : configured);
    }
}
