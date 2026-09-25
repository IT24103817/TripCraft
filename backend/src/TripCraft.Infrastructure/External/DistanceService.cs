using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.External;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.External;

/// <summary>
/// OpenRouteService distance matrix (ORS_API_KEY). City coordinates are the average of the city's
/// active attractions. On any failure, or with no key, the seeded city_distances table is used (PLAN.md section 9).
/// </summary>
public class DistanceService(
    HttpClient http, AppDbContext db, IAttractionRepository attractions, IConfiguration configuration,
    ILogger<DistanceService> logger) : IDistanceService
{
    public async Task<DistanceResult?> GetDistanceAsync(string fromCity, string toCity, CancellationToken ct)
    {
        var key = configuration["ORS_API_KEY"];
        if (!string.IsNullOrWhiteSpace(key))
        {
            try
            {
                var fromProvider = await FromProviderAsync(fromCity, toCity, key, ct);
                if (fromProvider is not null)
                    return fromProvider;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                logger.LogWarning("OpenRouteService failed for {From} -> {To} ({ErrorType}); using the static table",
                    fromCity, toCity, ex.GetType().Name);
            }
        }

        return await FromStaticTableAsync(fromCity, toCity, ct);
    }

    private async Task<DistanceResult?> FromProviderAsync(string fromCity, string toCity, string key, CancellationToken ct)
    {
        var from = await CityCentreAsync(fromCity, ct);
        var to = await CityCentreAsync(toCity, ct);
        if (from is null || to is null)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Post, "v2/matrix/driving-car")
        {
            // ORS wants [longitude, latitude].
            Content = JsonContent.Create(new
            {
                locations = new[] { new[] { from.Value.Lon, from.Value.Lat }, new[] { to.Value.Lon, to.Value.Lat } },
                metrics = new[] { "distance", "duration" },
                units = "km"
            })
        };
        request.Headers.TryAddWithoutValidation("Authorization", key); // header, never the URL, so it is not logged

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var matrix = await response.Content.ReadFromJsonAsync<OrsMatrixResponse>(ct);
        var km = matrix?.Distances?[0][1];
        var seconds = matrix?.Durations?[0][1];
        if (km is null || seconds is null)
            throw new InvalidOperationException("Route not found in the matrix.");

        return new DistanceResult(fromCity, toCity, Math.Round(km.Value, 1), (int)Math.Round(seconds.Value / 60),
            "openrouteservice");
    }

    private async Task<(double Lat, double Lon)?> CityCentreAsync(string city, CancellationToken ct)
    {
        var points = await attractions.QueryActive()
            .Where(a => a.City.ToLower() == city.ToLower())
            .Select(a => new { a.Latitude, a.Longitude })
            .ToListAsync(ct);
        return points.Count == 0 ? null : (points.Average(p => p.Latitude), points.Average(p => p.Longitude));
    }

    private async Task<DistanceResult?> FromStaticTableAsync(string fromCity, string toCity, CancellationToken ct)
    {
        var a = fromCity.ToLower();
        var b = toCity.ToLower();
        var row = await db.CityDistances.AsNoTracking().FirstOrDefaultAsync(d =>
            (d.FromCity.ToLower() == a && d.ToCity.ToLower() == b) ||
            (d.FromCity.ToLower() == b && d.ToCity.ToLower() == a), ct);
        return row is null
            ? null
            : new DistanceResult(fromCity, toCity, row.DistanceKm, row.DurationMinutes, "static-table");
    }

    private record OrsMatrixResponse(
        [property: JsonPropertyName("distances")] decimal?[][]? Distances,
        [property: JsonPropertyName("durations")] decimal?[][]? Durations);
}
