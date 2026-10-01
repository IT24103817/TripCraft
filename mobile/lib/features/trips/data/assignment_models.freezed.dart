// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint, type=warning, deprecated_member_use, deprecated_member_use_from_same_package
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'assignment_models.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$TripAssignment {

 String get tripRequestId; String? get guideId; String? get guideName; String? get guidePhone; String? get vehicleRegistrationNo; String? get vehicleType; int? get vehicleSeats;
/// Create a copy of TripAssignment
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripAssignmentCopyWith<TripAssignment> get copyWith => _$TripAssignmentCopyWithImpl<TripAssignment>(this as TripAssignment, _$identity);

  /// Serializes this TripAssignment to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TripAssignment;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripAssignment&&(identical(other.tripRequestId, _this.tripRequestId) || other.tripRequestId == _this.tripRequestId)&&(identical(other.guideId, _this.guideId) || other.guideId == _this.guideId)&&(identical(other.guideName, _this.guideName) || other.guideName == _this.guideName)&&(identical(other.guidePhone, _this.guidePhone) || other.guidePhone == _this.guidePhone)&&(identical(other.vehicleRegistrationNo, _this.vehicleRegistrationNo) || other.vehicleRegistrationNo == _this.vehicleRegistrationNo)&&(identical(other.vehicleType, _this.vehicleType) || other.vehicleType == _this.vehicleType)&&(identical(other.vehicleSeats, _this.vehicleSeats) || other.vehicleSeats == _this.vehicleSeats));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TripAssignment;
  return Object.hash(runtimeType,_this.tripRequestId,_this.guideId,_this.guideName,_this.guidePhone,_this.vehicleRegistrationNo,_this.vehicleType,_this.vehicleSeats);
}

@override
String toString() {
  final _this = this as TripAssignment;
  return 'TripAssignment(tripRequestId: ${_this.tripRequestId}, guideId: ${_this.guideId}, guideName: ${_this.guideName}, guidePhone: ${_this.guidePhone}, vehicleRegistrationNo: ${_this.vehicleRegistrationNo}, vehicleType: ${_this.vehicleType}, vehicleSeats: ${_this.vehicleSeats})';
}


}

/// @nodoc
abstract mixin class $TripAssignmentCopyWith<$Res>  {
  factory $TripAssignmentCopyWith(TripAssignment value, $Res Function(TripAssignment) _then) = _$TripAssignmentCopyWithImpl;
@useResult
$Res call({
 String tripRequestId, String? guideId, String? guideName, String? guidePhone, String? vehicleRegistrationNo, String? vehicleType, int? vehicleSeats
});




}
/// @nodoc
class _$TripAssignmentCopyWithImpl<$Res>
    implements $TripAssignmentCopyWith<$Res> {
  _$TripAssignmentCopyWithImpl(this._self, this._then);

  final TripAssignment _self;
  final $Res Function(TripAssignment) _then;

/// Create a copy of TripAssignment
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? tripRequestId = null,Object? guideId = freezed,Object? guideName = freezed,Object? guidePhone = freezed,Object? vehicleRegistrationNo = freezed,Object? vehicleType = freezed,Object? vehicleSeats = freezed,}) {
  return _then(TripAssignment(
tripRequestId: null == tripRequestId ? _self.tripRequestId : tripRequestId // ignore: cast_nullable_to_non_nullable
as String,guideId: freezed == guideId ? _self.guideId : guideId // ignore: cast_nullable_to_non_nullable
as String?,guideName: freezed == guideName ? _self.guideName : guideName // ignore: cast_nullable_to_non_nullable
as String?,guidePhone: freezed == guidePhone ? _self.guidePhone : guidePhone // ignore: cast_nullable_to_non_nullable
as String?,vehicleRegistrationNo: freezed == vehicleRegistrationNo ? _self.vehicleRegistrationNo : vehicleRegistrationNo // ignore: cast_nullable_to_non_nullable
as String?,vehicleType: freezed == vehicleType ? _self.vehicleType : vehicleType // ignore: cast_nullable_to_non_nullable
as String?,vehicleSeats: freezed == vehicleSeats ? _self.vehicleSeats : vehicleSeats // ignore: cast_nullable_to_non_nullable
as int?,
  ));
}

}


