using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Quotations;
using TripCraft.Application.Quotations.Reports;
using TripCraft.Application.Resources;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Infrastructure.Persistence.Reporting;

/// <summary>Report aggregates for Component C. Shared read model: it reads A's, B's and C's tables and never writes.</summary>
public class ReportQueries(AppDbContext db) : IReportQueries
{
    public async Task<IReadOnlyList<RevenueMonthDto>> RevenueByMonthAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endExclusive = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        // Revenue = confirmed bookings (v1.1): the month of the manager's Confirm, the human approval gate. Quotes
        // that were only sent or accepted are not revenue yet.
        var approved = await (
                from decision in db.ApprovalDecisions.AsNoTracking()
                join quotation in db.Quotations on decision.QuotationId equals quotation.Id
                where decision.Decision == QuotationDecision.Confirmed && quotation.Status == QuotationStatus.Approved
                      && decision.DecidedAt >= start && decision.DecidedAt < endExclusive
                select new { decision.DecidedAt, quotation.TotalLkr, quotation.TotalUsd })
            .ToListAsync(ct);

        return approved
            .GroupBy(a => a.DecidedAt.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new RevenueMonthDto(g.Key, g.Count(), g.Sum(a => a.TotalLkr), g.Sum(a => a.TotalUsd)))
            .ToList();
    }

    public async Task<IReadOnlyList<ResourceName>> GuidesAndVehiclesAsync(CancellationToken ct)
    {
        var guides = await db.Guides.AsNoTracking().Where(g => !g.IsDeleted && g.IsActive)
            .Select(g => new ResourceName(nameof(ResourceType.Guide), g.Id, g.Name)).ToListAsync(ct);
        var vehicles = await db.Vehicles.AsNoTracking().Where(v => !v.IsDeleted && v.IsActive)
            .Select(v => new ResourceName(nameof(ResourceType.Vehicle), v.Id, v.RegistrationNo)).ToListAsync(ct);
        return [.. guides, .. vehicles];
    }

    public async Task<IReadOnlyList<HoldSpan>> HoldsAsync(DateOnly from, DateOnly to, CancellationToken ct) =>
        (await db.ResourceHolds.AsNoTracking()
            .Where(h => h.Status == HoldStatus.Held && h.ResourceType != ResourceType.Room
                        && h.FromDate <= to && from <= h.ToDate)
            .ToListAsync(ct))
        .Select(h => new HoldSpan(h.ResourceType.ToString(), h.ResourceId, h.FromDate, h.ToDate))
        .ToList();

    public async Task<IReadOnlyList<StatusCountDto>> TripsByStatusAsync(DateOnly from, DateOnly to, CancellationToken ct) =>
        (await db.TripRequests.AsNoTracking()
            .Where(t => t.StartDate >= from && t.StartDate <= to)
            .GroupBy(t => t.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct))
        .Select(x => new StatusCountDto(x.Key.ToString(), x.Count))
        .OrderByDescending(x => x.Count)
        .ToList();
}
