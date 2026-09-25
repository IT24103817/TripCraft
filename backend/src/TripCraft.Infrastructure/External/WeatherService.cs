using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Workflows.External;

namespace TripCraft.Infrastructure.External;

/// <summary>
/// OpenWeatherMap 5-day / 3-hour forecast (OWM_API_KEY). Returns null with a logged warning on any
/// failure or when the date is outside the forecast window: weather is advisory (PLAN.md section 9).
/// </summary>
public class WeatherService(HttpClient http, IConfiguration configuration, ILogger<WeatherService> logger)
    : IWeatherService
{
    /// <summary>Sri Lanka is UTC+5:30; forecast slots are grouped by local date.</summary>
    private static readonly TimeSpan SriLankaOffset = TimeSpan.FromMinutes(330);

    public async Task<WeatherForecast?> GetForecastAsync(string city, DateOnly date, CancellationToken ct)
    {
        var key = configuration["OWM_API_KEY"];
        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogWarning("Weather skipped for {City} on {Date}: OWM_API_KEY is not set", city, date);
            return null;
        }

        try
        {
            // The key must go in the query string for OWM, so this client has HTTP logging removed (see setup).
            var url = $"data/2.5/forecast?q={Uri.EscapeDataString(city)},LK&units=metric&appid={Uri.EscapeDataString(key)}";
            var body = await http.GetFromJsonAsync<OwmForecastResponse>(url, ct);

            var slots = (body?.List ?? [])
                .Where(s => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(s.Dt).ToOffset(SriLankaOffset).DateTime) == date)
                .ToList();
            if (slots.Count == 0)
            {
                logger.LogWarning("Weather skipped for {City} on {Date}: outside the 5-day forecast", city, date);
                return null;
            }

            var summary = slots.SelectMany(s => s.Weather ?? []).Select(w => w.Description)
                .GroupBy(d => d).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "unknown";
            return new WeatherForecast(city, date, summary, slots.Max(s => s.Pop));
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Weather provider failed for {City} on {Date} ({ErrorType})", city, date, ex.GetType().Name);
            return null;
        }
    }

    private record OwmForecastResponse([property: JsonPropertyName("list")] List<OwmSlot>? List);

    private record OwmSlot(
        [property: JsonPropertyName("dt")] long Dt,
        [property: JsonPropertyName("pop")] double Pop,
        [property: JsonPropertyName("weather")] List<OwmWeather>? Weather);

    private record OwmWeather([property: JsonPropertyName("description")] string Description);
}
