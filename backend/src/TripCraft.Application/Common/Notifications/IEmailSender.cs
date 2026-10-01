namespace TripCraft.Application.Common.Notifications;

/// <summary>Sends one email. SMTP when SMTP_HOST is set, otherwise a .eml file in a pickup folder (development).</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct);
}
