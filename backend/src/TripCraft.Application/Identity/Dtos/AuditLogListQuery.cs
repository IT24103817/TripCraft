using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Identity.Dtos;

/// <summary>GET /api/admin/audit-logs?actor=&amp;entity=&amp;action=&amp;from=&amp;to=&amp;search=&amp;sort=&amp;page=&amp;pageSize=</summary>
public class AuditLogListQuery : PagedQuery
{
    /// <summary>Part of the actor's email (case-insensitive); "system" = rows written without a user.</summary>
    public string? Actor { get; set; }

    /// <summary>Exact entity name, e.g. "TripRequest", "AgentWorkflow", "Attraction".</summary>
    public string? Entity { get; set; }

    /// <summary>Exact action, e.g. "TripRequestStatusChanged".</summary>
    public string? Action { get; set; }

    /// <summary>Only rows on or after this date (UTC).</summary>
    public DateOnly? From { get; set; }

    /// <summary>Only rows on or before this date (UTC).</summary>
    public DateOnly? To { get; set; }
}