/// Adds pattern-matching-related methods to [TripAssignment].
extension TripAssignmentPatterns on TripAssignment {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripAssignment value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripAssignment() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripAssignment value)  $default,){
final _that = this;
switch (_that) {
case _TripAssignment():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripAssignment value)?  $default,){
final _that = this;
switch (_that) {
case _TripAssignment() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String tripRequestId,  String? guideId,  String? guideName,  String? guidePhone,  String? vehicleRegistrationNo,  String? vehicleType,  int? vehicleSeats)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripAssignment() when $default != null:
return $default(_that.tripRequestId,_that.guideId,_that.guideName,_that.guidePhone,_that.vehicleRegistrationNo,_that.vehicleType,_that.vehicleSeats);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String tripRequestId,  String? guideId,  String? guideName,  String? guidePhone,  String? vehicleRegistrationNo,  String? vehicleType,  int? vehicleSeats)  $default,) {final _that = this;
switch (_that) {
case _TripAssignment():
return $default(_that.tripRequestId,_that.guideId,_that.guideName,_that.guidePhone,_that.vehicleRegistrationNo,_that.vehicleType,_that.vehicleSeats);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String tripRequestId,  String? guideId,  String? guideName,  String? guidePhone,  String? vehicleRegistrationNo,  String? vehicleType,  int? vehicleSeats)?  $default,) {final _that = this;
switch (_that) {
case _TripAssignment() when $default != null:
return $default(_that.tripRequestId,_that.guideId,_that.guideName,_that.guidePhone,_that.vehicleRegistrationNo,_that.vehicleType,_that.vehicleSeats);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TripAssignment implements TripAssignment {
  const _TripAssignment({required this.tripRequestId, this.guideId, this.guideName, this.guidePhone, this.vehicleRegistrationNo, this.vehicleType, this.vehicleSeats});
  factory _TripAssignment.fromJson(Map<String, dynamic> json) => _$TripAssignmentFromJson(json);

@override final  String tripRequestId;
@override final  String? guideId;
@override final  String? guideName;
@override final  String? guidePhone;
@override final  String? vehicleRegistrationNo;
@override final  String? vehicleType;
@override final  int? vehicleSeats;

/// Create a copy of TripAssignment
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripAssignmentCopyWith<_TripAssignment> get copyWith => __$TripAssignmentCopyWithImpl<_TripAssignment>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TripAssignmentToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripAssignment&&(identical(other.tripRequestId, tripRequestId) || other.tripRequestId == tripRequestId)&&(identical(other.guideId, guideId) || other.guideId == guideId)&&(identical(other.guideName, guideName) || other.guideName == guideName)&&(identical(other.guidePhone, guidePhone) || other.guidePhone == guidePhone)&&(identical(other.vehicleRegistrationNo, vehicleRegistrationNo) || other.vehicleRegistrationNo == vehicleRegistrationNo)&&(identical(other.vehicleType, vehicleType) || other.vehicleType == vehicleType)&&(identical(other.vehicleSeats, vehicleSeats) || other.vehicleSeats == vehicleSeats));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,tripRequestId,guideId,guideName,guidePhone,vehicleRegistrationNo,vehicleType,vehicleSeats);
}

@override
String toString() {
    return 'TripAssignment(tripRequestId: $tripRequestId, guideId: $guideId, guideName: $guideName, guidePhone: $guidePhone, vehicleRegistrationNo: $vehicleRegistrationNo, vehicleType: $vehicleType, vehicleSeats: $vehicleSeats)';
}


}

