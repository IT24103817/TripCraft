using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Recomputes the quotation total server-side from database prices, with the same formula as the
/// agent's calculate_quotation tool (agents/README.md). Student C's QuotationCalculator must give the
/// same result; when it exists this class should call it instead.
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

        var subtotal = Round(guideRate * days.Count);
        subtotal += Round(kmRate * days.Sum(d => d.TransferKm));

        foreach (var group in roomTypePerRoomNight.GroupBy(id => id))
        {
            if (!card.RoomNightRates.TryGetValue(group.Key, out var nightRate))
                return (null, $"No night rate for room type {group.Key}.");
            subtotal += Round(nightRate * group.Count());
        }

        foreach (var stop in days.SelectMany(d => d.Stops ?? []))
        {
            var fee = facts.AttractionEntryFeesLkr[Guid.Parse(stop.AttractionId)];
            if (fee > 0)
                subtotal += Round(fee * pax);
        }

        var margin = Round(subtotal * card.MarginPct / 100);
        return (subtotal + margin, null);
    }

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
