using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TripCraft.Infrastructure.External;

namespace TripCraft.Tests.Workflows.External;

public class WeatherServiceTests
{
    private static readonly DateOnly Day = new(2026, 10, 12);

    private static WeatherService Service(StubHandler handler, string? key = "owm-test-key") =>
        new(handler.Client(), key is null ? StubHandler.Config() : StubHandler.Config(("OWM_API_KEY", key)),
            NullLogger<WeatherService>.Instance);

    [Fact]
    public async Task Provider_exception_returns_null()
    {
        var result = await Service(StubHandler.Throws(new HttpRequestException("down"))).GetForecastAsync("Ella", Day, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Unauthorized_returns_null()
    {
        var result = await Service(StubHandler.Status(HttpStatusCode.Unauthorized)).GetForecastAsync("Ella", Day, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task No_key_returns_null_without_calling_the_provider()
    {
        var handler = StubHandler.Json("{}");

        var result = await Service(handler, key: null).GetForecastAsync("Ella", Day, CancellationToken.None);

        result.Should().BeNull();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Date_outside_the_forecast_returns_null()
    {
        var result = await Service(StubHandler.Json("""{"list":[]}""")).GetForecastAsync("Ella", Day, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Success_summarises_the_slots_of_that_local_day()
    {
        // 2026-10-12 03:00 UTC and 09:00 UTC are both 12 Oct in Sri Lanka; 2026-10-12 20:00 UTC is 13 Oct.
        var handler = StubHandler.Json("""
            {"list":[
              {"dt":1791774000,"pop":0.2,"weather":[{"description":"light rain"}]},
              {"dt":1791795600,"pop":0.7,"weather":[{"description":"light rain"}]},
              {"dt":1791835200,"pop":0.9,"weather":[{"description":"thunderstorm"}]}
            ]}
            """);

        var result = await Service(handler).GetForecastAsync("Ella", Day, CancellationToken.None);

        result!.Summary.Should().Be("light rain");
        result.RainProbability.Should().Be(0.7);
    }
}
