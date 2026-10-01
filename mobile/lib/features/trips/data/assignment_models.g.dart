// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'assignment_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_TripAssignment _$TripAssignmentFromJson(Map<String, dynamic> json) =>
    _TripAssignment(
      tripRequestId: json['tripRequestId'] as String,
      guideId: json['guideId'] as String?,
      guideName: json['guideName'] as String?,
      guidePhone: json['guidePhone'] as String?,
      vehicleRegistrationNo: json['vehicleRegistrationNo'] as String?,
      vehicleType: json['vehicleType'] as String?,
      vehicleSeats: (json['vehicleSeats'] as num?)?.toInt(),
    );

Map<String, dynamic> _$TripAssignmentToJson(_TripAssignment instance) =>
    <String, dynamic>{
      'tripRequestId': instance.tripRequestId,
      'guideId': instance.guideId,
      'guideName': instance.guideName,
      'guidePhone': instance.guidePhone,
      'vehicleRegistrationNo': instance.vehicleRegistrationNo,
      'vehicleType': instance.vehicleType,
      'vehicleSeats': instance.vehicleSeats,
    };

_GuideRating _$GuideRatingFromJson(Map<String, dynamic> json) => _GuideRating(
  tripRequestId: json['tripRequestId'] as String,
  guideId: json['guideId'] as String,
  guideName: json['guideName'] as String,
  stars: (json['stars'] as num).toInt(),
  comment: json['comment'] as String?,
  ratedAt: json['ratedAt'] as String,
);

Map<String, dynamic> _$GuideRatingToJson(_GuideRating instance) =>
    <String, dynamic>{
      'tripRequestId': instance.tripRequestId,
      'guideId': instance.guideId,
      'guideName': instance.guideName,
      'stars': instance.stars,
      'comment': instance.comment,
      'ratedAt': instance.ratedAt,
    };
