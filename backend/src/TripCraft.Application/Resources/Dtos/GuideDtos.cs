using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Resources.Dtos;

public record GuideDto(Guid Id, Guid? UserId, string Name, string Phone, IReadOnlyList<string> Languages,
    decimal DayRateLkr, int MaxPax, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static GuideDto FromEntity(Guide g) => new(g.Id, g.UserId, g.Name, g.Phone,
        g.Languages.Select(l => l.LanguageCode).OrderBy(c => c).ToList(), g.DayRateLkr, g.MaxPax, g.IsActive,
        g.CreatedAt, g.UpdatedAt);
}

/// <summary>Fields shared by create and update, so both use GuideDetailsValidator.</summary>
public interface IGuideDetails
{
    string Name { get; }
    string Phone { get; }
    List<string> Languages { get; }
    decimal DayRateLkr { get; }
    int MaxPax { get; }
}

/// <summary>PUT /api/guides/{id}. Languages are ISO 639-1 codes, e.g. ["en","de"].</summary>
public record SaveGuideRequest(string Name, string Phone, List<string> Languages, decimal DayRateLkr, int MaxPax,
    bool IsActive) : IGuideDetails;

/// <summary>POST /api/guides (v1.1): the guide and their Guide login are created together; Email is the login.</summary>
public record CreateGuideRequest(string Name, string Phone, List<string> Languages, decimal DayRateLkr, int MaxPax,
    bool IsActive, string Email) : IGuideDetails;

/// <summary>
/// Returned once by create and reset-password. The temporary password is never stored or shown again; the guide
/// must change it at the first login (must_change_password).
/// </summary>
public record GuideAccountDto(GuideDto Guide, string Email, string TemporaryPassword);

/// <summary>GET /api/guides?search=&amp;language=&amp;isActive=&amp;sort=&amp;page=&amp;pageSize=</summary>
public class GuideListQuery : PagedQuery
{
    public string? Language { get; set; }
    public bool? IsActive { get; set; }
}
