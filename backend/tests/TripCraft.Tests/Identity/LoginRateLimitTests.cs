using System.Net;
using FluentAssertions;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Identity;

/// <summary>Own factory so the rate-limit counter is not shared with other tests.</summary>
public class LoginRateLimitTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Sixth_login_attempt_within_a_minute_returns_429()
    {
        var client = factory.CreateClient();

        for (var i = 0; i < 5; i++)
        {
            var attempt = await AuthHelper.LoginAsync(client, "tourist1@tripcraft.test", "WrongPassw0rd!");
            attempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var sixth = await AuthHelper.LoginAsync(client, "tourist1@tripcraft.test", "WrongPassw0rd!");
        sixth.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
