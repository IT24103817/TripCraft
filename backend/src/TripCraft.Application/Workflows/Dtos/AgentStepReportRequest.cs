using System.Text.Json;
using System.Text.Json.Serialization;

namespace TripCraft.Application.Workflows.Dtos;

/// <summary>
/// POST /api/internal/workflows/{id}/steps. Sent by the agent service after every agent, in snake_case.
/// Only summaries — the agent service never sends prompts or model text.
/// </summary>
public record AgentStepReportRequest(
    [property: JsonPropertyName("agent_name")] string AgentName,
    [property: JsonPropertyName("tool_calls")] List<AgentToolCall>? ToolCalls,
    [property: JsonPropertyName("input_summary")] JsonElement? InputSummary,
    [property: JsonPropertyName("output_summary")] JsonElement? OutputSummary,
    [property: JsonPropertyName("validation_result")] JsonElement? ValidationResult,
    [property: JsonPropertyName("duration_ms")] int DurationMs,
    [property: JsonPropertyName("retries")] int Retries,
    [property: JsonPropertyName("status")] string Status);

public record AgentToolCall(
    [property: JsonPropertyName("tool")] string Tool,
    [property: JsonPropertyName("args")] JsonElement? Args,
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("duration_ms")] int DurationMs,
    [property: JsonPropertyName("error")] string? Error);

public record AgentStepCreatedResponse(Guid Id, int StepNo);
