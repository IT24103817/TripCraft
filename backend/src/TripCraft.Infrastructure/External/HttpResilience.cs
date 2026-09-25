using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace TripCraft.Infrastructure.External;

public static class HttpResilience
{
    /// <summary>
    /// One retry on a network error, a 5xx/408 reply or a per-try timeout. The retry handler is added
    /// first so it wraps the timeout: each try gets its own `perTry` budget.
    /// </summary>
    public static IHttpClientBuilder AddRetryAndTimeout(this IHttpClientBuilder builder, TimeSpan perTry) =>
        builder
            .ConfigureHttpClient(c => c.Timeout = perTry * 2 + TimeSpan.FromSeconds(1))
            .AddPolicyHandler(HttpPolicyExtensions.HandleTransientHttpError().Or<TimeoutRejectedException>().RetryAsync(1))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(perTry));
}
