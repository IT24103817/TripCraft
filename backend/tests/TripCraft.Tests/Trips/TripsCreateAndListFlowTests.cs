using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Infrastructure.Persistence.Seeding;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

/// <summary>
/// End-to-end through the real HTTP pipeline (in-memory database): log in via /api/auth/login,
/// create trip requests, then list them with a filter and check the pagination fields.
/// Own factory so this class has its own login rate-limit counter.
/// </summary>
public class TripsCreateAndListFlowTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Tourist_logs_in_creates_trips_and_lists_them_with_filter_and_paging()
    {
        var client = factory.CreateClient();

        // 1. Log in through the real endpoint.
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("tourist1@tripcraft.test", DataSeeder.DemoPassword));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await login.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        // 2. Create three trips; two mention Ella.
        foreach (var objective in new[] { "Hike around Ella for a few days", "Tea country: Kandy then Ella", "Beach days in Galle" })
        {
            var created = await client.PostAsJsonAsync("/api/trip-requests", TripRequestsEndpointsTests.NewTrip(objective));
            created.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // 3. List with a filter and a page size smaller than the result count.
        var page = await client.GetFromJsonAsync<PagedResult<TripRequestDto>>(
            "/api/trip-requests?status=Submitted&search=ella&sort=createdAt&page=2&pageSize=1", TestJson.Options);

        page!.Page.Should().Be(2);
        page.PageSize.Should().Be(1);
        page.Total.Should().Be(2);
        page.Items.Should().ContainSingle()
            .Which.Objective.Should().Be("Tea country: Kandy then Ella");
    }
}
