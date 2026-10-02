import 'package:freezed_annotation/freezed_annotation.dart';

part 'tourist_profile.freezed.dart';
part 'tourist_profile.g.dart';

// Field names follow backend/src/TripCraft.Application/Trips/Dtos/TouristProfileDto.cs.

/// GET /api/tourists/me. The strings are empty until the tourist submits a first trip or uploads a photo.
@freezed
abstract class TouristProfile with _$TouristProfile {
  const factory TouristProfile({
    @Default('') String nationality,
    @Default('') String passportNumberMasked,
    @Default(false) bool hasPassportPhoto,
  }) = _TouristProfile;

  factory TouristProfile.fromJson(Map<String, dynamic> json) =>
      _$TouristProfileFromJson(json);
}
