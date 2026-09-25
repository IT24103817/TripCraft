using System.Net.Http.Headers;
using System.Net.Http.Json;
using TripCraft.Application.Identity.Dtos;

namespace TripCraft.Tests.Common;

public static class AuthHelper
{
    public static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password) =>
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));

    /// <summary>Logs in and sets the bearer token on the client for the following requests.</summary>
    public static async Task AuthenticateAsync(HttpClient client, string email, string password)
    {
        var response = await LoginAsync(client, email, password);
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);
    }
}
