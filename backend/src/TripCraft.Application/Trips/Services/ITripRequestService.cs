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

    /// <summary>Audit events of the trip and its agent workflows, oldest first.</summary>
    Task<IReadOnlyList<TripHistoryEntryDto>> GetHistoryAsync(CurrentUser user, Guid id, CancellationToken ct);

    /// <summary>Cancels with a reason, through TripStatusMachine; the tourist only until the cut-off. Releases holds.</summary>
    Task<TripRequestDto> CancelAsync(CurrentUser user, Guid id, string reason, CancellationToken ct);

    /// <summary>Whether the caller may cancel now, the last day, and the operator contact.</summary>
    Task<CancellationInfoDto> GetCancellationInfoAsync(CurrentUser user, Guid id, CancellationToken ct);
}
