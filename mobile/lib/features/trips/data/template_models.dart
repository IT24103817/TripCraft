import 'package:freezed_annotation/freezed_annotation.dart';

import 'trip_models.dart';

part 'template_models.freezed.dart';
part 'template_models.g.dart';

// Field names follow backend/src/TripCraft.Application/Trips/Templates/TripTemplateDtos.cs.

/// A mood package (GET /api/trip-templates). [itinerary] is only filled by GET /api/trip-templates/{id}.
/// [fromPriceUsd] is the price for [pricedForPax] people (2) at today's rates.
@freezed
abstract class TripTemplate with _$TripTemplate {
  const factory TripTemplate({
    required String id,
    required String slug,
    required String name,
    required String moodTag,
    required String summary,
    required String objective,
    required int days,
    @Default(<String>[]) List<String> cities,
    @Default(<String, dynamic>{}) Map<String, dynamic> preferences,
    @Default(2) int pricedForPax,
    required double fromPriceLkr,
    required double fromPriceUsd,
    List<TemplateDay>? itinerary,
  }) = _TripTemplate;

  factory TripTemplate.fromJson(Map<String, dynamic> json) =>
      _$TripTemplateFromJson(json);
}

@freezed
abstract class TemplateDay with _$TemplateDay {
  const factory TemplateDay({
    required int day,
    required String city,
    @Default(<TemplateStop>[]) List<TemplateStop> stops,
  }) = _TemplateDay;

  factory TemplateDay.fromJson(Map<String, dynamic> json) =>
      _$TemplateDayFromJson(json);
}

@freezed
abstract class TemplateStop with _$TemplateStop {
  const factory TemplateStop({
    String? attractionId,
    required String name,
    @Default(0) double entryFeeLkr,
    double? latitude,
    double? longitude,
  }) = _TemplateStop;

  factory TemplateStop.fromJson(Map<String, dynamic> json) =>
      _$TemplateStopFromJson(json);
}

/// Body of POST /api/trip-templates/{id}/book ("Book as is"). [startDate] is "yyyy-MM-dd".
@freezed
abstract class BookTemplateRequest with _$BookTemplateRequest {
  const factory BookTemplateRequest({
    required String startDate,
    required int pax,
    required double budgetUsd,
    required String nationality,
    required String passportNumber,
  }) = _BookTemplateRequest;

  factory BookTemplateRequest.fromJson(Map<String, dynamic> json) =>
      _$BookTemplateRequestFromJson(json);
}

/// 202 body of the book call: the new trip and the planning that started for it.
@freezed
abstract class BookTemplateResult with _$BookTemplateResult {
  const factory BookTemplateResult({
    required TripRequest trip,
    required StartPlanningResult planning,
  }) = _BookTemplateResult;

  factory BookTemplateResult.fromJson(Map<String, dynamic> json) =>
      _$BookTemplateResultFromJson(json);
}
