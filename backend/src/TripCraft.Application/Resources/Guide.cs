using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

/// <summary>A tour guide. UserId links the guide's login (role Guide) so they can see their own schedule.</summary>
public class Guide : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal DayRateLkr { get; set; }
    public int MaxPax { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Soft delete: holds and past schedules still point at the guide.</summary>
    public bool IsDeleted { get; set; }

    public List<GuideLanguage> Languages { get; set; } = [];
}