/// @nodoc
abstract mixin class _$TripAssignmentCopyWith<$Res> implements $TripAssignmentCopyWith<$Res> {
  factory _$TripAssignmentCopyWith(_TripAssignment value, $Res Function(_TripAssignment) _then) = __$TripAssignmentCopyWithImpl;
@override @useResult
$Res call({
 String tripRequestId, String? guideId, String? guideName, String? guidePhone, String? vehicleRegistrationNo, String? vehicleType, int? vehicleSeats
});




}
/// @nodoc
class __$TripAssignmentCopyWithImpl<$Res>
    implements _$TripAssignmentCopyWith<$Res> {
  __$TripAssignmentCopyWithImpl(this._self, this._then);

  final _TripAssignment _self;
  final $Res Function(_TripAssignment) _then;

/// Create a copy of TripAssignment
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? tripRequestId = null,Object? guideId = freezed,Object? guideName = freezed,Object? guidePhone = freezed,Object? vehicleRegistrationNo = freezed,Object? vehicleType = freezed,Object? vehicleSeats = freezed,}) {
  return _then(_TripAssignment(
tripRequestId: null == tripRequestId ? _self.tripRequestId : tripRequestId // ignore: cast_nullable_to_non_nullable
as String,guideId: freezed == guideId ? _self.guideId : guideId // ignore: cast_nullable_to_non_nullable
as String?,guideName: freezed == guideName ? _self.guideName : guideName // ignore: cast_nullable_to_non_nullable
as String?,guidePhone: freezed == guidePhone ? _self.guidePhone : guidePhone // ignore: cast_nullable_to_non_nullable
as String?,vehicleRegistrationNo: freezed == vehicleRegistrationNo ? _self.vehicleRegistrationNo : vehicleRegistrationNo // ignore: cast_nullable_to_non_nullable
as String?,vehicleType: freezed == vehicleType ? _self.vehicleType : vehicleType // ignore: cast_nullable_to_non_nullable
as String?,vehicleSeats: freezed == vehicleSeats ? _self.vehicleSeats : vehicleSeats // ignore: cast_nullable_to_non_nullable
as int?,
  ));
}


}


/// @nodoc
mixin _$GuideRating {

 String get tripRequestId; String get guideId; String get guideName; int get stars; String? get comment; String get ratedAt;
/// Create a copy of GuideRating
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$GuideRatingCopyWith<GuideRating> get copyWith => _$GuideRatingCopyWithImpl<GuideRating>(this as GuideRating, _$identity);

  /// Serializes this GuideRating to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as GuideRating;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is GuideRating&&(identical(other.tripRequestId, _this.tripRequestId) || other.tripRequestId == _this.tripRequestId)&&(identical(other.guideId, _this.guideId) || other.guideId == _this.guideId)&&(identical(other.guideName, _this.guideName) || other.guideName == _this.guideName)&&(identical(other.stars, _this.stars) || other.stars == _this.stars)&&(identical(other.comment, _this.comment) || other.comment == _this.comment)&&(identical(other.ratedAt, _this.ratedAt) || other.ratedAt == _this.ratedAt));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as GuideRating;
  return Object.hash(runtimeType,_this.tripRequestId,_this.guideId,_this.guideName,_this.stars,_this.comment,_this.ratedAt);
}

@override
String toString() {
  final _this = this as GuideRating;
  return 'GuideRating(tripRequestId: ${_this.tripRequestId}, guideId: ${_this.guideId}, guideName: ${_this.guideName}, stars: ${_this.stars}, comment: ${_this.comment}, ratedAt: ${_this.ratedAt})';
}


}

