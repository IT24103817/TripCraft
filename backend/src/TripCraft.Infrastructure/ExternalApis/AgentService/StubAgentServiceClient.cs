using Microsoft.Extensions.Logging;
using TripCraft.Application.Workflows;

namespace TripCraft.Infrastructure.ExternalApis.AgentService;

/// <summary>
/// Placeholder until the Python LangGraph service exists. Accepts every request and only logs it.
/// TODO: replace with an HttpClient-based client that POSTs to AGENT_SERVICE_URL with the
/// X-Internal-Key header, a 30 s timeout, and wraps failures in AgentServiceException.
/// </summary>
public class StubAgentServiceClient(ILogger<StubAgentServiceClient> logger) : IAgentServiceClient
{
    public Task StartWorkflowAsync(StartAgentWorkflowRequest request, CancellationToken ct)
    {
        logger.LogInformation(
            "Stub agent service: accepted workflow {WorkflowId} for trip {TripRequestId} ({Days} skeleton days)",
            request.WorkflowId, request.TripRequestId, request.Skeleton.Count);
        return Task.CompletedTask;
    }
}
