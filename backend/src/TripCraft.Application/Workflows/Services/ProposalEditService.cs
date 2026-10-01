using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.External;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows.Services;

public interface IProposalEditService
{
    Task<EditableProposalDto> EditDayAsync(CurrentUser user, Guid tripId, int dayNumber, EditItineraryDayRequest request,
        CancellationToken ct);
    Task<EditableProposalDto> SwapResourcesAsync(CurrentUser user, Guid tripId, SwapResourcesRequest request, CancellationToken ct);
    Task<RepriceResponse> RepriceAsync(CurrentUser user, Guid quotationId, CancellationToken ct);
}

/// <summary>
/// "Edit directly" on the review page (trip PendingReview, v1.1). The manager changes a day's stops or swaps the
/// guide, vehicle or hotel; each edit marks the proposal as changed since it was priced. Re-price then makes a new
/// quotation version with today's rates, checks it with ProposalValidator (a Hard rule is a 409) and supersedes the
/// old version, so the review page can show v1 and v2 side by side. Only then can it be sent to the client.
/// </summary>
public class ProposalEditService(
    ITripRequestRepository trips,
    IAgentWorkflowRepository workflows,
    IAttractionRepository attractions,
    IResourceCatalog resources,
    IQuotationStore quotations,
    IExchangeRateService exchangeRates,
    ProposalFactsLoader factsLoader,
    ProposalValidator validator,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IProposalEditService
{
    public async Task<EditableProposalDto> EditDayAsync(CurrentUser user, Guid tripId, int dayNumber,
        EditItineraryDayRequest request, CancellationToken ct)
    {
        var (trip, workflow, outcome) = await LoadAsync(tripId, ct);
        var days = outcome.Proposal.Days ?? [];
        var day = days.FirstOrDefault(d => d.Day == dayNumber) ?? throw new NotFoundException($"Day {dayNumber} is not in this proposal.");

        // Same rules as the saved-itinerary editor: active attractions in that day's city.
        var ids = request.AttractionIds.ToList();
        var found = await attractions.QueryActive().Where(a => ids.Contains(a.Id)).ToListAsync(ct);
        var errors = ids.Where(id => found.All(a => a.Id != id))
            .Select(id => new ValidationFailure("attractionIds", $"Attraction {id} does not exist."))
            .Concat(found.Where(a => !string.Equals(a.City, day.City, StringComparison.OrdinalIgnoreCase))
                .Select(a => new ValidationFailure("attractionIds", $"{a.Name} is in {a.City}, not {day.City}.")))
            .ToList();
        if (errors.Count > 0)
            throw new ValidationException(errors);

        var stops = ids.Select(id => found.First(a => a.Id == id))
            .Select(a => new ProposalStop(a.Id.ToString(), a.Name, a.EntryFeeLkr)).ToList();
        var newDays = days.Select(d => d.Day == dayNumber ? d with { Stops = stops } : d).ToList();
        return await SaveEditAsync(user, trip, workflow, outcome with
        {
            Proposal = outcome.Proposal with { Days = newDays }
        }, "ProposalDayEdited", new { Day = dayNumber, Stops = stops.Select(s => s.Name) }, ct);
    }

    public async Task<EditableProposalDto> SwapResourcesAsync(CurrentUser user, Guid tripId, SwapResourcesRequest request,
        CancellationToken ct)
    {
        var (trip, workflow, outcome) = await LoadAsync(tripId, ct);
        var current = outcome.Proposal.Resources ?? new ProposalResources(null, null, [], []);
        var resourcesAfter = current;

        if (request.GuideId is { } guideId)
        {
            var language = ProposalValidator.RequestedLanguage(trip);
            var free = await resources.FindAvailableGuidesAsync(trip.StartDate, trip.EndDate, language, trip.Pax, ct);
            if (free.All(g => g.Id != guideId))
                throw new ConflictException($"That guide is not free on these dates, does not speak '{language}' or cannot take {trip.Pax} people.");
            resourcesAfter = resourcesAfter with { GuideId = guideId.ToString() };
        }
        if (request.VehicleId is { } vehicleId)
        {
            var free = await resources.FindAvailableVehiclesAsync(trip.StartDate, trip.EndDate, trip.Pax, ct);
            if (free.All(v => v.Id != vehicleId))
                throw new ConflictException($"That vehicle is not free on these dates or has fewer than {trip.Pax} seats.");
            resourcesAfter = resourcesAfter with { VehicleId = vehicleId.ToString() };
        }
        foreach (var swap in request.Rooms ?? [])
            resourcesAfter = resourcesAfter with { Rooms = await SwapRoomsAsync(trip, outcome.Proposal, resourcesAfter.Rooms ?? [], swap, ct) };

        return await SaveEditAsync(user, trip, workflow, outcome with
        {
            Proposal = outcome.Proposal with { Resources = resourcesAfter }
        }, "ProposalResourcesSwapped", request, ct);
    }

    public async Task<RepriceResponse> RepriceAsync(CurrentUser user, Guid quotationId, CancellationToken ct)
    {
        var previous = await quotations.GetAsync(quotationId, ct) ?? throw new NotFoundException("Quotation not found.");
        var latest = await quotations.GetLatestForTripAsync(previous.TripRequestId, ct);
        if (latest is not null && latest.Id != previous.Id)
            throw new ConflictException($"Version {latest.Version} replaced this quotation; re-price that one.");
        var (trip, workflow, outcome) = await LoadAsync(previous.TripRequestId, ct);

        // 1. Today's rates and exchange rate.
        var card = await resources.GetRateCardAsync(ct);
        var fx = await exchangeRates.GetUsdToLkrAsync(ct);
        var breakdown = QuotationCalculator.Calculate(await PriceItemsAsync(outcome.Proposal, trip.Pax, card, ct),
            card.MarginPct, fx.Rate);
        var priced = new ProposalQuotation(
            breakdown.Lines.Select(l => new ProposalQuotationLine(l.LineType, l.Description, l.Qty, l.UnitLkr, l.AmountLkr)).ToList(),
            breakdown.SubtotalLkr, breakdown.MarginPct, breakdown.MarginLkr, breakdown.TotalLkr, fx.Rate, fx.AsOf, fx.Stale,
            breakdown.TotalUsd);
        var proposal = outcome.Proposal with { Quotation = priced };

        // 2. The same deterministic rules as an agent proposal. A Hard rule is a 409; over budget is only a warning.
        var request = new AgentProposalRequest(null, proposal.Days, proposal.Resources, priced, proposal.AgentViolations,
            "Completed", proposal.Replans, null);
        var facts = await factsLoader.LoadAsync(request, trip, ct);
        var validation = validator.Validate(request, trip, facts);
        if (validation.HasHard)
            throw new ConflictException("The edited proposal breaks a rule: " +
                                        string.Join(" ", validation.Violations.Where(v => v.Severity == ViolationSeverity.Hard).Select(v => v.Message)));

        // 3. New version, old one superseded, workflow points at the new one.
        var newId = await quotations.AddVersionAsync(
            WorkflowProposalService.ToDraft(trip, workflow, priced, facts, ProposalSnapshot.Serialize(proposal)), ct);
        await quotations.SetStatusAsync(previous.Id, QuotationDecision.Superseded, ct);
        workflow.Status = validation.HasSoft ? AgentWorkflowStatus.RevisionRequested : AgentWorkflowStatus.PendingApproval;
        workflow.ValidationResult = WorkflowJson.Serialize(validation);
        workflow.FinalOutcome = WorkflowJson.Serialize(new WorkflowOutcome(proposal with { QuotationId = newId }, null));
        audit.Record(user.Id, "QuotationRepriced", "Quotation", newId,
            new { QuotationId = previous.Id, previous.Version, previous.TotalLkr, previous.TotalUsd },
            new { breakdown.TotalLkr, breakdown.TotalUsd, fx.Rate, fx.Stale });
        await unitOfWork.SaveChangesAsync(ct);

        return new RepriceResponse(newId, previous.Version + 1, breakdown.TotalLkr, breakdown.TotalUsd,
            previous.TotalLkr, previous.TotalUsd, workflow.Status.ToString(), validation);
    }

    /// <summary>The trip must be in review; its newest workflow holds the proposal being edited.</summary>
    private async Task<(TripRequest, AgentWorkflow, WorkflowOutcome)> LoadAsync(Guid tripId, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (trip.Status != TripRequestStatus.PendingReview)
            throw new ConflictException($"The proposal can only be edited while the trip is pending review; it is {TripStatusMachine.Describe(trip.Status)}.");
        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct)
                       ?? throw new ConflictException("The trip request has no agent workflow.");
        var outcome = WorkflowJson.Deserialize<WorkflowOutcome>(workflow.FinalOutcome)
                      ?? throw new ConflictException("The workflow has no proposal to edit.");
        return (trip, workflow, outcome);
    }

    private async Task<EditableProposalDto> SaveEditAsync(CurrentUser user, TripRequest trip, AgentWorkflow workflow,
        WorkflowOutcome edited, string action, object change, CancellationToken ct)
    {
        workflow.FinalOutcome = WorkflowJson.Serialize(edited with { EditedSinceQuotation = true });
        audit.Record(user.Id, action, nameof(TripRequest), trip.Id, null, change);
        await unitOfWork.SaveChangesAsync(ct);
        return new EditableProposalDto(workflow.Id, trip.Id, true, edited.Proposal.Days ?? [], edited.Proposal.Resources);
    }

    /// <summary>Every night in the city gets the new room type, with enough rooms for the party; it must be free.</summary>
    private async Task<List<ProposalRoom>> SwapRoomsAsync(TripRequest trip, StoredProposal proposal,
        List<ProposalRoom> rooms, RoomSwap swap, CancellationToken ct)
    {
        var nights = (proposal.Days ?? [])
            .Where(d => string.Equals(d.City, swap.City, StringComparison.OrdinalIgnoreCase) && d.Date < trip.EndDate)
            .Select(d => d.Date).ToList();
        if (nights.Count == 0)
            throw new ConflictException($"The trip spends no night in {swap.City}.");

        var roomType = await resources.GetRoomTypeAsync(swap.RoomTypeId, ct)
                       ?? throw new NotFoundException("Room type not found.");
        var needed = (int)Math.Ceiling(trip.Pax / (double)roomType.Capacity);
        foreach (var night in nights)
        {
            var free = await resources.FindAvailableRoomsAsync(swap.City, night, needed, ct);
            if (free.All(r => r.RoomTypeId != swap.RoomTypeId))
                throw new ConflictException($"{roomType.HotelName} — {roomType.RoomTypeName} has fewer than {needed} rooms free on {night:yyyy-MM-dd}, or is not in {swap.City}.");
        }

        var kept = rooms.Where(r => !nights.Contains(r.Night)).ToList();
        foreach (var night in nights)
            for (var i = 0; i < needed; i++)
                kept.Add(new ProposalRoom(roomType.HotelId.ToString(), roomType.RoomTypeId.ToString(), night));
        return kept.OrderBy(r => r.Night).ToList();
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
            throw new ConflictException("The proposed guide or vehicle no longer exists; swap it or request a revision.");

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
                throw new ConflictException("A proposed room type no longer exists; swap it or request a revision.");
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
