using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;

namespace TripCraft.Application.Quotations.Documents;

public interface ITripDocumentService
{
    Task<byte[]> RenderItineraryAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct);
}

/// <summary>
/// The printable itinerary and quotation (v1.1) for the tourist (own trip) or a manager: the saved itinerary once
/// confirmed, otherwise the proposal that was priced; the newest quotation that was sent, with its deposit.
/// 409 before any quotation was sent to the client.
/// </summary>
public class TripDocumentService(
    ITripRequestRepository trips,
    IAgentWorkflowRepository workflows,
    IQuotationRepository quotations,
    IUserRepository users,
    IItineraryPdfRenderer renderer) : ITripDocumentService
{
    public async Task<byte[]> RenderItineraryAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);
        var sent = await quotations.Query().Where(q => q.TripRequestId == trip.Id && q.Status == QuotationStatus.Approved)
                       .OrderByDescending(q => q.Version).FirstOrDefaultAsync(ct)
                   ?? throw new ConflictException("The itinerary PDF is available once a quotation has been sent.");

        var saved = await trips.GetItineraryAsync(trip.Id, ct);
        var days = saved is not null
            ? saved.Days.OrderBy(d => d.DayNumber).Select(d => new ItineraryDocumentDay(d.DayNumber,
                trip.StartDate.AddDays(d.DayNumber - 1), d.City,
                d.Stops.OrderBy(s => s.Sequence).Select(s => s.Attraction?.Name ?? "Stop").ToList(), d.Notes)).ToList()
            : await ProposalDaysAsync(trip, ct);
        var tourist = trip.Tourist is null ? null : await users.GetByIdAsync(trip.Tourist.UserId, ct);

        var document = new ItineraryDocument(trip.Objective, tourist?.FullName ?? "Guest", trip.Status.ToString(),
            trip.StartDate, trip.EndDate, trip.Pax, trip.CityList, saved is not null, days,
            new ItineraryDocumentQuotation(sent.Version,
                sent.Lines.Select(l => new ItineraryDocumentLine(l.Description, l.Qty, l.UnitLkr, l.AmountLkr)).ToList(),
                sent.SubtotalLkr, sent.TotalLkr - sent.SubtotalLkr, sent.TotalLkr, sent.TotalUsd, sent.FxRate, sent.DepositPct,
                QuotationCalculator.Round(sent.TotalLkr * sent.DepositPct / 100),
                QuotationCalculator.Round(sent.TotalUsd * sent.DepositPct / 100), sent.DepositPaidAt is not null,
                sent.AcceptedAt is not null));
        return renderer.Render(document);
    }

    private async Task<List<ItineraryDocumentDay>> ProposalDaysAsync(TripRequest trip, CancellationToken ct)
    {
        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct);
        var proposal = WorkflowJson.Deserialize<WorkflowOutcome>(workflow?.FinalOutcome)?.Proposal;
        return (proposal?.Days ?? []).OrderBy(d => d.Day).Select(d => new ItineraryDocumentDay(d.Day, d.Date, d.City,
            (d.Stops ?? []).Select(s => s.Name).ToList(), d.Weather is null ? $"By {d.Transport}" : $"By {d.Transport}; weather: {d.Weather}"))
            .ToList();
    }
}
