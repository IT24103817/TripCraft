using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Workflows.External;

namespace TripCraft.Infrastructure.External;

/// <summary>
/// open.er-api.com (no key). A fresh rate is cached for 1 hour. On failure the last known rate is
/// returned with Stale = true (PLAN.md section 9). Before any success, FX_FALLBACK_LKR_PER_USD
/// (default 300) is used, also flagged stale, so a quotation can still be drafted and reviewed.
/// </summary>
public class ExchangeRateService(
    HttpClient http, IMemoryCache cache, IConfiguration configuration, ILogger<ExchangeRateService> logger)
    : IExchangeRateService
{
    public const string FreshKey = "fx:usd-lkr:fresh";
    public const string LastKnownKey = "fx:usd-lkr:last";
    public static readonly TimeSpan CacheFor = TimeSpan.FromHours(1);
    private const decimal DefaultFallbackRate = 300m;

    public async Task<ExchangeRate> GetUsdToLkrAsync(CancellationToken ct)
    {
        if (cache.TryGetValue(FreshKey, out ExchangeRate? fresh) && fresh is not null)
            return fresh;

        try
        {
            var body = await http.GetFromJsonAsync<ErApiResponse>("v6/latest/USD", ct);
            if (body?.Result != "success" || body.Rates is null || !body.Rates.TryGetValue("LKR", out var lkr) || lkr <= 0)
                throw new InvalidOperationException("Response has no LKR rate.");

            var rate = new ExchangeRate("USD", "LKR", lkr,
                DateTimeOffset.FromUnixTimeSeconds(body.TimeLastUpdateUnix).UtcDateTime, Stale: false);
            cache.Set(FreshKey, rate, CacheFor);
            cache.Set(LastKnownKey, rate); // no expiry: this is the fallback
            return rate;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Exchange rate provider failed ({ErrorType}); using the last known rate", ex.GetType().Name);
            if (cache.TryGetValue(LastKnownKey, out ExchangeRate? last) && last is not null)
                return last with { Stale = true };

            var fallback = decimal.TryParse(configuration["FX_FALLBACK_LKR_PER_USD"], out var configured) && configured > 0
                ? configured
                : DefaultFallbackRate;
            // AsOf = Unix epoch means "unknown": no real rate has been fetched since start-up.
            return new ExchangeRate("USD", "LKR", fallback, DateTime.UnixEpoch, Stale: true);
        }
    }

    private record ErApiResponse(
        [property: JsonPropertyName("result")] string? Result,
        [property: JsonPropertyName("time_last_update_unix")] long TimeLastUpdateUnix,
        [property: JsonPropertyName("rates")] Dictionary<string, decimal>? Rates);
}
