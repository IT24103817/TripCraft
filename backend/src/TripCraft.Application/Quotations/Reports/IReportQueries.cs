namespace TripCraft.Application.Quotations.Reports;

/// <summary>Read-only aggregates across components for the reports (implemented with SQL in Infrastructure).</summary>
public interface IReportQueries
{
    Task<IReadOnlyList<RevenueMonthDto>> RevenueByMonthAsync(DateOnly from, DateOnly to, CancellationToken ct);

    /// <summary>Active guides and vehicles (every one is listed, even with no holds).</summary>
    Task<IReadOnlyList<ResourceName>> GuidesAndVehiclesAsync(CancellationToken ct);

    /// <summary>Held guide and vehicle holds overlapping [from, to].</summary>
    Task<IReadOnlyList<HoldSpan>> HoldsAsync(DateOnly from, DateOnly to, CancellationToken ct);

    /// <summary>Trip requests starting in [from, to], counted per status.</summary>
    Task<IReadOnlyList<StatusCountDto>> TripsByStatusAsync(DateOnly from, DateOnly to, CancellationToken ct);
}
