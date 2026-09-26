using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Quotations.Reports;

namespace TripCraft.Api.Controllers.Quotations;

/// <summary>Component C — reporting and analytics for the Operations Manager dashboard.</summary>
[ApiController]
[Route("api/reports")]
[Authorize(Roles = Roles.OperationsManager)]
public class ReportsController(IReportService reports) : ControllerBase
{
    /// <summary>Approved quotations per month (by approval date), in LKR and USD.</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<IReadOnlyList<RevenueMonthDto>>> Revenue([FromQuery] ReportRangeQuery query, CancellationToken ct) =>
        Ok(await reports.RevenueAsync(query, ct));

    /// <summary>Held days of every active guide and vehicle in the range, as a percentage.</summary>
    [HttpGet("utilisation")]
    public async Task<ActionResult<IReadOnlyList<UtilisationDto>>> Utilisation([FromQuery] ReportRangeQuery query, CancellationToken ct) =>
        Ok(await reports.UtilisationAsync(query, ct));

    /// <summary>Trip requests starting in the range, per status.</summary>
    [HttpGet("trips-by-status")]
    public async Task<ActionResult<IReadOnlyList<StatusCountDto>>> TripsByStatus([FromQuery] ReportRangeQuery query, CancellationToken ct) =>
        Ok(await reports.TripsByStatusAsync(query, ct));
}
