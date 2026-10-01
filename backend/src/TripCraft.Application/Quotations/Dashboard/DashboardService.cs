using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Identity;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations.Dashboard;

public interface IDashboardService
{
    Task<DashboardActionsDto> GetActionsAsync(CancellationToken ct);
    Task<IReadOnlyList<UpcomingTripDto>> GetUpcomingAsync(CancellationToken ct);
}

/// <summary>The manager's dashboard (v1.1): what needs action now, and the trips running today and tomorrow.</summary>
public class DashboardService(
    ITripRequestRepository trips,
    IQuotationRepository quotations,
    IResourceRepository resources,
    IUserRepository users,
    TripSettings settings) : IDashboardService
{
    public const int CancellationWindowDays = 7;

    public async Task<DashboardActionsDto> GetActionsAsync(CancellationToken ct)
    {
        // Trips in review, split by the newest quotation: still Pending (a new proposal) or Declined by the client.
        var inReview = await trips.Query().Where(t => t.Status == TripRequestStatus.PendingReview).Select(t => t.Id).ToListAsync(ct);
        var newestStatus = await quotations.Query().Where(q => inReview.Contains(q.TripRequestId))
            .GroupBy(q => q.TripRequestId)
            .Select(g => g.OrderByDescending(q => q.Version).Select(q => q.Status).First())
            .ToListAsync(ct);

        var since = DateTime.UtcNow.AddDays(-CancellationWindowDays);
        return new DashboardActionsDto(
            ProposalsToReview: newestStatus.Count(s => s == QuotationStatus.Pending),
            ClientAcceptedToConfirm: await trips.Query().CountAsync(t => t.Status == TripRequestStatus.ClientAccepted, ct),
            GuideChangeRequests: await resources.GuideChangeRequests().CountAsync(r => r.Status == GuideChangeRequestStatus.Open, ct),
            RecentCancellations: await trips.Query().CountAsync(t => t.Status == TripRequestStatus.Cancelled && t.UpdatedAt >= since, ct),
            DeclinedQuotations: newestStatus.Count(s => s == QuotationStatus.Declined));
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
