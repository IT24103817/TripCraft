using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Identity;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface IGuideService
{
    Task<PagedResult<GuideDto>> ListAsync(GuideListQuery query, CancellationToken ct);
    Task<GuideDto> GetAsync(Guid id, CancellationToken ct);
    Task<GuideDto> CreateAsync(CurrentUser user, SaveGuideRequest request, CancellationToken ct);
    Task<GuideDto> UpdateAsync(CurrentUser user, Guid id, SaveGuideRequest request, CancellationToken ct);
    Task DeleteAsync(CurrentUser user, Guid id, CancellationToken ct);
}

/// <summary>Guide CRUD (Component B). Every change is audited; delete is soft and refused while the guide holds a future trip.</summary>
public class GuideService(IResourceRepository resources, IUserRepository users, IAuditLogger audit, IUnitOfWork unitOfWork)
    : IGuideService
{
    public static readonly IReadOnlyDictionary<string, Expression<Func<Guide, object>>> SortableFields =
        new Dictionary<string, Expression<Func<Guide, object>>>
        {
            ["name"] = g => g.Name,
            ["dayRateLkr"] = g => g.DayRateLkr,
            ["maxPax"] = g => g.MaxPax,
            ["createdAt"] = g => g.CreatedAt
        };

    public async Task<PagedResult<GuideDto>> ListAsync(GuideListQuery query, CancellationToken ct)
    {
        var q = resources.Guides();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(g => g.Name.ToLower().Contains(term) || g.Phone.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(query.Language))
        {
            var code = query.Language.Trim().ToLower();
            q = q.Where(g => g.Languages.Any(l => l.LanguageCode == code));
        }
        if (query.IsActive.HasValue)
            q = q.Where(g => g.IsActive == query.IsActive.Value);

        return await q.ApplySort(query.Sort, SortableFields, "name")
            .ToPagedResultAsync(query.Page, query.PageSize, GuideDto.FromEntity, ct);
    }

    public async Task<GuideDto> GetAsync(Guid id, CancellationToken ct) => GuideDto.FromEntity(await LoadAsync(id, ct));

    public async Task<GuideDto> CreateAsync(CurrentUser user, SaveGuideRequest request, CancellationToken ct)
    {
        await EnsureUserLinkAsync(request.UserId, guideId: null, ct);
        var guide = new Guide();
        ApplyDetails(guide, request);
        guide.Languages = request.Languages
            .Select(code => new GuideLanguage { GuideId = guide.Id, LanguageCode = code.Trim().ToLower() })
            .ToList();
        resources.Add(guide);

        audit.Record(user.Id, "GuideCreated", nameof(Guide), guide.Id, null, GuideDto.FromEntity(guide));
        await unitOfWork.SaveChangesAsync(ct);
        return GuideDto.FromEntity(guide);
    }

    public async Task<GuideDto> UpdateAsync(CurrentUser user, Guid id, SaveGuideRequest request, CancellationToken ct)
    {
        var guide = await LoadAsync(id, ct);
        await EnsureUserLinkAsync(request.UserId, guide.Id, ct);
        var before = GuideDto.FromEntity(guide);

        // Keep the languages that stay, remove the dropped ones, add the new ones (unique per guide).
        var wanted = request.Languages.Select(c => c.Trim().ToLower()).ToHashSet();
        foreach (var dropped in guide.Languages.Where(l => !wanted.Contains(l.LanguageCode)).ToList())
        {
            guide.Languages.Remove(dropped);
            resources.Remove(dropped);
        }
        foreach (var code in wanted.Where(c => guide.Languages.All(l => l.LanguageCode != c)).ToList())
        {
            // Added explicitly: a new row with a pre-set Guid key would otherwise be treated as an existing one.
            // EF's relationship fix-up then puts it into guide.Languages.
            resources.Add(new GuideLanguage { GuideId = guide.Id, LanguageCode = code });
        }
        ApplyDetails(guide, request);

        audit.Record(user.Id, "GuideUpdated", nameof(Guide), guide.Id, before, GuideDto.FromEntity(guide));
        await unitOfWork.SaveChangesAsync(ct);
        return GuideDto.FromEntity(guide);
    }

    public async Task DeleteAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var guide = await LoadAsync(id, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (await resources.Holds().AnyAsync(h => h.ResourceType == ResourceType.Guide && h.ResourceId == id
                                                  && h.Status == HoldStatus.Held && h.ToDate >= today, ct))
            throw new ConflictException("This guide is held for an upcoming trip; release the hold first.");

        guide.IsDeleted = true;
        guide.IsActive = false;
        audit.Record(user.Id, "GuideDeleted", nameof(Guide), guide.Id, GuideDto.FromEntity(guide), null);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Guide> LoadAsync(Guid id, CancellationToken ct) =>
        await resources.FindGuideAsync(id, ct) ?? throw new NotFoundException("Guide not found.");

    /// <summary>A linked login must be an active Guide user and not linked to another guide (409).</summary>
    private async Task EnsureUserLinkAsync(Guid? userId, Guid? guideId, CancellationToken ct)
    {
        if (userId is null)
            return;
        var user = await users.GetByIdAsync(userId.Value, ct);
        if (user is null || user.Role != UserRole.Guide)
            throw new ConflictException("The linked user must be an existing user with the Guide role.");
        if (await resources.Guides().AnyAsync(g => g.UserId == userId && g.Id != guideId, ct))
            throw new ConflictException("That user is already linked to another guide.");
    }

    private static void ApplyDetails(Guide guide, SaveGuideRequest request)
    {
        guide.Name = request.Name.Trim();
        guide.Phone = request.Phone.Trim();
        guide.DayRateLkr = request.DayRateLkr;
        guide.MaxPax = request.MaxPax;
        guide.IsActive = request.IsActive;
        guide.UserId = request.UserId;
    }
}
