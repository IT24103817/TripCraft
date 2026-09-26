using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;

namespace TripCraft.Tests.Common;

/// <summary>Every error is RFC 7807 ProblemDetails with a traceId; unexpected errors never leak details.</summary>
public class ErrorHandlingTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private static async Task<JsonElement> ProblemAsync(HttpResponseMessage response)
    {
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }

    [Fact]
    public async Task Malformed_json_body_returns_400_problem_details()
    {
        var client = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        var response = await client.PostAsync("/api/trip-requests",
            new StringContent("{ \"objective\": \"Kandy\", \"pax\": ", System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await ProblemAsync(response);
        problem.GetProperty("status").GetInt32().Should().Be(400);
        problem.TryGetProperty("traceId", out _).Should().BeTrue();
        problem.TryGetProperty("errors", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Unknown_id_returns_404_problem_details()
    {
        var client = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        var response = await client.GetAsync($"/api/trip-requests/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await ProblemAsync(response);
        problem.GetProperty("detail").GetString().Should().Be("Trip request not found.");
        problem.TryGetProperty("traceId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Missing_or_bad_token_returns_401_problem_details()
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", "not.a.jwt");

        var response = await client.GetAsync("/api/trip-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ProblemAsync(response)).GetProperty("status").GetInt32().Should().Be(401);
    }

    [Fact]
    public async Task Unhandled_exception_returns_500_with_trace_id_and_no_internal_details()
    {
        var failing = new Mock<ITripRequestService>();
        failing.Setup(s => s.ListAsync(It.IsAny<CurrentUser>(), It.IsAny<TripRequestListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Npgsql: password authentication failed for user secret_admin"));
        var app = factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddScoped(_ => failing.Object)));
        var client = await ((TestWebApplicationFactory)factory).CreateClientAsAsync("manager1@tripcraft.test");
        var faultyClient = app.CreateClient();
        faultyClient.DefaultRequestHeaders.Authorization = client.DefaultRequestHeaders.Authorization;

        var response = await faultyClient.GetAsync("/api/trip-requests");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var body = await response.Content.ReadAsStringAsync();
        var problem = await ProblemAsync(response);
        problem.GetProperty("title").GetString().Should().Be("An unexpected error occurred");
        problem.TryGetProperty("traceId", out _).Should().BeTrue();
        // No detail for unexpected errors: the field is omitted (or null).
        if (problem.TryGetProperty("detail", out var detail))
            detail.ValueKind.Should().Be(JsonValueKind.Null);
        body.Should().NotContain("secret_admin").And.NotContain("InvalidOperationException").And.NotContain(" at TripCraft");
    }
}
