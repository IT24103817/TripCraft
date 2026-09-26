// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint, type=warning, deprecated_member_use, deprecated_member_use_from_same_package
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'guide_models.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$GuideSchedule {

 String get guideId; String get guideName; List<GuideTrip> get trips;
/// Create a copy of GuideSchedule
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$GuideScheduleCopyWith<GuideSchedule> get copyWith => _$GuideScheduleCopyWithImpl<GuideSchedule>(this as GuideSchedule, _$identity);

  /// Serializes this GuideSchedule to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as GuideSchedule;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is GuideSchedule&&(identical(other.guideId, _this.guideId) || other.guideId == _this.guideId)&&(identical(other.guideName, _this.guideName) || other.guideName == _this.guideName)&&const DeepCollectionEquality().equals(other.trips, _this.trips));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as GuideSchedule;
  return Object.hash(runtimeType,_this.guideId,_this.guideName,const DeepCollectionEquality().hash(_this.trips));
}

@override
String toString() {
  final _this = this as GuideSchedule;
  return 'GuideSchedule(guideId: ${_this.guideId}, guideName: ${_this.guideName}, trips: ${_this.trips})';
}


}

/// @nodoc
abstract mixin class $GuideScheduleCopyWith<$Res>  {
  factory $GuideScheduleCopyWith(GuideSchedule value, $Res Function(GuideSchedule) _then) = _$GuideScheduleCopyWithImpl;
@useResult
$Res call({
 String guideId, String guideName, List<GuideTrip> trips
});




}
/// @nodoc
class _$GuideScheduleCopyWithImpl<$Res>
    implements $GuideScheduleCopyWith<$Res> {
  _$GuideScheduleCopyWithImpl(this._self, this._then);

  final GuideSchedule _self;
  final $Res Function(GuideSchedule) _then;

/// Create a copy of GuideSchedule
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? guideId = null,Object? guideName = null,Object? trips = null,}) {
  return _then(GuideSchedule(
guideId: null == guideId ? _self.guideId : guideId // ignore: cast_nullable_to_non_nullable
as String,guideName: null == guideName ? _self.guideName : guideName // ignore: cast_nullable_to_non_nullable
as String,trips: null == trips ? _self.trips : trips // ignore: cast_nullable_to_non_nullable
as List<GuideTrip>,
  ));
}

}


/// Adds pattern-matching-related methods to [GuideSchedule].
extension GuideSchedulePatterns on GuideSchedule {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _GuideSchedule value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _GuideSchedule() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _GuideSchedule value)  $default,){
final _that = this;
switch (_that) {
case _GuideSchedule():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _GuideSchedule value)?  $default,){
final _that = this;
switch (_that) {
case _GuideSchedule() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String guideId,  String guideName,  List<GuideTrip> trips)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _GuideSchedule() when $default != null:
return $default(_that.guideId,_that.guideName,_that.trips);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String guideId,  String guideName,  List<GuideTrip> trips)  $default,) {final _that = this;
switch (_that) {
case _GuideSchedule():
return $default(_that.guideId,_that.guideName,_that.trips);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String guideId,  String guideName,  List<GuideTrip> trips)?  $default,) {final _that = this;
switch (_that) {
case _GuideSchedule() when $default != null:
return $default(_that.guideId,_that.guideName,_that.trips);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _GuideSchedule implements GuideSchedule {
  const _GuideSchedule({required this.guideId, required this.guideName,  List<GuideTrip> trips = const <GuideTrip>[]}): _trips = trips;
  factory _GuideSchedule.fromJson(Map<String, dynamic> json) => _$GuideScheduleFromJson(json);

@override final  String guideId;
@override final  String guideName;
 final  List<GuideTrip> _trips;
@override@JsonKey() List<GuideTrip> get trips {
  if (_trips is EqualUnmodifiableListView) return _trips;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_trips);
}


/// Create a copy of GuideSchedule
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$GuideScheduleCopyWith<_GuideSchedule> get copyWith => __$GuideScheduleCopyWithImpl<_GuideSchedule>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$GuideScheduleToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _GuideSchedule&&(identical(other.guideId, guideId) || other.guideId == guideId)&&(identical(other.guideName, guideName) || other.guideName == guideName)&&const DeepCollectionEquality().equals(other.trips, _trips));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,guideId,guideName,const DeepCollectionEquality().hash(_trips));
}

@override
String toString() {
    return 'GuideSchedule(guideId: $guideId, guideName: $guideName, trips: $trips)';
}


}

/// @nodoc
abstract mixin class _$GuideScheduleCopyWith<$Res> implements $GuideScheduleCopyWith<$Res> {
  factory _$GuideScheduleCopyWith(_GuideSchedule value, $Res Function(_GuideSchedule) _then) = __$GuideScheduleCopyWithImpl;
@override @useResult
$Res call({
 String guideId, String guideName, List<GuideTrip> trips
});




}
/// @nodoc
class __$GuideScheduleCopyWithImpl<$Res>
    implements _$GuideScheduleCopyWith<$Res> {
  __$GuideScheduleCopyWithImpl(this._self, this._then);

  final _GuideSchedule _self;
  final $Res Function(_GuideSchedule) _then;

/// Create a copy of GuideSchedule
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? guideId = null,Object? guideName = null,Object? trips = null,}) {
  return _then(_GuideSchedule(
guideId: null == guideId ? _self.guideId : guideId // ignore: cast_nullable_to_non_nullable
as String,guideName: null == guideName ? _self.guideName : guideName // ignore: cast_nullable_to_non_nullable
as String,trips: null == trips ? _self._trips : trips // ignore: cast_nullable_to_non_nullable
as List<GuideTrip>,
  ));
}


}


