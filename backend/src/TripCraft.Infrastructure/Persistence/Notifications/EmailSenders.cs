using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using TripCraft.Application.Common.Notifications;

namespace TripCraft.Infrastructure.Persistence.Notifications;

/// <summary>SMTP_HOST, SMTP_PORT (default 587), SMTP_USER, SMTP_PASSWORD and SMTP_FROM from configuration.</summary>
public class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken ct)
    {
        using var client = new SmtpClient(configuration["SMTP_HOST"], int.TryParse(configuration["SMTP_PORT"], out var p) ? p : 587)
        {
            EnableSsl = true
        };
        if (!string.IsNullOrWhiteSpace(configuration["SMTP_USER"]))
            client.Credentials = new System.Net.NetworkCredential(configuration["SMTP_USER"], configuration["SMTP_PASSWORD"]);
        using var message = new MailMessage(configuration["SMTP_FROM"] ?? "no-reply@tripcraft.test", to, subject, body);
        await client.SendMailAsync(message, ct);
    }
}

/// <summary>Development: writes each email as a .eml file into EMAIL_PICKUP_DIR (default: temp/tripcraft-mail).</summary>
public class PickupDirectoryEmailSender(IConfiguration configuration) : IEmailSender
{
    public string Folder { get; } = configuration["EMAIL_PICKUP_DIR"] is { Length: > 0 } dir
        ? dir
        : Path.Combine(Path.GetTempPath(), "tripcraft-mail");

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct)
    {
        Directory.CreateDirectory(Folder);
        var file = Path.Combine(Folder, $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.eml");
        var text = $"To: {to}\r\nSubject: {subject}\r\nContent-Type: text/plain; charset=utf-8\r\n\r\n{body}\r\n";
        await File.WriteAllTextAsync(file, text, ct);
    }
}
