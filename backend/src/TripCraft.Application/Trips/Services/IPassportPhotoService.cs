using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Services;

public interface IPassportPhotoService
{
    Task<PassportPhotoResponse> UploadAsync(CurrentUser user, Guid tripRequestId, Stream content, string contentType,
        long length, CancellationToken ct);
}
