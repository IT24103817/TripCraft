namespace TripCraft.Application.Workflows;

/// <summary>
/// Thrown by IAgentServiceClient when the Python agent service cannot be reached or rejects the call.
/// The caller turns this into a FailedSafely workflow instead of a 500.
/// </summary>
public class AgentServiceException(string message, Exception? inner = null) : Exception(message, inner);
