using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Trips;

/// <summary>
/// A pre-planned package shown on the tourist's home screen ("Trips picked for your mood", v1.1). Booking one creates a
/// normal trip request from it, so the agents and the manager's review still run. Plan is JSON: the days with their
/// city and attraction names. The price is not stored; it is computed from today's rate cards.
/// </summary>
public class TripTemplate : BaseEntity
{
    /// <summary>Stable key, also the name of the hero image asset in the app (e.g. "hill-country-escape").</summary>
    public string Slug { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string MoodTag { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    /// <summary>The objective a booking sends to the agents.</summary>
    public string Objective { get; set; } = string.Empty;

    public int Days { get; set; }

    /// <summary>"|Kandy|Ella|", as on TripRequest.</summary>
    public string Cities { get; set; } = string.Empty;

    /// <summary>JSON object, e.g. {"language":"en","transport":"train","pace":"relaxed"}.</summary>
    public string Preferences { get; set; } = "{}";

    /// <summary>JSON array of <see cref="Templates.TemplateDay"/>.</summary>
    public string Plan { get; set; } = "[]";

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public IReadOnlyList<string> CityList =>
        Cities.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
