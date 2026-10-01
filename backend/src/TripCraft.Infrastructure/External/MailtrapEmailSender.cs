using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Common.Notifications;
using TripCraft.Infrastructure.Persistence.Notifications;

namespace TripCraft.Infrastructure.External;

/// <summary>
/// The fourth third-party integration (v1.1): tourist emails through the Mailtrap email sandbox HTTP API
/// (MAILTRAP_API_TOKEN, MAILTRAP_INBOX_ID; the sandbox catches every email, nothing reaches a real inbox).
/// Same pattern as FX, distance and weather: typed HttpClient, 5 s per try and one retry (ExternalServicesSetup),
/// then a fallback — here the pickup folder, so a booking never fails because email is down.
/// </summary>
public class MailtrapEmailSender(
    HttpClient http,
    IConfiguration configuration,
    PickupDirectoryEmailSender fallback,
    ILogger<MailtrapEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken ct)
    {
        var token = configuration["MAILTRAP_API_TOKEN"];
        var inbox = configuration["MAILTRAP_INBOX_ID"];
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(inbox))
        {
            logger.LogInformation("Mailtrap is not configured; email written to the pickup folder");
            await fallback.SendAsync(to, subject, body, ct);
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/send/{Uri.EscapeDataString(inbox)}")
            {
                Content = JsonContent.Create(new
                {
                    from = new { email = configuration["MAIL_FROM"] ?? "no-reply@tripcraft.test", name = "TripCraft" },
                    to = new[] { new { email = to } },
                    subject,
                    text = body,
                    category = "TripCraft"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await http.SendAsync(request, ct);
            if (response.IsSuccessStatusCode)
                return;
            logger.LogWarning("Mailtrap returned {StatusCode}; email written to the pickup folder", (int)response.StatusCode);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Mailtrap failed ({ErrorType}); email written to the pickup folder", ex.GetType().Name);
        }
        await fallback.SendAsync(to, subject, body, ct);
    }
}
