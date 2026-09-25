namespace TripCraft.Application.Common.Exceptions;

/// <summary>Mapped to 401 Unauthorized by the exception middleware.</summary>
public class AuthenticationFailedException(string message) : Exception(message);
