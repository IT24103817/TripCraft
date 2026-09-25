using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Services;

public interface ITripRequestService
{
    Task<TripRequestDto> CreateAsync(CurrentUser user, CreateTripRequestRequest request, CancellationToken ct);
    Task<PagedResult<TripRequestDto>> ListAsync(CurrentUser user, TripRequestListQuery query, CancellationToken ct);
    Task<TripRequestDto> GetAsync(CurrentUser user, Guid id, CancellationToken ct);
    Task<TripRequestDto> UpdateAsync(CurrentUser user, Guid id, UpdateTripRequestRequest request, CancellationToken ct);
    Task<ItineraryDto> GetItineraryAsync(CurrentUser user, Guid id, CancellationToken ct);
}
