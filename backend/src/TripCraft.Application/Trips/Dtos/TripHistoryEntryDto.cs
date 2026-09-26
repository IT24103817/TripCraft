using System.Text.Json;
using TripCraft.Application.Common.Auditing;

namespace TripCraft.Application.Trips.Dtos;

/// <summary>
/// One event in a trip's history (GET /api/trip-requests/{id}/history), taken from audit_logs.
/// Actor is a role ("Tourist", "OperationsManager") or "System" — never another user's email.
/// FromStatus/ToStatus are set when the row records a status change.
/// </summary>
public record TripHistoryEntryDto(DateTime At, string Action, string Entity, string Actor, string? FromStatus,
    string? ToStatus)
{
    public static TripHistoryEntryDto FromAudit(AuditLogView log) => new(
        log.At, log.Action, log.Entity, log.ActorRole?.ToString() ?? "System",
        ReadStatus(log.Before), ReadStatus(log.After));

    /// <summary>The "status" field of a JSON snapshot, or null if there is none.</summary>
    public static string? ReadStatus(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            return null;
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Name.Equals("status", StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind == JsonValueKind.String)
                return property.Value.GetString();
        }
        return null;
    }
}
