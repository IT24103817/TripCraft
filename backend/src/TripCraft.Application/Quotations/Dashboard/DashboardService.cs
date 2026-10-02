using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Identity;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations.Dashboard;

public interface IDashboardService
{
    Task<DashboardActionsDto> GetActionsAsync(CancellationToken ct);
    Task<IReadOnlyList<AttentionItemDto>> GetAttentionAsync(TripRequestStatus? status, CancellationToken ct);
    Task<IReadOnlyList<UpcomingTripDto>> GetUpcomingAsync(CancellationToken ct);
}

/// <summary>
/// The manager's dashboard (v1.1): trips waiting for the operator (accepted → Confirm, declined → decide, needs
/// operator), and the trips running today and tomorrow.
/// </summary>
public class DashboardService(
    ITripRequestRepository trips,
    IQuotationRepository quotations,
    IResourceRepository resources,
    IUserRepository users,
    IAgentWorkflowRepository workflows,
    TripSettings settings) : IDashboardService
{
    public const int CancellationWindowDays = 7;

    public async Task<DashboardActionsDto> GetActionsAsync(CancellationToken ct)
    {
        var since = DateTime.UtcNow.AddDays(-CancellationWindowDays);
        return new DashboardActionsDto(
            AcceptedToConfirm: await trips.Query().CountAsync(t => t.Status == TripRequestStatus.ClientAccepted, ct),
            DeclinedNeedsDecision: await trips.Query().CountAsync(t => t.Status == TripRequestStatus.ClientDeclined, ct),
            NeedsOperator: await trips.Query().CountAsync(t => t.Status == TripRequestStatus.NeedsOperator, ct),
            GuideChangeRequests: await resources.GuideChangeRequests().CountAsync(r => r.Status == GuideChangeRequestStatus.Open, ct),
            RecentCancellations: await trips.Query().CountAsync(t => t.Status == TripRequestStatus.Cancelled && t.UpdatedAt >= since, ct));
    }

    public async Task<IReadOnlyList<AttentionItemDto>> GetAttentionAsync(TripRequestStatus? status, CancellationToken ct)
    {
        TripRequestStatus[] wanted = status is { } one
            ? [one]
            : [TripRequestStatus.ClientAccepted, TripRequestStatus.ClientDeclined, TripRequestStatus.NeedsOperator];
        var waiting = await trips.Query().Where(t => wanted.Contains(t.Status)).OrderBy(t => t.UpdatedAt).ToListAsync(ct);
        var ids = waiting.Select(t => t.Id).ToList();

        var newest = (await quotations.Query().Where(q => ids.Contains(q.TripRequestId)).ToListAsync(ct))
            .GroupBy(q => q.TripRequestId).ToDictionary(g => g.Key, g => g.OrderByDescending(q => q.Version).First());
        var newestIds = newest.Values.Select(q => q.Id).ToList();
        var declineReasons = (await quotations.Decisions()
                .Where(d => newestIds.Contains(d.QuotationId) && d.Decision == QuotationDecision.Declined).ToListAsync(ct))
            .GroupBy(d => d.QuotationId).ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.DecidedAt).First().Comment);
        var errors = (await workflows.Query().Where(w => ids.Contains(w.TripRequestId)).ToListAsync(ct))
            .GroupBy(w => w.TripRequestId).ToDictionary(g => g.Key, g => g.OrderByDescending(w => w.StartedAt).First().ErrorSummary);
        var names = (await users.ListAsync(ct)).ToDictionary(u => u.Id, u => u.FullName);

        return waiting.Select(t =>
        {
            var quotation = newest.GetValueOrDefault(t.Id);
            var detail = t.Status switch
            {
                TripRequestStatus.ClientDeclined => declineReasons.GetValueOrDefault(quotation?.Id ?? Guid.Empty) ?? "Declined without a reason.",
                TripRequestStatus.NeedsOperator => errors.GetValueOrDefault(t.Id) ?? "Planning stopped; see the agent run.",
                _ => quotation is null ? "Accepted" : $"Version {quotation.Version} accepted"
            };
            return new AttentionItemDto(t.Id, t.Objective, t.Status.ToString(), t.StartDate, t.EndDate, t.Pax,
                t.Tourist is { } tourist ? names.GetValueOrDefault(tourist.UserId, "Tourist") : "Tourist", detail, t.UpdatedAt,
                quotation?.TotalUsd);
        }).ToList();
    }

    public async Task<IReadOnlyList<UpcomingTripDto>> GetUpcomingAsync(CancellationToken ct)
    {
        var today = settings.Today();
        var tomorrow = today.AddDays(1);
        var running = await trips.Query()
            .Where(t => (t.Status == TripRequestStatus.Confirmed || t.Status == TripRequestStatus.InProgress)
                        && t.StartDate <= tomorrow && t.EndDate >= today)
            .OrderBy(t => t.StartDate).ToListAsync(ct);
        var ids = running.Select(t => t.Id).ToList();
        var holds = await resources.Holds()
            .Where(h => h.TripRequestId != null && ids.Contains(h.TripRequestId.Value) && h.Status == HoldStatus.Held
                        && h.ResourceType != ResourceType.Room)
            .ToListAsync(ct);
        var guideIds = holds.Where(h => h.ResourceType == ResourceType.Guide).Select(h => h.ResourceId).ToList();
        var vehicleIds = holds.Where(h => h.ResourceType == ResourceType.Vehicle).Select(h => h.ResourceId).ToList();
        var guides = await resources.Guides().Where(g => guideIds.Contains(g.Id)).ToDictionaryAsync(g => g.Id, g => g.Name, ct);
        var vehicles = await resources.Vehicles().Where(v => vehicleIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, ct);
        var names = (await users.ListAsync(ct)).ToDictionary(u => u.Id, u => u.FullName);

        return running.Select(t =>
        {
            var day = t.StartDate <= today ? today : tomorrow;
            var guideId = holds.FirstOrDefault(h => h.TripRequestId == t.Id && h.ResourceType == ResourceType.Guide)?.ResourceId;
            var vehicleId = holds.FirstOrDefault(h => h.TripRequestId == t.Id && h.ResourceType == ResourceType.Vehicle)?.ResourceId;
            var vehicle = vehicleId is { } v ? vehicles.GetValueOrDefault(v) : null;
            return new UpcomingTripDto(t.Id, t.Objective, t.StartDate, t.EndDate, t.Pax, t.Status.ToString(),
                t.Tourist is { } tourist ? names.GetValueOrDefault(tourist.UserId, "Tourist") : "Tourist",
                guideId is { } g ? guides.GetValueOrDefault(g) : null, vehicle?.RegistrationNo, vehicle?.Type,
                day == today ? "today" : "tomorrow", day.DayNumber - t.StartDate.DayNumber + 1);
        }).ToList();
    }
}
