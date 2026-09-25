using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace TripCraft.Tests.Workflows.External;

/// <summary>A fake HttpMessageHandler: answers every request with the given function and records the requests.</summary>
public class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];
    public List<string> Bodies { get; } = [];

    public static StubHandler Throws(Exception ex) => new(_ => throw ex);
    public static StubHandler Status(HttpStatusCode code) => new(_ => new HttpResponseMessage(code));
    public static StubHandler Json(string json) => new(_ => new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    });

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Requests.Add(request);
        Bodies.Add(request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct));
        return respond(request);
    }

    public HttpClient Client(string baseUrl = "https://example.test/") => new(this) { BaseAddress = new Uri(baseUrl) };

    public static IConfiguration Config(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value)).Build();
}
