using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Trips;

/// <summary>Tourist profile. One per user account with the Tourist role.</summary>
public class Tourist : BaseEntity
{
    public Guid UserId { get; set; }
    public string Nationality { get; set; } = string.Empty;

    /// <summary>Only the last 4 characters are kept, e.g. "****4521" (PLAN.md section 10).</summary>
    public string PassportNumberMasked { get; set; } = string.Empty;

    public string? PassportPhotoUrl { get; set; }
}
