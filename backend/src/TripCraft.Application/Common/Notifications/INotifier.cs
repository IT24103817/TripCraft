using TripCraft.Application.Trips;

namespace TripCraft.Application.Common.Notifications;

/// <summary>
/// Stages in-app notifications (and outbox emails) on the current unit of work, so they are saved in the same
/// transaction as the business change that caused them. Nothing is sent here.
/// </summary>
public interface INotifier
{
    void NotifyUser(Guid userId, string type, string title, string body, Guid? tripRequestId);

    /// <summary>Every active Operations Manager.</summary>
    Task NotifyManagersAsync(string type, string title, string body, Guid? tripRequestId, CancellationToken ct);

    /// <summary>The tourist who owns the trip (the trip must be loaded with its tourist).</summary>
    void NotifyTourist(TripRequest trip, string type, string title, string body);

    /// <summary>The login linked to the guide, if any.</summary>
    Task NotifyGuideAsync(Guid guideId, string type, string title, string body, Guid? tripRequestId, CancellationToken ct);

    /// <summary>Stages an outbox email; IEmailDispatcher sends it after the commit.</summary>
    void QueueEmail(string to, string subject, string body, Guid? tripRequestId);
}
