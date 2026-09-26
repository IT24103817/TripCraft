using TripCraft.Application.Quotations;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Recomputes the quotation total server-side from database prices with Component C's QuotationCalculator
/// (the same formula as the agent's calculate_quotation tool, agents/README.md):
///   guide = day rate x trip days;  vehicle = km rate x total transfer km;
///   rooms = night rate x room-nights;  entry = entry fee x pax for each stop;
///   total = subtotal + subtotal x margin% / 100. Every amount rounded to 2 decimals, half away from zero.
/// </summary>
public static class ProposalQuotationCheck
{
    /// <summary>Returns the recomputed total in LKR, or an error when a price is missing.</summary>
    public static (decimal? TotalLkr, string? Error) RecomputeTotalLkr(
        IReadOnlyList<ProposalDay> days, Guid guideId, Guid vehicleId, IReadOnlyList<Guid> roomTypePerRoomNight,
        int pax, ProposalFacts facts)
    {
        var card = facts.RateCard;
        if (card is null)
            return (null, "No rate card available.");
        if (!card.GuideDayRates.TryGetValue(guideId, out var guideRate))
            return (null, $"No day rate for guide {guideId}.");
        if (!card.VehicleKmRates.TryGetValue(vehicleId, out var kmRate))
            return (null, $"No km rate for vehicle {vehicleId}.");

        var items = new List<PriceItem>
        {
            new("guide", "Guide", days.Count, guideRate),
            new("vehicle", "Vehicle", days.Sum(d => d.TransferKm), kmRate)
        };
        foreach (var group in roomTypePerRoomNight.GroupBy(id => id))
        {
            if (!card.RoomNightRates.TryGetValue(group.Key, out var nightRate))
                return (null, $"No night rate for room type {group.Key}.");
            items.Add(new PriceItem("room", "Room", group.Count(), nightRate));
        }
        foreach (var stop in days.SelectMany(d => d.Stops ?? []))
            items.Add(new PriceItem("entry", stop.Name, pax, facts.AttractionEntryFeesLkr[Guid.Parse(stop.AttractionId)]));

        // The exchange rate does not change the LKR total, so 1 is passed here.
        return (QuotationCalculator.Calculate(items, card.MarginPct, 1m).TotalLkr, null);
    }

    public static decimal Round(decimal value) => QuotationCalculator.Round(value);
}
