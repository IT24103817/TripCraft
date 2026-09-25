using System.Text.Json;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Planning;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows;

/// <summary>
/// Deterministic validation of an agent proposal (PLAN.md section 5). A pure class: the facts from the
/// database are passed in, so every rule is covered by plain unit tests. The LLM cannot switch any rule off.
/// Hard violations stop the workflow (FailedSafely); the only Soft one, over budget, asks for a revision.
/// </summary>
public class ProposalValidator
{
    public const decimal QuotationToleranceLkr = 1m;

    public ProposalValidationResult Validate(AgentProposalRequest proposal, TripRequest trip, ProposalFacts facts)
    {
        var violations = new List<ProposalRuleViolation>();

        // 1. JSON structure must be complete before any other rule can run.
        if (!CheckStructure(proposal, trip, violations))
            return new ProposalValidationResult(false, violations);

        var days = proposal.Days!;
        var resources = proposal.Resources!;
        var quotation = proposal.Quotation!;

        // 2. Days: 1–3 stops, every attraction exists.
        foreach (var day in days)
        {
            if (day.Stops!.Count is < 1 or > TripPlanningRules.MaxStopsPerDay)
                Hard(violations, "DAY_STOPS", $"Day {day.Day} has {day.Stops.Count} stops (allowed 1–3).");
            foreach (var stop in day.Stops.Where(s => !Known(s.AttractionId, facts.AttractionEntryFeesLkr)))
                Hard(violations, "UNKNOWN_ATTRACTION", $"Day {day.Day}: attraction {stop.AttractionId} does not exist.");
        }

        // 3. Guide: exists, speaks the requested language, not already held.
        var guideId = ParseId(resources.GuideId);
        if (guideId is null || facts.Guide is null || facts.Guide.Id != guideId)
            Hard(violations, "UNKNOWN_GUIDE", $"Guide '{resources.GuideId}' does not exist.");
        else
        {
            var language = RequestedLanguage(trip);
            if (!facts.Guide.Languages.Contains(language, StringComparer.OrdinalIgnoreCase))
                Hard(violations, "GUIDE_LANGUAGE", $"Guide {facts.Guide.Name} does not speak '{language}'.");
            if (facts.GuideHoldOverlaps)
                Hard(violations, "GUIDE_HOLD_OVERLAP", $"Guide {facts.Guide.Name} is already held in these dates.");
        }

        // 4. Vehicle: exists, enough seats, not already held.
        var vehicleId = ParseId(resources.VehicleId);
        if (vehicleId is null || facts.Vehicle is null || facts.Vehicle.Id != vehicleId)
            Hard(violations, "UNKNOWN_VEHICLE", $"Vehicle '{resources.VehicleId}' does not exist.");
        else
        {
            if (facts.Vehicle.Seats < trip.Pax)
                Hard(violations, "VEHICLE_SEATS", $"Vehicle has {facts.Vehicle.Seats} seats but pax is {trip.Pax}.");
            if (facts.VehicleHoldOverlaps)
                Hard(violations, "VEHICLE_HOLD_OVERLAP", $"Vehicle {facts.Vehicle.RegistrationNo} is already held in these dates.");
        }

        // 5. Rooms: hotel and room type exist and match; enough beds every night.
        var roomTypePerRoomNight = CheckRooms(resources.Rooms!, days, trip.Pax, facts, violations);

        // 6. Quotation recomputed server-side; then the budget (the only Soft rule).
        if (guideId is not null && vehicleId is not null && roomTypePerRoomNight is not null
            && !violations.Any(v => v.Code == "UNKNOWN_ATTRACTION"))
        {
            var (total, error) = ProposalQuotationCheck.RecomputeTotalLkr(
                days, guideId.Value, vehicleId.Value, roomTypePerRoomNight, trip.Pax, facts);
            if (error is not null)
                Hard(violations, "QUOTATION_MISMATCH", error);
            else if (Math.Abs(total!.Value - quotation.TotalLkr) > QuotationToleranceLkr)
                Hard(violations, "QUOTATION_MISMATCH",
                    $"Proposal total LKR {quotation.TotalLkr} differs from the server total LKR {total}.");
            else
            {
                var totalUsd = ProposalQuotationCheck.Round(total.Value / quotation.FxRate);
                if (totalUsd > trip.BudgetUsd)
                    violations.Add(new ProposalRuleViolation("OVER_BUDGET",
                        $"Total USD {totalUsd} is over the budget of USD {trip.BudgetUsd}.", ViolationSeverity.Soft));
            }
        }

        return new ProposalValidationResult(violations.Count == 0, violations);
    }