/// @nodoc
mixin _$GuideTrip {

 String get tripRequestId; String get objective; String get startDate; String get endDate; int get pax; String get status; String? get vehicleRegistrationNo; List<GuideDay> get days;
/// Create a copy of GuideTrip
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$GuideTripCopyWith<GuideTrip> get copyWith => _$GuideTripCopyWithImpl<GuideTrip>(this as GuideTrip, _$identity);

  /// Serializes this GuideTrip to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as GuideTrip;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is GuideTrip&&(identical(other.tripRequestId, _this.tripRequestId) || other.tripRequestId == _this.tripRequestId)&&(identical(other.objective, _this.objective) || other.objective == _this.objective)&&(identical(other.startDate, _this.startDate) || other.startDate == _this.startDate)&&(identical(other.endDate, _this.endDate) || other.endDate == _this.endDate)&&(identical(other.pax, _this.pax) || other.pax == _this.pax)&&(identical(other.status, _this.status) || other.status == _this.status)&&(identical(other.vehicleRegistrationNo, _this.vehicleRegistrationNo) || other.vehicleRegistrationNo == _this.vehicleRegistrationNo)&&const DeepCollectionEquality().equals(other.days, _this.days));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as GuideTrip;
  return Object.hash(runtimeType,_this.tripRequestId,_this.objective,_this.startDate,_this.endDate,_this.pax,_this.status,_this.vehicleRegistrationNo,const DeepCollectionEquality().hash(_this.days));
}

@override
String toString() {
  final _this = this as GuideTrip;
  return 'GuideTrip(tripRequestId: ${_this.tripRequestId}, objective: ${_this.objective}, startDate: ${_this.startDate}, endDate: ${_this.endDate}, pax: ${_this.pax}, status: ${_this.status}, vehicleRegistrationNo: ${_this.vehicleRegistrationNo}, days: ${_this.days})';
}


}

/// @nodoc
abstract mixin class $GuideTripCopyWith<$Res>  {
  factory $GuideTripCopyWith(GuideTrip value, $Res Function(GuideTrip) _then) = _$GuideTripCopyWithImpl;
@useResult
$Res call({
 String tripRequestId, String objective, String startDate, String endDate, int pax, String status, String? vehicleRegistrationNo, List<GuideDay> days
});




}
/// @nodoc
class _$GuideTripCopyWithImpl<$Res>
    implements $GuideTripCopyWith<$Res> {
  _$GuideTripCopyWithImpl(this._self, this._then);

  final GuideTrip _self;
  final $Res Function(GuideTrip) _then;

/// Create a copy of GuideTrip
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? tripRequestId = null,Object? objective = null,Object? startDate = null,Object? endDate = null,Object? pax = null,Object? status = null,Object? vehicleRegistrationNo = freezed,Object? days = null,}) {
  return _then(GuideTrip(
tripRequestId: null == tripRequestId ? _self.tripRequestId : tripRequestId // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,startDate: null == startDate ? _self.startDate : startDate // ignore: cast_nullable_to_non_nullable
as String,endDate: null == endDate ? _self.endDate : endDate // ignore: cast_nullable_to_non_nullable
as String,pax: null == pax ? _self.pax : pax // ignore: cast_nullable_to_non_nullable
as int,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as String,vehicleRegistrationNo: freezed == vehicleRegistrationNo ? _self.vehicleRegistrationNo : vehicleRegistrationNo // ignore: cast_nullable_to_non_nullable
as String?,days: null == days ? _self.days : days // ignore: cast_nullable_to_non_nullable
as List<GuideDay>,
  ));
}

}


/// Adds pattern-matching-related methods to [GuideTrip].
extension GuideTripPatterns on GuideTrip {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _GuideTrip value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _GuideTrip() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _GuideTrip value)  $default,){
final _that = this;
switch (_that) {
case _GuideTrip():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _GuideTrip value)?  $default,){
final _that = this;
switch (_that) {
case _GuideTrip() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String tripRequestId,  String objective,  String startDate,  String endDate,  int pax,  String status,  String? vehicleRegistrationNo,  List<GuideDay> days)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _GuideTrip() when $default != null:
return $default(_that.tripRequestId,_that.objective,_that.startDate,_that.endDate,_that.pax,_that.status,_that.vehicleRegistrationNo,_that.days);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String tripRequestId,  String objective,  String startDate,  String endDate,  int pax,  String status,  String? vehicleRegistrationNo,  List<GuideDay> days)  $default,) {final _that = this;
switch (_that) {
case _GuideTrip():
return $default(_that.tripRequestId,_that.objective,_that.startDate,_that.endDate,_that.pax,_that.status,_that.vehicleRegistrationNo,_that.days);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String tripRequestId,  String objective,  String startDate,  String endDate,  int pax,  String status,  String? vehicleRegistrationNo,  List<GuideDay> days)?  $default,) {final _that = this;
switch (_that) {
case _GuideTrip() when $default != null:
return $default(_that.tripRequestId,_that.objective,_that.startDate,_that.endDate,_that.pax,_that.status,_that.vehicleRegistrationNo,_that.days);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _GuideTrip implements GuideTrip {
  const _GuideTrip({required this.tripRequestId, required this.objective, required this.startDate, required this.endDate, required this.pax, required this.status, this.vehicleRegistrationNo,  List<GuideDay> days = const <GuideDay>[]}): _days = days;
  factory _GuideTrip.fromJson(Map<String, dynamic> json) => _$GuideTripFromJson(json);

@override final  String tripRequestId;
@override final  String objective;
@override final  String startDate;
@override final  String endDate;
@override final  int pax;
@override final  String status;
@override final  String? vehicleRegistrationNo;
 final  List<GuideDay> _days;
@override@JsonKey() List<GuideDay> get days {
  if (_days is EqualUnmodifiableListView) return _days;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_days);
}


/// Create a copy of GuideTrip
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$GuideTripCopyWith<_GuideTrip> get copyWith => __$GuideTripCopyWithImpl<_GuideTrip>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$GuideTripToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _GuideTrip&&(identical(other.tripRequestId, tripRequestId) || other.tripRequestId == tripRequestId)&&(identical(other.objective, objective) || other.objective == objective)&&(identical(other.startDate, startDate) || other.startDate == startDate)&&(identical(other.endDate, endDate) || other.endDate == endDate)&&(identical(other.pax, pax) || other.pax == pax)&&(identical(other.status, status) || other.status == status)&&(identical(other.vehicleRegistrationNo, vehicleRegistrationNo) || other.vehicleRegistrationNo == vehicleRegistrationNo)&&const DeepCollectionEquality().equals(other.days, _days));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,tripRequestId,objective,startDate,endDate,pax,status,vehicleRegistrationNo,const DeepCollectionEquality().hash(_days));
}

