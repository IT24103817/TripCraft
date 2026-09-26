using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Workflows;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Workflows;

public class AgentWorkflowRepository(AppDbContext db) : IAgentWorkflowRepository
{
    public void Add(AgentWorkflow workflow) => db.AgentWorkflows.Add(workflow);

    public Task<AgentWorkflow?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.AgentWorkflows.FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<AgentWorkflow?> GetLatestForTripAsync(Guid tripRequestId, CancellationToken ct) =>
        db.AgentWorkflows.Where(w => w.TripRequestId == tripRequestId)
            .OrderByDescending(w => w.StartedAt)
            .FirstOrDefaultAsync(ct);

    public Task<bool> HasActiveForTripAsync(Guid tripRequestId, CancellationToken ct) =>
        db.AgentWorkflows.AnyAsync(w => w.TripRequestId == tripRequestId && w.Status == AgentWorkflowStatus.Planning, ct);

    public IQueryable<AgentWorkflow> Query() => db.AgentWorkflows.AsNoTracking();

    public void AddStep(AgentStep step) => db.AgentSteps.Add(step);

    public async Task<int> NextStepNoAsync(Guid workflowId, CancellationToken ct) =>
        (await db.AgentSteps.Where(s => s.WorkflowId == workflowId).MaxAsync(s => (int?)s.StepNo, ct) ?? 0) + 1;

    public Task<List<AgentStep>> ListStepsAsync(Guid workflowId, CancellationToken ct) =>
        db.AgentSteps.AsNoTracking().Where(s => s.WorkflowId == workflowId).OrderBy(s => s.StepNo).ToListAsync(ct);
}
