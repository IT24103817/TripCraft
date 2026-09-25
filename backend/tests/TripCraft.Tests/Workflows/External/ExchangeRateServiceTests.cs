using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using TripCraft.Infrastructure.External;

namespace TripCraft.Tests.Workflows.External;

public class ExchangeRateServiceTests
{
    private const string Success = """{"result":"success","time_last_update_unix":1790812800,"rates":{"USD":1,"LKR":299.5}}""";
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    private ExchangeRateService Service(StubHandler handler, params (string, string)[] config) =>
        new(handler.Client(), _cache, StubHandler.Config(config), NullLogger<ExchangeRateService>.Instance);

    [Fact]
    public async Task Success_is_cached_for_an_hour()
    {
        var handler = StubHandler.Json(Success);

        var first = await Service(handler).GetUsdToLkrAsync(CancellationToken.None);
        var second = await Service(handler).GetUsdToLkrAsync(CancellationToken.None);

        first.Rate.Should().Be(299.5m);
        first.Stale.Should().BeFalse();
        second.Should().Be(first);
        handler.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task Provider_failure_returns_the_last_known_rate_flagged_stale()
    {
        await Service(StubHandler.Json(Success)).GetUsdToLkrAsync(CancellationToken.None);
        _cache.Remove(ExchangeRateService.FreshKey); // the 1 h cache expired

        var rate = await Service(StubHandler.Throws(new HttpRequestException("down"))).GetUsdToLkrAsync(CancellationToken.None);

        rate.Rate.Should().Be(299.5m);
        rate.Stale.Should().BeTrue();
    }

    [Fact]
    public async Task Failure_before_any_success_uses_the_configured_fallback_flagged_stale()
    {
        var rate = await Service(StubHandler.Status(HttpStatusCode.InternalServerError), ("FX_FALLBACK_LKR_PER_USD", "305"))
            .GetUsdToLkrAsync(CancellationToken.None);

        rate.Rate.Should().Be(305m);
        rate.Stale.Should().BeTrue();
        rate.AsOf.Should().Be(DateTime.UnixEpoch);
    }

    [Fact]
    public async Task Response_without_lkr_is_treated_as_a_failure()
    {
        var rate = await Service(StubHandler.Json("""{"result":"error"}""")).GetUsdToLkrAsync(CancellationToken.None);

        rate.Stale.Should().BeTrue();
        rate.Rate.Should().Be(300m);
    }
}
