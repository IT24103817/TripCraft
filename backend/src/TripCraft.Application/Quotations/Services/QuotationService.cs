using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Trips;

namespace TripCraft.Application.Quotations.Services;

public interface IQuotationService
{
    Task<PagedResult<QuotationDto>> ListAsync(QuotationListQuery query, CancellationToken ct);
    Task<QuotationDto> GetAsync(CurrentUser user, Guid id, CancellationToken ct);
    Task<QuotationDto> SetPaymentAsync(CurrentUser user, Guid id, bool paid, CancellationToken ct);
}

/// <summary>
/// Component C: quotation list and detail. Re-pricing (a new version) is in ProposalEditService; the tourist's
/// accept/decline in QuotationClientService; Confirm in TripConfirmationService.
/// </summary>
public class QuotationService(
    IQuotationRepository quotations,
    ITripRequestRepository trips,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IQuotationService
{
    public static readonly IReadOnlyDictionary<string, Expression<Func<Quotation, object>>> SortableFields =
        new Dictionary<string, Expression<Func<Quotation, object>>>
        {
            ["createdAt"] = q => q.CreatedAt,
            ["totalLkr"] = q => q.TotalLkr,
            ["totalUsd"] = q => q.TotalUsd,
            ["version"] = q => q.Version,
            ["status"] = q => q.Status
        };

    public async Task<PagedResult<QuotationDto>> ListAsync(QuotationListQuery query, CancellationToken ct)
    {
        var q = quotations.Query();
        if (query.Status.HasValue)
            q = q.Where(x => x.Status == query.Status.Value);
        if (query.TripRequestId.HasValue)
            q = q.Where(x => x.TripRequestId == query.TripRequestId.Value);
        if (query.From.HasValue)
        {
            var from = query.From.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            q = q.Where(x => x.CreatedAt >= from);
        }
        if (query.To.HasValue)
        {
            var toExclusive = query.To.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            q = q.Where(x => x.CreatedAt < toExclusive);
        }
        if (query.MinTotalUsd.HasValue)
            q = q.Where(x => x.TotalUsd >= query.MinTotalUsd.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Search the trip objective (quotations have no text of their own worth searching).
            var term = query.Search.Trim().ToLower();
            var tripIds = trips.Query().Where(t => t.Objective.ToLower().Contains(term)).Select(t => t.Id);
            q = q.Where(x => tripIds.Contains(x.TripRequestId));
        }

        return await q.ApplySort(query.Sort, SortableFields, "-createdAt")
            .ToPagedResultAsync(query.Page, query.PageSize, x => QuotationDto.FromEntity(x), ct);
    }

    public async Task<QuotationDto> GetAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var quotation = await LoadForUserAsync(user, id, ct);
        var decisions = await quotations.Decisions().Where(d => d.QuotationId == id).ToListAsync(ct);
        return QuotationDto.FromEntity(quotation, decisions);
    }

    /// <summary>
    /// The manager marks the deposit paid (or unpaid again) — only on the newest version, after the client accepted
    /// it (trip ClientAccepted, Confirmed, InProgress or Completed). Audited.
    /// </summary>
    public async Task<QuotationDto> SetPaymentAsync(CurrentUser user, Guid id, bool paid, CancellationToken ct)
    {
        var quotation = await quotations.FindAsync(id, ct) ?? throw new NotFoundException("Quotation not found.");
        var newest = await quotations.LatestVersionAsync(quotation.TripRequestId, ct);
        var trip = await trips.GetByIdAsync(quotation.TripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (quotation.Version != newest || quotation.AcceptedAt is null
            || trip.Status is not (TripRequestStatus.ClientAccepted or TripRequestStatus.Confirmed
                or TripRequestStatus.InProgress or TripRequestStatus.Completed))
            throw new ConflictException("The deposit can only be recorded on the newest quotation after the client accepted it.");

        var before = new { DepositPaid = quotation.DepositPaidAt is not null };
        quotation.DepositPaidAt = paid ? quotation.DepositPaidAt ?? DateTime.UtcNow : null;
        audit.Record(user.Id, paid ? "DepositMarkedPaid" : "DepositMarkedUnpaid", nameof(Quotation), quotation.Id, before,
            new { DepositPaid = paid });
        await unitOfWork.SaveChangesAsync(ct);
        return await GetAsync(user, id, ct);
    }

    /// <summary>Managers see every quotation; a tourist only the quotations of their own trips (403 otherwise).</summary>
    private async Task<Quotation> LoadForUserAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var quotation = await quotations.Query().FirstOrDefaultAsync(q => q.Id == id, ct)
                        ?? throw new NotFoundException("Quotation not found.");
        if (user.IsTourist)
        {
            var trip = await trips.GetByIdAsync(quotation.TripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
            TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);
        }
        return quotation;
    }
}
