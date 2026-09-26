using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Services;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.External;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations.Services;

public interface IQuotationService
{
    Task<PagedResult<QuotationDto>> ListAsync(QuotationListQuery query, CancellationToken ct);
    Task<QuotationDto> GetAsync(CurrentUser user, Guid id, CancellationToken ct);
    Task<RecalculationDto> RecalculateAsync(CurrentUser user, Guid id, CancellationToken ct);
    Task<QuotationDto> AcceptAsync(CurrentUser user, Guid id, CancellationToken ct);
}

/// <summary>
/// Component C: quotation list and detail, the re-pricing business operation (today's rates and exchange rate)
/// and the tourist accepting an approved price.
/// </summary>
public class QuotationService(
    IQuotationRepository quotations,
    ITripRequestRepository trips,
    IAgentWorkflowRepository workflows,
    IAttractionRepository attractions,
    IResourceCatalog resources,
    IExchangeRateService exchangeRates,
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
    /// Business operation: re-price a Pending quotation from its proposal with today's rate card and exchange
    /// rate (e.g. after the FX provider was down and the rate was stale). Lines and totals are replaced in place.
    /// </summary>
    public async Task<RecalculationDto> RecalculateAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var quotation = await quotations.FindAsync(id, ct) ?? throw new NotFoundException("Quotation not found.");
        if (quotation.Status != QuotationStatus.Pending)
            throw new ConflictException($"Only a Pending quotation can be recalculated (it is {quotation.Status}).");
        var trip = await trips.GetByIdAsync(quotation.TripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        var workflow = quotation.WorkflowId is { } workflowId ? await workflows.GetByIdAsync(workflowId, ct) : null;
        var proposal = WorkflowJson.Deserialize<WorkflowOutcome>(workflow?.FinalOutcome)?.Proposal
                       ?? throw new ConflictException("This quotation has no agent proposal to recalculate from.");

        var card = await resources.GetRateCardAsync(ct);
        var fx = await exchangeRates.GetUsdToLkrAsync(ct);
        var breakdown = QuotationCalculator.Calculate(await PriceItemsAsync(proposal, trip.Pax, card, ct), card.MarginPct, fx.Rate);

        var before = new { quotation.TotalLkr, quotation.TotalUsd, quotation.FxRate };
        foreach (var line in quotation.Lines.ToList())
            quotations.Remove(line);
        foreach (var line in breakdown.Lines)
            quotations.Add(new QuotationLine
            {
                QuotationId = quotation.Id, LineType = line.LineType, Description = line.Description, Qty = line.Qty,
                UnitLkr = line.UnitLkr, AmountLkr = line.AmountLkr
            });
        quotation.SubtotalLkr = breakdown.SubtotalLkr;
        quotation.MarginPct = breakdown.MarginPct;
        quotation.TotalLkr = breakdown.TotalLkr;
        quotation.TotalUsd = breakdown.TotalUsd;
        quotation.FxRate = fx.Rate;
        quotation.FxAsOf = fx.AsOf;
        quotation.FxStale = fx.Stale;

        audit.Record(user.Id, "QuotationRecalculated", nameof(Quotation), quotation.Id, before,
            new { quotation.TotalLkr, quotation.TotalUsd, quotation.FxRate, quotation.FxStale });
        await unitOfWork.SaveChangesAsync(ct);

        var saved = await quotations.FindAsync(id, ct) ?? quotation;
        return new RecalculationDto(QuotationDto.FromEntity(saved), before.TotalLkr, before.TotalUsd,
            before.TotalLkr != saved.TotalLkr || before.TotalUsd != saved.TotalUsd);
    }

    /// <summary>The tourist accepts the price the manager approved (shown on the phone). Once only.</summary>
    public async Task<QuotationDto> AcceptAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        await LoadForUserAsync(user, id, ct);
        var quotation = await quotations.FindAsync(id, ct) ?? throw new NotFoundException("Quotation not found.");
        if (quotation.Status != QuotationStatus.Approved)
            throw new ConflictException("Only an approved quotation can be accepted.");
        if (quotation.AcceptedAt is not null)
            throw new ConflictException("You have already accepted this quotation.");

        quotation.AcceptedAt = DateTime.UtcNow;
        audit.Record(user.Id, "QuotationAccepted", nameof(Quotation), quotation.Id, null,
            new { quotation.AcceptedAt, quotation.TotalUsd });
        await unitOfWork.SaveChangesAsync(ct);
        return QuotationDto.FromEntity(quotation);
    }

    /// <summary>Managers see every quotation; a tourist only the quotations of their own trips (403 otherwise).</summary>
    private async Task<Quotation> LoadForUserAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var quotation = await quotations.Query().FirstOrDefaultAsync(q => q.Id == id, ct)
                        ?? throw new NotFoundException("Quotation not found.");
        if (user.IsTourist)
        {
            var trip = await trips.GetByIdAsync(quotation.TripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
            TripRequestService.EnsureCanAccess(user, trip);
        }
        return quotation;
    }

    /// <summary>Guide days, vehicle km, room-nights per room type and entry tickets, with names for the lines.</summary>
    private async Task<List<PriceItem>> PriceItemsAsync(StoredProposal proposal, int pax, RateCard card, CancellationToken ct)
    {
        var days = proposal.Days ?? [];
        var guideId = ProposalValidator.ParseId(proposal.Resources?.GuideId);
        var vehicleId = ProposalValidator.ParseId(proposal.Resources?.VehicleId);
        var guide = guideId is { } g ? await resources.GetGuideAsync(g, ct) : null;
        var vehicle = vehicleId is { } v ? await resources.GetVehicleAsync(v, ct) : null;
        if (guide is null || vehicle is null)
            throw new ConflictException("The proposed guide or vehicle no longer exists; request a revision instead.");

        var items = new List<PriceItem>
        {
            new("guide", $"Guide {guide.Name}, {days.Count} days", days.Count, card.GuideDayRates.GetValueOrDefault(guide.Id)),
            new("vehicle", $"{vehicle.Type} {vehicle.RegistrationNo}, {days.Sum(d => d.TransferKm)} km",
                days.Sum(d => d.TransferKm), card.VehicleKmRates.GetValueOrDefault(vehicle.Id))
        };
        foreach (var group in (proposal.Resources?.Rooms ?? []).GroupBy(r => r.RoomTypeId))
        {
            var room = ProposalValidator.ParseId(group.Key) is { } rid ? await resources.GetRoomTypeAsync(rid, ct) : null;
            if (room is null)
                throw new ConflictException("A proposed room type no longer exists; request a revision instead.");
            items.Add(new PriceItem("room", $"{room.HotelName} — {room.RoomTypeName}, {group.Count()} room-nights",
                group.Count(), card.RoomNightRates.GetValueOrDefault(room.RoomTypeId)));
        }

        var stopIds = days.SelectMany(d => d.Stops ?? []).Select(s => ProposalValidator.ParseId(s.AttractionId))
            .OfType<Guid>().ToList();
        var fees = await attractions.QueryActive().Where(a => stopIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => (a.Name, a.EntryFeeLkr), ct);
        foreach (var id in stopIds)
            if (fees.TryGetValue(id, out var attraction))
                items.Add(new PriceItem("entry", $"{attraction.Name} entry, {pax} people", pax, attraction.EntryFeeLkr));
        return items;
    }
}
