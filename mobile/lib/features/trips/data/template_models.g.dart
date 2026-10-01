// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'template_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_TripTemplate _$TripTemplateFromJson(
  Map<String, dynamic> json,
) => _TripTemplate(
  id: json['id'] as String,
  slug: json['slug'] as String,
  name: json['name'] as String,
  moodTag: json['moodTag'] as String,
  summary: json['summary'] as String,
  objective: json['objective'] as String,
  days: (json['days'] as num).toInt(),
  cities:
      (json['cities'] as List<dynamic>?)?.map((e) => e as String).toList() ??
      const <String>[],
  preferences:
      json['preferences'] as Map<String, dynamic>? ?? const <String, dynamic>{},
  pricedForPax: (json['pricedForPax'] as num?)?.toInt() ?? 2,
  fromPriceLkr: (json['fromPriceLkr'] as num).toDouble(),
  fromPriceUsd: (json['fromPriceUsd'] as num).toDouble(),
  itinerary: (json['itinerary'] as List<dynamic>?)
      ?.map((e) => TemplateDay.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$TripTemplateToJson(_TripTemplate instance) =>
    <String, dynamic>{
      'id': instance.id,
      'slug': instance.slug,
      'name': instance.name,
      'moodTag': instance.moodTag,
      'summary': instance.summary,
      'objective': instance.objective,
      'days': instance.days,
      'cities': instance.cities,
      'preferences': instance.preferences,
      'pricedForPax': instance.pricedForPax,
      'fromPriceLkr': instance.fromPriceLkr,
      'fromPriceUsd': instance.fromPriceUsd,
      'itinerary': instance.itinerary,
    };

_TemplateDay _$TemplateDayFromJson(Map<String, dynamic> json) => _TemplateDay(
  day: (json['day'] as num).toInt(),
  city: json['city'] as String,
  stops:
      (json['stops'] as List<dynamic>?)
          ?.map((e) => TemplateStop.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const <TemplateStop>[],
);

Map<String, dynamic> _$TemplateDayToJson(_TemplateDay instance) =>
    <String, dynamic>{
      'day': instance.day,
      'city': instance.city,
      'stops': instance.stops,
    };

_TemplateStop _$TemplateStopFromJson(Map<String, dynamic> json) =>
    _TemplateStop(
      attractionId: json['attractionId'] as String?,
      name: json['name'] as String,
      entryFeeLkr: (json['entryFeeLkr'] as num?)?.toDouble() ?? 0,
      latitude: (json['latitude'] as num?)?.toDouble(),
      longitude: (json['longitude'] as num?)?.toDouble(),
    );

Map<String, dynamic> _$TemplateStopToJson(_TemplateStop instance) =>
    <String, dynamic>{
      'attractionId': instance.attractionId,
      'name': instance.name,
      'entryFeeLkr': instance.entryFeeLkr,
      'latitude': instance.latitude,
      'longitude': instance.longitude,
    };

_BookTemplateRequest _$BookTemplateRequestFromJson(Map<String, dynamic> json) =>
    _BookTemplateRequest(
      startDate: json['startDate'] as String,
      pax: (json['pax'] as num).toInt(),
      budgetUsd: (json['budgetUsd'] as num).toDouble(),
      nationality: json['nationality'] as String,
      passportNumber: json['passportNumber'] as String,
    );

Map<String, dynamic> _$BookTemplateRequestToJson(
  _BookTemplateRequest instance,
) => <String, dynamic>{
  'startDate': instance.startDate,
  'pax': instance.pax,
  'budgetUsd': instance.budgetUsd,
  'nationality': instance.nationality,
  'passportNumber': instance.passportNumber,
};

_BookTemplateResult _$BookTemplateResultFromJson(Map<String, dynamic> json) =>
    _BookTemplateResult(
      trip: TripRequest.fromJson(json['trip'] as Map<String, dynamic>),
      planning: StartPlanningResult.fromJson(
        json['planning'] as Map<String, dynamic>,
      ),
    );

Map<String, dynamic> _$BookTemplateResultToJson(_BookTemplateResult instance) =>
    <String, dynamic>{'trip': instance.trip, 'planning': instance.planning};
