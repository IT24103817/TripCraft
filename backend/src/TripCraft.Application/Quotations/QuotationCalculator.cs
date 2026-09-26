namespace TripCraft.Application.Quotations;

/// <summary>One priced item before rounding: guide days, vehicle km, room-nights or entry tickets.</summary>
public record PriceItem(string LineType, string Description, decimal Qty, decimal UnitLkr);

public record CalculatedLine(string LineType, string Description, decimal Qty, decimal UnitLkr, decimal AmountLkr);

public record QuotationBreakdown(IReadOnlyList<CalculatedLine> Lines, decimal SubtotalLkr, decimal MarginPct,
    decimal MarginLkr, decimal TotalLkr, decimal FxRate, decimal TotalUsd);

/// <summary>
/// Component C business rule (PLAN.md section 3C), pure so it is unit tested and gives the same result as the
/// agent's calculate_quotation tool:
///   each line = qty × unit (guide day rate × days, km rate × km, night rate × room-nights, entry fee × pax);
///   subtotal = Σ lines; margin = subtotal × margin% / 100; total = subtotal + margin; USD = total / LKR-per-USD.
/// Every amount is rounded to 2 decimals, half away from zero.
/// </summary>
public static class QuotationCalculator
{
    public static QuotationBreakdown Calculate(IEnumerable<PriceItem> items, decimal marginPct, decimal lkrPerUsd)
    {
        if (lkrPerUsd <= 0)
            throw new ArgumentOutOfRangeException(nameof(lkrPerUsd), "The exchange rate must be positive.");
        if (marginPct is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(marginPct), "The margin must be between 0 and 100 %.");

        var lines = items
            .Where(i => i.Qty > 0 && i.UnitLkr > 0)
            .Select(i => new CalculatedLine(i.LineType, i.Description, i.Qty, i.UnitLkr, Round(i.Qty * i.UnitLkr)))
            .ToList();
        var subtotal = lines.Sum(l => l.AmountLkr);
        var margin = Round(subtotal * marginPct / 100);
        var total = subtotal + margin;
        return new QuotationBreakdown(lines, subtotal, marginPct, margin, total, lkrPerUsd, Round(total / lkrPerUsd));
    }

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
