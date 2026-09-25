using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Services;

public interface IWorkflowQueryService
{
    Task<WorkflowDto> GetAsync(CurrentUser user, Guid id, CancellationToken ct);

    /// <summary>The newest workflow of a trip (the mobile app only knows the trip id). 404 if planning never started.</summary>
    Task<WorkflowDto> GetLatestForTripAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct);
    Task<IReadOnlyList<AgentStepDto>> ListStepsAsync(CurrentUser user, Guid id, CancellationToken ct);
    Task<PagedResult<WorkflowSummaryDto>> ListAsync(WorkflowListQuery query, CancellationToken ct);
}
