using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Resources.Dtos;

public record GuideDto(Guid Id, Guid? UserId, string Name, string Phone, IReadOnlyList<string> Languages,
    decimal DayRateLkr, int MaxPax, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static GuideDto FromEntity(Guide g) => new(g.Id, g.UserId, g.Name, g.Phone,
        g.Languages.Select(l => l.LanguageCode).OrderBy(c => c).ToList(), g.DayRateLkr, g.MaxPax, g.IsActive,
        g.CreatedAt, g.UpdatedAt);
}

/// <summary>POST /api/guides and PUT /api/guides/{id}. Languages are ISO 639-1 codes, e.g. ["en","de"].</summary>
public record SaveGuideRequest(string Name, string Phone, List<string> Languages, decimal DayRateLkr, int MaxPax,
    bool IsActive, Guid? UserId);

/// <summary>GET /api/guides?search=&amp;language=&amp;isActive=&amp;sort=&amp;page=&amp;pageSize=</summary>
public class GuideListQuery : PagedQuery
{
    public string? Language { get; set; }
    public bool? IsActive { get; set; }
}
