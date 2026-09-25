using System.Net;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Identity;

/// <summary>PLAN.md section 10: every request needs a valid, unexpired JWT signed with JWT_SECRET.</summary>
public class TokenValidationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private const string TestSecret = "test-secret-that-is-at-least-32-bytes-long!";
    private const string TestIssuer = "tripcraft-tests";

    private async Task<string> TokenAsync(DateTime expires, string secret = TestSecret)
    {
        var manager = await factory.QueryDbAsync(db => db.Users.SingleAsync(u => u.Email == "manager1@tripcraft.test"));
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = TestIssuer,
            Audience = TestIssuer,
            NotBefore = expires.AddMinutes(-60),
            IssuedAt = expires.AddMinutes(-60),
            Expires = expires,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity([
                new Claim("sub", manager.Id.ToString()), new Claim("email", manager.Email), new Claim("role", "OperationsManager")
            ])
        });
    }

    private async Task<HttpStatusCode> CallWith(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return (await client.GetAsync("/api/trip-requests")).StatusCode;
    }

    [Fact]
    public async Task A_valid_token_is_accepted() =>
        (await CallWith(await TokenAsync(DateTime.UtcNow.AddMinutes(30)))).Should().Be(HttpStatusCode.OK);

    [Fact]
    public async Task An_expired_token_returns_401()
    {
        // Expired 5 minutes ago: beyond the API's 1-minute clock skew.
        (await CallWith(await TokenAsync(DateTime.UtcNow.AddMinutes(-5)))).Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_token_signed_with_another_key_returns_401() =>
        (await CallWith(await TokenAsync(DateTime.UtcNow.AddMinutes(30), "a-completely-different-secret-of-32-bytes!")))
        .Should().Be(HttpStatusCode.Unauthorized);

    [Fact]
    public async Task A_tampered_token_returns_401()
    {
        var token = await TokenAsync(DateTime.UtcNow.AddMinutes(30));
        var tampered = token[..^4] + (token[^4..] == "AAAA" ? "BBBB" : "AAAA");

        (await CallWith(tampered)).Should().Be(HttpStatusCode.Unauthorized);
    }
}
