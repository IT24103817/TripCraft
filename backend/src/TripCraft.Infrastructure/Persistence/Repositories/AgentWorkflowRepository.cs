using TripCraft.Application.Workflows;

namespace TripCraft.Infrastructure.Persistence.Repositories;

public class AgentWorkflowRepository(AppDbContext db) : IAgentWorkflowRepository
{
    public void Add(AgentWorkflow workflow) => db.AgentWorkflows.Add(workflow);
}