/// @nodoc
abstract mixin class $GuideRatingCopyWith<$Res>  {
  factory $GuideRatingCopyWith(GuideRating value, $Res Function(GuideRating) _then) = _$GuideRatingCopyWithImpl;
@useResult
$Res call({
 String tripRequestId, String guideId, String guideName, int stars, String? comment, String ratedAt
});




}
/// @nodoc
class _$GuideRatingCopyWithImpl<$Res>
    implements $GuideRatingCopyWith<$Res> {
  _$GuideRatingCopyWithImpl(this._self, this._then);

  final GuideRating _self;
  final $Res Function(GuideRating) _then;

/// Create a copy of GuideRating
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? tripRequestId = null,Object? guideId = null,Object? guideName = null,Object? stars = null,Object? comment = freezed,Object? ratedAt = null,}) {
  return _then(GuideRating(
tripRequestId: null == tripRequestId ? _self.tripRequestId : tripRequestId // ignore: cast_nullable_to_non_nullable
as String,guideId: null == guideId ? _self.guideId : guideId // ignore: cast_nullable_to_non_nullable
as String,guideName: null == guideName ? _self.guideName : guideName // ignore: cast_nullable_to_non_nullable
as String,stars: null == stars ? _self.stars : stars // ignore: cast_nullable_to_non_nullable
as int,comment: freezed == comment ? _self.comment : comment // ignore: cast_nullable_to_non_nullable
as String?,ratedAt: null == ratedAt ? _self.ratedAt : ratedAt // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [GuideRating].
extension GuideRatingPatterns on GuideRating {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _GuideRating value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _GuideRating() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _GuideRating value)  $default,){
final _that = this;
switch (_that) {
case _GuideRating():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _GuideRating value)?  $default,){
final _that = this;
switch (_that) {
case _GuideRating() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String tripRequestId,  String guideId,  String guideName,  int stars,  String? comment,  String ratedAt)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _GuideRating() when $default != null:
return $default(_that.tripRequestId,_that.guideId,_that.guideName,_that.stars,_that.comment,_that.ratedAt);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String tripRequestId,  String guideId,  String guideName,  int stars,  String? comment,  String ratedAt)  $default,) {final _that = this;
switch (_that) {
case _GuideRating():
return $default(_that.tripRequestId,_that.guideId,_that.guideName,_that.stars,_that.comment,_that.ratedAt);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String tripRequestId,  String guideId,  String guideName,  int stars,  String? comment,  String ratedAt)?  $default,) {final _that = this;
switch (_that) {
case _GuideRating() when $default != null:
return $default(_that.tripRequestId,_that.guideId,_that.guideName,_that.stars,_that.comment,_that.ratedAt);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _GuideRating implements GuideRating {
  const _GuideRating({required this.tripRequestId, required this.guideId, required this.guideName, required this.stars, this.comment, required this.ratedAt});
  factory _GuideRating.fromJson(Map<String, dynamic> json) => _$GuideRatingFromJson(json);

@override final  String tripRequestId;
@override final  String guideId;
@override final  String guideName;
@override final  int stars;
@override final  String? comment;
@override final  String ratedAt;

/// Create a copy of GuideRating
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$GuideRatingCopyWith<_GuideRating> get copyWith => __$GuideRatingCopyWithImpl<_GuideRating>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$GuideRatingToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _GuideRating&&(identical(other.tripRequestId, tripRequestId) || other.tripRequestId == tripRequestId)&&(identical(other.guideId, guideId) || other.guideId == guideId)&&(identical(other.guideName, guideName) || other.guideName == guideName)&&(identical(other.stars, stars) || other.stars == stars)&&(identical(other.comment, comment) || other.comment == comment)&&(identical(other.ratedAt, ratedAt) || other.ratedAt == ratedAt));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,tripRequestId,guideId,guideName,stars,comment,ratedAt);
}

@override
String toString() {
    return 'GuideRating(tripRequestId: $tripRequestId, guideId: $guideId, guideName: $guideName, stars: $stars, comment: $comment, ratedAt: $ratedAt)';
}


}

/// @nodoc
abstract mixin class _$GuideRatingCopyWith<$Res> implements $GuideRatingCopyWith<$Res> {
  factory _$GuideRatingCopyWith(_GuideRating value, $Res Function(_GuideRating) _then) = __$GuideRatingCopyWithImpl;
@override @useResult
$Res call({
 String tripRequestId, String guideId, String guideName, int stars, String? comment, String ratedAt
});




}
/// @nodoc
class __$GuideRatingCopyWithImpl<$Res>
    implements _$GuideRatingCopyWith<$Res> {
  __$GuideRatingCopyWithImpl(this._self, this._then);

  final _GuideRating _self;
  final $Res Function(_GuideRating) _then;

/// Create a copy of GuideRating
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? tripRequestId = null,Object? guideId = null,Object? guideName = null,Object? stars = null,Object? comment = freezed,Object? ratedAt = null,}) {
  return _then(_GuideRating(
tripRequestId: null == tripRequestId ? _self.tripRequestId : tripRequestId // ignore: cast_nullable_to_non_nullable
as String,guideId: null == guideId ? _self.guideId : guideId // ignore: cast_nullable_to_non_nullable
as String,guideName: null == guideName ? _self.guideName : guideName // ignore: cast_nullable_to_non_nullable
as String,stars: null == stars ? _self.stars : stars // ignore: cast_nullable_to_non_nullable
as int,comment: freezed == comment ? _self.comment : comment // ignore: cast_nullable_to_non_nullable
as String?,ratedAt: null == ratedAt ? _self.ratedAt : ratedAt // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}

// dart format on
