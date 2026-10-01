using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface IGuideChangeService
{
    Task<GuideChangeRequestDto> RequestAsync(CurrentUser user, Guid tripRequestId, string reason, CancellationToken ct);
    Task<IReadOnlyList<GuideChangeRequestDto>> ListOpenAsync(CancellationToken ct);
    Task<GuideChangeRequestDto> ResolveAsync(CurrentUser user, Guid id, Guid replacementGuideId, CancellationToken ct);
}

/// <summary>
/// A guide asks to be replaced on a confirmed trip (v1.1). The managers see the request on the dashboard with the
/// guides who could take over (same language, free, enough capacity). Resolve swaps the guide holds in one
/// transaction and tells the old guide, the new guide and the tourist.
/// </summary>
public class GuideChangeService(
    IResourceRepository resources,
    IResourceCatalog catalog,
    IResourceHoldService holds,
    ITripRequestRepository trips,
    IAgentWorkflowRepository workflows,
    INotifier notifier,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IGuideChangeService
{
    public async Task<GuideChangeRequestDto> RequestAsync(CurrentUser user, Guid tripRequestId, string reason, CancellationToken ct)
    {
        var guide = await resources.FindGuideByUserAsync(user.Id, ct)
                    ?? throw new ForbiddenException("No guide profile is linked to your account.");
        if (await HeldGuideAsync(tripRequestId, ct) != guide.Id)
            throw new ForbiddenException("You are not the guide of this trip.");
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (trip.Status != TripRequestStatus.Confirmed)
            throw new ConflictException("A replacement can only be requested before the trip starts (while it is confirmed).");
        if (await resources.GuideChangeRequests().AnyAsync(r => r.TripRequestId == trip.Id && r.Status == GuideChangeRequestStatus.Open, ct))
            throw new ConflictException("There is already an open replacement request for this trip.");

        var request = new GuideChangeRequest { TripRequestId = trip.Id, GuideId = guide.Id, Reason = reason.Trim() };
        resources.Add(request);
        await notifier.NotifyManagersAsync("GuideChangeRequested", "A guide asked to be replaced",
            $"{guide.Name}, trip {trip.StartDate:dd MMM}: {request.Reason}", trip.Id, ct);
        audit.Record(user.Id, "GuideChangeRequested", nameof(GuideChangeRequest), request.Id, null,
            new { request.TripRequestId, request.Reason });
        await unitOfWork.SaveChangesAsync(ct);
        return await ToDtoAsync(request, trip, ct);
    }

    public async Task<IReadOnlyList<GuideChangeRequestDto>> ListOpenAsync(CancellationToken ct)
    {
        var open = await resources.GuideChangeRequests().Where(r => r.Status == GuideChangeRequestStatus.Open)
            .OrderBy(r => r.CreatedAt).ToListAsync(ct);
        var result = new List<GuideChangeRequestDto>();
        foreach (var request in open)
        {
            var trip = await trips.GetByIdAsync(request.TripRequestId, ct);
            if (trip is not null)
                result.Add(await ToDtoAsync(request, trip, ct));
        }
        return result;
    }

    public async Task<GuideChangeRequestDto> ResolveAsync(CurrentUser user, Guid id, Guid replacementGuideId, CancellationToken ct)
    {
        var request = await resources.FindGuideChangeRequestAsync(id, ct) ?? throw new NotFoundException("Request not found.");
        if (request.Status != GuideChangeRequestStatus.Open)
            throw new ConflictException("This request has already been resolved.");
        var trip = await trips.GetByIdAsync(request.TripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (trip.Status != TripRequestStatus.Confirmed)
            throw new ConflictException($"The guide can only be swapped while the trip is confirmed; it is {TripStatusMachine.Describe(trip.Status)}.");
        if ((await CandidatesAsync(trip, ct)).All(g => g.Id != replacementGuideId))
            throw new ConflictException("That guide is not free for the whole trip, does not speak the trip's language or cannot take the party.");

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await holds.ReleaseTripHoldsAsync(trip.Id, ResourceType.Guide, ct);
        await holds.CreateHoldAsync(new ResourceHoldRequest(ResourceType.Guide, replacementGuideId, trip.Id,
            trip.StartDate, trip.EndDate, 1), ct);
        request.Status = GuideChangeRequestStatus.Resolved;
        request.ReplacementGuideId = replacementGuideId;
        request.ResolvedBy = user.Id;
        request.ResolvedAt = DateTime.UtcNow;
        await PointWorkflowAtAsync(trip.Id, replacementGuideId, ct);

        var dates = $"{trip.StartDate:dd MMM} – {trip.EndDate:dd MMM}";
        await notifier.NotifyGuideAsync(request.GuideId, "GuideReplaced", "You were replaced on a trip",
            $"Your request was accepted; the trip of {dates} has a new guide.", trip.Id, ct);
        await notifier.NotifyGuideAsync(replacementGuideId, "TripAssigned", "New trip assigned",
            $"{dates}, {trip.Pax} people. See your schedule.", trip.Id, ct);
        notifier.NotifyTourist(trip, "GuideChanged", "Your guide has changed",
            $"A new guide will meet you on {trip.StartDate:dd MMM}. Your vouchers stay the same.");
        audit.Record(user.Id, "GuideChangeResolved", nameof(GuideChangeRequest), request.Id,
            new { request.GuideId }, new { ReplacementGuideId = replacementGuideId });

        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return await ToDtoAsync(request, trip, ct);
    }

    /// <summary>The guide currently held for the trip, or null.</summary>
    private Task<Guid?> HeldGuideAsync(Guid tripRequestId, CancellationToken ct) =>
        resources.Holds().Where(h => h.TripRequestId == tripRequestId && h.ResourceType == ResourceType.Guide
                                     && h.Status == HoldStatus.Held)
            .Select(h => (Guid?)h.ResourceId).FirstOrDefaultAsync(ct);

    private Task<IReadOnlyList<GuideOption>> CandidatesAsync(TripRequest trip, CancellationToken ct) =>
        catalog.FindAvailableGuidesAsync(trip.StartDate, trip.EndDate, ProposalValidator.RequestedLanguage(trip), trip.Pax, ct);

    /// <summary>The confirmed proposal names the guide too (used e.g. to notify the guide on cancellation).</summary>
    private async Task PointWorkflowAtAsync(Guid tripRequestId, Guid guideId, CancellationToken ct)
    {
        var workflow = await workflows.GetLatestForTripAsync(tripRequestId, ct);
        var outcome = WorkflowJson.Deserialize<WorkflowOutcome>(workflow?.FinalOutcome);
        if (workflow is null || outcome?.Proposal.Resources is null)
            return;
        workflow.FinalOutcome = WorkflowJson.Serialize(outcome with
        {
            Proposal = outcome.Proposal with { Resources = outcome.Proposal.Resources with { GuideId = guideId.ToString() } }
        });
    }

    private async Task<GuideChangeRequestDto> ToDtoAsync(GuideChangeRequest r, TripRequest trip, CancellationToken ct)
    {
        var ids = new[] { r.GuideId, r.ReplacementGuideId ?? Guid.Empty };
        var names = await resources.Guides().Where(g => ids.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Name, ct);
        var candidates = r.Status == GuideChangeRequestStatus.Open ? await CandidatesAsync(trip, ct) : [];
        return new GuideChangeRequestDto(r.Id, trip.Id, trip.Objective, trip.StartDate, trip.EndDate, trip.Pax,
            ProposalValidator.RequestedLanguage(trip), r.GuideId, names.GetValueOrDefault(r.GuideId, "Guide"), r.Reason,
            r.Status.ToString(), r.ReplacementGuideId,
            r.ReplacementGuideId is { } rid ? names.GetValueOrDefault(rid) : null, r.CreatedAt, r.ResolvedAt, candidates);
    }
}
