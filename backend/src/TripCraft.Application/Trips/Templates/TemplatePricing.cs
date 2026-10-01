using TripCraft.Application.Quotations;

namespace TripCraft.Application.Trips.Templates;

/// <summary>Today's prices the from-price is built from (cheapest suitable guide, vehicle and rooms).</summary>
public record TemplatePriceInputs(
    decimal GuideDayRateLkr,
    decimal VehicleKmRateLkr,
    IReadOnlyDictionary<string, IReadOnlyList<(int Capacity, decimal RatePerNightLkr)>> RoomsByCity,
    IReadOnlyDictionary<string, decimal> EntryFeesByAttraction,
    IReadOnlyList<decimal> TransferKmBetweenDays,
    decimal MarginPct,
    decimal LkrPerUsd);

/// <summary>
/// "From" price of a package for a party (pure, unit tested): guide days + vehicle km + the cheapest way to sleep
/// the party in each night's city + entry tickets, then the operator margin — the same QuotationCalculator as a
/// real quotation, so the home screen and the final price use one rule.
/// </summary>
public static class TemplatePricing
{
    public static QuotationBreakdown Calculate(IReadOnlyList<TemplateDay> days, int pax, TemplatePriceInputs p)
    {
        var items = new List<PriceItem>
        {
            new("guide", $"Guide, {days.Count} days", days.Count, p.GuideDayRateLkr),
            new("vehicle", $"Vehicle, {p.TransferKmBetweenDays.Sum()} km", p.TransferKmBetweenDays.Sum(), p.VehicleKmRateLkr)
        };

        // Every day except the last is a night, spent in that day's city.
        foreach (var night in days.OrderBy(d => d.Day).SkipLast(1))
            items.Add(new PriceItem("room", $"Night in {night.City}", 1, CheapestNight(night.City, pax, p.RoomsByCity)));

        foreach (var stop in days.SelectMany(d => d.Stops))
            items.Add(new PriceItem("entry", $"{stop} entry, {pax} people", pax, p.EntryFeesByAttraction.GetValueOrDefault(stop)));

        return QuotationCalculator.Calculate(items, p.MarginPct, p.LkrPerUsd);
    }

    /// <summary>The cheapest single room type that sleeps the party with enough rooms of it (e.g. 2 doubles for 4).</summary>
    public static decimal CheapestNight(string city, int pax,
        IReadOnlyDictionary<string, IReadOnlyList<(int Capacity, decimal RatePerNightLkr)>> roomsByCity)
    {
        if (!roomsByCity.TryGetValue(city, out var rooms) || rooms.Count == 0)
            return 0;
        return rooms.Min(r => (int)Math.Ceiling(pax / (double)r.Capacity) * r.RatePerNightLkr);
    }
}
