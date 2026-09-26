using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Auditing;

namespace TripCraft.Infrastructure.Persistence.Auditing;

public class AuditLogReader(AppDbContext db) : IAuditLogReader
{
    /// <summary>Left join to users: system rows (seeding, agent callbacks) have no actor.</summary>
    public IQueryable<AuditLogView> Query() =>
        from log in db.AuditLogs.AsNoTracking()
        join user in db.Users.AsNoTracking() on log.ActorId equals user.Id into actors
        from actor in actors.DefaultIfEmpty()
        select new AuditLogView
        {
            Id = log.Id,
            At = log.At,
            ActorId = log.ActorId,
            ActorEmail = actor == null ? null : actor.Email,
            ActorRole = actor == null ? null : actor.Role,
            Action = log.Action,
            Entity = log.Entity,
            EntityId = log.EntityId,
            Before = log.Before,
            After = log.After
        };
}
