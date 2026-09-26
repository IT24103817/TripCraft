using TripCraft.Application.Common.Auditing;

namespace TripCraft.Application.Identity.Dtos;

/// <summary>One audit_logs row for the Admin audit screen. Before/After are the JSON snapshots as stored.</summary>
public record AuditLogDto(Guid Id, DateTime At, string? ActorEmail, string? ActorRole, string Action, string Entity,
    Guid EntityId, string? Before, string? After)
{
    public static AuditLogDto FromView(AuditLogView v) => new(
        v.Id, v.At, v.ActorEmail, v.ActorRole?.ToString(), v.Action, v.Entity, v.EntityId, v.Before, v.After);
}
