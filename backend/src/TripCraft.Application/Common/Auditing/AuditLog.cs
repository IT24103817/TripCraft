using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Common.Auditing;

/// <summary>
/// audit_logs from PLAN.md section 4. Owned by Component C (Quotation, Approval &amp; Reporting);
/// created here with the section 4 columns so every component can write audit rows. C may extend it.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>User who made the change. Null for system actions (e.g. seeding, agent callbacks).</summary>
    public Guid? ActorId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public Guid EntityId { get; set; }

    /// <summary>JSON snapshot before the change (jsonb). Null on create.</summary>
    public string? Before { get; set; }

    /// <summary>JSON snapshot after the change (jsonb).</summary>
    public string? After { get; set; }

    public DateTime At { get; set; }
}