@override
String toString() {
    return 'GuideTrip(tripRequestId: $tripRequestId, objective: $objective, startDate: $startDate, endDate: $endDate, pax: $pax, status: $status, vehicleRegistrationNo: $vehicleRegistrationNo, days: $days)';
}


}

/// @nodoc
abstract mixin class _$GuideTripCopyWith<$Res> implements $GuideTripCopyWith<$Res> {
  factory _$GuideTripCopyWith(_GuideTrip value, $Res Function(_GuideTrip) _then) = __$GuideTripCopyWithImpl;
@override @useResult
$Res call({
 String tripRequestId, String objective, String startDate, String endDate, int pax, String status, String? vehicleRegistrationNo, List<GuideDay> days
});




}
/// @nodoc
class __$GuideTripCopyWithImpl<$Res>
    implements _$GuideTripCopyWith<$Res> {
  __$GuideTripCopyWithImpl(this._self, this._then);

  final _GuideTrip _self;
  final $Res Function(_GuideTrip) _then;

/// Create a copy of GuideTrip
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? tripRequestId = null,Object? objective = null,Object? startDate = null,Object? endDate = null,Object? pax = null,Object? status = null,Object? vehicleRegistrationNo = freezed,Object? days = null,}) {
  return _then(_GuideTrip(
tripRequestId: null == tripRequestId ? _self.tripRequestId : tripRequestId // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,startDate: null == startDate ? _self.startDate : startDate // ignore: cast_nullable_to_non_nullable
as String,endDate: null == endDate ? _self.endDate : endDate // ignore: cast_nullable_to_non_nullable
as String,pax: null == pax ? _self.pax : pax // ignore: cast_nullable_to_non_nullable
as int,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as String,vehicleRegistrationNo: freezed == vehicleRegistrationNo ? _self.vehicleRegistrationNo : vehicleRegistrationNo // ignore: cast_nullable_to_non_nullable
as String?,days: null == days ? _self._days : days // ignore: cast_nullable_to_non_nullable
as List<GuideDay>,
  ));
}


}


/// @nodoc
mixin _$GuideDay {

 int get dayNumber; String get date; String get city; String? get hotelName; List<ScheduleStop> get stops;
/// Create a copy of GuideDay
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$GuideDayCopyWith<GuideDay> get copyWith => _$GuideDayCopyWithImpl<GuideDay>(this as GuideDay, _$identity);

  /// Serializes this GuideDay to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as GuideDay;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is GuideDay&&(identical(other.dayNumber, _this.dayNumber) || other.dayNumber == _this.dayNumber)&&(identical(other.date, _this.date) || other.date == _this.date)&&(identical(other.city, _this.city) || other.city == _this.city)&&(identical(other.hotelName, _this.hotelName) || other.hotelName == _this.hotelName)&&const DeepCollectionEquality().equals(other.stops, _this.stops));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as GuideDay;
  return Object.hash(runtimeType,_this.dayNumber,_this.date,_this.city,_this.hotelName,const DeepCollectionEquality().hash(_this.stops));
}

@override
String toString() {
  final _this = this as GuideDay;
  return 'GuideDay(dayNumber: ${_this.dayNumber}, date: ${_this.date}, city: ${_this.city}, hotelName: ${_this.hotelName}, stops: ${_this.stops})';
}


}

/// @nodoc
abstract mixin class $GuideDayCopyWith<$Res>  {
  factory $GuideDayCopyWith(GuideDay value, $Res Function(GuideDay) _then) = _$GuideDayCopyWithImpl;
@useResult
$Res call({
 int dayNumber, String date, String city, String? hotelName, List<ScheduleStop> stops
});




}
/// @nodoc
class _$GuideDayCopyWithImpl<$Res>
    implements $GuideDayCopyWith<$Res> {
  _$GuideDayCopyWithImpl(this._self, this._then);

  final GuideDay _self;
  final $Res Function(GuideDay) _then;

/// Create a copy of GuideDay
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? dayNumber = null,Object? date = null,Object? city = null,Object? hotelName = freezed,Object? stops = null,}) {
  return _then(GuideDay(
dayNumber: null == dayNumber ? _self.dayNumber : dayNumber // ignore: cast_nullable_to_non_nullable
as int,date: null == date ? _self.date : date // ignore: cast_nullable_to_non_nullable
as String,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,hotelName: freezed == hotelName ? _self.hotelName : hotelName // ignore: cast_nullable_to_non_nullable
as String?,stops: null == stops ? _self.stops : stops // ignore: cast_nullable_to_non_nullable
as List<ScheduleStop>,
  ));
}

}


