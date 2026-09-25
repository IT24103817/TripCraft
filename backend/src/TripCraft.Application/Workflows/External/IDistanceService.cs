namespace TripCraft.Application.Workflows.External;

/// <summary>Driving distance between cities from OpenRouteService, falling back to city_distances. Never throws.</summary>
public interface IDistanceService
{
    /// <summary>Null only when neither the provider nor the static table knows the pair.</summary>
    Task<DistanceResult?> GetDistanceAsync(string fromCity, string toCity, CancellationToken ct);
}

/// <summary>Source is "openrouteservice" or "static-table".</summary>
public record DistanceResult(string FromCity, string ToCity, decimal DistanceKm, int DurationMinutes, string Source);
