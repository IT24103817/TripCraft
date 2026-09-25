namespace TripCraft.Application.Common.Exceptions;

/// <summary>Mapped to 404 Not Found by the exception middleware.</summary>
public class NotFoundException(string message) : Exception(message);
