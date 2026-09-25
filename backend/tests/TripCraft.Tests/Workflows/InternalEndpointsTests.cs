using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Workflows;

/// <summary>The internal API used by the agent service: X-Internal-Key only, never a JWT.</summary>
public class InternalEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Theory]
    [InlineData("/api/internal/attractions?city=Kandy")]
    [InlineData("/api/internal/rates")]
    [InlineData("/api/internal/fx-rate")]
    public async Task Request_without_key_returns_401(string url)
    {
        var response = await factory.CreateClient().GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Wrong_key_or_a_manager_jwt_alone_returns_401()
    {
        var wrongKey = factory.CreateClient();
        wrongKey.DefaultRequestHeaders.Add("X-Internal-Key", "not-the-key");
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        (await wrongKey.GetAsync("/api/internal/rates")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await manager.GetAsync("/api/internal/rates")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Steps_endpoint_without_key_returns_401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync($"/api/internal/workflows/{Guid.NewGuid()}/steps", Step("planner"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tool_endpoints_with_key_reuse_trips_resources_and_external_services()
    {
        var client = factory.CreateInternalClient();

        var attractions = await client.GetFromJsonAsync<JsonElement>("/api/internal/attractions?city=kandy");
        attractions.EnumerateArray().Select(a => a.GetProperty("city").GetString()).Should().AllBe("Kandy").And.HaveCount(2);

        var distance = await client.GetFromJsonAsync<JsonElement>("/api/internal/distance?from=Kandy&to=Ella");
        distance.GetProperty("distanceKm").GetDecimal().Should().Be(140);
        (await client.GetAsync("/api/internal/distance?from=Kandy&to=Galle")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync("/api/internal/weather?city=Ella&date=2026-10-12")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var guides = await client.GetFromJsonAsync<JsonElement>("/api/internal/availability/guides?from=2026-10-10&to=2026-10-14&language=en&pax=4");
        guides.GetArrayLength().Should().Be(1);
        var vehicles = await client.GetFromJsonAsync<JsonElement>("/api/internal/availability/vehicles?from=2026-10-10&to=2026-10-14&seats=4");
        vehicles.EnumerateArray().Single().GetProperty("seats").GetInt32().Should().Be(6);

        (await client.GetFromJsonAsync<JsonElement>("/api/internal/rates")).GetProperty("marginPct").GetDecimal().Should().Be(15);
        (await client.GetFromJsonAsync<JsonElement>("/api/internal/rate-card")).GetProperty("marginPct").GetDecimal().Should().Be(15);
        (await client.GetFromJsonAsync<JsonElement>("/api/internal/fx-rate")).GetProperty("rate").GetDecimal().Should().Be(300);
    }

    [Fact]
    public async Task Invalid_tool_query_returns_400()
    {
        var response = await factory.CreateInternalClient().GetAsync("/api/internal/availability/guides?from=2026-10-10&to=2026-10-14&language=English&pax=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Steps_endpoint_creates_numbered_rows_and_updates_current_step()
    {
        var (_, workflowId) = await factory.StartPlanningAsync();
        var client = factory.CreateInternalClient();

        var first = await client.PostAsJsonAsync($"/api/internal/workflows/{workflowId}/steps", Step("planner"));
        var second = await client.PostAsJsonAsync($"/api/internal/workflows/{workflowId}/steps", Step("itinerary"));

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        (await second.Content.ReadFromJsonAsync<AgentStepCreatedResponse>(TestJson.Options))!.StepNo.Should().Be(2);
        var (steps, currentStep) = await factory.QueryDbAsync(async db => (
            await db.AgentSteps.Where(s => s.WorkflowId == workflowId).OrderBy(s => s.StepNo).ToListAsync(),
            (await db.AgentWorkflows.SingleAsync(w => w.Id == workflowId)).CurrentStep));
        steps.Select(s => s.AgentName).Should().Equal("planner", "itinerary");
        steps[0].ToolName.Should().Be("parse_dates,list_agents");
        steps[0].DurationMs.Should().Be(120);
        steps[0].InputSummary.Should().Contain("\"pax\":4").And.Contain("parse_dates");
        currentStep.Should().Be("itinerary");
    }

    [Fact]
    public async Task Step_with_an_oversized_summary_is_rejected()
    {
        var (_, workflowId) = await factory.StartPlanningAsync();
        var huge = JsonDocument.Parse(JsonSerializer.Serialize(new { text = new string('x', 9000) })).RootElement;

        var response = await factory.CreateInternalClient().PostAsJsonAsync(
            $"/api/internal/workflows/{workflowId}/steps", Step("planner") with { OutputSummary = huge });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static AgentStepReportRequest Step(string agent) => new(
        agent,
        [
            new AgentToolCall("parse_dates", null, true, 1, null),
            new AgentToolCall("list_agents", null, true, 1, null)
        ],
        JsonDocument.Parse("""{"pax":4}""").RootElement,
        JsonDocument.Parse("""{"steps":6}""").RootElement,
        JsonDocument.Parse("""{"ok":true}""").RootElement,
        120, 0, "Succeeded");
}
