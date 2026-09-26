using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

/// <summary>One language a guide speaks, as an ISO 639-1 code ("en", "de"). Unique per guide.</summary>
public class GuideLanguage : BaseEntity
{
    public Guid GuideId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
}
