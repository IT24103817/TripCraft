using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Identity;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Common;

public static class TestClients
{
    /// <summary>
    /// An HttpClient signed in as a seeded user. The token comes straight from ITokenService so
    /// endpoint tests do not use up the login rate limit (5/min).
    /// </summary>
    public static async Task<HttpClient> CreateClientAsAsync(this TestWebApplicationFactory factory, string email)
    {
        var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == email);
        var (token, _) = scope.ServiceProvider.GetRequiredService<ITokenService>().CreateToken(user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Runs a query against the test database, e.g. to check audit rows were written.</summary>
    public static async Task<T> QueryDbAsync<T>(this TestWebApplicationFactory factory, Func<AppDbContext, Task<T>> query)
    {
        using var scope = factory.Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }
}
