using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Workflows.External;
using TripCraft.Infrastructure.Persistence.Notifications;

namespace TripCraft.Infrastructure.External;

public static class ExternalServicesSetup
{
    /// <summary>PLAN.md section 9: 5 s per try, one retry, then each service's own fallback.</summary>
    public static readonly TimeSpan PerTryTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// OpenRouteService on HeiGIT's host. api.openrouteservice.org is deprecated (quota reduced from 24 Aug 2026,
    /// shut down 2–6 Nov 2026); the same keys work at api.heigit.org/openrouteservice/v2/…
    /// </summary>
    public const string DefaultOrsBaseUrl = "https://api.heigit.org/openrouteservice/";

    public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddHttpClient<IExchangeRateService, ExchangeRateService>(c =>
                c.BaseAddress = BaseUrl(configuration, "FX_API_BASE_URL", "https://open.er-api.com/"))
            .AddRetryAndTimeout(PerTryTimeout);

        services.AddHttpClient<IDistanceService, DistanceService>(c => c.BaseAddress = OrsBaseUrl(configuration))
            .AddRetryAndTimeout(PerTryTimeout);

        services.AddHttpClient<IWeatherService, WeatherService>(c =>
                c.BaseAddress = BaseUrl(configuration, "OWM_API_BASE_URL", "https://api.openweathermap.org/"))
            .AddRetryAndTimeout(PerTryTimeout)
            .RemoveAllLoggers(); // OWM needs the key in the URL; the default logger would print it

        // Email (v1.1): SMTP when SMTP_HOST is set; otherwise the Mailtrap sandbox API with the pickup folder as fallback.
        services.AddSingleton<PickupDirectoryEmailSender>();
        if (!string.IsNullOrWhiteSpace(configuration["SMTP_HOST"]))
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        else
            services.AddHttpClient<IEmailSender, MailtrapEmailSender>(c =>
                    c.BaseAddress = BaseUrl(configuration, "MAILTRAP_API_BASE_URL", "https://sandbox.api.mailtrap.io/"))
                .AddRetryAndTimeout(PerTryTimeout);

        return services;
    }

    /// <summary>
    /// The OpenRouteService base URL: ORS_BASE_URL, else the older name ORS_API_BASE_URL, else HeiGIT's host. It
    /// always ends in "/": DistanceService calls the relative path "v2/matrix/driving-car", and without the slash
    /// the "openrouteservice" segment would be replaced instead of kept (HeiGIT's docs show the base without one).
    /// </summary>
    public static Uri OrsBaseUrl(IConfiguration configuration)
    {
        var configured = configuration["ORS_BASE_URL"];
        if (string.IsNullOrWhiteSpace(configured))
            configured = configuration["ORS_API_BASE_URL"];
        var url = string.IsNullOrWhiteSpace(configured) ? DefaultOrsBaseUrl : configured.Trim();
        return new Uri(url.EndsWith('/') ? url : url + "/");
    }

    /// <summary>
    /// The provider's real URL unless an override is set (FX_API_BASE_URL, OWM_API_BASE_URL, MAILTRAP_API_BASE_URL).
    /// Overrides exist so the fallbacks can be tested by pointing a provider at an unreachable host.
    /// </summary>
    public static Uri BaseUrl(IConfiguration configuration, string name, string defaultUrl)
    {
        var configured = configuration[name];
        return new Uri(string.IsNullOrWhiteSpace(configured) ? defaultUrl : configured);
    }
}
