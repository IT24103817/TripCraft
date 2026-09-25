namespace TripCraft.Application.Workflows.External;

/// <summary>OpenWeatherMap 5-day forecast (PLAN.md section 9). Never throws.</summary>
public interface IWeatherService
{
    /// <summary>Null when the provider fails or the date is outside the 5-day window (a warning is logged).</summary>
    Task<WeatherForecast?> GetForecastAsync(string city, DateOnly date, CancellationToken ct);
}

/// <summary>RainProbability is 0..1 (the highest 3-hour value that day).</summary>
public record WeatherForecast(string City, DateOnly Date, string Summary, double RainProbability);
