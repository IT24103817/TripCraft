using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Workflows;

namespace TripCraft.Infrastructure.Workflows;

/// <summary>
/// Calls the Python LangGraph service at AGENT_SERVICE_URL with the X-Internal-Key header
/// (10 s per try, one retry — see setup). Never throws for service problems: it sets the workflow
/// to FailedSafely with an error summary and returns false (PLAN.md section 5, safe failure).
/// </summary>
public class AgentServiceClient(HttpClient http, IConfiguration configuration, ILogger<AgentServiceClient> logger)
    : IAgentServiceClient
{
    public Task<bool> StartAsync(AgentWorkflow workflow, StartAgentWorkflowRequest request, CancellationToken ct) =>
        SendAsync(workflow, "run-workflow", Body(request, managerComment: null), ct);

    public Task<bool> ReplanAsync(AgentWorkflow workflow, StartAgentWorkflowRequest request, string managerComment,
        CancellationToken ct) =>
        SendAsync(workflow, "replan", Body(request, managerComment), ct);

    /// <summary>The agent's WorkflowRequest. It accepts camelCase and preferencesJson as sent here.</summary>
    private object Body(StartAgentWorkflowRequest r, string? managerComment) => new
    {
        r.WorkflowId,
        r.TripRequestId,
        r.Objective,
        r.StartDate,
        r.EndDate,
        r.Pax,
        r.BudgetUsd,
        r.PreferencesJson,
        r.Skeleton,
        PreviousViolations = r.PreviousViolations ?? [],
        Cities = r.Cities ?? [],
        r.LlmProvider, // null: the agent service uses its own LLM_PROVIDER
        ManagerComment = managerComment,
        CallbackBaseUrl = configuration["AGENT_CALLBACK_BASE_URL"]
    };

    private async Task<bool> SendAsync(AgentWorkflow workflow, string path, object body, CancellationToken ct)
    {
        var key = configuration["INTERNAL_AGENT_KEY"];
        if (http.BaseAddress is null)
            return FailSafely(workflow, "AGENT_SERVICE_URL is not configured");
        if (string.IsNullOrWhiteSpace(key))
            return FailSafely(workflow, "INTERNAL_AGENT_KEY is not configured");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = JsonContent.Create(body) };
            request.Headers.Add("X-Internal-Key", key);
            using var response = await http.SendAsync(request, ct);
            return response.IsSuccessStatusCode
                || FailSafely(workflow, $"agent service returned {(int)response.StatusCode}");
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            return FailSafely(workflow, ex.GetType().Name);
        }
    }

    private bool FailSafely(AgentWorkflow workflow, string reason)
    {
        logger.LogWarning("Agent service call failed for workflow {WorkflowId}: {Reason}", workflow.Id, reason);
        workflow.Status = AgentWorkflowStatus.FailedSafely;
        workflow.ErrorSummary = $"Agent service unavailable: {reason}";
        workflow.CurrentStep = "failed";
        workflow.FinishedAt = DateTime.UtcNow;
        return false;
    }
}
