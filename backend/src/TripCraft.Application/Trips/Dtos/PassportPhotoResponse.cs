namespace TripCraft.Application.Trips.Dtos;

/// <summary>Returned by POST /api/trip-requests/{id}/passport-photo. The storage key is not exposed.</summary>
public record PassportPhotoResponse(Guid TripRequestId, string ContentType, long SizeBytes, DateTime UploadedAt);
