namespace TripCraft.Application.Common.Exceptions;

/// <summary>
/// Mapped to 403 Forbidden. Thrown by resource-based checks in services,
/// e.g. a tourist opening another tourist's trip.
/// </summary>
public class ForbiddenException(string message) : Exception(message);
