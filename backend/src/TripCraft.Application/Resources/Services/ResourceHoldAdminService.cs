using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface IResourceHoldAdminService
{
    Task<PagedResult<HoldDto>> ListAsync(HoldListQuery query, CancellationToken ct);
    Task<HoldDto> CreateAsync(CurrentUser user, CreateHoldRequest request, CancellationToken ct);
    Task<HoldDto> ReleaseAsync(CurrentUser user, Guid id, CancellationToken ct);
    Task<HoldDto> GetAsync(Guid id, CancellationToken ct);
    Task<HoldDto> UpdateBlockAsync(CurrentUser user, Guid id, UpdateBlockRequest request, CancellationToken ct);
}

/// <summary>The manager's view of holds (availability calendar), manual blocks and releases.</summary>
public class ResourceHoldAdminService(
    IResourceRepository resources,
    ResourceHoldService holdService,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IResourceHoldAdminService
{
    public static readonly IReadOnlyDictionary<string, Expression<Func<ResourceHold, object>>> SortableFields =
        new Dictionary<string, Expression<Func<ResourceHold, object>>>
        {
            ["fromDate"] = h => h.FromDate,
            ["toDate"] = h => h.ToDate,
            ["resourceType"] = h => h.ResourceType
        };

    public async Task<PagedResult<HoldDto>> ListAsync(HoldListQuery query, CancellationToken ct)
    {
        var q = resources.Holds();
        if (query.From.HasValue)
            q = q.Where(h => h.ToDate >= query.From.Value);
        if (query.To.HasValue)
            q = q.Where(h => h.FromDate <= query.To.Value);
        if (query.Type.HasValue)
            q = q.Where(h => h.ResourceType == query.Type.Value);
        if (query.Status.HasValue)
            q = q.Where(h => h.Status == query.Status.Value);

        var page = await q.ApplySort(query.Sort, SortableFields, "fromDate")
            .ToPagedResultAsync(query.Page, query.PageSize, h => h, ct);
        var names = await NamesAsync(page.Items, ct);
        return new PagedResult<HoldDto>(page.Items.Select(h => ToDto(h, names)).ToList(), page.Page, page.PageSize, page.Total);
    }

    /// <summary>A manual block goes through the same overlap check as an approval, in its own transaction.</summary>
    public async Task<HoldDto> CreateAsync(CurrentUser user, CreateHoldRequest request, CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await holdService.CreateHoldAsync(new ResourceHoldRequest(request.ResourceType, request.ResourceId, Guid.Empty,
            request.FromDate, request.ToDate, request.Quantity, request.Note?.Trim()), ct);
        var hold = resources.StagedHolds().Single();
        audit.Record(user.Id, "ResourceHoldCreated", nameof(ResourceHold), hold.Id, null,
            new { hold.ResourceType, hold.ResourceId, hold.FromDate, hold.ToDate, hold.Quantity, hold.Note });
        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return ToDto(hold, await NamesAsync([hold], ct));
    }

    public async Task<HoldDto> ReleaseAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var hold = await resources.FindHoldAsync(id, ct) ?? throw new NotFoundException("Hold not found.");
        if (hold.Status == HoldStatus.Released)
            throw new ConflictException("This hold is already released.");
        hold.Status = HoldStatus.Released;
        audit.Record(user.Id, "ResourceHoldReleased", nameof(ResourceHold), hold.Id,
            new { Status = nameof(HoldStatus.Held) }, new { Status = nameof(HoldStatus.Released) });
        await unitOfWork.SaveChangesAsync(ct);
        return ToDto(hold, await NamesAsync([hold], ct));
    }

    public async Task<HoldDto> GetAsync(Guid id, CancellationToken ct)
    {
        var hold = await resources.FindHoldAsync(id, ct) ?? throw new NotFoundException("Hold not found.");
        return ToDto(hold, await NamesAsync([hold], ct));
    }

    /// <summary>
    /// Changes a manual block (v1.1 availability grid). The block is replaced in one transaction: the old one is
    /// released first, so the overlap check of the new dates does not count the block against itself.
    /// A trip's hold cannot be edited here (409): it changes only through Confirm, cancellation or a guide change.
    /// </summary>
    public async Task<HoldDto> UpdateBlockAsync(CurrentUser user, Guid id, UpdateBlockRequest request, CancellationToken ct)
    {
        var old = await resources.FindHoldAsync(id, ct) ?? throw new NotFoundException("Hold not found.");
        if (old.TripRequestId is not null)
            throw new ConflictException("This hold belongs to a trip; only manual blocks can be edited.");
        if (old.Status == HoldStatus.Released)
            throw new ConflictException("This block was already released.");

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        old.Status = HoldStatus.Released;
        await unitOfWork.SaveChangesAsync(ct);
        try
        {
            await holdService.CreateHoldAsync(new ResourceHoldRequest(old.ResourceType, old.ResourceId, Guid.Empty,
                request.FromDate, request.ToDate, request.Quantity, request.Note?.Trim()), ct);
        }
        catch (ConflictException)
        {
            unitOfWork.DiscardChanges(); // the transaction is not committed, so the release is rolled back too
            throw;
        }
        var replacement = resources.StagedHolds().Single();
        audit.Record(user.Id, "ResourceBlockUpdated", nameof(ResourceHold), replacement.Id,
            new { HoldId = old.Id, old.FromDate, old.ToDate, old.Quantity, old.Note },
            new { replacement.FromDate, replacement.ToDate, replacement.Quantity, replacement.Note });
        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return ToDto(replacement, await NamesAsync([replacement], ct));
    }

    /// <summary>Display names for the resources of these holds: guide name, vehicle registration, "Hotel — room type".</summary>
    private async Task<Dictionary<Guid, string>> NamesAsync(IReadOnlyCollection<ResourceHold> holds, CancellationToken ct)
    {
        var ids = holds.Select(h => h.ResourceId).Distinct().ToList();
        var names = await resources.Guides().Where(g => ids.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Name, ct);
        foreach (var v in await resources.Vehicles().Where(v => ids.Contains(v.Id)).ToListAsync(ct))
            names[v.Id] = v.RegistrationNo;
        foreach (var r in await resources.RoomTypes().Where(r => ids.Contains(r.Id)).ToListAsync(ct))
            names[r.Id] = $"{r.Hotel!.Name} — {r.Name}";
        return names;
    }

    private static HoldDto ToDto(ResourceHold h, IReadOnlyDictionary<Guid, string> names) =>
        new(h.Id, h.ResourceType, h.ResourceId, names.GetValueOrDefault(h.ResourceId, "(deleted)"), h.TripRequestId,
            h.FromDate, h.ToDate, h.Quantity, h.Status.ToString(), h.Note);
}
