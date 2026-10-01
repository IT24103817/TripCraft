using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;

namespace TripCraft.Application.Common.Notifications;

public interface INotificationService
{
    Task<NotificationListDto> ListMineAsync(CurrentUser user, CancellationToken ct);
    Task MarkReadAsync(CurrentUser user, Guid id, CancellationToken ct);
    Task MarkAllReadAsync(CurrentUser user, CancellationToken ct);
}

/// <summary>Each user sees and marks only their own notifications.</summary>
public class NotificationService(INotificationRepository notifications, IUnitOfWork unitOfWork) : INotificationService
{
    public const int PageSize = 50;

    public async Task<NotificationListDto> ListMineAsync(CurrentUser user, CancellationToken ct)
    {
        var mine = notifications.Query().Where(n => n.UserId == user.Id);
        var unread = await mine.CountAsync(n => n.ReadAt == null, ct);
        var items = await mine.OrderByDescending(n => n.CreatedAt).Take(PageSize).ToListAsync(ct);
        return new NotificationListDto(unread, items.Select(NotificationDto.FromEntity).ToList());
    }

    public async Task MarkReadAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var notification = await notifications.FindAsync(id, ct);
        // Someone else's notification is reported as missing, so ids cannot be probed.
        if (notification is null || notification.UserId != user.Id)
            throw new NotFoundException("Notification not found.");
        notification.ReadAt ??= DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(CurrentUser user, CancellationToken ct)
    {
        foreach (var notification in await notifications.ListUnreadForUpdateAsync(user.Id, ct))
            notification.ReadAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
    }
}
