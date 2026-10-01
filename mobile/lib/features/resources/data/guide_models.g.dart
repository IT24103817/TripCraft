// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'guide_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_GuideSchedule _$GuideScheduleFromJson(Map<String, dynamic> json) =>
    _GuideSchedule(
      guideId: json['guideId'] as String,
      guideName: json['guideName'] as String,
      trips:
          (json['trips'] as List<dynamic>?)
              ?.map((e) => GuideTrip.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const <GuideTrip>[],
    );

Map<String, dynamic> _$GuideScheduleToJson(_GuideSchedule instance) =>
    <String, dynamic>{
      'guideId': instance.guideId,
      'guideName': instance.guideName,
      'trips': instance.trips,
    };

_GuideTrip _$GuideTripFromJson(Map<String, dynamic> json) => _GuideTrip(
  tripRequestId: json['tripRequestId'] as String,
  objective: json['objective'] as String,
  startDate: json['startDate'] as String,
  endDate: json['endDate'] as String,
  pax: (json['pax'] as num).toInt(),
  status: json['status'] as String,
  vehicleRegistrationNo: json['vehicleRegistrationNo'] as String?,
  vehicleType: json['vehicleType'] as String?,
  vehicleSeats: (json['vehicleSeats'] as num?)?.toInt(),
  days:
      (json['days'] as List<dynamic>?)
          ?.map((e) => GuideDay.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const <GuideDay>[],
);

Map<String, dynamic> _$GuideTripToJson(_GuideTrip instance) =>
    <String, dynamic>{
      'tripRequestId': instance.tripRequestId,
      'objective': instance.objective,
      'startDate': instance.startDate,
      'endDate': instance.endDate,
      'pax': instance.pax,
      'status': instance.status,
      'vehicleRegistrationNo': instance.vehicleRegistrationNo,
      'vehicleType': instance.vehicleType,
      'vehicleSeats': instance.vehicleSeats,
      'days': instance.days,
    };

_GuideDay _$GuideDayFromJson(Map<String, dynamic> json) => _GuideDay(
  dayNumber: (json['dayNumber'] as num).toInt(),
  date: json['date'] as String,
  city: json['city'] as String,
  hotelName: json['hotelName'] as String?,
  stops:
      (json['stops'] as List<dynamic>?)
          ?.map((e) => ScheduleStop.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const <ScheduleStop>[],
);

Map<String, dynamic> _$GuideDayToJson(_GuideDay instance) => <String, dynamic>{
  'dayNumber': instance.dayNumber,
  'date': instance.date,
  'city': instance.city,
  'hotelName': instance.hotelName,
  'stops': instance.stops,
};

_ScheduleStop _$ScheduleStopFromJson(Map<String, dynamic> json) =>
    _ScheduleStop(
      stopId: json['stopId'] as String,
      sequence: (json['sequence'] as num).toInt(),
      attractionName: json['attractionName'] as String,
      latitude: (json['latitude'] as num).toDouble(),
      longitude: (json['longitude'] as num).toDouble(),
      checkedInAt: json['checkedInAt'] as String?,
    );

Map<String, dynamic> _$ScheduleStopToJson(_ScheduleStop instance) =>
    <String, dynamic>{
      'stopId': instance.stopId,
      'sequence': instance.sequence,
      'attractionName': instance.attractionName,
      'latitude': instance.latitude,
      'longitude': instance.longitude,
      'checkedInAt': instance.checkedInAt,
    };

_CheckInResult _$CheckInResultFromJson(Map<String, dynamic> json) =>
    _CheckInResult(
      stopId: json['stopId'] as String,
      distanceMeters: (json['distanceMeters'] as num?)?.toInt(),
      checkedInAt: json['checkedInAt'] as String,
      tripStatus: json['tripStatus'] as String,
      method: json['method'] as String? ?? 'Gps',
      stopName: json['stopName'] as String? ?? '',
    );

Map<String, dynamic> _$CheckInResultToJson(_CheckInResult instance) =>
    <String, dynamic>{
      'stopId': instance.stopId,
      'distanceMeters': instance.distanceMeters,
      'checkedInAt': instance.checkedInAt,
      'tripStatus': instance.tripStatus,
      'method': instance.method,
      'stopName': instance.stopName,
    };

_HotelInfo _$HotelInfoFromJson(Map<String, dynamic> json) => _HotelInfo(
  id: json['id'] as String,
  name: json['name'] as String,
  city: json['city'] as String,
  starRating: (json['starRating'] as num).toInt(),
  roomTypes:
      (json['roomTypes'] as List<dynamic>?)
          ?.map((e) => RoomTypeInfo.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const <RoomTypeInfo>[],
);

Map<String, dynamic> _$HotelInfoToJson(_HotelInfo instance) =>
    <String, dynamic>{
      'id': instance.id,
      'name': instance.name,
      'city': instance.city,
      'starRating': instance.starRating,
      'roomTypes': instance.roomTypes,
    };

_RoomTypeInfo _$RoomTypeInfoFromJson(Map<String, dynamic> json) =>
    _RoomTypeInfo(
      name: json['name'] as String,
      capacity: (json['capacity'] as num).toInt(),
    );

Map<String, dynamic> _$RoomTypeInfoToJson(_RoomTypeInfo instance) =>
    <String, dynamic>{'name': instance.name, 'capacity': instance.capacity};
