using TripCraft.Application.Identity;

namespace TripCraft.Application.Common.Auditing;

/// <summary>Read side of audit_logs: rows joined with the user who made the change (null for system actions).</summary>
public interface IAuditLogReader
{
    IQueryable<AuditLogView> Query();
}

/// <summary>One audit row plus the actor's email and role. A class with init setters so EF can filter on it.</summary>
public class AuditLogView
{
    public Guid Id { get; init; }
    public DateTime At { get; init; }
    public Guid? ActorId { get; init; }
    public string? ActorEmail { get; init; }
    public UserRole? ActorRole { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Entity { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public string? Before { get; init; }
    public string? After { get; init; }
}
