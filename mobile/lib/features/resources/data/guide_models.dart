import 'package:freezed_annotation/freezed_annotation.dart';

import 'check_in.dart';

part 'guide_models.freezed.dart';
part 'guide_models.g.dart';

// Field names follow backend/src/TripCraft.Application/Resources/Dtos (ScheduleDtos.cs, HotelDtos.cs).

/// GET /api/guides/me/schedule.
@freezed
abstract class GuideSchedule with _$GuideSchedule {
  const factory GuideSchedule({
    required String guideId,
    required String guideName,
    @Default(<GuideTrip>[]) List<GuideTrip> trips,
  }) = _GuideSchedule;

  factory GuideSchedule.fromJson(Map<String, dynamic> json) =>
      _$GuideScheduleFromJson(json);
}

@freezed
abstract class GuideTrip with _$GuideTrip {
  const factory GuideTrip({
    required String tripRequestId,
    required String objective,
    required String startDate,
    required String endDate,
    required int pax,
    required String status,
    String? vehicleRegistrationNo,
    String? vehicleType,
    int? vehicleSeats,
    @Default(<GuideDay>[]) List<GuideDay> days,
  }) = _GuideTrip;

  factory GuideTrip.fromJson(Map<String, dynamic> json) =>
      _$GuideTripFromJson(json);
}

@freezed
abstract class GuideDay with _$GuideDay {
  const GuideDay._();

  const factory GuideDay({
    required int dayNumber,
    required String date,
    required String city,
    String? hotelName,
    @Default(<ScheduleStop>[]) List<ScheduleStop> stops,
  }) = _GuideDay;

  factory GuideDay.fromJson(Map<String, dynamic> json) =>
      _$GuideDayFromJson(json);

  /// The stops as the check-in screen needs them.
  List<GuideStop> get guideStops => [
    for (final s in stops)
      GuideStop(
        id: s.stopId,
        name: s.attractionName,
        latitude: s.latitude,
        longitude: s.longitude,
        checkedInAt: s.checkedInAt,
      ),
  ];
}

@freezed
abstract class ScheduleStop with _$ScheduleStop {
  const factory ScheduleStop({
    required String stopId,
    required int sequence,
    required String attractionName,
    required double latitude,
    required double longitude,
    String? checkedInAt,
  }) = _ScheduleStop;

  factory ScheduleStop.fromJson(Map<String, dynamic> json) =>
      _$ScheduleStopFromJson(json);
}

/// 200 body of POST /api/check-ins.
@freezed
abstract class CheckInResult with _$CheckInResult {
  const factory CheckInResult({
    required String stopId,
    required int distanceMeters,
    required String checkedInAt,
    required String tripStatus,
  }) = _CheckInResult;

  factory CheckInResult.fromJson(Map<String, dynamic> json) =>
      _$CheckInResultFromJson(json);
}

/// GET /api/hotels/{id} (the fields a guide needs after scanning a voucher).
@freezed
abstract class HotelInfo with _$HotelInfo {
  const factory HotelInfo({
    required String id,
    required String name,
    required String city,
    required int starRating,
    @Default(<RoomTypeInfo>[]) List<RoomTypeInfo> roomTypes,
  }) = _HotelInfo;

  factory HotelInfo.fromJson(Map<String, dynamic> json) =>
      _$HotelInfoFromJson(json);
}

@freezed
abstract class RoomTypeInfo with _$RoomTypeInfo {
  const factory RoomTypeInfo({required String name, required int capacity}) =
      _RoomTypeInfo;

  factory RoomTypeInfo.fromJson(Map<String, dynamic> json) =>
      _$RoomTypeInfoFromJson(json);
}
