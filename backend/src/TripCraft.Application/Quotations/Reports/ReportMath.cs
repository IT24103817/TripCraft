namespace TripCraft.Application.Quotations.Reports;

/// <summary>Pure helpers for the utilisation report (unit tested).</summary>
public static class ReportMath
{
    public static int DaysInRange(DateOnly from, DateOnly to) => to.DayNumber - from.DayNumber + 1;

    /// <summary>Days of the hold [holdFrom, holdTo] that fall inside [from, to]; 0 if they do not overlap.</summary>
    public static int ClippedDays(DateOnly holdFrom, DateOnly holdTo, DateOnly from, DateOnly to)
    {
        var start = holdFrom > from ? holdFrom : from;
        var end = holdTo < to ? holdTo : to;
        return end < start ? 0 : DaysInRange(start, end);
    }

    /// <summary>Percentage with one decimal; 0 for an empty range.</summary>
    public static decimal Percent(int part, int whole) =>
        whole <= 0 ? 0 : Math.Round(100m * part / whole, 1, MidpointRounding.AwayFromZero);
}