/// Adds pattern-matching-related methods to [GuideDay].
extension GuideDayPatterns on GuideDay {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _GuideDay value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _GuideDay() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _GuideDay value)  $default,){
final _that = this;
switch (_that) {
case _GuideDay():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _GuideDay value)?  $default,){
final _that = this;
switch (_that) {
case _GuideDay() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( int dayNumber,  String date,  String city,  String? hotelName,  List<ScheduleStop> stops)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _GuideDay() when $default != null:
return $default(_that.dayNumber,_that.date,_that.city,_that.hotelName,_that.stops);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( int dayNumber,  String date,  String city,  String? hotelName,  List<ScheduleStop> stops)  $default,) {final _that = this;
switch (_that) {
case _GuideDay():
return $default(_that.dayNumber,_that.date,_that.city,_that.hotelName,_that.stops);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( int dayNumber,  String date,  String city,  String? hotelName,  List<ScheduleStop> stops)?  $default,) {final _that = this;
switch (_that) {
case _GuideDay() when $default != null:
return $default(_that.dayNumber,_that.date,_that.city,_that.hotelName,_that.stops);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _GuideDay extends GuideDay {
  const _GuideDay({required this.dayNumber, required this.date, required this.city, this.hotelName,  List<ScheduleStop> stops = const <ScheduleStop>[]}): _stops = stops,super._();
  factory _GuideDay.fromJson(Map<String, dynamic> json) => _$GuideDayFromJson(json);

@override final  int dayNumber;
@override final  String date;
@override final  String city;
@override final  String? hotelName;
 final  List<ScheduleStop> _stops;
@override@JsonKey() List<ScheduleStop> get stops {
  if (_stops is EqualUnmodifiableListView) return _stops;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_stops);
}


/// Create a copy of GuideDay
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$GuideDayCopyWith<_GuideDay> get copyWith => __$GuideDayCopyWithImpl<_GuideDay>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$GuideDayToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _GuideDay&&(identical(other.dayNumber, dayNumber) || other.dayNumber == dayNumber)&&(identical(other.date, date) || other.date == date)&&(identical(other.city, city) || other.city == city)&&(identical(other.hotelName, hotelName) || other.hotelName == hotelName)&&const DeepCollectionEquality().equals(other.stops, _stops));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,dayNumber,date,city,hotelName,const DeepCollectionEquality().hash(_stops));
}

@override
String toString() {
    return 'GuideDay(dayNumber: $dayNumber, date: $date, city: $city, hotelName: $hotelName, stops: $stops)';
}


}

/// @nodoc
abstract mixin class _$GuideDayCopyWith<$Res> implements $GuideDayCopyWith<$Res> {
  factory _$GuideDayCopyWith(_GuideDay value, $Res Function(_GuideDay) _then) = __$GuideDayCopyWithImpl;
@override @useResult
$Res call({
 int dayNumber, String date, String city, String? hotelName, List<ScheduleStop> stops
});




}
/// @nodoc
class __$GuideDayCopyWithImpl<$Res>
    implements _$GuideDayCopyWith<$Res> {
  __$GuideDayCopyWithImpl(this._self, this._then);

  final _GuideDay _self;
  final $Res Function(_GuideDay) _then;

/// Create a copy of GuideDay
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? dayNumber = null,Object? date = null,Object? city = null,Object? hotelName = freezed,Object? stops = null,}) {
  return _then(_GuideDay(
dayNumber: null == dayNumber ? _self.dayNumber : dayNumber // ignore: cast_nullable_to_non_nullable
as int,date: null == date ? _self.date : date // ignore: cast_nullable_to_non_nullable
as String,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,hotelName: freezed == hotelName ? _self.hotelName : hotelName // ignore: cast_nullable_to_non_nullable
as String?,stops: null == stops ? _self._stops : stops // ignore: cast_nullable_to_non_nullable
as List<ScheduleStop>,
  ));
}


}


/// @nodoc
mixin _$ScheduleStop {

 String get stopId; int get sequence; String get attractionName; double get latitude; double get longitude; String? get checkedInAt;
/// Create a copy of ScheduleStop
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$ScheduleStopCopyWith<ScheduleStop> get copyWith => _$ScheduleStopCopyWithImpl<ScheduleStop>(this as ScheduleStop, _$identity);

  /// Serializes this ScheduleStop to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as ScheduleStop;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ScheduleStop&&(identical(other.stopId, _this.stopId) || other.stopId == _this.stopId)&&(identical(other.sequence, _this.sequence) || other.sequence == _this.sequence)&&(identical(other.attractionName, _this.attractionName) || other.attractionName == _this.attractionName)&&(identical(other.latitude, _this.latitude) || other.latitude == _this.latitude)&&(identical(other.longitude, _this.longitude) || other.longitude == _this.longitude)&&(identical(other.checkedInAt, _this.checkedInAt) || other.checkedInAt == _this.checkedInAt));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as ScheduleStop;
  return Object.hash(runtimeType,_this.stopId,_this.sequence,_this.attractionName,_this.latitude,_this.longitude,_this.checkedInAt);
}

@override
String toString() {
  final _this = this as ScheduleStop;
  return 'ScheduleStop(stopId: ${_this.stopId}, sequence: ${_this.sequence}, attractionName: ${_this.attractionName}, latitude: ${_this.latitude}, longitude: ${_this.longitude}, checkedInAt: ${_this.checkedInAt})';
}


}

