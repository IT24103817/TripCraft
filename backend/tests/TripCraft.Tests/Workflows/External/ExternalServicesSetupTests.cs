using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Workflows.External;
using TripCraft.Infrastructure.External;

namespace TripCraft.Tests.Workflows.External;

public class ExternalServicesSetupTests
{
    [Fact]
    public void Base_url_is_the_real_provider_when_no_override_is_set()
    {
        var url = ExternalServicesSetup.BaseUrl(StubHandler.Config(), "FX_API_BASE_URL", "https://open.er-api.com/");

        url.Should().Be(new Uri("https://open.er-api.com/"));
    }

    [Fact]
    public void Base_url_override_is_used_when_set()
    {
        var config = StubHandler.Config(("FX_API_BASE_URL", "http://127.0.0.1:9/"));

        ExternalServicesSetup.BaseUrl(config, "FX_API_BASE_URL", "https://open.er-api.com/")
            .Should().Be(new Uri("http://127.0.0.1:9/"));
    }

    [Fact]
    public void OpenRouteService_defaults_to_the_heigit_host_and_keeps_its_path()
    {
        var url = ExternalServicesSetup.OrsBaseUrl(StubHandler.Config());

        url.Should().Be(new Uri("https://api.heigit.org/openrouteservice/"));
        new Uri(url, "v2/matrix/driving-car").ToString()
            .Should().Be("https://api.heigit.org/openrouteservice/v2/matrix/driving-car");
    }

    [Theory]
    [InlineData("ORS_BASE_URL", "https://ors.example.test/openrouteservice", "https://ors.example.test/openrouteservice/")]
    [InlineData("ORS_BASE_URL", "http://127.0.0.1:9/", "http://127.0.0.1:9/")]
    [InlineData("ORS_API_BASE_URL", "http://127.0.0.1:9/", "http://127.0.0.1:9/")] // the older name still works
    public void OpenRouteService_base_url_can_be_overridden_and_always_ends_with_a_slash(string name, string value,
        string expected)
    {
        ExternalServicesSetup.OrsBaseUrl(StubHandler.Config((name, value))).Should().Be(new Uri(expected));
    }

    [Fact]
    public void ORS_BASE_URL_wins_over_the_older_name()
    {
        var config = StubHandler.Config(("ORS_BASE_URL", "https://new.example.test/"),
            ("ORS_API_BASE_URL", "https://old.example.test/"));

        ExternalServicesSetup.OrsBaseUrl(config).Should().Be(new Uri("https://new.example.test/"));
    }

    [Fact]
    public async Task Unreachable_fx_host_falls_back_to_the_configured_rate_flagged_stale()
    {
        // Port 9 (discard) is closed, so the connection is refused: the same as the provider being blocked.
        var config = StubHandler.Config(("FX_API_BASE_URL", "http://127.0.0.1:9/"), ("FX_FALLBACK_LKR_PER_USD", "300"));
        var services = new ServiceCollection().AddLogging();
        services.AddExternalServices(config);
        services.AddSingleton(config);
        using var provider = services.BuildServiceProvider();

        var rate = await provider.GetRequiredService<IExchangeRateService>().GetUsdToLkrAsync(CancellationToken.None);

        rate.Rate.Should().Be(300m);
        rate.Stale.Should().BeTrue();
    }
}
