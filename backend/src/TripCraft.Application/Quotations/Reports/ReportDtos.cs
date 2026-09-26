namespace TripCraft.Application.Quotations.Reports;

/// <summary>GET /api/reports/*?from=&amp;to= (inclusive dates, at most one year).</summary>
public class ReportRangeQuery
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}

/// <summary>Approved quotations per month of the approval decision.</summary>
public record RevenueMonthDto(string Month, int Quotations, decimal TotalLkr, decimal TotalUsd);

/// <summary>Held days of a guide or vehicle inside the range, out of the days in the range.</summary>
public record UtilisationDto(string ResourceType, Guid ResourceId, string Name, int HeldDays, int DaysInRange,
    decimal UtilisationPct);

public record StatusCountDto(string Status, int Count);

/// <summary>A raw hold span, clipped by ReportMath.</summary>
public record HoldSpan(string ResourceType, Guid ResourceId, DateOnly From, DateOnly To);

public record ResourceName(string ResourceType, Guid ResourceId, string Name);
