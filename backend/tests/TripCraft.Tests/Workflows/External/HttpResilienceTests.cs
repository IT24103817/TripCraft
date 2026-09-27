using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly.Timeout;
using TripCraft.Infrastructure.External;

namespace TripCraft.Tests.Workflows.External;

/// <summary>Timing tests run on their own: under the parallel suite a thread-pool stall can outlast the timeouts.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class TimingSensitiveCollection
{
    public const string Name = "Timing-sensitive tests";
}

/// <summary>
/// The real Polly pipeline every third-party wrapper uses (HttpResilience.AddRetryAndTimeout): one retry on 5xx,
/// none on 429 (the wrapper falls back at once instead of hammering a rate-limited provider), and a per-try timeout.
/// </summary>
[Collection(TimingSensitiveCollection.Name)]
public class HttpResilienceTests
{
    /// <summary>Counts the tries; like the real handler, it honours cancellation (Polly's timeout relies on it).</summary>
    private sealed class CountingHandler(Func<int, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            respond(++Calls, ct);
    }

    private static HttpClient Client(CountingHandler handler, TimeSpan perTry)
    {
        var services = new ServiceCollection();
        services.AddHttpClient("provider", c => c.BaseAddress = new Uri("https://provider.test/"))
            .AddRetryAndTimeout(perTry)
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        return services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient("provider");
    }

    [Fact]
    public async Task A_5xx_is_retried_once_and_the_second_answer_is_used()
    {
        var handler = new CountingHandler((call, _) => Task.FromResult(new HttpResponseMessage(
            call == 1 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK)));

        var response = await Client(handler, TimeSpan.FromSeconds(5)).GetAsync("rate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        handler.Calls.Should().Be(2);
    }

    [Fact]
    public async Task A_429_is_not_retried()
    {
        var handler = new CountingHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.TooManyRequests)));

        var response = await Client(handler, TimeSpan.FromSeconds(5)).GetAsync("rate");

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        handler.Calls.Should().Be(1);
    }

    [Fact]
    public async Task A_slow_provider_times_out_per_try_and_is_retried_once()
    {
        var handler = new CountingHandler(async (_, ct) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var act = () => Client(handler, TimeSpan.FromMilliseconds(100)).GetAsync("rate");

        await act.Should().ThrowAsync<TimeoutRejectedException>();
        handler.Calls.Should().Be(2);
    }
}
