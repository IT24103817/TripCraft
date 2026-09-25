using FluentValidation;
using FluentValidation.Results;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Services;

/// <summary>
/// Passport photo upload from the Flutter camera (PLAN.md sections 6 and 10). Only the owning Tourist may
/// upload. JPEG or PNG only (checked by the file's first bytes, not just the declared type), at most 5 MB,
/// saved under a random name. The photo belongs to the tourist profile, so the newest upload wins.
/// </summary>
public class PassportPhotoService(
    ITripRequestRepository trips,
    IPassportPhotoStore store,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IPassportPhotoService
{
    public const long MaxBytes = 5 * 1024 * 1024;

    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47];

    public async Task<PassportPhotoResponse> UploadAsync(CurrentUser user, Guid tripRequestId, Stream content,
        string contentType, long length, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (!user.IsTourist || trip.Tourist?.UserId != user.Id)
            throw new ForbiddenException("Only the tourist who owns this trip can upload the passport photo.");

        if (length is <= 0 or > MaxBytes)
            throw Invalid("The photo must be between 1 byte and 5 MB.");

        // Read the whole file (≤ 5 MB) so the signature and the size are checked on the real bytes.
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        if (buffer.Length > MaxBytes)
            throw Invalid("The photo must be at most 5 MB.");
        var extension = DetectExtension(buffer.GetBuffer().AsSpan(0, (int)buffer.Length))
                        ?? throw Invalid("The photo must be a JPEG or PNG image.");

        buffer.Position = 0;
        var key = await store.SaveAsync(buffer, extension, ct);
        var tourist = trip.Tourist!;
        var hadPhoto = tourist.PassportPhotoUrl is not null;
        tourist.PassportPhotoUrl = key;

        // The storage key is not written to the audit log; only the fact and the size.
        audit.Record(user.Id, "PassportPhotoUploaded", nameof(Tourist), tourist.Id,
            new { HadPhoto = hadPhoto }, new { HadPhoto = true, SizeBytes = buffer.Length, TripRequestId = trip.Id });
        await unitOfWork.SaveChangesAsync(ct);

        return new PassportPhotoResponse(trip.Id, extension == ".png" ? "image/png" : "image/jpeg", buffer.Length,
            DateTime.UtcNow);
    }

    /// <summary>".jpg" or ".png" from the file signature; null for anything else.</summary>
    public static string? DetectExtension(ReadOnlySpan<byte> bytes) =>
        bytes.StartsWith(JpegSignature) ? ".jpg" : bytes.StartsWith(PngSignature) ? ".png" : null;

    private static ValidationException Invalid(string message) =>
        new([new ValidationFailure("file", message)]);
}
