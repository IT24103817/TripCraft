using System.Linq.Expressions;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Services;

public class AttractionService(
    IAttractionRepository attractions,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IAttractionService
{
    /// <summary>Whitelist for ?sort=. Anything else is rejected by the validator with 400.</summary>
    public static readonly IReadOnlyDictionary<string, Expression<Func<Attraction, object>>> SortableFields =
        new Dictionary<string, Expression<Func<Attraction, object>>>
        {
            ["name"] = a => a.Name,
            ["city"] = a => a.City,
            ["category"] = a => a.Category,
            ["durationMinutes"] = a => a.DurationMinutes,
            ["entryFeeLkr"] = a => a.EntryFeeLkr
        };

    public async Task<PagedResult<AttractionDto>> ListAsync(AttractionListQuery query, CancellationToken ct)
    {
        var q = attractions.QueryActive();

        if (!string.IsNullOrWhiteSpace(query.City))
            q = q.Where(a => a.City.ToLower() == query.City.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(query.Category))
            q = q.Where(a => a.Category.ToLower() == query.Category.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(a => a.Name.ToLower().Contains(term));
        }

        return await q
            .ApplySort(query.Sort, SortableFields, "name")
            .ToPagedResultAsync(query.Page, query.PageSize, AttractionDto.FromEntity, ct);
    }

    public async Task<AttractionDto> GetAsync(Guid id, CancellationToken ct) =>
        AttractionDto.FromEntity(await LoadAsync(id, ct));

    public async Task<AttractionDto> CreateAsync(CurrentUser user, SaveAttractionRequest request, CancellationToken ct)
    {
        await EnsureUniqueNameAsync(request, excludeId: null, ct);

        var attraction = new Attraction();
        Apply(attraction, request);
        attractions.Add(attraction);

        audit.Record(user.Id, "AttractionCreated", nameof(Attraction), attraction.Id, null, AttractionDto.FromEntity(attraction));
        await unitOfWork.SaveChangesAsync(ct);
        return AttractionDto.FromEntity(attraction);
    }

    public async Task<AttractionDto> UpdateAsync(CurrentUser user, Guid id, SaveAttractionRequest request, CancellationToken ct)
    {
        var attraction = await LoadAsync(id, ct);
        await EnsureUniqueNameAsync(request, excludeId: id, ct);

        var before = AttractionDto.FromEntity(attraction);
        Apply(attraction, request);

        audit.Record(user.Id, "AttractionUpdated", nameof(Attraction), attraction.Id, before, AttractionDto.FromEntity(attraction));
        await unitOfWork.SaveChangesAsync(ct);
        return AttractionDto.FromEntity(attraction);
    }

    /// <summary>Soft delete: the row stays so existing itineraries still show the attraction.</summary>
    public async Task DeleteAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var attraction = await LoadAsync(id, ct);
        attraction.IsDeleted = true;

        audit.Record(user.Id, "AttractionDeleted", nameof(Attraction), attraction.Id, AttractionDto.FromEntity(attraction), null);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Attraction> LoadAsync(Guid id, CancellationToken ct) =>
        await attractions.GetActiveByIdAsync(id, ct) ?? throw new NotFoundException("Attraction not found.");

    private async Task EnsureUniqueNameAsync(SaveAttractionRequest request, Guid? excludeId, CancellationToken ct)
    {
        if (await attractions.NameExistsInCityAsync(request.Name.Trim(), request.City.Trim(), excludeId, ct))
            throw new ConflictException($"An attraction called '{request.Name}' already exists in {request.City}.");
    }

    private static void Apply(Attraction a, SaveAttractionRequest r)
    {
        a.Name = r.Name.Trim();
        a.City = r.City.Trim();
        a.Category = r.Category.Trim();
        a.DurationMinutes = r.DurationMinutes;
        a.EntryFeeLkr = r.EntryFeeLkr;
        a.Latitude = r.Latitude;
        a.Longitude = r.Longitude;
    }
}
