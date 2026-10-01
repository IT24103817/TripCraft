import 'package:freezed_annotation/freezed_annotation.dart';

part 'assignment_models.freezed.dart';
part 'assignment_models.g.dart';

// Field names follow backend/src/TripCraft.Application/Resources/Dtos/TripAssignmentDtos.cs.

/// GET /api/trip-requests/{id}/assignment: the guide and vehicle booked for the trip. Every field is null
/// before the operator confirms the trip.
@freezed
abstract class TripAssignment with _$TripAssignment {
  const factory TripAssignment({
    required String tripRequestId,
    String? guideId,
    String? guideName,
    String? guidePhone,
    String? vehicleRegistrationNo,
    String? vehicleType,
    int? vehicleSeats,
  }) = _TripAssignment;

  factory TripAssignment.fromJson(Map<String, dynamic> json) =>
      _$TripAssignmentFromJson(json);
}

/// GET or POST /api/trip-requests/{id}/guide-rating: the tourist's 1–5 star rating of the guide.
@freezed
abstract class GuideRating with _$GuideRating {
  const factory GuideRating({
    required String tripRequestId,
    required String guideId,
    required String guideName,
    required int stars,
    String? comment,
    required String ratedAt,
  }) = _GuideRating;

  factory GuideRating.fromJson(Map<String, dynamic> json) =>
      _$GuideRatingFromJson(json);
}
