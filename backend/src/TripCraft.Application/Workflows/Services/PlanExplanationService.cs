using System.Text.Json;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows.Services;

/// <summary>One line of "Why this plan": Topic is guide, vehicle, hotels, driving or budget.</summary>
public record PlanExplanationItem(string Topic, string Title, string Text);

public record PlanExplanationDto(IReadOnlyList<PlanExplanationItem> Items);

public interface IPlanExplanationService
{
    Task<PlanExplanationDto> ExplainAsync(Guid tripRequestId, CancellationToken ct);
}

/// <summary>
/// "Why this plan" on the manager's review page (v1.1): plain sentences built from the proposal under review, the
/// resource facts and the agents' step summaries (e.g. the Resources agent's reason for the guide). No LLM call.
/// </summary>
public class PlanExplanationService(
    ITripRequestRepository trips,
    IAgentWorkflowRepository workflows,
    IResourceCatalog catalog,
    IQuotationStore quotations) : IPlanExplanationService
{
    public async Task<PlanExplanationDto> ExplainAsync(Guid tripRequestId, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct);
        var proposal = WorkflowJson.Deserialize<WorkflowOutcome>(workflow?.FinalOutcome)?.Proposal;
        if (workflow is null || proposal?.Days is null)
            throw new NotFoundException("There is no proposal to explain yet.");

        var steps = await workflows.ListStepsAsync(workflow.Id, ct);
        var items = new List<PlanExplanationItem>();
        items.Add(await GuideAsync(trip, proposal, steps, ct));
        items.Add(await VehicleAsync(trip, proposal, ct));
        items.Add(await HotelsAsync(proposal, steps, ct));
        items.Add(Driving(proposal.Days));
        if (await quotations.GetLatestForTripAsync(trip.Id, ct) is { } quotation)
            items.Add(Budget(trip, quotation.TotalUsd));
        return new PlanExplanationDto(items);
    }

    private async Task<PlanExplanationItem> GuideAsync(TripRequest trip, StoredProposal proposal, List<AgentStep> steps,
        CancellationToken ct)
    {
        var id = ProposalValidator.ParseId(proposal.Resources?.GuideId);
        var guide = id is { } g ? await catalog.GetGuideAsync(g, ct) : null;
        if (guide is null)
            return new("guide", "Guide", "No guide is assigned in this proposal.");

        var card = await catalog.GetRateCardAsync(ct);
        var rate = card.GuideDayRates.TryGetValue(guide.Id, out var r) ? $", LKR {r:N0} a day" : "";
        var language = ProposalValidator.RequestedLanguage(trip);
        var agentReason = SummaryText(steps, "resources", "guide_choice");
        var agentGuide = SummaryText(steps, "resources", "guide_id");
        var why = agentReason is not null && agentGuide == guide.Id.ToString()
            ? $"The Resources agent chose {agentReason}."
            : $"{guide.Name} was picked by the operator when editing the plan.";
        return new("guide", $"Guide: {guide.Name}",
            $"{why} Speaks {string.Join(", ", guide.Languages)} (the trip asks for '{language}'), takes up to {guide.MaxPax} people{rate}.");
    }

    private async Task<PlanExplanationItem> VehicleAsync(TripRequest trip, StoredProposal proposal, CancellationToken ct)
    {
        var id = ProposalValidator.ParseId(proposal.Resources?.VehicleId);
        var vehicle = id is { } v ? await catalog.GetVehicleAsync(v, ct) : null;
        if (vehicle is null)
            return new("vehicle", "Vehicle", "No vehicle is assigned in this proposal.");
        var card = await catalog.GetRateCardAsync(ct);
        var rate = card.VehicleKmRates.TryGetValue(vehicle.Id, out var r) ? $", LKR {r:N0} per km" : "";
        return new("vehicle", $"Vehicle: {vehicle.Type} {vehicle.RegistrationNo}",
            $"{vehicle.Seats} seats for {trip.Pax} travellers{rate}. It was free on every day of the trip.");
    }

    private async Task<PlanExplanationItem> HotelsAsync(StoredProposal proposal, List<AgentStep> steps, CancellationToken ct)
    {
        var lines = new List<string>();
        foreach (var group in (proposal.Resources?.Rooms ?? []).GroupBy(r => r.RoomTypeId))
        {
            var room = ProposalValidator.ParseId(group.Key) is { } id ? await catalog.GetRoomTypeAsync(id, ct) : null;
            var nights = group.Select(r => r.Night).Distinct().Count();
            var perNight = group.Count() / Math.Max(1, nights);
            lines.Add($"{room?.HotelName ?? "Hotel"} — {room?.RoomTypeName ?? "room"}: {perNight} room(s) × {nights} night(s)");
        }
        var tier = SummaryText(steps, "planner", "hotel_tier");
        var tierText = tier is null ? "" : $" Hotel tier: {tier}{(tier == "budget" ? " (chosen to fit the budget)" : "")}.";
        return new("hotels", "Hotels", lines.Count == 0 ? "No hotel nights (a one-day trip)." : string.Join("; ", lines) + "." + tierText);
    }

    private static PlanExplanationItem Driving(List<Dtos.ProposalDay> days)
    {
        var transfers = days.Where(d => d.TransferKm > 0).OrderBy(d => d.Day)
            .Select(d => $"Day {d.Day} to {d.City}: {d.TransferKm:N0} km by {d.Transport}" +
                         (d.DrivingMinutes > 0 ? $", about {d.DrivingMinutes / 60} h {d.DrivingMinutes % 60} min" : ""))
            .ToList();
        return new("driving", "Driving", transfers.Count == 0
            ? "No transfers between cities: every day stays in one place."
            : string.Join("; ", transfers) + ". No day is over the 4-hour driving limit.");
    }

    public static PlanExplanationItem Budget(TripRequest trip, decimal totalUsd)
    {
        var difference = trip.BudgetUsd == 0 ? 0 : Math.Round((trip.BudgetUsd - totalUsd) / trip.BudgetUsd * 100);
        var text = totalUsd <= trip.BudgetUsd
            ? $"Total USD {totalUsd:N2} for {trip.Pax} people against a budget of USD {trip.BudgetUsd:N2}: {difference}% under budget."
            : $"Total USD {totalUsd:N2} for {trip.Pax} people is over the budget of USD {trip.BudgetUsd:N2} by {-difference}%. Request a revision or edit the plan.";
        return new("budget", "Budget", text);
    }

    /// <summary>A string field from the newest step summary of an agent, or null.</summary>
    private static string? SummaryText(List<AgentStep> steps, string agent, string field)
    {
        var step = steps.Where(s => s.AgentName == agent && s.ToolName == null).OrderByDescending(s => s.StepNo).FirstOrDefault()
                   ?? steps.Where(s => s.AgentName == agent).OrderByDescending(s => s.StepNo).FirstOrDefault();
        if (step is null)
            return null;
        try
        {
            using var doc = JsonDocument.Parse(step.OutputSummary);
            return doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty(field, out var value)
                   && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
