using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Common.Notifications;

/// <summary>
/// One in-app message for one user (v1.1). Flutter polls GET /api/notifications/mine and shows new ones as local
/// notifications; React shows them under the bell. TripRequestId lets the client open the trip.
/// </summary>
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Guid? TripRequestId { get; set; }
    public DateTime? ReadAt { get; set; }
}
