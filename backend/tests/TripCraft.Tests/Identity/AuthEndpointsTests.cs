using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Infrastructure.Persistence.Seeding;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Identity;

public class AuthEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Register_then_login_returns_jwt_with_tourist_role()
    {
        var client = factory.CreateClient();
        var register = new RegisterRequest("new.tourist@example.com", "Str0ngPass!", "New Tourist");

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", register);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await registerResponse.Content.ReadFromJsonAsync<UserDto>(TestJson.Options);
        created!.Role.Should().Be("Tourist");

        var loginResponse = await AuthHelper.LoginAsync(client, register.Email, register.Password);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(auth!.AccessToken);
        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == created.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "new.tourist@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Tourist");
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401_problem_details()
    {
        var client = factory.CreateClient();

        var response = await AuthHelper.LoginAsync(client, "tourist1@tripcraft.test", "WrongPassw0rd!");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("traceId").And.NotContain("PasswordHash");
    }

    [Fact]
    public async Task Tourist_calling_admin_endpoint_returns_403()
    {
        var client = factory.CreateClient();
        await AuthHelper.AuthenticateAsync(client, "tourist1@tripcraft.test", DataSeeder.DemoPassword);

        var response = await client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_can_list_seeded_users()
    {
        var client = factory.CreateClient();
        await AuthHelper.AuthenticateAsync(client, "admin1@tripcraft.test", DataSeeder.DemoPassword);

        var users = await client.GetFromJsonAsync<List<UserDto>>("/api/admin/users", TestJson.Options);

        users.Should().Contain(u => u.Email == "manager1@tripcraft.test" && u.Role == "OperationsManager");
    }

    [Fact]
    public async Task Me_without_token_returns_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
