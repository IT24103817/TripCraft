namespace TripCraft.Application.Workflows.Ports;

/// <summary>
/// The Resource Management business operation (Student B): check for overlapping holds and stage a
/// new ResourceHold on the current unit of work. Called only inside the approval transaction, so the
/// hold is committed or rolled back together with the approval.
/// </summary>
public interface IResourceHoldService
{
    /// <exception cref="Common.Exceptions.ConflictException">The resource is already held in that range.</exception>
    Task CreateHoldAsync(ResourceHoldRequest hold, CancellationToken ct);

    /// <summary>
    /// Stages the release of the trip's Held holds (all of them, or only one resource type, e.g. the guide when
    /// it is swapped). Returns how many were released. Runs inside the caller's transaction.
    /// </summary>
    Task<int> ReleaseTripHoldsAsync(Guid tripRequestId, ResourceType? onlyType, CancellationToken ct);
}

/// <summary>
/// A hold for [From, To] inclusive. Rooms use one hold per room type and night with a quantity.
/// TripRequestId is Guid.Empty for a manual block (e.g. vehicle maintenance), which may carry a Note.
/// </summary>
public record ResourceHoldRequest(
    ResourceType Type, Guid ResourceId, Guid TripRequestId, DateOnly From, DateOnly To, int Quantity, string? Note = null);
