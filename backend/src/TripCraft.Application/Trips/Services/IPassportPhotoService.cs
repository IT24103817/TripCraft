using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Services;

public interface IPassportPhotoService
{
    Task<PassportPhotoResponse> UploadAsync(CurrentUser user, Guid tripRequestId, Stream content, string contentType,
        long length, CancellationToken ct);

    /// <summary>
    /// v1.1: the photo for the signed-in tourist's profile without a trip (the package "Book" sheet asks for it
    /// before booking). Creates the tourist profile if this is the tourist's first action.
    /// </summary>
    Task<TouristProfileDto> UploadForProfileAsync(CurrentUser user, Stream content, long length, CancellationToken ct);

    /// <summary>The signed-in tourist's profile: nationality, masked passport number, whether a photo is on file.</summary>
    Task<TouristProfileDto> GetProfileAsync(CurrentUser user, CancellationToken ct);
}
