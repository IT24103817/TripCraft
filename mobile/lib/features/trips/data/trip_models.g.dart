// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'trip_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_TripRequest _$TripRequestFromJson(Map<String, dynamic> json) => _TripRequest(
  id: json['id'] as String,
  objective: json['objective'] as String,
  startDate: json['startDate'] as String,
  endDate: json['endDate'] as String,
  pax: (json['pax'] as num).toInt(),
  budgetUsd: (json['budgetUsd'] as num).toDouble(),
  preferences:
      json['preferences'] as Map<String, dynamic>? ?? const <String, dynamic>{},
  status: json['status'] as String,
  createdAt: json['createdAt'] as String,
  cities:
      (json['cities'] as List<dynamic>?)?.map((e) => e as String).toList() ??
      const <String>[],
);

Map<String, dynamic> _$TripRequestToJson(_TripRequest instance) =>
    <String, dynamic>{
      'id': instance.id,
      'objective': instance.objective,
      'startDate': instance.startDate,
      'endDate': instance.endDate,
      'pax': instance.pax,
      'budgetUsd': instance.budgetUsd,
      'preferences': instance.preferences,
      'status': instance.status,
      'createdAt': instance.createdAt,
      'cities': instance.cities,
    };

_CreateTripRequest _$CreateTripRequestFromJson(Map<String, dynamic> json) =>
    _CreateTripRequest(
      objective: json['objective'] as String,
      startDate: json['startDate'] as String,
      endDate: json['endDate'] as String,
      pax: (json['pax'] as num).toInt(),
      budgetUsd: (json['budgetUsd'] as num).toDouble(),
      preferences: json['preferences'] as Map<String, dynamic>,
      nationality: json['nationality'] as String,
      passportNumber: json['passportNumber'] as String,
      cities: (json['cities'] as List<dynamic>)
          .map((e) => e as String)
          .toList(),
    );

Map<String, dynamic> _$CreateTripRequestToJson(_CreateTripRequest instance) =>
    <String, dynamic>{
      'objective': instance.objective,
      'startDate': instance.startDate,
      'endDate': instance.endDate,
      'pax': instance.pax,
      'budgetUsd': instance.budgetUsd,
      'preferences': instance.preferences,
      'nationality': instance.nationality,
      'passportNumber': instance.passportNumber,
      'cities': instance.cities,
    };

_StartPlanningResult _$StartPlanningResultFromJson(Map<String, dynamic> json) =>
    _StartPlanningResult(
      workflowId: json['workflowId'] as String,
      workflowStatus: json['workflowStatus'] as String,
      tripStatus: json['tripStatus'] as String,
      errorSummary: json['errorSummary'] as String?,
    );

Map<String, dynamic> _$StartPlanningResultToJson(
  _StartPlanningResult instance,
) => <String, dynamic>{
  'workflowId': instance.workflowId,
  'workflowStatus': instance.workflowStatus,
  'tripStatus': instance.tripStatus,
  'errorSummary': instance.errorSummary,
};

_Attraction _$AttractionFromJson(Map<String, dynamic> json) => _Attraction(
  id: json['id'] as String,
  name: json['name'] as String,
  city: json['city'] as String,
  latitude: (json['latitude'] as num).toDouble(),
  longitude: (json['longitude'] as num).toDouble(),
);

Map<String, dynamic> _$AttractionToJson(_Attraction instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'city': instance.city,
      'latitude': instance.latitude,
      'longitude': instance.longitude,
    };

_TripWorkflow _$TripWorkflowFromJson(Map<String, dynamic> json) =>
    _TripWorkflow(
      id: json['id'] as String,
      status: json['status'] as String,
      currentStep: json['currentStep'] as String?,
      errorSummary: json['errorSummary'] as String?,
      finalOutcome: json['finalOutcome'] == null
          ? null
          : WorkflowOutcome.fromJson(
              json['finalOutcome'] as Map<String, dynamic>,
            ),
    );

Map<String, dynamic> _$TripWorkflowToJson(_TripWorkflow instance) =>
    <String, dynamic>{
      'id': instance.id,
      'status': instance.status,
      'currentStep': instance.currentStep,
      'errorSummary': instance.errorSummary,
      'finalOutcome': instance.finalOutcome,
    };

_WorkflowOutcome _$WorkflowOutcomeFromJson(Map<String, dynamic> json) =>
    _WorkflowOutcome(
      proposal: ProposalSummary.fromJson(
        json['proposal'] as Map<String, dynamic>,
      ),
    );

Map<String, dynamic> _$WorkflowOutcomeToJson(_WorkflowOutcome instance) =>
    <String, dynamic>{'proposal': instance.proposal};

