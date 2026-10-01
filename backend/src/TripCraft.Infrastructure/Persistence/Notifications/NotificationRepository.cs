using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Notifications;

namespace TripCraft.Infrastructure.Persistence.Notifications;

public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public IQueryable<Notification> Query() => db.Notifications.AsNoTracking();

    public Task<List<Notification>> ListUnreadForUpdateAsync(Guid userId, CancellationToken ct) =>
        db.Notifications.Where(n => n.UserId == userId && n.ReadAt == null).ToListAsync(ct);

    public Task<Notification?> FindAsync(Guid id, CancellationToken ct) =>
        db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);

    public Task<List<EmailMessage>> ListUnsentEmailsAsync(CancellationToken ct) =>
        db.EmailOutbox.Where(e => e.SentAt == null && e.Error == null).OrderBy(e => e.CreatedAt).ToListAsync(ct);

    public void Add<T>(T entity) where T : class => db.Add(entity);
}
