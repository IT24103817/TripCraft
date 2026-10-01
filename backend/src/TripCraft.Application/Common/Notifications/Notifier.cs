using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Identity;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;

namespace TripCraft.Application.Common.Notifications;

public class Notifier(INotificationRepository notifications, IUserRepository users, IResourceRepository resources)
    : INotifier
{
    public void NotifyUser(Guid userId, string type, string title, string body, Guid? tripRequestId) =>
        notifications.Add(new Notification
        {
            UserId = userId, Type = type, Title = title, Body = body, TripRequestId = tripRequestId
        });

    public async Task NotifyManagersAsync(string type, string title, string body, Guid? tripRequestId, CancellationToken ct)
    {
        var managers = (await users.ListAsync(ct)).Where(u => u.Role == UserRole.OperationsManager && u.IsActive);
        foreach (var manager in managers)
            NotifyUser(manager.Id, type, title, body, tripRequestId);
    }

    public void NotifyTourist(TripRequest trip, string type, string title, string body)
    {
        if (trip.Tourist?.UserId is { } userId)
            NotifyUser(userId, type, title, body, trip.Id);
    }

    public async Task NotifyGuideAsync(Guid guideId, string type, string title, string body, Guid? tripRequestId,
        CancellationToken ct)
    {
        var userId = await resources.Guides().Where(g => g.Id == guideId).Select(g => g.UserId).FirstOrDefaultAsync(ct);
        if (userId is { } id)
            NotifyUser(id, type, title, body, tripRequestId);
    }

    public void QueueEmail(string to, string subject, string body, Guid? tripRequestId) =>
        notifications.Add(new EmailMessage { To = to, Subject = subject, Body = body, TripRequestId = tripRequestId });
}
