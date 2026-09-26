using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Workflows;
using TripCraft.Application.Resources.Services;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.External;

namespace TripCraft.Infrastructure.Workflows;

public static class WorkflowsSetup
{
    /// <summary>10 s per try and one retry for the agent service (it only accepts the job and returns 202).</summary>
    public static readonly TimeSpan AgentPerTryTimeout = TimeSpan.FromSeconds(10);

    public static IServiceCollection AddWorkflows(this IServiceCollection services, IConfiguration configuration)
    {
        var agentUrl = configuration["AGENT_SERVICE_URL"];
        services.AddHttpClient<IAgentServiceClient, AgentServiceClient>(c =>
            {
                if (!string.IsNullOrWhiteSpace(agentUrl))
                    c.BaseAddress = new Uri(agentUrl.TrimEnd('/') + "/");
            })
            .AddRetryAndTimeout(AgentPerTryTimeout);

        // Resource Management (Component B): the real catalog and hold service.
        services.AddScoped<IResourceCatalog>(sp => sp.GetRequiredService<ResourceCatalog>());
        services.AddScoped<IResourceHoldService>(sp => sp.GetRequiredService<ResourceHoldService>());
        // TODO(Student C): replace with the real quotation store.
        services.AddScoped<IQuotationStore, PendingQuotationStore>();

        return services;
    }
}