/// @nodoc
abstract mixin class $ScheduleStopCopyWith<$Res>  {
  factory $ScheduleStopCopyWith(ScheduleStop value, $Res Function(ScheduleStop) _then) = _$ScheduleStopCopyWithImpl;
@useResult
$Res call({
 String stopId, int sequence, String attractionName, double latitude, double longitude, String? checkedInAt
});




}
/// @nodoc
class _$ScheduleStopCopyWithImpl<$Res>
    implements $ScheduleStopCopyWith<$Res> {
  _$ScheduleStopCopyWithImpl(this._self, this._then);

  final ScheduleStop _self;
  final $Res Function(ScheduleStop) _then;

/// Create a copy of ScheduleStop
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? stopId = null,Object? sequence = null,Object? attractionName = null,Object? latitude = null,Object? longitude = null,Object? checkedInAt = freezed,}) {
  return _then(ScheduleStop(
stopId: null == stopId ? _self.stopId : stopId // ignore: cast_nullable_to_non_nullable
as String,sequence: null == sequence ? _self.sequence : sequence // ignore: cast_nullable_to_non_nullable
as int,attractionName: null == attractionName ? _self.attractionName : attractionName // ignore: cast_nullable_to_non_nullable
as String,latitude: null == latitude ? _self.latitude : latitude // ignore: cast_nullable_to_non_nullable
as double,longitude: null == longitude ? _self.longitude : longitude // ignore: cast_nullable_to_non_nullable
as double,checkedInAt: freezed == checkedInAt ? _self.checkedInAt : checkedInAt // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}

}


/// Adds pattern-matching-related methods to [ScheduleStop].
extension ScheduleStopPatterns on ScheduleStop {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _ScheduleStop value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _ScheduleStop() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _ScheduleStop value)  $default,){
final _that = this;
switch (_that) {
case _ScheduleStop():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _ScheduleStop value)?  $default,){
final _that = this;
switch (_that) {
case _ScheduleStop() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String stopId,  int sequence,  String attractionName,  double latitude,  double longitude,  String? checkedInAt)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _ScheduleStop() when $default != null:
return $default(_that.stopId,_that.sequence,_that.attractionName,_that.latitude,_that.longitude,_that.checkedInAt);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String stopId,  int sequence,  String attractionName,  double latitude,  double longitude,  String? checkedInAt)  $default,) {final _that = this;
switch (_that) {
case _ScheduleStop():
return $default(_that.stopId,_that.sequence,_that.attractionName,_that.latitude,_that.longitude,_that.checkedInAt);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String stopId,  int sequence,  String attractionName,  double latitude,  double longitude,  String? checkedInAt)?  $default,) {final _that = this;
switch (_that) {
case _ScheduleStop() when $default != null:
return $default(_that.stopId,_that.sequence,_that.attractionName,_that.latitude,_that.longitude,_that.checkedInAt);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _ScheduleStop implements ScheduleStop {
  const _ScheduleStop({required this.stopId, required this.sequence, required this.attractionName, required this.latitude, required this.longitude, this.checkedInAt});
  factory _ScheduleStop.fromJson(Map<String, dynamic> json) => _$ScheduleStopFromJson(json);

@override final  String stopId;
@override final  int sequence;
@override final  String attractionName;
@override final  double latitude;
@override final  double longitude;
@override final  String? checkedInAt;

/// Create a copy of ScheduleStop
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$ScheduleStopCopyWith<_ScheduleStop> get copyWith => __$ScheduleStopCopyWithImpl<_ScheduleStop>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$ScheduleStopToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _ScheduleStop&&(identical(other.stopId, stopId) || other.stopId == stopId)&&(identical(other.sequence, sequence) || other.sequence == sequence)&&(identical(other.attractionName, attractionName) || other.attractionName == attractionName)&&(identical(other.latitude, latitude) || other.latitude == latitude)&&(identical(other.longitude, longitude) || other.longitude == longitude)&&(identical(other.checkedInAt, checkedInAt) || other.checkedInAt == checkedInAt));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,stopId,sequence,attractionName,latitude,longitude,checkedInAt);
}

@override
String toString() {
    return 'ScheduleStop(stopId: $stopId, sequence: $sequence, attractionName: $attractionName, latitude: $latitude, longitude: $longitude, checkedInAt: $checkedInAt)';
}


}