_ProposalSummary _$ProposalSummaryFromJson(Map<String, dynamic> json) =>
    _ProposalSummary(
      days: (json['days'] as List<dynamic>?)
          ?.map((e) => ProposalDay.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$ProposalSummaryToJson(_ProposalSummary instance) =>
    <String, dynamic>{'days': instance.days};

_ProposalDay _$ProposalDayFromJson(Map<String, dynamic> json) => _ProposalDay(
  day: (json['day'] as num).toInt(),
  date: json['date'] as String,
  city: json['city'] as String,
  stops: (json['stops'] as List<dynamic>?)
      ?.map((e) => ProposalStop.fromJson(e as Map<String, dynamic>))
      .toList(),
  transport: json['transport'] as String,
  weather: json['weather'] as String?,
);

Map<String, dynamic> _$ProposalDayToJson(_ProposalDay instance) =>
    <String, dynamic>{
      'day': instance.day,
      'date': instance.date,
      'city': instance.city,
      'stops': instance.stops,
      'transport': instance.transport,
      'weather': instance.weather,
    };

_ProposalStop _$ProposalStopFromJson(Map<String, dynamic> json) =>
    _ProposalStop(
      attractionId: json['attraction_id'] as String,
      name: json['name'] as String,
    );

Map<String, dynamic> _$ProposalStopToJson(_ProposalStop instance) =>
    <String, dynamic>{
      'attraction_id': instance.attractionId,
      'name': instance.name,
    };

_Itinerary _$ItineraryFromJson(Map<String, dynamic> json) => _Itinerary(
  days: (json['days'] as List<dynamic>)
      .map((e) => ItineraryDay.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$ItineraryToJson(_Itinerary instance) =>
    <String, dynamic>{'days': instance.days};

_ItineraryDay _$ItineraryDayFromJson(Map<String, dynamic> json) =>
    _ItineraryDay(
      dayNumber: (json['dayNumber'] as num).toInt(),
      city: json['city'] as String,
      notes: json['notes'] as String?,
      stops: (json['stops'] as List<dynamic>)
          .map((e) => ItineraryStop.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$ItineraryDayToJson(_ItineraryDay instance) =>
    <String, dynamic>{
      'dayNumber': instance.dayNumber,
      'city': instance.city,
      'notes': instance.notes,
      'stops': instance.stops,
    };

_ItineraryStop _$ItineraryStopFromJson(Map<String, dynamic> json) =>
    _ItineraryStop(
      attractionId: json['attractionId'] as String,
      attractionName: json['attractionName'] as String,
      latitude: (json['latitude'] as num?)?.toDouble(),
      longitude: (json['longitude'] as num?)?.toDouble(),
    );

Map<String, dynamic> _$ItineraryStopToJson(_ItineraryStop instance) =>
    <String, dynamic>{
      'attractionId': instance.attractionId,
      'attractionName': instance.attractionName,
      'latitude': instance.latitude,
      'longitude': instance.longitude,
    };

_TripHistoryEntry _$TripHistoryEntryFromJson(Map<String, dynamic> json) =>
    _TripHistoryEntry(
      at: json['at'] as String,
      action: json['action'] as String,
      actor: json['actor'] as String,
      fromStatus: json['fromStatus'] as String?,
      toStatus: json['toStatus'] as String?,
      reason: json['reason'] as String?,
    );

Map<String, dynamic> _$TripHistoryEntryToJson(_TripHistoryEntry instance) =>
    <String, dynamic>{
      'at': instance.at,
      'action': instance.action,
      'actor': instance.actor,
      'fromStatus': instance.fromStatus,
      'toStatus': instance.toStatus,
      'reason': instance.reason,
    };

_CancellationInfo _$CancellationInfoFromJson(Map<String, dynamic> json) =>
    _CancellationInfo(
      canCancel: json['canCancel'] as bool,
      cancelUntil: json['cancelUntil'] as String,
      cutoffDays: (json['cutoffDays'] as num).toInt(),
      closedReason: json['closedReason'] as String?,
      operatorContact: json['operatorContact'] as String,
    );

Map<String, dynamic> _$CancellationInfoToJson(_CancellationInfo instance) =>
    <String, dynamic>{
      'canCancel': instance.canCancel,
      'cancelUntil': instance.cancelUntil,
      'cutoffDays': instance.cutoffDays,
      'closedReason': instance.closedReason,
      'operatorContact': instance.operatorContact,
    };

_TripVoucher _$TripVoucherFromJson(Map<String, dynamic> json) => _TripVoucher(
  id: json['id'] as String,
  type: json['type'] as String,
  hotelId: json['hotelId'] as String?,
  hotelName: json['hotelName'] as String?,
  night: json['night'] as String?,
  rooms: (json['rooms'] as num).toInt(),
  code: json['code'] as String,
  qrPayload: json['qrPayload'] as String,
);

Map<String, dynamic> _$TripVoucherToJson(_TripVoucher instance) =>
    <String, dynamic>{
      'id': instance.id,
      'type': instance.type,
      'hotelId': instance.hotelId,
      'hotelName': instance.hotelName,
      'night': instance.night,
      'rooms': instance.rooms,
      'code': instance.code,
      'qrPayload': instance.qrPayload,
    };
