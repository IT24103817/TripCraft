using System.Text.Json;
using System.Text.Json.Serialization;
using TripCraft.Application.Common.Auditing;

namespace TripCraft.Infrastructure.Persistence.Auditing;

/// <summary>Adds an audit_logs row to the current DbContext; it is saved with the business change.</summary>
public class AuditLogger(AppDbContext db) : IAuditLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public void Record(Guid? actorId, string action, string entity, Guid entityId, object? before, object? after)
    {
        db.AuditLogs.Add(new AuditLog
        {
            ActorId = actorId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Before = before is null ? null : JsonSerializer.Serialize(before, JsonOptions),
            After = after is null ? null : JsonSerializer.Serialize(after, JsonOptions),
            At = DateTime.UtcNow
        });
    }
}