    private static bool CheckStructure(AgentProposalRequest p, TripRequest trip, List<ProposalRuleViolation> v)
    {
        var tripDays = TripPlanningRules.TripDays(trip.StartDate, trip.EndDate);
        if (p.Days is null || p.Days.Count != tripDays)
            Hard(v, "SCHEMA_INCOMPLETE", $"Expected {tripDays} days, got {p.Days?.Count ?? 0}.");
        else if (p.Days.Any(d => d.Stops is null || d.Date < trip.StartDate || d.Date > trip.EndDate))
            Hard(v, "SCHEMA_INCOMPLETE", "Every day needs a stops list and a date inside the trip.");
        if (p.Resources?.Rooms is null)
            Hard(v, "SCHEMA_INCOMPLETE", "Resources with a rooms list are missing.");
        if (p.Quotation?.Lines is null || p.Quotation.FxRate <= 0)
            Hard(v, "SCHEMA_INCOMPLETE", "Quotation with lines and a positive fx_rate is missing.");
        return v.Count == 0;
    }

    /// <summary>Returns the room type of every room-night, or null when any room is unknown.</summary>
    private static List<Guid>? CheckRooms(List<ProposalRoom> rooms, List<ProposalDay> days, int pax,
        ProposalFacts facts, List<ProposalRuleViolation> v)
    {
        var roomTypes = new List<Guid>();
        var capacityPerNight = new Dictionary<DateOnly, int>();
        var allKnown = true;

        foreach (var room in rooms)
        {
            var roomTypeId = ParseId(room.RoomTypeId);
            if (roomTypeId is null || !facts.RoomTypes.TryGetValue(roomTypeId.Value, out var roomType))
            {
                Hard(v, "UNKNOWN_ROOM_TYPE", $"Room type '{room.RoomTypeId}' does not exist.");
                allKnown = false;
                continue;
            }
            if (ParseId(room.HotelId) != roomType.HotelId)
            {
                Hard(v, "UNKNOWN_HOTEL", $"Hotel '{room.HotelId}' does not own room type {roomType.RoomTypeName}.");
                allKnown = false;
            }
            roomTypes.Add(roomType.RoomTypeId);
            capacityPerNight[room.Night] = capacityPerNight.GetValueOrDefault(room.Night) + roomType.Capacity;
        }

        // Every date except the last is a night.
        foreach (var night in days.OrderBy(d => d.Date).SkipLast(1).Select(d => d.Date))
        {
            var beds = capacityPerNight.GetValueOrDefault(night);
            if (beds < pax)
                Hard(v, "ROOMS_BELOW_PAX", $"Night {night:yyyy-MM-dd} sleeps {beds} but pax is {pax}.");
        }
        return allKnown ? roomTypes : null;
    }

    /// <summary>"language" from the trip preferences JSON, default "en".</summary>
    public static string RequestedLanguage(TripRequest trip)
    {
        using var doc = JsonDocument.Parse(trip.Preferences);
        return doc.RootElement.ValueKind == JsonValueKind.Object
               && doc.RootElement.TryGetProperty("language", out var lang)
               && lang.ValueKind == JsonValueKind.String
            ? lang.GetString()!
            : "en";
    }

    public static Guid? ParseId(string? id) => Guid.TryParse(id, out var guid) ? guid : null;

    private static bool Known(string id, IReadOnlyDictionary<Guid, decimal> ids) =>
        ParseId(id) is { } guid && ids.ContainsKey(guid);

    private static void Hard(List<ProposalRuleViolation> v, string code, string message) =>
        v.Add(new ProposalRuleViolation(code, message, ViolationSeverity.Hard));
}
