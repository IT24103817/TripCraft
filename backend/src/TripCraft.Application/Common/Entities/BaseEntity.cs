namespace TripCraft.Application.Common.Entities;

/// <summary>
/// Every table has a uuid primary key and audit timestamps.
/// CreatedAt and UpdatedAt are filled in by AppDbContext.SaveChangesAsync — never set them by hand.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
