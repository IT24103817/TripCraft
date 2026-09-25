using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Services;

public interface IAttractionService
{
    Task<PagedResult<AttractionDto>> ListAsync(AttractionListQuery query, CancellationToken ct);
    Task<AttractionDto> GetAsync(Guid id, CancellationToken ct);
    Task<AttractionDto> CreateAsync(CurrentUser user, SaveAttractionRequest request, CancellationToken ct);
    Task<AttractionDto> UpdateAsync(CurrentUser user, Guid id, SaveAttractionRequest request, CancellationToken ct);
    Task DeleteAsync(CurrentUser user, Guid id, CancellationToken ct);
}
