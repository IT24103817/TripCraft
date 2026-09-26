namespace TripCraft.Application.Quotations.Reports;

public interface IReportService
{
    Task<IReadOnlyList<RevenueMonthDto>> RevenueAsync(ReportRangeQuery query, CancellationToken ct);
    Task<IReadOnlyList<UtilisationDto>> UtilisationAsync(ReportRangeQuery query, CancellationToken ct);
    Task<IReadOnlyList<StatusCountDto>> TripsByStatusAsync(ReportRangeQuery query, CancellationToken ct);
}

/// <summary>Component C reporting: revenue per month, guide/vehicle utilisation and trip requests by status.</summary>
public class ReportService(IReportQueries queries) : IReportService
{
    public Task<IReadOnlyList<RevenueMonthDto>> RevenueAsync(ReportRangeQuery q, CancellationToken ct) =>
        queries.RevenueByMonthAsync(q.From, q.To, ct);

    public async Task<IReadOnlyList<UtilisationDto>> UtilisationAsync(ReportRangeQuery q, CancellationToken ct)
    {
        var days = ReportMath.DaysInRange(q.From, q.To);
        var holds = await queries.HoldsAsync(q.From, q.To, ct);
        var resources = await queries.GuidesAndVehiclesAsync(ct);
        return resources
            .Select(r =>
            {
                var held = holds.Where(h => h.ResourceId == r.ResourceId)
                    .Sum(h => ReportMath.ClippedDays(h.From, h.To, q.From, q.To));
                held = Math.Min(held, days); // holds of one resource never overlap, but be safe
                return new UtilisationDto(r.ResourceType, r.ResourceId, r.Name, held, days, ReportMath.Percent(held, days));
            })
            .OrderBy(u => u.ResourceType).ThenByDescending(u => u.UtilisationPct).ThenBy(u => u.Name)
            .ToList();
    }

    public Task<IReadOnlyList<StatusCountDto>> TripsByStatusAsync(ReportRangeQuery q, CancellationToken ct) =>
        queries.TripsByStatusAsync(q.From, q.To, ct);
}
