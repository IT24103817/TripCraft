namespace TripCraft.Application.Common.Notifications;

public record NotificationDto(Guid Id, string Type, string Title, string Body, Guid? TripRequestId, bool IsRead,
    DateTime CreatedAt)
{
    public static NotificationDto FromEntity(Notification n) =>
        new(n.Id, n.Type, n.Title, n.Body, n.TripRequestId, n.ReadAt is not null, n.CreatedAt);
}

/// <summary>GET /api/notifications/mine: the newest 50 and how many are unread in total.</summary>
public record NotificationListDto(int UnreadCount, IReadOnlyList<NotificationDto> Items);
