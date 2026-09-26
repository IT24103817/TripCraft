using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TripCraft.Api.Controllers;
using TripCraft.Application.Common;

namespace TripCraft.Tests.Common;

public class HealthEndpointTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Health_is_public_and_reports_status_version_and_a_real_db_ping()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>(TestJson.Options);
        body!.Status.Should().Be("ok");
        body.Db.Should().Be("ok");
        body.DbLatencyMs.Should().BeGreaterThanOrEqualTo(0); // read by tests/perf/db-response.js
        body.Version.Should().StartWith("1.0.0");
    }

    [Fact]
    public async Task Health_returns_503_degraded_when_the_database_does_not_answer()
    {
        var failingDb = new Mock<IDatabaseHealth>();
        failingDb.Setup(d => d.CanConnectAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var app = factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddScoped(_ => failingDb.Object)));

        var response = await app.CreateClient().GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>(TestJson.Options);
        body!.Status.Should().Be("degraded");
        body.Db.Should().Be("fail");
    }
}
