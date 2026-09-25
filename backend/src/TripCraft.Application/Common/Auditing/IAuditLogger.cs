namespace TripCraft.Application.Common.Auditing;

public interface IAuditLogger
{
    /// <summary>
    /// Stages an audit row. It is saved by the same IUnitOfWork.SaveChangesAsync call as the
    /// business change, so both are committed together or not at all.
    /// </summary>
    void Record(Guid? actorId, string action, string entity, Guid entityId, object? before, object? after);
}
