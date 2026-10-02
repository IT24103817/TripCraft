// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint, type=warning, deprecated_member_use, deprecated_member_use_from_same_package
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'tourist_profile.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$TouristProfile {

 String get nationality; String get passportNumberMasked; bool get hasPassportPhoto;
/// Create a copy of TouristProfile
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TouristProfileCopyWith<TouristProfile> get copyWith => _$TouristProfileCopyWithImpl<TouristProfile>(this as TouristProfile, _$identity);

  /// Serializes this TouristProfile to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TouristProfile;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TouristProfile&&(identical(other.nationality, _this.nationality) || other.nationality == _this.nationality)&&(identical(other.passportNumberMasked, _this.passportNumberMasked) || other.passportNumberMasked == _this.passportNumberMasked)&&(identical(other.hasPassportPhoto, _this.hasPassportPhoto) || other.hasPassportPhoto == _this.hasPassportPhoto));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TouristProfile;
  return Object.hash(runtimeType,_this.nationality,_this.passportNumberMasked,_this.hasPassportPhoto);
}

@override
String toString() {
  final _this = this as TouristProfile;
  return 'TouristProfile(nationality: ${_this.nationality}, passportNumberMasked: ${_this.passportNumberMasked}, hasPassportPhoto: ${_this.hasPassportPhoto})';
}


}

/// @nodoc
abstract mixin class $TouristProfileCopyWith<$Res>  {
  factory $TouristProfileCopyWith(TouristProfile value, $Res Function(TouristProfile) _then) = _$TouristProfileCopyWithImpl;
@useResult
$Res call({
 String nationality, String passportNumberMasked, bool hasPassportPhoto
});




}
/// @nodoc
class _$TouristProfileCopyWithImpl<$Res>
    implements $TouristProfileCopyWith<$Res> {
  _$TouristProfileCopyWithImpl(this._self, this._then);

  final TouristProfile _self;
  final $Res Function(TouristProfile) _then;

/// Create a copy of TouristProfile
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? nationality = null,Object? passportNumberMasked = null,Object? hasPassportPhoto = null,}) {
  return _then(TouristProfile(
nationality: null == nationality ? _self.nationality : nationality // ignore: cast_nullable_to_non_nullable
as String,passportNumberMasked: null == passportNumberMasked ? _self.passportNumberMasked : passportNumberMasked // ignore: cast_nullable_to_non_nullable
as String,hasPassportPhoto: null == hasPassportPhoto ? _self.hasPassportPhoto : hasPassportPhoto // ignore: cast_nullable_to_non_nullable
as bool,
  ));
}

}


/// Adds pattern-matching-related methods to [TouristProfile].
extension TouristProfilePatterns on TouristProfile {
/// A variant of `map` that fallback to returning `orElse`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TouristProfile value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TouristProfile() when $default != null:
return $default(_that);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// Callbacks receives the raw object, upcasted.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case final Subclass2 value:
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TouristProfile value)  $default,){
final _that = this;
switch (_that) {
case _TouristProfile():
return $default(_that);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `map` that fallback to returning `null`.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case final Subclass value:
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TouristProfile value)?  $default,){
final _that = this;
switch (_that) {
case _TouristProfile() when $default != null:
return $default(_that);case _:
  return null;

}
}
/// A variant of `when` that fallback to an `orElse` callback.
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return orElse();
/// }
/// ```

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String nationality,  String passportNumberMasked,  bool hasPassportPhoto)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TouristProfile() when $default != null:
return $default(_that.nationality,_that.passportNumberMasked,_that.hasPassportPhoto);case _:
  return orElse();

}
}
/// A `switch`-like method, using callbacks.
///
/// As opposed to `map`, this offers destructuring.
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case Subclass2(:final field2):
///     return ...;
/// }
/// ```

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String nationality,  String passportNumberMasked,  bool hasPassportPhoto)  $default,) {final _that = this;
switch (_that) {
case _TouristProfile():
return $default(_that.nationality,_that.passportNumberMasked,_that.hasPassportPhoto);case _:
  throw StateError('Unexpected subclass');

}
}
/// A variant of `when` that fallback to returning `null`
///
/// It is equivalent to doing:
/// ```dart
/// switch (sealedClass) {
///   case Subclass(:final field):
///     return ...;
///   case _:
///     return null;
/// }
/// ```

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String nationality,  String passportNumberMasked,  bool hasPassportPhoto)?  $default,) {final _that = this;
switch (_that) {
case _TouristProfile() when $default != null:
return $default(_that.nationality,_that.passportNumberMasked,_that.hasPassportPhoto);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TouristProfile implements TouristProfile {
  const _TouristProfile({this.nationality = '', this.passportNumberMasked = '', this.hasPassportPhoto = false});
  factory _TouristProfile.fromJson(Map<String, dynamic> json) => _$TouristProfileFromJson(json);

@override@JsonKey() final  String nationality;
@override@JsonKey() final  String passportNumberMasked;
@override@JsonKey() final  bool hasPassportPhoto;

/// Create a copy of TouristProfile
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TouristProfileCopyWith<_TouristProfile> get copyWith => __$TouristProfileCopyWithImpl<_TouristProfile>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TouristProfileToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TouristProfile&&(identical(other.nationality, nationality) || other.nationality == nationality)&&(identical(other.passportNumberMasked, passportNumberMasked) || other.passportNumberMasked == passportNumberMasked)&&(identical(other.hasPassportPhoto, hasPassportPhoto) || other.hasPassportPhoto == hasPassportPhoto));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,nationality,passportNumberMasked,hasPassportPhoto);
}

@override
String toString() {
    return 'TouristProfile(nationality: $nationality, passportNumberMasked: $passportNumberMasked, hasPassportPhoto: $hasPassportPhoto)';
}


}

/// @nodoc
abstract mixin class _$TouristProfileCopyWith<$Res> implements $TouristProfileCopyWith<$Res> {
  factory _$TouristProfileCopyWith(_TouristProfile value, $Res Function(_TouristProfile) _then) = __$TouristProfileCopyWithImpl;
@override @useResult
$Res call({
 String nationality, String passportNumberMasked, bool hasPassportPhoto
});




}
/// @nodoc
class __$TouristProfileCopyWithImpl<$Res>
    implements _$TouristProfileCopyWith<$Res> {
  __$TouristProfileCopyWithImpl(this._self, this._then);

  final _TouristProfile _self;
  final $Res Function(_TouristProfile) _then;

/// Create a copy of TouristProfile
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? nationality = null,Object? passportNumberMasked = null,Object? hasPassportPhoto = null,}) {
  return _then(_TouristProfile(
nationality: null == nationality ? _self.nationality : nationality // ignore: cast_nullable_to_non_nullable
as String,passportNumberMasked: null == passportNumberMasked ? _self.passportNumberMasked : passportNumberMasked // ignore: cast_nullable_to_non_nullable
as String,hasPassportPhoto: null == hasPassportPhoto ? _self.hasPassportPhoto : hasPassportPhoto // ignore: cast_nullable_to_non_nullable
as bool,
  ));
}


}

// dart format on
