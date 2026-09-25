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
                c.BaseAddress = new Uri("https://open.er-api.com/"))
            .AddRetryAndTimeout(PerTryTimeout);

        services.AddHttpClient<IDistanceService, DistanceService>(c =>
                c.BaseAddress = new Uri("https://api.openrouteservice.org/"))
            .AddRetryAndTimeout(PerTryTimeout);

        services.AddHttpClient<IWeatherService, WeatherService>(c =>
                c.BaseAddress = new Uri("https://api.openweathermap.org/"))
            .AddRetryAndTimeout(PerTryTimeout)
            .RemoveAllLoggers(); // OWM needs the key in the URL; the default logger would print it

        return services;
    }
}
