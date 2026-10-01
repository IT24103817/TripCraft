import 'package:freezed_annotation/freezed_annotation.dart';

part 'trip_models.freezed.dart';
part 'trip_models.g.dart';

// Field names follow backend/src/TripCraft.Application/Trips/Dtos. DateOnly values stay "yyyy-MM-dd" strings.

@freezed
abstract class TripRequest with _$TripRequest {
  const factory TripRequest({
    required String id,
    required String objective,
    required String startDate,
    required String endDate,
    required int pax,
    required double budgetUsd,
    @Default(<String, dynamic>{}) Map<String, dynamic> preferences,
    required String status,
    required String createdAt,
    @Default(<String>[]) List<String> cities,
  }) = _TripRequest;

  factory TripRequest.fromJson(Map<String, dynamic> json) =>
      _$TripRequestFromJson(json);
}

/// Body of POST /api/trip-requests (CreateTripRequestRequest). [cities] come from GET /api/attractions/cities,
/// in the order the tourist wants to visit them.
@freezed
abstract class CreateTripRequest with _$CreateTripRequest {
  const factory CreateTripRequest({
    required String objective,
    required String startDate,
    required String endDate,
    required int pax,
    required double budgetUsd,
    required Map<String, dynamic> preferences,
    required String nationality,
    required String passportNumber,
    required List<String> cities,
  }) = _CreateTripRequest;

  factory CreateTripRequest.fromJson(Map<String, dynamic> json) =>
      _$CreateTripRequestFromJson(json);
}

/// 202 body of POST /api/trip-requests/{id}/start-planning.
@freezed
abstract class StartPlanningResult with _$StartPlanningResult {
  const factory StartPlanningResult({
    required String workflowId,
    required String workflowStatus,
    required String tripStatus,
    String? errorSummary,
  }) = _StartPlanningResult;

  factory StartPlanningResult.fromJson(Map<String, dynamic> json) =>
      _$StartPlanningResultFromJson(json);
}

/// GET /api/attractions/{id}: used for the map markers.
@freezed
abstract class Attraction with _$Attraction {
  const factory Attraction({
    required String id,
    required String name,
    required String city,
    required double latitude,
    required double longitude,
  }) = _Attraction;

  factory Attraction.fromJson(Map<String, dynamic> json) =>
      _$AttractionFromJson(json);
}

/// One day of the itinerary, from either the saved itinerary or the agents' proposal.
@freezed
abstract class TripDay with _$TripDay {
  const factory TripDay({
    required int day,
    required String city,
    String? date,
    String? transport,
    String? weather,
    required List<TripStop> stops,
  }) = _TripDay;
}

@freezed
abstract class TripStop with _$TripStop {
  const factory TripStop({required String attractionId, required String name}) =
      _TripStop;
}

/// The fields of GET /api/trip-requests/{id}/workflow the trip screen needs.
@freezed
abstract class TripWorkflow with _$TripWorkflow {
  const factory TripWorkflow({
    required String id,
    required String status,
    String? currentStep,
    String? errorSummary,
    WorkflowOutcome? finalOutcome,
  }) = _TripWorkflow;

  factory TripWorkflow.fromJson(Map<String, dynamic> json) =>
      _$TripWorkflowFromJson(json);
}

@freezed
abstract class WorkflowOutcome with _$WorkflowOutcome {
  const factory WorkflowOutcome({required ProposalSummary proposal}) =
      _WorkflowOutcome;

  factory WorkflowOutcome.fromJson(Map<String, dynamic> json) =>
      _$WorkflowOutcomeFromJson(json);
}

@freezed
abstract class ProposalSummary with _$ProposalSummary {
  const factory ProposalSummary({List<ProposalDay>? days}) = _ProposalSummary;

  factory ProposalSummary.fromJson(Map<String, dynamic> json) =>
      _$ProposalSummaryFromJson(json);
}

/// A proposal day keeps the agent's snake_case names.
@freezed
abstract class ProposalDay with _$ProposalDay {
  const factory ProposalDay({
    required int day,
    required String date,
    required String city,
    List<ProposalStop>? stops,
    required String transport,
    String? weather,
  }) = _ProposalDay;

  factory ProposalDay.fromJson(Map<String, dynamic> json) =>
      _$ProposalDayFromJson(json);
}

@freezed
abstract class ProposalStop with _$ProposalStop {
  const factory ProposalStop({
    @JsonKey(name: 'attraction_id') required String attractionId,
    required String name,
  }) = _ProposalStop;

  factory ProposalStop.fromJson(Map<String, dynamic> json) =>
      _$ProposalStopFromJson(json);
}

/// GET /api/trip-requests/{id}/itinerary (ItineraryDto).
@freezed
abstract class Itinerary with _$Itinerary {
  const factory Itinerary({required List<ItineraryDay> days}) = _Itinerary;

  factory Itinerary.fromJson(Map<String, dynamic> json) =>
      _$ItineraryFromJson(json);
}

@freezed
abstract class ItineraryDay with _$ItineraryDay {
  const factory ItineraryDay({
    required int dayNumber,
    required String city,
    required List<ItineraryStop> stops,
  }) = _ItineraryDay;

  factory ItineraryDay.fromJson(Map<String, dynamic> json) =>
      _$ItineraryDayFromJson(json);
}

@freezed
abstract class ItineraryStop with _$ItineraryStop {
  const factory ItineraryStop({
    required String attractionId,
    required String attractionName,
  }) = _ItineraryStop;

  factory ItineraryStop.fromJson(Map<String, dynamic> json) =>
      _$ItineraryStopFromJson(json);
}

/// One event of GET /api/trip-requests/{id}/history (TripHistoryEntryDto). Actor is a role or "System".
@freezed
abstract class TripHistoryEntry with _$TripHistoryEntry {
  const factory TripHistoryEntry({
    required String at,
    required String action,
    required String actor,
    String? fromStatus,
    String? toStatus,
    String? reason,
  }) = _TripHistoryEntry;

  factory TripHistoryEntry.fromJson(Map<String, dynamic> json) =>
      _$TripHistoryEntryFromJson(json);
}

/// GET /api/trip-requests/{id}/cancellation (CancellationInfoDto). When [canCancel] is false, [closedReason]
/// says why and the app offers [operatorContact] instead.
@freezed
abstract class CancellationInfo with _$CancellationInfo {
  const factory CancellationInfo({
    required bool canCancel,
    required String cancelUntil,
    required int cutoffDays,
    String? closedReason,
    required String operatorContact,
  }) = _CancellationInfo;

  factory CancellationInfo.fromJson(Map<String, dynamic> json) =>
      _$CancellationInfoFromJson(json);
}

/// One voucher of GET /api/trips/{id}/vouchers (VoucherDto). [type] is "Trip" or "HotelNight".
/// The QR code shows [qrPayload] ("TRIPCRAFT-VOUCHER:...").
@freezed
abstract class TripVoucher with _$TripVoucher {
  const factory TripVoucher({
    required String id,
    required String type,
    String? hotelId,
    String? hotelName,
    String? night,
    required int rooms,
    required String code,
    required String qrPayload,
  }) = _TripVoucher;

  factory TripVoucher.fromJson(Map<String, dynamic> json) =>
      _$TripVoucherFromJson(json);
}
