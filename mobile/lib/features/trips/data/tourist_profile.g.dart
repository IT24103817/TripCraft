// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'tourist_profile.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_TouristProfile _$TouristProfileFromJson(Map<String, dynamic> json) =>
    _TouristProfile(
      nationality: json['nationality'] as String? ?? '',
      passportNumberMasked: json['passportNumberMasked'] as String? ?? '',
      hasPassportPhoto: json['hasPassportPhoto'] as bool? ?? false,
    );

Map<String, dynamic> _$TouristProfileToJson(_TouristProfile instance) =>
    <String, dynamic>{
      'nationality': instance.nationality,
      'passportNumberMasked': instance.passportNumberMasked,
      'hasPassportPhoto': instance.hasPassportPhoto,
    };
