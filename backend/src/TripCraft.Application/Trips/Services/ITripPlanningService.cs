using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Services;

public interface ITripPlanningService
{
    /// <summary>POST /api/trip-requests/{id}/start-planning — validates, builds the skeleton, starts the agents.</summary>
    Task<StartPlanningResponse> StartPlanningAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct);
}
