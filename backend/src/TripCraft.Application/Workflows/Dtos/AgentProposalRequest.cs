using System.Text.Json;
using System.Text.Json.Serialization;

namespace TripCraft.Application.Workflows.Dtos;

/// <summary>
/// POST /api/internal/workflows/{id}/proposal. The final result of one agent run, in snake_case.
/// Everything except status is nullable so an incomplete proposal reaches ProposalValidator
/// (Hard violation) instead of being rejected with a 400 that would leave the workflow stuck.
/// </summary>
public record AgentProposalRequest(
    [property: JsonPropertyName("plan")] JsonElement? Plan,
    [property: JsonPropertyName("days")] List<ProposalDay>? Days,
    [property: JsonPropertyName("resources")] ProposalResources? Resources,
    [property: JsonPropertyName("quotation")] ProposalQuotation? Quotation,
    [property: JsonPropertyName("violations")] List<ProposalViolation>? Violations,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("replans")] int Replans,
    [property: JsonPropertyName("error_summary")] string? ErrorSummary);

public record ProposalDay(
    [property: JsonPropertyName("day")] int Day,
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("stops")] List<ProposalStop>? Stops,
    [property: JsonPropertyName("transport")] string Transport,
    [property: JsonPropertyName("transfer_km")] decimal TransferKm,
    [property: JsonPropertyName("driving_minutes")] int DrivingMinutes,
    [property: JsonPropertyName("weather")] string? Weather);

/// <summary>AttractionId is kept as text: a bad id becomes a Hard violation, not a 400.</summary>
public record ProposalStop(
    [property: JsonPropertyName("attraction_id")] string AttractionId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("entry_fee_lkr")] decimal EntryFeeLkr);

public record ProposalResources(
    [property: JsonPropertyName("guide_id")] string? GuideId,
    [property: JsonPropertyName("vehicle_id")] string? VehicleId,
    [property: JsonPropertyName("rooms")] List<ProposalRoom>? Rooms,
    [property: JsonPropertyName("gaps")] List<string>? Gaps);

/// <summary>One room for one night.</summary>
public record ProposalRoom(
    [property: JsonPropertyName("hotel_id")] string HotelId,
    [property: JsonPropertyName("room_type_id")] string RoomTypeId,
    [property: JsonPropertyName("night")] DateOnly Night);

public record ProposalQuotation(
    [property: JsonPropertyName("lines")] List<ProposalQuotationLine>? Lines,
    [property: JsonPropertyName("subtotal_lkr")] decimal SubtotalLkr,
    [property: JsonPropertyName("margin_pct")] decimal MarginPct,
    [property: JsonPropertyName("margin_lkr")] decimal MarginLkr,
    [property: JsonPropertyName("total_lkr")] decimal TotalLkr,
    [property: JsonPropertyName("fx_rate")] decimal FxRate,
    [property: JsonPropertyName("fx_as_of")] DateTime FxAsOf,
    [property: JsonPropertyName("fx_stale")] bool FxStale,
    [property: JsonPropertyName("total_usd")] decimal TotalUsd);

public record ProposalQuotationLine(
    [property: JsonPropertyName("line_type")] string LineType,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("qty")] decimal Qty,
    [property: JsonPropertyName("unit_lkr")] decimal UnitLkr,
    [property: JsonPropertyName("amount_lkr")] decimal AmountLkr);

public record ProposalViolation(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message);

public record ProposalOutcomeResponse(Guid WorkflowId, string Status, Guid? QuotationId, ProposalValidationResult Validation);
