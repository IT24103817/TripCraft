namespace TripCraft.Application.Common.Notifications;

/// <summary>Data access for notifications and the email outbox. Add only stages a row; the caller commits.</summary>
public interface INotificationRepository
{
    /// <summary>Read-only.</summary>
    IQueryable<Notification> Query();

    /// <summary>The user's unread notifications, tracked so they can be marked read.</summary>
    Task<List<Notification>> ListUnreadForUpdateAsync(Guid userId, CancellationToken ct);

    /// <summary>Tracked.</summary>
    Task<Notification?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>Emails not sent and not failed yet, tracked.</summary>
    Task<List<EmailMessage>> ListUnsentEmailsAsync(CancellationToken ct);

    void Add<T>(T entity) where T : class;
}