/// @nodoc
abstract mixin class _$ScheduleStopCopyWith<$Res> implements $ScheduleStopCopyWith<$Res> {
  factory _$ScheduleStopCopyWith(_ScheduleStop value, $Res Function(_ScheduleStop) _then) = __$ScheduleStopCopyWithImpl;
@override @useResult
$Res call({
 String stopId, int sequence, String attractionName, double latitude, double longitude, String? checkedInAt
});




}
/// @nodoc
class __$ScheduleStopCopyWithImpl<$Res>
    implements _$ScheduleStopCopyWith<$Res> {
  __$ScheduleStopCopyWithImpl(this._self, this._then);

  final _ScheduleStop _self;
  final $Res Function(_ScheduleStop) _then;

/// Create a copy of ScheduleStop
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? stopId = null,Object? sequence = null,Object? attractionName = null,Object? latitude = null,Object? longitude = null,Object? checkedInAt = freezed,}) {
  return _then(_ScheduleStop(
stopId: null == stopId ? _self.stopId : stopId // ignore: cast_nullable_to_non_nullable
as String,sequence: null == sequence ? _self.sequence : sequence // ignore: cast_nullable_to_non_nullable
as int,attractionName: null == attractionName ? _self.attractionName : attractionName // ignore: cast_nullable_to_non_nullable
as String,latitude: null == latitude ? _self.latitude : latitude // ignore: cast_nullable_to_non_nullable
as double,longitude: null == longitude ? _self.longitude : longitude // ignore: cast_nullable_to_non_nullable
as double,checkedInAt: freezed == checkedInAt ? _self.checkedInAt : checkedInAt // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}


/// @nodoc
mixin _$CheckInResult {

 String get stopId; int get distanceMeters; String get checkedInAt; String get tripStatus;
/// Create a copy of CheckInResult
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$CheckInResultCopyWith<CheckInResult> get copyWith => _$CheckInResultCopyWithImpl<CheckInResult>(this as CheckInResult, _$identity);

  /// Serializes this CheckInResult to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as CheckInResult;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is CheckInResult&&(identical(other.stopId, _this.stopId) || other.stopId == _this.stopId)&&(identical(other.distanceMeters, _this.distanceMeters) || other.distanceMeters == _this.distanceMeters)&&(identical(other.checkedInAt, _this.checkedInAt) || other.checkedInAt == _this.checkedInAt)&&(identical(other.tripStatus, _this.tripStatus) || other.tripStatus == _this.tripStatus));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as CheckInResult;
  return Object.hash(runtimeType,_this.stopId,_this.distanceMeters,_this.checkedInAt,_this.tripStatus);
}

@override
String toString() {
  final _this = this as CheckInResult;
  return 'CheckInResult(stopId: ${_this.stopId}, distanceMeters: ${_this.distanceMeters}, checkedInAt: ${_this.checkedInAt}, tripStatus: ${_this.tripStatus})';
}


}

/// @nodoc
abstract mixin class $CheckInResultCopyWith<$Res>  {
  factory $CheckInResultCopyWith(CheckInResult value, $Res Function(CheckInResult) _then) = _$CheckInResultCopyWithImpl;
@useResult
$Res call({
 String stopId, int distanceMeters, String checkedInAt, String tripStatus
});




}
/// @nodoc
class _$CheckInResultCopyWithImpl<$Res>
    implements $CheckInResultCopyWith<$Res> {
  _$CheckInResultCopyWithImpl(this._self, this._then);

  final CheckInResult _self;
  final $Res Function(CheckInResult) _then;

/// Create a copy of CheckInResult
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? stopId = null,Object? distanceMeters = null,Object? checkedInAt = null,Object? tripStatus = null,}) {
  return _then(CheckInResult(
stopId: null == stopId ? _self.stopId : stopId // ignore: cast_nullable_to_non_nullable
as String,distanceMeters: null == distanceMeters ? _self.distanceMeters : distanceMeters // ignore: cast_nullable_to_non_nullable
as int,checkedInAt: null == checkedInAt ? _self.checkedInAt : checkedInAt // ignore: cast_nullable_to_non_nullable
as String,tripStatus: null == tripStatus ? _self.tripStatus : tripStatus // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [CheckInResult].
extension CheckInResultPatterns on CheckInResult {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _CheckInResult value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _CheckInResult() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _CheckInResult value)  $default,){
final _that = this;
switch (_that) {
case _CheckInResult():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _CheckInResult value)?  $default,){
final _that = this;
switch (_that) {
case _CheckInResult() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String stopId,  int distanceMeters,  String checkedInAt,  String tripStatus)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _CheckInResult() when $default != null:
return $default(_that.stopId,_that.distanceMeters,_that.checkedInAt,_that.tripStatus);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String stopId,  int distanceMeters,  String checkedInAt,  String tripStatus)  $default,) {final _that = this;
switch (_that) {
case _CheckInResult():
return $default(_that.stopId,_that.distanceMeters,_that.checkedInAt,_that.tripStatus);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String stopId,  int distanceMeters,  String checkedInAt,  String tripStatus)?  $default,) {final _that = this;
switch (_that) {
case _CheckInResult() when $default != null:
return $default(_that.stopId,_that.distanceMeters,_that.checkedInAt,_that.tripStatus);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _CheckInResult implements CheckInResult {
  const _CheckInResult({required this.stopId, required this.distanceMeters, required this.checkedInAt, required this.tripStatus});
  factory _CheckInResult.fromJson(Map<String, dynamic> json) => _$CheckInResultFromJson(json);

@override final  String stopId;
@override final  int distanceMeters;
@override final  String checkedInAt;
@override final  String tripStatus;

/// Create a copy of CheckInResult
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$CheckInResultCopyWith<_CheckInResult> get copyWith => __$CheckInResultCopyWithImpl<_CheckInResult>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$CheckInResultToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _CheckInResult&&(identical(other.stopId, stopId) || other.stopId == stopId)&&(identical(other.distanceMeters, distanceMeters) || other.distanceMeters == distanceMeters)&&(identical(other.checkedInAt, checkedInAt) || other.checkedInAt == checkedInAt)&&(identical(other.tripStatus, tripStatus) || other.tripStatus == tripStatus));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,stopId,distanceMeters,checkedInAt,tripStatus);
}

@override
String toString() {
    return 'CheckInResult(stopId: $stopId, distanceMeters: $distanceMeters, checkedInAt: $checkedInAt, tripStatus: $tripStatus)';
}


}

/// @nodoc
abstract mixin class _$CheckInResultCopyWith<$Res> implements $CheckInResultCopyWith<$Res> {
  factory _$CheckInResultCopyWith(_CheckInResult value, $Res Function(_CheckInResult) _then) = __$CheckInResultCopyWithImpl;
@override @useResult
$Res call({
 String stopId, int distanceMeters, String checkedInAt, String tripStatus
});




}
/// @nodoc
class __$CheckInResultCopyWithImpl<$Res>
    implements _$CheckInResultCopyWith<$Res> {
  __$CheckInResultCopyWithImpl(this._self, this._then);

  final _CheckInResult _self;
  final $Res Function(_CheckInResult) _then;

/// Create a copy of CheckInResult
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? stopId = null,Object? distanceMeters = null,Object? checkedInAt = null,Object? tripStatus = null,}) {
  return _then(_CheckInResult(
stopId: null == stopId ? _self.stopId : stopId // ignore: cast_nullable_to_non_nullable
as String,distanceMeters: null == distanceMeters ? _self.distanceMeters : distanceMeters // ignore: cast_nullable_to_non_nullable
as int,checkedInAt: null == checkedInAt ? _self.checkedInAt : checkedInAt // ignore: cast_nullable_to_non_nullable
as String,tripStatus: null == tripStatus ? _self.tripStatus : tripStatus // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}


/// @nodoc
mixin _$HotelInfo {

 String get id; String get name; String get city; int get starRating; List<RoomTypeInfo> get roomTypes;
/// Create a copy of HotelInfo
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$HotelInfoCopyWith<HotelInfo> get copyWith => _$HotelInfoCopyWithImpl<HotelInfo>(this as HotelInfo, _$identity);

  /// Serializes this HotelInfo to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as HotelInfo;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is HotelInfo&&(identical(other.id, _this.id) || other.id == _this.id)&&(identical(other.name, _this.name) || other.name == _this.name)&&(identical(other.city, _this.city) || other.city == _this.city)&&(identical(other.starRating, _this.starRating) || other.starRating == _this.starRating)&&const DeepCollectionEquality().equals(other.roomTypes, _this.roomTypes));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as HotelInfo;
  return Object.hash(runtimeType,_this.id,_this.name,_this.city,_this.starRating,const DeepCollectionEquality().hash(_this.roomTypes));
}

@override
String toString() {
  final _this = this as HotelInfo;
  return 'HotelInfo(id: ${_this.id}, name: ${_this.name}, city: ${_this.city}, starRating: ${_this.starRating}, roomTypes: ${_this.roomTypes})';
}


}

/// @nodoc
abstract mixin class $HotelInfoCopyWith<$Res>  {
  factory $HotelInfoCopyWith(HotelInfo value, $Res Function(HotelInfo) _then) = _$HotelInfoCopyWithImpl;
@useResult
$Res call({
 String id, String name, String city, int starRating, List<RoomTypeInfo> roomTypes
});




}
/// @nodoc
class _$HotelInfoCopyWithImpl<$Res>
    implements $HotelInfoCopyWith<$Res> {
  _$HotelInfoCopyWithImpl(this._self, this._then);

  final HotelInfo _self;
  final $Res Function(HotelInfo) _then;

/// Create a copy of HotelInfo
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? id = null,Object? name = null,Object? city = null,Object? starRating = null,Object? roomTypes = null,}) {
  return _then(HotelInfo(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,starRating: null == starRating ? _self.starRating : starRating // ignore: cast_nullable_to_non_nullable
as int,roomTypes: null == roomTypes ? _self.roomTypes : roomTypes // ignore: cast_nullable_to_non_nullable
as List<RoomTypeInfo>,
  ));
}

}


