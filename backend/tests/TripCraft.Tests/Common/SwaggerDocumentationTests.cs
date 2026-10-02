using System.Text.Json;
using FluentAssertions;

namespace TripCraft.Tests.Common;

/// <summary>Every endpoint is documented in Swagger with its success schema and its ProblemDetails errors.</summary>
public class SwaggerDocumentationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private async Task<JsonElement> SwaggerAsync()
    {
        var json = await factory.CreateClient().GetStringAsync("/swagger/v1/swagger.json");
        return JsonDocument.Parse(json).RootElement;
    }

    private static IEnumerable<(string Name, JsonElement Operation)> Operations(JsonElement swagger) =>
        from path in swagger.GetProperty("paths").EnumerateObject()
        from method in path.Value.EnumerateObject()
        select ($"{method.Name.ToUpperInvariant()} {path.Name}", method.Value);

    [Fact]
    public async Task Every_operation_has_a_success_schema_and_documented_errors()
    {
        var problems = new List<string>();
        foreach (var (name, operation) in Operations(await SwaggerAsync()))
        {
            var responses = operation.GetProperty("responses").EnumerateObject().ToList();
            var success = responses.Where(r => r.Name.StartsWith('2')).ToList();
            if (success.Count == 0) problems.Add($"{name}: no 2xx response");
            problems.AddRange(success.Where(r => r.Name != "204" && !r.Value.TryGetProperty("content", out _))
                .Select(r => $"{name}: {r.Name} has no schema"));
            if (!responses.Any(r => r.Name[0] is '4' or '5')) problems.Add($"{name}: no error responses");
        }

        problems.Should().BeEmpty();
    }

    [Fact]
    public async Task Send_documents_its_real_error_codes_as_problem_details()
    {
        var send = Operations(await SwaggerAsync()).Single(o => o.Name == "POST /api/quotations/{id}/send").Operation;
        var codes = send.GetProperty("responses").EnumerateObject().Select(r => r.Name);

        codes.Should().Contain(["200", "401", "403", "404", "409", "500"]);
        send.GetProperty("responses").GetProperty("409").GetProperty("content")
            .TryGetProperty("application/problem+json", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Internal_endpoints_use_the_internal_key_scheme()
    {
        var swagger = await SwaggerAsync();
        swagger.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("InternalKey", out _).Should().BeTrue();

        foreach (var (name, operation) in Operations(swagger).Where(o => o.Name.Contains("/api/internal/")))
            operation.GetProperty("security").GetRawText().Should().Contain("InternalKey", name);
    }
}
