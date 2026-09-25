using System.Text.Json;
using System.Text.RegularExpressions;

namespace TripCraft.Application.Trips.Planning;

/// <summary>
/// Component A business operation (PLAN.md section 3): validate passport/dates and build a
/// day-by-day itinerary skeleton from the objective. Pure functions — no database, clock or HTTP —
/// so every rule is covered by plain unit tests.
/// </summary>
public static class TripPlanningRules
{
    public const int MaxTripDays = 30;

    /// <summary>Operator rule from PLAN.md section 5: at most 3 stops per day.</summary>
    public const int MaxStopsPerDay = 3;
    public const int RelaxedStopsPerDay = 2;

    private static readonly Regex MaskedPassportPattern = new(@"^\*{4}[A-Z0-9]{4}$");

    /// <summary>Returns every reason the trip cannot be planned yet. An empty list means it is ready.</summary>
    public static List<string> ValidateForPlanning(TripRequest trip, Tourist tourist, DateOnly today)
    {
        var errors = new List<string>();

        if (trip.StartDate < today)
            errors.Add("Start date is in the past.");
        if (trip.EndDate < trip.StartDate)
            errors.Add("End date is before start date.");
        else if (TripDays(trip.StartDate, trip.EndDate) > MaxTripDays)
            errors.Add($"Trips longer than {MaxTripDays} days cannot be planned.");
        if (trip.Pax <= 0)
            errors.Add("Pax must be greater than zero.");
        if (trip.BudgetUsd <= 0)
            errors.Add("Budget must be greater than zero.");
        if (!MaskedPassportPattern.IsMatch(tourist.PassportNumberMasked))
            errors.Add("Tourist has no valid passport number on file.");

        return errors;
    }

    /// <summary>
    /// Finds known city names in the objective, in the order they are first mentioned.
    /// "5 days in Kandy and Ella" with known cities [Colombo, Ella, Kandy] gives [Kandy, Ella].
    /// </summary>
    public static List<string> ExtractCities(string objective, IEnumerable<string> knownCities)
    {
        return knownCities
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(city => new
            {
                City = city,
                Match = Regex.Match(objective, $@"\b{Regex.Escape(city)}\b", RegexOptions.IgnoreCase)
            })
            .Where(x => x.Match.Success)
            .OrderBy(x => x.Match.Index)
            .Select(x => x.City)
            .ToList();
    }

    /// <summary>
    /// Splits the trip days across the cities in order. Days that do not divide evenly go to the
    /// earlier cities: 5 days over [Kandy, Ella] gives Kandy, Kandy, Kandy, Ella, Ella.
    /// </summary>
    public static List<SkeletonDay> BuildSkeleton(DateOnly start, DateOnly end, IReadOnlyList<string> cities, string? pace)
    {
        var totalDays = TripDays(start, end);
        if (cities.Count == 0)
            throw new ArgumentException("At least one city is required.", nameof(cities));
        if (cities.Count > totalDays)
            throw new ArgumentException("More cities than trip days.", nameof(cities));

        var maxStops = string.Equals(pace, "relaxed", StringComparison.OrdinalIgnoreCase)
            ? RelaxedStopsPerDay
            : MaxStopsPerDay;

        var baseDays = totalDays / cities.Count;
        var extraDays = totalDays % cities.Count;

        var skeleton = new List<SkeletonDay>();
        for (var c = 0; c < cities.Count; c++)
        {
            var daysInCity = baseDays + (c < extraDays ? 1 : 0);
            for (var d = 0; d < daysInCity; d++)
            {
                var dayNumber = skeleton.Count + 1;
                skeleton.Add(new SkeletonDay(dayNumber, start.AddDays(dayNumber - 1), cities[c], maxStops));
            }
        }
        return skeleton;
    }

    /// <summary>Keeps only the last 4 characters: "N 1234567" becomes "****4567" (PLAN.md section 10).</summary>
    public static string MaskPassport(string passportNumber)
    {
        var cleaned = passportNumber.Replace(" ", "").ToUpperInvariant();
        return "****" + cleaned[^4..];
    }

    /// <summary>Reads "pace" from the preferences JSON, e.g. {"pace":"relaxed"}. Null if missing.</summary>
    public static string? ReadPace(string preferencesJson)
    {
        using var doc = JsonDocument.Parse(preferencesJson);
        return doc.RootElement.ValueKind == JsonValueKind.Object
               && doc.RootElement.TryGetProperty("pace", out var pace)
               && pace.ValueKind == JsonValueKind.String
            ? pace.GetString()
            : null;
    }

    /// <summary>Inclusive day count: 10 Oct to 14 Oct is 5 days.</summary>
    public static int TripDays(DateOnly start, DateOnly end) => end.DayNumber - start.DayNumber + 1;
}
