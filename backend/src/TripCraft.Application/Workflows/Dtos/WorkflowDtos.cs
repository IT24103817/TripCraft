using System.Text.Json;
using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Workflows.Dtos;

/// <summary>
/// GET /api/workflows/{id}: status, plan, validation, current step, outcome and timings. ResourceNames maps the
/// proposal's guide, vehicle and room-type ids to display names, so reviewers never read raw ids.
/// </summary>
public record WorkflowDto(
    Guid Id,
    Guid TripRequestId,
    string Status,
    string? CurrentStep,
    JsonElement Plan,
    JsonElement? ValidationResult,
    JsonElement? FinalOutcome,
    string? ErrorSummary,
    DateTime StartedAt,
    DateTime? FinishedAt,
    long? ElapsedMs,
    int StepCount,
    long TotalStepDurationMs,
    IReadOnlyDictionary<string, string> ResourceNames);

public record WorkflowSummaryDto(Guid Id, Guid TripRequestId, string Status, string? CurrentStep,
    DateTime StartedAt, DateTime? FinishedAt, string? ErrorSummary, string Objective);

public record AgentStepDto(
    Guid Id,
    int StepNo,
    string AgentName,
    string? ToolName,
    JsonElement InputSummary,
    JsonElement OutputSummary,
    JsonElement ValidationResult,
    int DurationMs,
    int Retries,
    string Status,
    DateTime CreatedAt);

/// <summary>
/// GET /api/workflows?status=&amp;search=&amp;sort=&amp;page=&amp;pageSize= (staff only). Search matches the trip
/// objective; sort is one of WorkflowQueryService.SortableFields (default newest first).
/// </summary>
public class WorkflowListQuery : PagedQuery
{
    public AgentWorkflowStatus? Status { get; set; }
}
