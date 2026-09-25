namespace TripCraft.Application.Common.Exceptions;

/// <summary>Mapped to 409 Conflict by the exception middleware.</summary>
public class ConflictException(string message) : Exception(message);
