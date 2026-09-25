namespace TripCraft.Application.Workflows;

/// <summary>
/// Thrown by the placeholder ports while Resource Management (Student B) or Quotations (Student C)
/// are not merged yet. Mapped to 503 Service Unavailable.
/// </summary>
public class ComponentNotAvailableException(string component)
    : Exception($"{component} is not available yet.");