/// Adds pattern-matching-related methods to [HotelInfo].
extension HotelInfoPatterns on HotelInfo {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _HotelInfo value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _HotelInfo() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _HotelInfo value)  $default,){
final _that = this;
switch (_that) {
case _HotelInfo():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _HotelInfo value)?  $default,){
final _that = this;
switch (_that) {
case _HotelInfo() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String id,  String name,  String city,  int starRating,  List<RoomTypeInfo> roomTypes)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _HotelInfo() when $default != null:
return $default(_that.id,_that.name,_that.city,_that.starRating,_that.roomTypes);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String id,  String name,  String city,  int starRating,  List<RoomTypeInfo> roomTypes)  $default,) {final _that = this;
switch (_that) {
case _HotelInfo():
return $default(_that.id,_that.name,_that.city,_that.starRating,_that.roomTypes);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String id,  String name,  String city,  int starRating,  List<RoomTypeInfo> roomTypes)?  $default,) {final _that = this;
switch (_that) {
case _HotelInfo() when $default != null:
return $default(_that.id,_that.name,_that.city,_that.starRating,_that.roomTypes);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _HotelInfo implements HotelInfo {
  const _HotelInfo({required this.id, required this.name, required this.city, required this.starRating,  List<RoomTypeInfo> roomTypes = const <RoomTypeInfo>[]}): _roomTypes = roomTypes;
  factory _HotelInfo.fromJson(Map<String, dynamic> json) => _$HotelInfoFromJson(json);

@override final  String id;
@override final  String name;
@override final  String city;
@override final  int starRating;
 final  List<RoomTypeInfo> _roomTypes;
@override@JsonKey() List<RoomTypeInfo> get roomTypes {
  if (_roomTypes is EqualUnmodifiableListView) return _roomTypes;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_roomTypes);
}


/// Create a copy of HotelInfo
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$HotelInfoCopyWith<_HotelInfo> get copyWith => __$HotelInfoCopyWithImpl<_HotelInfo>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$HotelInfoToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _HotelInfo&&(identical(other.id, id) || other.id == id)&&(identical(other.name, name) || other.name == name)&&(identical(other.city, city) || other.city == city)&&(identical(other.starRating, starRating) || other.starRating == starRating)&&const DeepCollectionEquality().equals(other.roomTypes, _roomTypes));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,id,name,city,starRating,const DeepCollectionEquality().hash(_roomTypes));
}

@override
String toString() {
    return 'HotelInfo(id: $id, name: $name, city: $city, starRating: $starRating, roomTypes: $roomTypes)';
}


}

