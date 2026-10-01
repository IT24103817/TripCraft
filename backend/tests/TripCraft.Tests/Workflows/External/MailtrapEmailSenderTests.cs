using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TripCraft.Infrastructure.External;
using TripCraft.Infrastructure.Persistence.Notifications;

namespace TripCraft.Tests.Workflows.External;

/// <summary>Mailtrap sandbox (fourth integration): success, and the pickup-folder fallback on every failure.</summary>
public class MailtrapEmailSenderTests
{
    private readonly string _pickup = Path.Combine(Path.GetTempPath(), "tripcraft-mailtrap-tests", Guid.NewGuid().ToString("N"));

    private (MailtrapEmailSender Sender, PickupDirectoryEmailSender Fallback) Create(StubHandler handler, bool configured = true)
    {
        var config = configured
            ? StubHandler.Config(("MAILTRAP_API_TOKEN", "mailtrap-test-token"), ("MAILTRAP_INBOX_ID", "12345"), ("EMAIL_PICKUP_DIR", _pickup))
            : StubHandler.Config(("EMAIL_PICKUP_DIR", _pickup));
        var fallback = new PickupDirectoryEmailSender(config);
        return (new MailtrapEmailSender(handler.Client("https://sandbox.mailtrap.test/"), config, fallback,
            NullLogger<MailtrapEmailSender>.Instance), fallback);
    }

    private int PickedUp() => Directory.Exists(_pickup) ? Directory.GetFiles(_pickup, "*.eml").Length : 0;

    [Fact]
    public async Task Success_posts_to_the_sandbox_inbox_with_a_bearer_token_and_writes_no_fallback()
    {
        var handler = StubHandler.Json("""{"success":true}""");
        var (sender, _) = Create(handler);

        await sender.SendAsync("tourist@x.test", "Hello", "Body", CancellationToken.None);

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.RequestUri!.AbsolutePath.Should().Be("/api/send/12345");
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.Bodies[0].Should().Contain("tourist@x.test").And.Contain("\"subject\":\"Hello\"");
        PickedUp().Should().Be(0);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task A_provider_error_falls_back_to_the_pickup_folder(HttpStatusCode status)
    {
        var (sender, _) = Create(StubHandler.Status(status));

        await sender.SendAsync("tourist@x.test", "Hello", "Body", CancellationToken.None);

        PickedUp().Should().Be(1);
    }

    [Fact]
    public async Task A_network_error_falls_back_to_the_pickup_folder()
    {
        var (sender, _) = Create(StubHandler.Throws(new HttpRequestException("down")));

        await sender.SendAsync("tourist@x.test", "Hello", "Body", CancellationToken.None);

        PickedUp().Should().Be(1);
    }

    [Fact]
    public async Task Without_settings_nothing_is_sent_to_mailtrap_and_the_email_is_picked_up()
    {
        var handler = StubHandler.Json("{}");
        var (sender, _) = Create(handler, configured: false);

        await sender.SendAsync("tourist@x.test", "Hello", "Body", CancellationToken.None);

        handler.Requests.Should().BeEmpty();
        PickedUp().Should().Be(1);
    }
}
