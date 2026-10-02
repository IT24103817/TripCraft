using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;

namespace TripCraft.Api.Controllers.Trips;

/// <summary>The signed-in tourist's own profile (v1.1): used by the package "Book as is" sheet.</summary>
[ApiController]
[Route("api/tourists/me")]
[Authorize(Roles = Roles.Tourist)]
public class TouristsController(IPassportPhotoService passportPhotos) : ControllerBase
{
    /// <summary>Nationality, masked passport number and whether a passport photo is on file.</summary>
    [HttpGet]
    public async Task<ActionResult<TouristProfileDto>> Get(CancellationToken ct) =>
        Ok(await passportPhotos.GetProfileAsync(User.GetCurrentUser(), ct));

    /// <summary>Passport photo for the profile (multipart field "file"); JPEG/PNG up to 5 MB, checked by content.</summary>
    [HttpPost("passport-photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PassportPhotoService.MaxBytes + 64 * 1024)]
    public async Task<ActionResult<TouristProfileDto>> UploadPassportPhoto(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await passportPhotos.UploadForProfileAsync(User.GetCurrentUser(), stream, file.Length, ct));
    }
}
