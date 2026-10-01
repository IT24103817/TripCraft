using Microsoft.Extensions.Logging;

namespace TripCraft.Application.Common.Notifications;

public interface IEmailDispatcher
{
    /// <summary>Sends every outbox email that is not sent yet. Called after the business transaction committed.</summary>
    Task DispatchPendingAsync(CancellationToken ct);
}

/// <summary>
/// Sends the outbox. A failed email is marked with its error and never undoes the booking that queued it.
/// </summary>
public class EmailDispatcher(
    INotificationRepository notifications,
    IEmailSender sender,
    IUnitOfWork unitOfWork,
    ILogger<EmailDispatcher> logger) : IEmailDispatcher
{
    public async Task DispatchPendingAsync(CancellationToken ct)
    {
        foreach (var email in await notifications.ListUnsentEmailsAsync(ct))
        {
            try
            {
                await sender.SendAsync(email.To, email.Subject, email.Body, ct);
                email.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Email {EmailId} could not be sent", email.Id);
                email.Error = ex.GetType().Name;
            }
        }
        await unitOfWork.SaveChangesAsync(ct);
    }
}