/// @nodoc
abstract mixin class _$HotelInfoCopyWith<$Res> implements $HotelInfoCopyWith<$Res> {
  factory _$HotelInfoCopyWith(_HotelInfo value, $Res Function(_HotelInfo) _then) = __$HotelInfoCopyWithImpl;
@override @useResult
$Res call({
 String id, String name, String city, int starRating, List<RoomTypeInfo> roomTypes
});




}
/// @nodoc
class __$HotelInfoCopyWithImpl<$Res>
    implements _$HotelInfoCopyWith<$Res> {
  __$HotelInfoCopyWithImpl(this._self, this._then);

  final _HotelInfo _self;
  final $Res Function(_HotelInfo) _then;

/// Create a copy of HotelInfo
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? id = null,Object? name = null,Object? city = null,Object? starRating = null,Object? roomTypes = null,}) {
  return _then(_HotelInfo(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,starRating: null == starRating ? _self.starRating : starRating // ignore: cast_nullable_to_non_nullable
as int,roomTypes: null == roomTypes ? _self._roomTypes : roomTypes // ignore: cast_nullable_to_non_nullable
as List<RoomTypeInfo>,
  ));
}


}


/// @nodoc
mixin _$RoomTypeInfo {

 String get name; int get capacity;
/// Create a copy of RoomTypeInfo
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$RoomTypeInfoCopyWith<RoomTypeInfo> get copyWith => _$RoomTypeInfoCopyWithImpl<RoomTypeInfo>(this as RoomTypeInfo, _$identity);

  /// Serializes this RoomTypeInfo to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as RoomTypeInfo;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is RoomTypeInfo&&(identical(other.name, _this.name) || other.name == _this.name)&&(identical(other.capacity, _this.capacity) || other.capacity == _this.capacity));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as RoomTypeInfo;
  return Object.hash(runtimeType,_this.name,_this.capacity);
}

@override
String toString() {
  final _this = this as RoomTypeInfo;
  return 'RoomTypeInfo(name: ${_this.name}, capacity: ${_this.capacity})';
}


}

/// @nodoc
abstract mixin class $RoomTypeInfoCopyWith<$Res>  {
  factory $RoomTypeInfoCopyWith(RoomTypeInfo value, $Res Function(RoomTypeInfo) _then) = _$RoomTypeInfoCopyWithImpl;
@useResult
$Res call({
 String name, int capacity
});




}
/// @nodoc
class _$RoomTypeInfoCopyWithImpl<$Res>
    implements $RoomTypeInfoCopyWith<$Res> {
  _$RoomTypeInfoCopyWithImpl(this._self, this._then);

  final RoomTypeInfo _self;
  final $Res Function(RoomTypeInfo) _then;

/// Create a copy of RoomTypeInfo
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? name = null,Object? capacity = null,}) {
  return _then(RoomTypeInfo(
name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,capacity: null == capacity ? _self.capacity : capacity // ignore: cast_nullable_to_non_nullable
as int,
  ));
}

}


/// Adds pattern-matching-related methods to [RoomTypeInfo].
extension RoomTypeInfoPatterns on RoomTypeInfo {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _RoomTypeInfo value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _RoomTypeInfo() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _RoomTypeInfo value)  $default,){
final _that = this;
switch (_that) {
case _RoomTypeInfo():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _RoomTypeInfo value)?  $default,){
final _that = this;
switch (_that) {
case _RoomTypeInfo() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String name,  int capacity)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _RoomTypeInfo() when $default != null:
return $default(_that.name,_that.capacity);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String name,  int capacity)  $default,) {final _that = this;
switch (_that) {
case _RoomTypeInfo():
return $default(_that.name,_that.capacity);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String name,  int capacity)?  $default,) {final _that = this;
switch (_that) {
case _RoomTypeInfo() when $default != null:
return $default(_that.name,_that.capacity);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _RoomTypeInfo implements RoomTypeInfo {
  const _RoomTypeInfo({required this.name, required this.capacity});
  factory _RoomTypeInfo.fromJson(Map<String, dynamic> json) => _$RoomTypeInfoFromJson(json);

@override final  String name;
@override final  int capacity;

/// Create a copy of RoomTypeInfo
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$RoomTypeInfoCopyWith<_RoomTypeInfo> get copyWith => __$RoomTypeInfoCopyWithImpl<_RoomTypeInfo>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$RoomTypeInfoToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _RoomTypeInfo&&(identical(other.name, name) || other.name == name)&&(identical(other.capacity, capacity) || other.capacity == capacity));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,name,capacity);
}

@override
String toString() {
    return 'RoomTypeInfo(name: $name, capacity: $capacity)';
}


}

/// @nodoc
abstract mixin class _$RoomTypeInfoCopyWith<$Res> implements $RoomTypeInfoCopyWith<$Res> {
  factory _$RoomTypeInfoCopyWith(_RoomTypeInfo value, $Res Function(_RoomTypeInfo) _then) = __$RoomTypeInfoCopyWithImpl;
@override @useResult
$Res call({
 String name, int capacity
});




}
/// @nodoc
class __$RoomTypeInfoCopyWithImpl<$Res>
    implements _$RoomTypeInfoCopyWith<$Res> {
  __$RoomTypeInfoCopyWithImpl(this._self, this._then);

  final _RoomTypeInfo _self;
  final $Res Function(_RoomTypeInfo) _then;

/// Create a copy of RoomTypeInfo
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? name = null,Object? capacity = null,}) {
  return _then(_RoomTypeInfo(
name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,capacity: null == capacity ? _self.capacity : capacity // ignore: cast_nullable_to_non_nullable
as int,
  ));
}


}

// dart format on
