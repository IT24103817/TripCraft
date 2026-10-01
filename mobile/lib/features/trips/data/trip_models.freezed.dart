// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint, type=warning, deprecated_member_use, deprecated_member_use_from_same_package
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'trip_models.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$TripRequest {

 String get id; String get objective; String get startDate; String get endDate; int get pax; double get budgetUsd; Map<String, dynamic> get preferences; String get status; String get createdAt; List<String> get cities;
/// Create a copy of TripRequest
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripRequestCopyWith<TripRequest> get copyWith => _$TripRequestCopyWithImpl<TripRequest>(this as TripRequest, _$identity);

  /// Serializes this TripRequest to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TripRequest;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripRequest&&(identical(other.id, _this.id) || other.id == _this.id)&&(identical(other.objective, _this.objective) || other.objective == _this.objective)&&(identical(other.startDate, _this.startDate) || other.startDate == _this.startDate)&&(identical(other.endDate, _this.endDate) || other.endDate == _this.endDate)&&(identical(other.pax, _this.pax) || other.pax == _this.pax)&&(identical(other.budgetUsd, _this.budgetUsd) || other.budgetUsd == _this.budgetUsd)&&const DeepCollectionEquality().equals(other.preferences, _this.preferences)&&(identical(other.status, _this.status) || other.status == _this.status)&&(identical(other.createdAt, _this.createdAt) || other.createdAt == _this.createdAt)&&const DeepCollectionEquality().equals(other.cities, _this.cities));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TripRequest;
  return Object.hash(runtimeType,_this.id,_this.objective,_this.startDate,_this.endDate,_this.pax,_this.budgetUsd,const DeepCollectionEquality().hash(_this.preferences),_this.status,_this.createdAt,const DeepCollectionEquality().hash(_this.cities));
}

@override
String toString() {
  final _this = this as TripRequest;
  return 'TripRequest(id: ${_this.id}, objective: ${_this.objective}, startDate: ${_this.startDate}, endDate: ${_this.endDate}, pax: ${_this.pax}, budgetUsd: ${_this.budgetUsd}, preferences: ${_this.preferences}, status: ${_this.status}, createdAt: ${_this.createdAt}, cities: ${_this.cities})';
}


}

/// @nodoc
abstract mixin class $TripRequestCopyWith<$Res>  {
  factory $TripRequestCopyWith(TripRequest value, $Res Function(TripRequest) _then) = _$TripRequestCopyWithImpl;
@useResult
$Res call({
 String id, String objective, String startDate, String endDate, int pax, double budgetUsd, Map<String, dynamic> preferences, String status, String createdAt, List<String> cities
});




}
/// @nodoc
class _$TripRequestCopyWithImpl<$Res>
    implements $TripRequestCopyWith<$Res> {
  _$TripRequestCopyWithImpl(this._self, this._then);

  final TripRequest _self;
  final $Res Function(TripRequest) _then;

/// Create a copy of TripRequest
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? id = null,Object? objective = null,Object? startDate = null,Object? endDate = null,Object? pax = null,Object? budgetUsd = null,Object? preferences = null,Object? status = null,Object? createdAt = null,Object? cities = null,}) {
  return _then(TripRequest(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,startDate: null == startDate ? _self.startDate : startDate // ignore: cast_nullable_to_non_nullable
as String,endDate: null == endDate ? _self.endDate : endDate // ignore: cast_nullable_to_non_nullable
as String,pax: null == pax ? _self.pax : pax // ignore: cast_nullable_to_non_nullable
as int,budgetUsd: null == budgetUsd ? _self.budgetUsd : budgetUsd // ignore: cast_nullable_to_non_nullable
as double,preferences: null == preferences ? _self.preferences : preferences // ignore: cast_nullable_to_non_nullable
as Map<String, dynamic>,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as String,createdAt: null == createdAt ? _self.createdAt : createdAt // ignore: cast_nullable_to_non_nullable
as String,cities: null == cities ? _self.cities : cities // ignore: cast_nullable_to_non_nullable
as List<String>,
  ));
}

}


/// Adds pattern-matching-related methods to [TripRequest].
extension TripRequestPatterns on TripRequest {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripRequest value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripRequest() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripRequest value)  $default,){
final _that = this;
switch (_that) {
case _TripRequest():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripRequest value)?  $default,){
final _that = this;
switch (_that) {
case _TripRequest() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String id,  String objective,  String startDate,  String endDate,  int pax,  double budgetUsd,  Map<String, dynamic> preferences,  String status,  String createdAt,  List<String> cities)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripRequest() when $default != null:
return $default(_that.id,_that.objective,_that.startDate,_that.endDate,_that.pax,_that.budgetUsd,_that.preferences,_that.status,_that.createdAt,_that.cities);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String id,  String objective,  String startDate,  String endDate,  int pax,  double budgetUsd,  Map<String, dynamic> preferences,  String status,  String createdAt,  List<String> cities)  $default,) {final _that = this;
switch (_that) {
case _TripRequest():
return $default(_that.id,_that.objective,_that.startDate,_that.endDate,_that.pax,_that.budgetUsd,_that.preferences,_that.status,_that.createdAt,_that.cities);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String id,  String objective,  String startDate,  String endDate,  int pax,  double budgetUsd,  Map<String, dynamic> preferences,  String status,  String createdAt,  List<String> cities)?  $default,) {final _that = this;
switch (_that) {
case _TripRequest() when $default != null:
return $default(_that.id,_that.objective,_that.startDate,_that.endDate,_that.pax,_that.budgetUsd,_that.preferences,_that.status,_that.createdAt,_that.cities);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TripRequest implements TripRequest {
  const _TripRequest({required this.id, required this.objective, required this.startDate, required this.endDate, required this.pax, required this.budgetUsd,  Map<String, dynamic> preferences = const <String, dynamic>{}, required this.status, required this.createdAt,  List<String> cities = const <String>[]}): _preferences = preferences,_cities = cities;
  factory _TripRequest.fromJson(Map<String, dynamic> json) => _$TripRequestFromJson(json);

@override final  String id;
@override final  String objective;
@override final  String startDate;
@override final  String endDate;
@override final  int pax;
@override final  double budgetUsd;
 final  Map<String, dynamic> _preferences;
@override@JsonKey() Map<String, dynamic> get preferences {
  if (_preferences is EqualUnmodifiableMapView) return _preferences;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableMapView(_preferences);
}

@override final  String status;
@override final  String createdAt;
 final  List<String> _cities;
@override@JsonKey() List<String> get cities {
  if (_cities is EqualUnmodifiableListView) return _cities;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_cities);
}


/// Create a copy of TripRequest
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripRequestCopyWith<_TripRequest> get copyWith => __$TripRequestCopyWithImpl<_TripRequest>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TripRequestToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripRequest&&(identical(other.id, id) || other.id == id)&&(identical(other.objective, objective) || other.objective == objective)&&(identical(other.startDate, startDate) || other.startDate == startDate)&&(identical(other.endDate, endDate) || other.endDate == endDate)&&(identical(other.pax, pax) || other.pax == pax)&&(identical(other.budgetUsd, budgetUsd) || other.budgetUsd == budgetUsd)&&const DeepCollectionEquality().equals(other.preferences, _preferences)&&(identical(other.status, status) || other.status == status)&&(identical(other.createdAt, createdAt) || other.createdAt == createdAt)&&const DeepCollectionEquality().equals(other.cities, _cities));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,id,objective,startDate,endDate,pax,budgetUsd,const DeepCollectionEquality().hash(_preferences),status,createdAt,const DeepCollectionEquality().hash(_cities));
}

@override
String toString() {
    return 'TripRequest(id: $id, objective: $objective, startDate: $startDate, endDate: $endDate, pax: $pax, budgetUsd: $budgetUsd, preferences: $preferences, status: $status, createdAt: $createdAt, cities: $cities)';
}


}

/// @nodoc
abstract mixin class _$TripRequestCopyWith<$Res> implements $TripRequestCopyWith<$Res> {
  factory _$TripRequestCopyWith(_TripRequest value, $Res Function(_TripRequest) _then) = __$TripRequestCopyWithImpl;
@override @useResult
$Res call({
 String id, String objective, String startDate, String endDate, int pax, double budgetUsd, Map<String, dynamic> preferences, String status, String createdAt, List<String> cities
});




}
/// @nodoc
class __$TripRequestCopyWithImpl<$Res>
    implements _$TripRequestCopyWith<$Res> {
  __$TripRequestCopyWithImpl(this._self, this._then);

  final _TripRequest _self;
  final $Res Function(_TripRequest) _then;

/// Create a copy of TripRequest
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? id = null,Object? objective = null,Object? startDate = null,Object? endDate = null,Object? pax = null,Object? budgetUsd = null,Object? preferences = null,Object? status = null,Object? createdAt = null,Object? cities = null,}) {
  return _then(_TripRequest(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,startDate: null == startDate ? _self.startDate : startDate // ignore: cast_nullable_to_non_nullable
as String,endDate: null == endDate ? _self.endDate : endDate // ignore: cast_nullable_to_non_nullable
as String,pax: null == pax ? _self.pax : pax // ignore: cast_nullable_to_non_nullable
as int,budgetUsd: null == budgetUsd ? _self.budgetUsd : budgetUsd // ignore: cast_nullable_to_non_nullable
as double,preferences: null == preferences ? _self._preferences : preferences // ignore: cast_nullable_to_non_nullable
as Map<String, dynamic>,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as String,createdAt: null == createdAt ? _self.createdAt : createdAt // ignore: cast_nullable_to_non_nullable
as String,cities: null == cities ? _self._cities : cities // ignore: cast_nullable_to_non_nullable
as List<String>,
  ));
}


}


/// @nodoc
mixin _$CreateTripRequest {

 String get objective; String get startDate; String get endDate; int get pax; double get budgetUsd; Map<String, dynamic> get preferences; String get nationality; String get passportNumber; List<String> get cities;
/// Create a copy of CreateTripRequest
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$CreateTripRequestCopyWith<CreateTripRequest> get copyWith => _$CreateTripRequestCopyWithImpl<CreateTripRequest>(this as CreateTripRequest, _$identity);

  /// Serializes this CreateTripRequest to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as CreateTripRequest;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is CreateTripRequest&&(identical(other.objective, _this.objective) || other.objective == _this.objective)&&(identical(other.startDate, _this.startDate) || other.startDate == _this.startDate)&&(identical(other.endDate, _this.endDate) || other.endDate == _this.endDate)&&(identical(other.pax, _this.pax) || other.pax == _this.pax)&&(identical(other.budgetUsd, _this.budgetUsd) || other.budgetUsd == _this.budgetUsd)&&const DeepCollectionEquality().equals(other.preferences, _this.preferences)&&(identical(other.nationality, _this.nationality) || other.nationality == _this.nationality)&&(identical(other.passportNumber, _this.passportNumber) || other.passportNumber == _this.passportNumber)&&const DeepCollectionEquality().equals(other.cities, _this.cities));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as CreateTripRequest;
  return Object.hash(runtimeType,_this.objective,_this.startDate,_this.endDate,_this.pax,_this.budgetUsd,const DeepCollectionEquality().hash(_this.preferences),_this.nationality,_this.passportNumber,const DeepCollectionEquality().hash(_this.cities));
}

@override
String toString() {
  final _this = this as CreateTripRequest;
  return 'CreateTripRequest(objective: ${_this.objective}, startDate: ${_this.startDate}, endDate: ${_this.endDate}, pax: ${_this.pax}, budgetUsd: ${_this.budgetUsd}, preferences: ${_this.preferences}, nationality: ${_this.nationality}, passportNumber: ${_this.passportNumber}, cities: ${_this.cities})';
}


}

/// @nodoc
abstract mixin class $CreateTripRequestCopyWith<$Res>  {
  factory $CreateTripRequestCopyWith(CreateTripRequest value, $Res Function(CreateTripRequest) _then) = _$CreateTripRequestCopyWithImpl;
@useResult
$Res call({
 String objective, String startDate, String endDate, int pax, double budgetUsd, Map<String, dynamic> preferences, String nationality, String passportNumber, List<String> cities
});




}
/// @nodoc
class _$CreateTripRequestCopyWithImpl<$Res>
    implements $CreateTripRequestCopyWith<$Res> {
  _$CreateTripRequestCopyWithImpl(this._self, this._then);

  final CreateTripRequest _self;
  final $Res Function(CreateTripRequest) _then;

/// Create a copy of CreateTripRequest
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? objective = null,Object? startDate = null,Object? endDate = null,Object? pax = null,Object? budgetUsd = null,Object? preferences = null,Object? nationality = null,Object? passportNumber = null,Object? cities = null,}) {
  return _then(CreateTripRequest(
objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,startDate: null == startDate ? _self.startDate : startDate // ignore: cast_nullable_to_non_nullable
as String,endDate: null == endDate ? _self.endDate : endDate // ignore: cast_nullable_to_non_nullable
as String,pax: null == pax ? _self.pax : pax // ignore: cast_nullable_to_non_nullable
as int,budgetUsd: null == budgetUsd ? _self.budgetUsd : budgetUsd // ignore: cast_nullable_to_non_nullable
as double,preferences: null == preferences ? _self.preferences : preferences // ignore: cast_nullable_to_non_nullable
as Map<String, dynamic>,nationality: null == nationality ? _self.nationality : nationality // ignore: cast_nullable_to_non_nullable
as String,passportNumber: null == passportNumber ? _self.passportNumber : passportNumber // ignore: cast_nullable_to_non_nullable
as String,cities: null == cities ? _self.cities : cities // ignore: cast_nullable_to_non_nullable
as List<String>,
  ));
}

}


/// Adds pattern-matching-related methods to [CreateTripRequest].
extension CreateTripRequestPatterns on CreateTripRequest {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _CreateTripRequest value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _CreateTripRequest() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _CreateTripRequest value)  $default,){
final _that = this;
switch (_that) {
case _CreateTripRequest():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _CreateTripRequest value)?  $default,){
final _that = this;
switch (_that) {
case _CreateTripRequest() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String objective,  String startDate,  String endDate,  int pax,  double budgetUsd,  Map<String, dynamic> preferences,  String nationality,  String passportNumber,  List<String> cities)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _CreateTripRequest() when $default != null:
return $default(_that.objective,_that.startDate,_that.endDate,_that.pax,_that.budgetUsd,_that.preferences,_that.nationality,_that.passportNumber,_that.cities);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String objective,  String startDate,  String endDate,  int pax,  double budgetUsd,  Map<String, dynamic> preferences,  String nationality,  String passportNumber,  List<String> cities)  $default,) {final _that = this;
switch (_that) {
case _CreateTripRequest():
return $default(_that.objective,_that.startDate,_that.endDate,_that.pax,_that.budgetUsd,_that.preferences,_that.nationality,_that.passportNumber,_that.cities);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String objective,  String startDate,  String endDate,  int pax,  double budgetUsd,  Map<String, dynamic> preferences,  String nationality,  String passportNumber,  List<String> cities)?  $default,) {final _that = this;
switch (_that) {
case _CreateTripRequest() when $default != null:
return $default(_that.objective,_that.startDate,_that.endDate,_that.pax,_that.budgetUsd,_that.preferences,_that.nationality,_that.passportNumber,_that.cities);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _CreateTripRequest implements CreateTripRequest {
  const _CreateTripRequest({required this.objective, required this.startDate, required this.endDate, required this.pax, required this.budgetUsd, required  Map<String, dynamic> preferences, required this.nationality, required this.passportNumber, required  List<String> cities}): _preferences = preferences,_cities = cities;
  factory _CreateTripRequest.fromJson(Map<String, dynamic> json) => _$CreateTripRequestFromJson(json);

@override final  String objective;
@override final  String startDate;
@override final  String endDate;
@override final  int pax;
@override final  double budgetUsd;
 final  Map<String, dynamic> _preferences;
@override Map<String, dynamic> get preferences {
  if (_preferences is EqualUnmodifiableMapView) return _preferences;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableMapView(_preferences);
}

@override final  String nationality;
@override final  String passportNumber;
 final  List<String> _cities;
@override List<String> get cities {
  if (_cities is EqualUnmodifiableListView) return _cities;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_cities);
}


/// Create a copy of CreateTripRequest
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$CreateTripRequestCopyWith<_CreateTripRequest> get copyWith => __$CreateTripRequestCopyWithImpl<_CreateTripRequest>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$CreateTripRequestToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _CreateTripRequest&&(identical(other.objective, objective) || other.objective == objective)&&(identical(other.startDate, startDate) || other.startDate == startDate)&&(identical(other.endDate, endDate) || other.endDate == endDate)&&(identical(other.pax, pax) || other.pax == pax)&&(identical(other.budgetUsd, budgetUsd) || other.budgetUsd == budgetUsd)&&const DeepCollectionEquality().equals(other.preferences, _preferences)&&(identical(other.nationality, nationality) || other.nationality == nationality)&&(identical(other.passportNumber, passportNumber) || other.passportNumber == passportNumber)&&const DeepCollectionEquality().equals(other.cities, _cities));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,objective,startDate,endDate,pax,budgetUsd,const DeepCollectionEquality().hash(_preferences),nationality,passportNumber,const DeepCollectionEquality().hash(_cities));
}

@override
String toString() {
    return 'CreateTripRequest(objective: $objective, startDate: $startDate, endDate: $endDate, pax: $pax, budgetUsd: $budgetUsd, preferences: $preferences, nationality: $nationality, passportNumber: $passportNumber, cities: $cities)';
}


}

/// @nodoc
abstract mixin class _$CreateTripRequestCopyWith<$Res> implements $CreateTripRequestCopyWith<$Res> {
  factory _$CreateTripRequestCopyWith(_CreateTripRequest value, $Res Function(_CreateTripRequest) _then) = __$CreateTripRequestCopyWithImpl;
@override @useResult
$Res call({
 String objective, String startDate, String endDate, int pax, double budgetUsd, Map<String, dynamic> preferences, String nationality, String passportNumber, List<String> cities
});




}
/// @nodoc
class __$CreateTripRequestCopyWithImpl<$Res>
    implements _$CreateTripRequestCopyWith<$Res> {
  __$CreateTripRequestCopyWithImpl(this._self, this._then);

  final _CreateTripRequest _self;
  final $Res Function(_CreateTripRequest) _then;

/// Create a copy of CreateTripRequest
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? objective = null,Object? startDate = null,Object? endDate = null,Object? pax = null,Object? budgetUsd = null,Object? preferences = null,Object? nationality = null,Object? passportNumber = null,Object? cities = null,}) {
  return _then(_CreateTripRequest(
objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,startDate: null == startDate ? _self.startDate : startDate // ignore: cast_nullable_to_non_nullable
as String,endDate: null == endDate ? _self.endDate : endDate // ignore: cast_nullable_to_non_nullable
as String,pax: null == pax ? _self.pax : pax // ignore: cast_nullable_to_non_nullable
as int,budgetUsd: null == budgetUsd ? _self.budgetUsd : budgetUsd // ignore: cast_nullable_to_non_nullable
as double,preferences: null == preferences ? _self._preferences : preferences // ignore: cast_nullable_to_non_nullable
as Map<String, dynamic>,nationality: null == nationality ? _self.nationality : nationality // ignore: cast_nullable_to_non_nullable
as String,passportNumber: null == passportNumber ? _self.passportNumber : passportNumber // ignore: cast_nullable_to_non_nullable
as String,cities: null == cities ? _self._cities : cities // ignore: cast_nullable_to_non_nullable
as List<String>,
  ));
}


}


/// @nodoc
mixin _$StartPlanningResult {

 String get workflowId; String get workflowStatus; String get tripStatus; String? get errorSummary;
/// Create a copy of StartPlanningResult
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$StartPlanningResultCopyWith<StartPlanningResult> get copyWith => _$StartPlanningResultCopyWithImpl<StartPlanningResult>(this as StartPlanningResult, _$identity);

  /// Serializes this StartPlanningResult to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as StartPlanningResult;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is StartPlanningResult&&(identical(other.workflowId, _this.workflowId) || other.workflowId == _this.workflowId)&&(identical(other.workflowStatus, _this.workflowStatus) || other.workflowStatus == _this.workflowStatus)&&(identical(other.tripStatus, _this.tripStatus) || other.tripStatus == _this.tripStatus)&&(identical(other.errorSummary, _this.errorSummary) || other.errorSummary == _this.errorSummary));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as StartPlanningResult;
  return Object.hash(runtimeType,_this.workflowId,_this.workflowStatus,_this.tripStatus,_this.errorSummary);
}

@override
String toString() {
  final _this = this as StartPlanningResult;
  return 'StartPlanningResult(workflowId: ${_this.workflowId}, workflowStatus: ${_this.workflowStatus}, tripStatus: ${_this.tripStatus}, errorSummary: ${_this.errorSummary})';
}


}

/// @nodoc
abstract mixin class $StartPlanningResultCopyWith<$Res>  {
  factory $StartPlanningResultCopyWith(StartPlanningResult value, $Res Function(StartPlanningResult) _then) = _$StartPlanningResultCopyWithImpl;
@useResult
$Res call({
 String workflowId, String workflowStatus, String tripStatus, String? errorSummary
});




}
/// @nodoc
class _$StartPlanningResultCopyWithImpl<$Res>
    implements $StartPlanningResultCopyWith<$Res> {
  _$StartPlanningResultCopyWithImpl(this._self, this._then);

  final StartPlanningResult _self;
  final $Res Function(StartPlanningResult) _then;

/// Create a copy of StartPlanningResult
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? workflowId = null,Object? workflowStatus = null,Object? tripStatus = null,Object? errorSummary = freezed,}) {
  return _then(StartPlanningResult(
workflowId: null == workflowId ? _self.workflowId : workflowId // ignore: cast_nullable_to_non_nullable
as String,workflowStatus: null == workflowStatus ? _self.workflowStatus : workflowStatus // ignore: cast_nullable_to_non_nullable
as String,tripStatus: null == tripStatus ? _self.tripStatus : tripStatus // ignore: cast_nullable_to_non_nullable
as String,errorSummary: freezed == errorSummary ? _self.errorSummary : errorSummary // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}

}


/// Adds pattern-matching-related methods to [StartPlanningResult].
extension StartPlanningResultPatterns on StartPlanningResult {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _StartPlanningResult value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _StartPlanningResult() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _StartPlanningResult value)  $default,){
final _that = this;
switch (_that) {
case _StartPlanningResult():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _StartPlanningResult value)?  $default,){
final _that = this;
switch (_that) {
case _StartPlanningResult() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String workflowId,  String workflowStatus,  String tripStatus,  String? errorSummary)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _StartPlanningResult() when $default != null:
return $default(_that.workflowId,_that.workflowStatus,_that.tripStatus,_that.errorSummary);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String workflowId,  String workflowStatus,  String tripStatus,  String? errorSummary)  $default,) {final _that = this;
switch (_that) {
case _StartPlanningResult():
return $default(_that.workflowId,_that.workflowStatus,_that.tripStatus,_that.errorSummary);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String workflowId,  String workflowStatus,  String tripStatus,  String? errorSummary)?  $default,) {final _that = this;
switch (_that) {
case _StartPlanningResult() when $default != null:
return $default(_that.workflowId,_that.workflowStatus,_that.tripStatus,_that.errorSummary);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _StartPlanningResult implements StartPlanningResult {
  const _StartPlanningResult({required this.workflowId, required this.workflowStatus, required this.tripStatus, this.errorSummary});
  factory _StartPlanningResult.fromJson(Map<String, dynamic> json) => _$StartPlanningResultFromJson(json);

@override final  String workflowId;
@override final  String workflowStatus;
@override final  String tripStatus;
@override final  String? errorSummary;

/// Create a copy of StartPlanningResult
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$StartPlanningResultCopyWith<_StartPlanningResult> get copyWith => __$StartPlanningResultCopyWithImpl<_StartPlanningResult>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$StartPlanningResultToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _StartPlanningResult&&(identical(other.workflowId, workflowId) || other.workflowId == workflowId)&&(identical(other.workflowStatus, workflowStatus) || other.workflowStatus == workflowStatus)&&(identical(other.tripStatus, tripStatus) || other.tripStatus == tripStatus)&&(identical(other.errorSummary, errorSummary) || other.errorSummary == errorSummary));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,workflowId,workflowStatus,tripStatus,errorSummary);
}

@override
String toString() {
    return 'StartPlanningResult(workflowId: $workflowId, workflowStatus: $workflowStatus, tripStatus: $tripStatus, errorSummary: $errorSummary)';
}


}

/// @nodoc
abstract mixin class _$StartPlanningResultCopyWith<$Res> implements $StartPlanningResultCopyWith<$Res> {
  factory _$StartPlanningResultCopyWith(_StartPlanningResult value, $Res Function(_StartPlanningResult) _then) = __$StartPlanningResultCopyWithImpl;
@override @useResult
$Res call({
 String workflowId, String workflowStatus, String tripStatus, String? errorSummary
});




}
/// @nodoc
class __$StartPlanningResultCopyWithImpl<$Res>
    implements _$StartPlanningResultCopyWith<$Res> {
  __$StartPlanningResultCopyWithImpl(this._self, this._then);

  final _StartPlanningResult _self;
  final $Res Function(_StartPlanningResult) _then;

/// Create a copy of StartPlanningResult
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? workflowId = null,Object? workflowStatus = null,Object? tripStatus = null,Object? errorSummary = freezed,}) {
  return _then(_StartPlanningResult(
workflowId: null == workflowId ? _self.workflowId : workflowId // ignore: cast_nullable_to_non_nullable
as String,workflowStatus: null == workflowStatus ? _self.workflowStatus : workflowStatus // ignore: cast_nullable_to_non_nullable
as String,tripStatus: null == tripStatus ? _self.tripStatus : tripStatus // ignore: cast_nullable_to_non_nullable
as String,errorSummary: freezed == errorSummary ? _self.errorSummary : errorSummary // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}


/// @nodoc
mixin _$Attraction {

 String get id; String get name; String get city; double get latitude; double get longitude;
/// Create a copy of Attraction
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$AttractionCopyWith<Attraction> get copyWith => _$AttractionCopyWithImpl<Attraction>(this as Attraction, _$identity);

  /// Serializes this Attraction to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as Attraction;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is Attraction&&(identical(other.id, _this.id) || other.id == _this.id)&&(identical(other.name, _this.name) || other.name == _this.name)&&(identical(other.city, _this.city) || other.city == _this.city)&&(identical(other.latitude, _this.latitude) || other.latitude == _this.latitude)&&(identical(other.longitude, _this.longitude) || other.longitude == _this.longitude));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as Attraction;
  return Object.hash(runtimeType,_this.id,_this.name,_this.city,_this.latitude,_this.longitude);
}

@override
String toString() {
  final _this = this as Attraction;
  return 'Attraction(id: ${_this.id}, name: ${_this.name}, city: ${_this.city}, latitude: ${_this.latitude}, longitude: ${_this.longitude})';
}


}

/// @nodoc
abstract mixin class $AttractionCopyWith<$Res>  {
  factory $AttractionCopyWith(Attraction value, $Res Function(Attraction) _then) = _$AttractionCopyWithImpl;
@useResult
$Res call({
 String id, String name, String city, double latitude, double longitude
});




}
/// @nodoc
class _$AttractionCopyWithImpl<$Res>
    implements $AttractionCopyWith<$Res> {
  _$AttractionCopyWithImpl(this._self, this._then);

  final Attraction _self;
  final $Res Function(Attraction) _then;

/// Create a copy of Attraction
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? id = null,Object? name = null,Object? city = null,Object? latitude = null,Object? longitude = null,}) {
  return _then(Attraction(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,latitude: null == latitude ? _self.latitude : latitude // ignore: cast_nullable_to_non_nullable
as double,longitude: null == longitude ? _self.longitude : longitude // ignore: cast_nullable_to_non_nullable
as double,
  ));
}

}


/// Adds pattern-matching-related methods to [Attraction].
extension AttractionPatterns on Attraction {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _Attraction value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Attraction() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _Attraction value)  $default,){
final _that = this;
switch (_that) {
case _Attraction():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _Attraction value)?  $default,){
final _that = this;
switch (_that) {
case _Attraction() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String id,  String name,  String city,  double latitude,  double longitude)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Attraction() when $default != null:
return $default(_that.id,_that.name,_that.city,_that.latitude,_that.longitude);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String id,  String name,  String city,  double latitude,  double longitude)  $default,) {final _that = this;
switch (_that) {
case _Attraction():
return $default(_that.id,_that.name,_that.city,_that.latitude,_that.longitude);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String id,  String name,  String city,  double latitude,  double longitude)?  $default,) {final _that = this;
switch (_that) {
case _Attraction() when $default != null:
return $default(_that.id,_that.name,_that.city,_that.latitude,_that.longitude);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _Attraction implements Attraction {
  const _Attraction({required this.id, required this.name, required this.city, required this.latitude, required this.longitude});
  factory _Attraction.fromJson(Map<String, dynamic> json) => _$AttractionFromJson(json);

@override final  String id;
@override final  String name;
@override final  String city;
@override final  double latitude;
@override final  double longitude;

/// Create a copy of Attraction
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$AttractionCopyWith<_Attraction> get copyWith => __$AttractionCopyWithImpl<_Attraction>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$AttractionToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _Attraction&&(identical(other.id, id) || other.id == id)&&(identical(other.name, name) || other.name == name)&&(identical(other.city, city) || other.city == city)&&(identical(other.latitude, latitude) || other.latitude == latitude)&&(identical(other.longitude, longitude) || other.longitude == longitude));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,id,name,city,latitude,longitude);
}

@override
String toString() {
    return 'Attraction(id: $id, name: $name, city: $city, latitude: $latitude, longitude: $longitude)';
}


}

/// @nodoc
abstract mixin class _$AttractionCopyWith<$Res> implements $AttractionCopyWith<$Res> {
  factory _$AttractionCopyWith(_Attraction value, $Res Function(_Attraction) _then) = __$AttractionCopyWithImpl;
@override @useResult
$Res call({
 String id, String name, String city, double latitude, double longitude
});




}
/// @nodoc
class __$AttractionCopyWithImpl<$Res>
    implements _$AttractionCopyWith<$Res> {
  __$AttractionCopyWithImpl(this._self, this._then);

  final _Attraction _self;
  final $Res Function(_Attraction) _then;

/// Create a copy of Attraction
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? id = null,Object? name = null,Object? city = null,Object? latitude = null,Object? longitude = null,}) {
  return _then(_Attraction(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,latitude: null == latitude ? _self.latitude : latitude // ignore: cast_nullable_to_non_nullable
as double,longitude: null == longitude ? _self.longitude : longitude // ignore: cast_nullable_to_non_nullable
as double,
  ));
}


}

/// @nodoc
mixin _$TripDay {

 int get day; String get city; String? get date; String? get transport; String? get weather; List<TripStop> get stops;
/// Create a copy of TripDay
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripDayCopyWith<TripDay> get copyWith => _$TripDayCopyWithImpl<TripDay>(this as TripDay, _$identity);



@override
bool operator ==(Object other) {
  final _this = this as TripDay;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripDay&&(identical(other.day, _this.day) || other.day == _this.day)&&(identical(other.city, _this.city) || other.city == _this.city)&&(identical(other.date, _this.date) || other.date == _this.date)&&(identical(other.transport, _this.transport) || other.transport == _this.transport)&&(identical(other.weather, _this.weather) || other.weather == _this.weather)&&const DeepCollectionEquality().equals(other.stops, _this.stops));
}


@override
int get hashCode {
  final _this = this as TripDay;
  return Object.hash(runtimeType,_this.day,_this.city,_this.date,_this.transport,_this.weather,const DeepCollectionEquality().hash(_this.stops));
}

@override
String toString() {
  final _this = this as TripDay;
  return 'TripDay(day: ${_this.day}, city: ${_this.city}, date: ${_this.date}, transport: ${_this.transport}, weather: ${_this.weather}, stops: ${_this.stops})';
}


}

/// @nodoc
abstract mixin class $TripDayCopyWith<$Res>  {
  factory $TripDayCopyWith(TripDay value, $Res Function(TripDay) _then) = _$TripDayCopyWithImpl;
@useResult
$Res call({
 int day, String city, String? date, String? transport, String? weather, List<TripStop> stops
});




}
/// @nodoc
class _$TripDayCopyWithImpl<$Res>
    implements $TripDayCopyWith<$Res> {
  _$TripDayCopyWithImpl(this._self, this._then);

  final TripDay _self;
  final $Res Function(TripDay) _then;

/// Create a copy of TripDay
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? day = null,Object? city = null,Object? date = freezed,Object? transport = freezed,Object? weather = freezed,Object? stops = null,}) {
  return _then(TripDay(
day: null == day ? _self.day : day // ignore: cast_nullable_to_non_nullable
as int,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,date: freezed == date ? _self.date : date // ignore: cast_nullable_to_non_nullable
as String?,transport: freezed == transport ? _self.transport : transport // ignore: cast_nullable_to_non_nullable
as String?,weather: freezed == weather ? _self.weather : weather // ignore: cast_nullable_to_non_nullable
as String?,stops: null == stops ? _self.stops : stops // ignore: cast_nullable_to_non_nullable
as List<TripStop>,
  ));
}

}


/// Adds pattern-matching-related methods to [TripDay].
extension TripDayPatterns on TripDay {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripDay value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripDay() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripDay value)  $default,){
final _that = this;
switch (_that) {
case _TripDay():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripDay value)?  $default,){
final _that = this;
switch (_that) {
case _TripDay() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( int day,  String city,  String? date,  String? transport,  String? weather,  List<TripStop> stops)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripDay() when $default != null:
return $default(_that.day,_that.city,_that.date,_that.transport,_that.weather,_that.stops);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( int day,  String city,  String? date,  String? transport,  String? weather,  List<TripStop> stops)  $default,) {final _that = this;
switch (_that) {
case _TripDay():
return $default(_that.day,_that.city,_that.date,_that.transport,_that.weather,_that.stops);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( int day,  String city,  String? date,  String? transport,  String? weather,  List<TripStop> stops)?  $default,) {final _that = this;
switch (_that) {
case _TripDay() when $default != null:
return $default(_that.day,_that.city,_that.date,_that.transport,_that.weather,_that.stops);case _:
  return null;

}
}

}

/// @nodoc


class _TripDay implements TripDay {
  const _TripDay({required this.day, required this.city, this.date, this.transport, this.weather, required  List<TripStop> stops}): _stops = stops;
  

@override final  int day;
@override final  String city;
@override final  String? date;
@override final  String? transport;
@override final  String? weather;
 final  List<TripStop> _stops;
@override List<TripStop> get stops {
  if (_stops is EqualUnmodifiableListView) return _stops;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_stops);
}


/// Create a copy of TripDay
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripDayCopyWith<_TripDay> get copyWith => __$TripDayCopyWithImpl<_TripDay>(this, _$identity);



@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripDay&&(identical(other.day, day) || other.day == day)&&(identical(other.city, city) || other.city == city)&&(identical(other.date, date) || other.date == date)&&(identical(other.transport, transport) || other.transport == transport)&&(identical(other.weather, weather) || other.weather == weather)&&const DeepCollectionEquality().equals(other.stops, _stops));
}


@override
int get hashCode {
    return Object.hash(runtimeType,day,city,date,transport,weather,const DeepCollectionEquality().hash(_stops));
}

@override
String toString() {
    return 'TripDay(day: $day, city: $city, date: $date, transport: $transport, weather: $weather, stops: $stops)';
}


}

/// @nodoc
abstract mixin class _$TripDayCopyWith<$Res> implements $TripDayCopyWith<$Res> {
  factory _$TripDayCopyWith(_TripDay value, $Res Function(_TripDay) _then) = __$TripDayCopyWithImpl;
@override @useResult
$Res call({
 int day, String city, String? date, String? transport, String? weather, List<TripStop> stops
});




}
/// @nodoc
class __$TripDayCopyWithImpl<$Res>
    implements _$TripDayCopyWith<$Res> {
  __$TripDayCopyWithImpl(this._self, this._then);

  final _TripDay _self;
  final $Res Function(_TripDay) _then;

/// Create a copy of TripDay
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? day = null,Object? city = null,Object? date = freezed,Object? transport = freezed,Object? weather = freezed,Object? stops = null,}) {
  return _then(_TripDay(
day: null == day ? _self.day : day // ignore: cast_nullable_to_non_nullable
as int,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,date: freezed == date ? _self.date : date // ignore: cast_nullable_to_non_nullable
as String?,transport: freezed == transport ? _self.transport : transport // ignore: cast_nullable_to_non_nullable
as String?,weather: freezed == weather ? _self.weather : weather // ignore: cast_nullable_to_non_nullable
as String?,stops: null == stops ? _self._stops : stops // ignore: cast_nullable_to_non_nullable
as List<TripStop>,
  ));
}


}

/// @nodoc
mixin _$TripStop {

 String get attractionId; String get name;
/// Create a copy of TripStop
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripStopCopyWith<TripStop> get copyWith => _$TripStopCopyWithImpl<TripStop>(this as TripStop, _$identity);



@override
bool operator ==(Object other) {
  final _this = this as TripStop;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripStop&&(identical(other.attractionId, _this.attractionId) || other.attractionId == _this.attractionId)&&(identical(other.name, _this.name) || other.name == _this.name));
}


@override
int get hashCode {
  final _this = this as TripStop;
  return Object.hash(runtimeType,_this.attractionId,_this.name);
}

@override
String toString() {
  final _this = this as TripStop;
  return 'TripStop(attractionId: ${_this.attractionId}, name: ${_this.name})';
}


}

/// @nodoc
abstract mixin class $TripStopCopyWith<$Res>  {
  factory $TripStopCopyWith(TripStop value, $Res Function(TripStop) _then) = _$TripStopCopyWithImpl;
@useResult
$Res call({
 String attractionId, String name
});




}
/// @nodoc
class _$TripStopCopyWithImpl<$Res>
    implements $TripStopCopyWith<$Res> {
  _$TripStopCopyWithImpl(this._self, this._then);

  final TripStop _self;
  final $Res Function(TripStop) _then;

/// Create a copy of TripStop
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? attractionId = null,Object? name = null,}) {
  return _then(TripStop(
attractionId: null == attractionId ? _self.attractionId : attractionId // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [TripStop].
extension TripStopPatterns on TripStop {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripStop value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripStop() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripStop value)  $default,){
final _that = this;
switch (_that) {
case _TripStop():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripStop value)?  $default,){
final _that = this;
switch (_that) {
case _TripStop() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String attractionId,  String name)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripStop() when $default != null:
return $default(_that.attractionId,_that.name);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String attractionId,  String name)  $default,) {final _that = this;
switch (_that) {
case _TripStop():
return $default(_that.attractionId,_that.name);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String attractionId,  String name)?  $default,) {final _that = this;
switch (_that) {
case _TripStop() when $default != null:
return $default(_that.attractionId,_that.name);case _:
  return null;

}
}

}

/// @nodoc


class _TripStop implements TripStop {
  const _TripStop({required this.attractionId, required this.name});
  

@override final  String attractionId;
@override final  String name;

/// Create a copy of TripStop
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripStopCopyWith<_TripStop> get copyWith => __$TripStopCopyWithImpl<_TripStop>(this, _$identity);



@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripStop&&(identical(other.attractionId, attractionId) || other.attractionId == attractionId)&&(identical(other.name, name) || other.name == name));
}


@override
int get hashCode {
    return Object.hash(runtimeType,attractionId,name);
}

@override
String toString() {
    return 'TripStop(attractionId: $attractionId, name: $name)';
}


}

/// @nodoc
abstract mixin class _$TripStopCopyWith<$Res> implements $TripStopCopyWith<$Res> {
  factory _$TripStopCopyWith(_TripStop value, $Res Function(_TripStop) _then) = __$TripStopCopyWithImpl;
@override @useResult
$Res call({
 String attractionId, String name
});




}
/// @nodoc
class __$TripStopCopyWithImpl<$Res>
    implements _$TripStopCopyWith<$Res> {
  __$TripStopCopyWithImpl(this._self, this._then);

  final _TripStop _self;
  final $Res Function(_TripStop) _then;

/// Create a copy of TripStop
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? attractionId = null,Object? name = null,}) {
  return _then(_TripStop(
attractionId: null == attractionId ? _self.attractionId : attractionId // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}


/// @nodoc
mixin _$TripWorkflow {

 String get id; String get status; String? get currentStep; String? get errorSummary; WorkflowOutcome? get finalOutcome;
/// Create a copy of TripWorkflow
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripWorkflowCopyWith<TripWorkflow> get copyWith => _$TripWorkflowCopyWithImpl<TripWorkflow>(this as TripWorkflow, _$identity);

  /// Serializes this TripWorkflow to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TripWorkflow;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripWorkflow&&(identical(other.id, _this.id) || other.id == _this.id)&&(identical(other.status, _this.status) || other.status == _this.status)&&(identical(other.currentStep, _this.currentStep) || other.currentStep == _this.currentStep)&&(identical(other.errorSummary, _this.errorSummary) || other.errorSummary == _this.errorSummary)&&(identical(other.finalOutcome, _this.finalOutcome) || other.finalOutcome == _this.finalOutcome));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TripWorkflow;
  return Object.hash(runtimeType,_this.id,_this.status,_this.currentStep,_this.errorSummary,_this.finalOutcome);
}

@override
String toString() {
  final _this = this as TripWorkflow;
  return 'TripWorkflow(id: ${_this.id}, status: ${_this.status}, currentStep: ${_this.currentStep}, errorSummary: ${_this.errorSummary}, finalOutcome: ${_this.finalOutcome})';
}


}

/// @nodoc
abstract mixin class $TripWorkflowCopyWith<$Res>  {
  factory $TripWorkflowCopyWith(TripWorkflow value, $Res Function(TripWorkflow) _then) = _$TripWorkflowCopyWithImpl;
@useResult
$Res call({
 String id, String status, String? currentStep, String? errorSummary, WorkflowOutcome? finalOutcome
});


$WorkflowOutcomeCopyWith<$Res>? get finalOutcome;

}
/// @nodoc
class _$TripWorkflowCopyWithImpl<$Res>
    implements $TripWorkflowCopyWith<$Res> {
  _$TripWorkflowCopyWithImpl(this._self, this._then);

  final TripWorkflow _self;
  final $Res Function(TripWorkflow) _then;

/// Create a copy of TripWorkflow
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? id = null,Object? status = null,Object? currentStep = freezed,Object? errorSummary = freezed,Object? finalOutcome = freezed,}) {
  return _then(TripWorkflow(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as String,currentStep: freezed == currentStep ? _self.currentStep : currentStep // ignore: cast_nullable_to_non_nullable
as String?,errorSummary: freezed == errorSummary ? _self.errorSummary : errorSummary // ignore: cast_nullable_to_non_nullable
as String?,finalOutcome: freezed == finalOutcome ? _self.finalOutcome : finalOutcome // ignore: cast_nullable_to_non_nullable
as WorkflowOutcome?,
  ));
}
/// Create a copy of TripWorkflow
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$WorkflowOutcomeCopyWith<$Res>? get finalOutcome {
    if (_self.finalOutcome == null) {
    return null;
  }

  return $WorkflowOutcomeCopyWith<$Res>(_self.finalOutcome!, (value) {
    return _then(_self.copyWith(finalOutcome: value));
  });
}
}


/// Adds pattern-matching-related methods to [TripWorkflow].
extension TripWorkflowPatterns on TripWorkflow {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripWorkflow value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripWorkflow() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripWorkflow value)  $default,){
final _that = this;
switch (_that) {
case _TripWorkflow():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripWorkflow value)?  $default,){
final _that = this;
switch (_that) {
case _TripWorkflow() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String id,  String status,  String? currentStep,  String? errorSummary,  WorkflowOutcome? finalOutcome)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripWorkflow() when $default != null:
return $default(_that.id,_that.status,_that.currentStep,_that.errorSummary,_that.finalOutcome);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String id,  String status,  String? currentStep,  String? errorSummary,  WorkflowOutcome? finalOutcome)  $default,) {final _that = this;
switch (_that) {
case _TripWorkflow():
return $default(_that.id,_that.status,_that.currentStep,_that.errorSummary,_that.finalOutcome);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String id,  String status,  String? currentStep,  String? errorSummary,  WorkflowOutcome? finalOutcome)?  $default,) {final _that = this;
switch (_that) {
case _TripWorkflow() when $default != null:
return $default(_that.id,_that.status,_that.currentStep,_that.errorSummary,_that.finalOutcome);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TripWorkflow implements TripWorkflow {
  const _TripWorkflow({required this.id, required this.status, this.currentStep, this.errorSummary, this.finalOutcome});
  factory _TripWorkflow.fromJson(Map<String, dynamic> json) => _$TripWorkflowFromJson(json);

@override final  String id;
@override final  String status;
@override final  String? currentStep;
@override final  String? errorSummary;
@override final  WorkflowOutcome? finalOutcome;

/// Create a copy of TripWorkflow
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripWorkflowCopyWith<_TripWorkflow> get copyWith => __$TripWorkflowCopyWithImpl<_TripWorkflow>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TripWorkflowToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripWorkflow&&(identical(other.id, id) || other.id == id)&&(identical(other.status, status) || other.status == status)&&(identical(other.currentStep, currentStep) || other.currentStep == currentStep)&&(identical(other.errorSummary, errorSummary) || other.errorSummary == errorSummary)&&(identical(other.finalOutcome, finalOutcome) || other.finalOutcome == finalOutcome));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,id,status,currentStep,errorSummary,finalOutcome);
}

@override
String toString() {
    return 'TripWorkflow(id: $id, status: $status, currentStep: $currentStep, errorSummary: $errorSummary, finalOutcome: $finalOutcome)';
}


}

/// @nodoc
abstract mixin class _$TripWorkflowCopyWith<$Res> implements $TripWorkflowCopyWith<$Res> {
  factory _$TripWorkflowCopyWith(_TripWorkflow value, $Res Function(_TripWorkflow) _then) = __$TripWorkflowCopyWithImpl;
@override @useResult
$Res call({
 String id, String status, String? currentStep, String? errorSummary, WorkflowOutcome? finalOutcome
});


@override $WorkflowOutcomeCopyWith<$Res>? get finalOutcome;

}
/// @nodoc
class __$TripWorkflowCopyWithImpl<$Res>
    implements _$TripWorkflowCopyWith<$Res> {
  __$TripWorkflowCopyWithImpl(this._self, this._then);

  final _TripWorkflow _self;
  final $Res Function(_TripWorkflow) _then;

/// Create a copy of TripWorkflow
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? id = null,Object? status = null,Object? currentStep = freezed,Object? errorSummary = freezed,Object? finalOutcome = freezed,}) {
  return _then(_TripWorkflow(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as String,currentStep: freezed == currentStep ? _self.currentStep : currentStep // ignore: cast_nullable_to_non_nullable
as String?,errorSummary: freezed == errorSummary ? _self.errorSummary : errorSummary // ignore: cast_nullable_to_non_nullable
as String?,finalOutcome: freezed == finalOutcome ? _self.finalOutcome : finalOutcome // ignore: cast_nullable_to_non_nullable
as WorkflowOutcome?,
  ));
}

/// Create a copy of TripWorkflow
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$WorkflowOutcomeCopyWith<$Res>? get finalOutcome {
    if (_self.finalOutcome == null) {
    return null;
  }

  return $WorkflowOutcomeCopyWith<$Res>(_self.finalOutcome!, (value) {
    return _then(_self.copyWith(finalOutcome: value));
  });
}
}


/// @nodoc
mixin _$WorkflowOutcome {

 ProposalSummary get proposal;
/// Create a copy of WorkflowOutcome
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$WorkflowOutcomeCopyWith<WorkflowOutcome> get copyWith => _$WorkflowOutcomeCopyWithImpl<WorkflowOutcome>(this as WorkflowOutcome, _$identity);

  /// Serializes this WorkflowOutcome to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as WorkflowOutcome;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is WorkflowOutcome&&(identical(other.proposal, _this.proposal) || other.proposal == _this.proposal));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as WorkflowOutcome;
  return Object.hash(runtimeType,_this.proposal);
}

@override
String toString() {
  final _this = this as WorkflowOutcome;
  return 'WorkflowOutcome(proposal: ${_this.proposal})';
}


}

/// @nodoc
abstract mixin class $WorkflowOutcomeCopyWith<$Res>  {
  factory $WorkflowOutcomeCopyWith(WorkflowOutcome value, $Res Function(WorkflowOutcome) _then) = _$WorkflowOutcomeCopyWithImpl;
@useResult
$Res call({
 ProposalSummary proposal
});


$ProposalSummaryCopyWith<$Res> get proposal;

}
/// @nodoc
class _$WorkflowOutcomeCopyWithImpl<$Res>
    implements $WorkflowOutcomeCopyWith<$Res> {
  _$WorkflowOutcomeCopyWithImpl(this._self, this._then);

  final WorkflowOutcome _self;
  final $Res Function(WorkflowOutcome) _then;

/// Create a copy of WorkflowOutcome
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? proposal = null,}) {
  return _then(WorkflowOutcome(
proposal: null == proposal ? _self.proposal : proposal // ignore: cast_nullable_to_non_nullable
as ProposalSummary,
  ));
}
/// Create a copy of WorkflowOutcome
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$ProposalSummaryCopyWith<$Res> get proposal {
  
  return $ProposalSummaryCopyWith<$Res>(_self.proposal, (value) {
    return _then(_self.copyWith(proposal: value));
  });
}
}


/// Adds pattern-matching-related methods to [WorkflowOutcome].
extension WorkflowOutcomePatterns on WorkflowOutcome {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _WorkflowOutcome value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _WorkflowOutcome() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _WorkflowOutcome value)  $default,){
final _that = this;
switch (_that) {
case _WorkflowOutcome():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _WorkflowOutcome value)?  $default,){
final _that = this;
switch (_that) {
case _WorkflowOutcome() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( ProposalSummary proposal)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _WorkflowOutcome() when $default != null:
return $default(_that.proposal);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( ProposalSummary proposal)  $default,) {final _that = this;
switch (_that) {
case _WorkflowOutcome():
return $default(_that.proposal);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( ProposalSummary proposal)?  $default,) {final _that = this;
switch (_that) {
case _WorkflowOutcome() when $default != null:
return $default(_that.proposal);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _WorkflowOutcome implements WorkflowOutcome {
  const _WorkflowOutcome({required this.proposal});
  factory _WorkflowOutcome.fromJson(Map<String, dynamic> json) => _$WorkflowOutcomeFromJson(json);

@override final  ProposalSummary proposal;

/// Create a copy of WorkflowOutcome
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$WorkflowOutcomeCopyWith<_WorkflowOutcome> get copyWith => __$WorkflowOutcomeCopyWithImpl<_WorkflowOutcome>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$WorkflowOutcomeToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _WorkflowOutcome&&(identical(other.proposal, proposal) || other.proposal == proposal));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,proposal);
}

@override
String toString() {
    return 'WorkflowOutcome(proposal: $proposal)';
}


}

/// @nodoc
abstract mixin class _$WorkflowOutcomeCopyWith<$Res> implements $WorkflowOutcomeCopyWith<$Res> {
  factory _$WorkflowOutcomeCopyWith(_WorkflowOutcome value, $Res Function(_WorkflowOutcome) _then) = __$WorkflowOutcomeCopyWithImpl;
@override @useResult
$Res call({
 ProposalSummary proposal
});


@override $ProposalSummaryCopyWith<$Res> get proposal;

}
/// @nodoc
class __$WorkflowOutcomeCopyWithImpl<$Res>
    implements _$WorkflowOutcomeCopyWith<$Res> {
  __$WorkflowOutcomeCopyWithImpl(this._self, this._then);

  final _WorkflowOutcome _self;
  final $Res Function(_WorkflowOutcome) _then;

/// Create a copy of WorkflowOutcome
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? proposal = null,}) {
  return _then(_WorkflowOutcome(
proposal: null == proposal ? _self.proposal : proposal // ignore: cast_nullable_to_non_nullable
as ProposalSummary,
  ));
}

/// Create a copy of WorkflowOutcome
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$ProposalSummaryCopyWith<$Res> get proposal {
  
  return $ProposalSummaryCopyWith<$Res>(_self.proposal, (value) {
    return _then(_self.copyWith(proposal: value));
  });
}
}


/// @nodoc
mixin _$ProposalSummary {

 List<ProposalDay>? get days;
/// Create a copy of ProposalSummary
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$ProposalSummaryCopyWith<ProposalSummary> get copyWith => _$ProposalSummaryCopyWithImpl<ProposalSummary>(this as ProposalSummary, _$identity);

  /// Serializes this ProposalSummary to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as ProposalSummary;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ProposalSummary&&const DeepCollectionEquality().equals(other.days, _this.days));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as ProposalSummary;
  return Object.hash(runtimeType,const DeepCollectionEquality().hash(_this.days));
}

@override
String toString() {
  final _this = this as ProposalSummary;
  return 'ProposalSummary(days: ${_this.days})';
}


}

/// @nodoc
abstract mixin class $ProposalSummaryCopyWith<$Res>  {
  factory $ProposalSummaryCopyWith(ProposalSummary value, $Res Function(ProposalSummary) _then) = _$ProposalSummaryCopyWithImpl;
@useResult
$Res call({
 List<ProposalDay>? days
});




}
/// @nodoc
class _$ProposalSummaryCopyWithImpl<$Res>
    implements $ProposalSummaryCopyWith<$Res> {
  _$ProposalSummaryCopyWithImpl(this._self, this._then);

  final ProposalSummary _self;
  final $Res Function(ProposalSummary) _then;

/// Create a copy of ProposalSummary
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? days = freezed,}) {
  return _then(ProposalSummary(
days: freezed == days ? _self.days : days // ignore: cast_nullable_to_non_nullable
as List<ProposalDay>?,
  ));
}

}


/// Adds pattern-matching-related methods to [ProposalSummary].
extension ProposalSummaryPatterns on ProposalSummary {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _ProposalSummary value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _ProposalSummary() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _ProposalSummary value)  $default,){
final _that = this;
switch (_that) {
case _ProposalSummary():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _ProposalSummary value)?  $default,){
final _that = this;
switch (_that) {
case _ProposalSummary() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( List<ProposalDay>? days)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _ProposalSummary() when $default != null:
return $default(_that.days);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( List<ProposalDay>? days)  $default,) {final _that = this;
switch (_that) {
case _ProposalSummary():
return $default(_that.days);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( List<ProposalDay>? days)?  $default,) {final _that = this;
switch (_that) {
case _ProposalSummary() when $default != null:
return $default(_that.days);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _ProposalSummary implements ProposalSummary {
  const _ProposalSummary({ List<ProposalDay>? days}): _days = days;
  factory _ProposalSummary.fromJson(Map<String, dynamic> json) => _$ProposalSummaryFromJson(json);

 final  List<ProposalDay>? _days;
@override List<ProposalDay>? get days {
  final value = _days;
  if (value == null) return null;
  if (_days is EqualUnmodifiableListView) return _days;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(value);
}


/// Create a copy of ProposalSummary
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$ProposalSummaryCopyWith<_ProposalSummary> get copyWith => __$ProposalSummaryCopyWithImpl<_ProposalSummary>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$ProposalSummaryToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _ProposalSummary&&const DeepCollectionEquality().equals(other.days, _days));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,const DeepCollectionEquality().hash(_days));
}

@override
String toString() {
    return 'ProposalSummary(days: $days)';
}


}

/// @nodoc
abstract mixin class _$ProposalSummaryCopyWith<$Res> implements $ProposalSummaryCopyWith<$Res> {
  factory _$ProposalSummaryCopyWith(_ProposalSummary value, $Res Function(_ProposalSummary) _then) = __$ProposalSummaryCopyWithImpl;
@override @useResult
$Res call({
 List<ProposalDay>? days
});




}
/// @nodoc
class __$ProposalSummaryCopyWithImpl<$Res>
    implements _$ProposalSummaryCopyWith<$Res> {
  __$ProposalSummaryCopyWithImpl(this._self, this._then);

  final _ProposalSummary _self;
  final $Res Function(_ProposalSummary) _then;

/// Create a copy of ProposalSummary
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? days = freezed,}) {
  return _then(_ProposalSummary(
days: freezed == days ? _self._days : days // ignore: cast_nullable_to_non_nullable
as List<ProposalDay>?,
  ));
}


}


/// @nodoc
mixin _$ProposalDay {

 int get day; String get date; String get city; List<ProposalStop>? get stops; String get transport; String? get weather;
/// Create a copy of ProposalDay
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$ProposalDayCopyWith<ProposalDay> get copyWith => _$ProposalDayCopyWithImpl<ProposalDay>(this as ProposalDay, _$identity);

  /// Serializes this ProposalDay to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as ProposalDay;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ProposalDay&&(identical(other.day, _this.day) || other.day == _this.day)&&(identical(other.date, _this.date) || other.date == _this.date)&&(identical(other.city, _this.city) || other.city == _this.city)&&const DeepCollectionEquality().equals(other.stops, _this.stops)&&(identical(other.transport, _this.transport) || other.transport == _this.transport)&&(identical(other.weather, _this.weather) || other.weather == _this.weather));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as ProposalDay;
  return Object.hash(runtimeType,_this.day,_this.date,_this.city,const DeepCollectionEquality().hash(_this.stops),_this.transport,_this.weather);
}

@override
String toString() {
  final _this = this as ProposalDay;
  return 'ProposalDay(day: ${_this.day}, date: ${_this.date}, city: ${_this.city}, stops: ${_this.stops}, transport: ${_this.transport}, weather: ${_this.weather})';
}


}

/// @nodoc
abstract mixin class $ProposalDayCopyWith<$Res>  {
  factory $ProposalDayCopyWith(ProposalDay value, $Res Function(ProposalDay) _then) = _$ProposalDayCopyWithImpl;
@useResult
$Res call({
 int day, String date, String city, List<ProposalStop>? stops, String transport, String? weather
});




}
/// @nodoc
class _$ProposalDayCopyWithImpl<$Res>
    implements $ProposalDayCopyWith<$Res> {
  _$ProposalDayCopyWithImpl(this._self, this._then);

  final ProposalDay _self;
  final $Res Function(ProposalDay) _then;

/// Create a copy of ProposalDay
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? day = null,Object? date = null,Object? city = null,Object? stops = freezed,Object? transport = null,Object? weather = freezed,}) {
  return _then(ProposalDay(
day: null == day ? _self.day : day // ignore: cast_nullable_to_non_nullable
as int,date: null == date ? _self.date : date // ignore: cast_nullable_to_non_nullable
as String,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,stops: freezed == stops ? _self.stops : stops // ignore: cast_nullable_to_non_nullable
as List<ProposalStop>?,transport: null == transport ? _self.transport : transport // ignore: cast_nullable_to_non_nullable
as String,weather: freezed == weather ? _self.weather : weather // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}

}


/// Adds pattern-matching-related methods to [ProposalDay].
extension ProposalDayPatterns on ProposalDay {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _ProposalDay value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _ProposalDay() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _ProposalDay value)  $default,){
final _that = this;
switch (_that) {
case _ProposalDay():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _ProposalDay value)?  $default,){
final _that = this;
switch (_that) {
case _ProposalDay() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( int day,  String date,  String city,  List<ProposalStop>? stops,  String transport,  String? weather)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _ProposalDay() when $default != null:
return $default(_that.day,_that.date,_that.city,_that.stops,_that.transport,_that.weather);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( int day,  String date,  String city,  List<ProposalStop>? stops,  String transport,  String? weather)  $default,) {final _that = this;
switch (_that) {
case _ProposalDay():
return $default(_that.day,_that.date,_that.city,_that.stops,_that.transport,_that.weather);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( int day,  String date,  String city,  List<ProposalStop>? stops,  String transport,  String? weather)?  $default,) {final _that = this;
switch (_that) {
case _ProposalDay() when $default != null:
return $default(_that.day,_that.date,_that.city,_that.stops,_that.transport,_that.weather);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _ProposalDay implements ProposalDay {
  const _ProposalDay({required this.day, required this.date, required this.city,  List<ProposalStop>? stops, required this.transport, this.weather}): _stops = stops;
  factory _ProposalDay.fromJson(Map<String, dynamic> json) => _$ProposalDayFromJson(json);

@override final  int day;
@override final  String date;
@override final  String city;
 final  List<ProposalStop>? _stops;
@override List<ProposalStop>? get stops {
  final value = _stops;
  if (value == null) return null;
  if (_stops is EqualUnmodifiableListView) return _stops;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(value);
}

@override final  String transport;
@override final  String? weather;

/// Create a copy of ProposalDay
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$ProposalDayCopyWith<_ProposalDay> get copyWith => __$ProposalDayCopyWithImpl<_ProposalDay>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$ProposalDayToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _ProposalDay&&(identical(other.day, day) || other.day == day)&&(identical(other.date, date) || other.date == date)&&(identical(other.city, city) || other.city == city)&&const DeepCollectionEquality().equals(other.stops, _stops)&&(identical(other.transport, transport) || other.transport == transport)&&(identical(other.weather, weather) || other.weather == weather));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,day,date,city,const DeepCollectionEquality().hash(_stops),transport,weather);
}

@override
String toString() {
    return 'ProposalDay(day: $day, date: $date, city: $city, stops: $stops, transport: $transport, weather: $weather)';
}


}

/// @nodoc
abstract mixin class _$ProposalDayCopyWith<$Res> implements $ProposalDayCopyWith<$Res> {
  factory _$ProposalDayCopyWith(_ProposalDay value, $Res Function(_ProposalDay) _then) = __$ProposalDayCopyWithImpl;
@override @useResult
$Res call({
 int day, String date, String city, List<ProposalStop>? stops, String transport, String? weather
});




}
/// @nodoc
class __$ProposalDayCopyWithImpl<$Res>
    implements _$ProposalDayCopyWith<$Res> {
  __$ProposalDayCopyWithImpl(this._self, this._then);

  final _ProposalDay _self;
  final $Res Function(_ProposalDay) _then;

/// Create a copy of ProposalDay
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? day = null,Object? date = null,Object? city = null,Object? stops = freezed,Object? transport = null,Object? weather = freezed,}) {
  return _then(_ProposalDay(
day: null == day ? _self.day : day // ignore: cast_nullable_to_non_nullable
as int,date: null == date ? _self.date : date // ignore: cast_nullable_to_non_nullable
as String,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,stops: freezed == stops ? _self._stops : stops // ignore: cast_nullable_to_non_nullable
as List<ProposalStop>?,transport: null == transport ? _self.transport : transport // ignore: cast_nullable_to_non_nullable
as String,weather: freezed == weather ? _self.weather : weather // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}


/// @nodoc
mixin _$ProposalStop {

@JsonKey(name: 'attraction_id') String get attractionId; String get name;
/// Create a copy of ProposalStop
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$ProposalStopCopyWith<ProposalStop> get copyWith => _$ProposalStopCopyWithImpl<ProposalStop>(this as ProposalStop, _$identity);

  /// Serializes this ProposalStop to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as ProposalStop;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ProposalStop&&(identical(other.attractionId, _this.attractionId) || other.attractionId == _this.attractionId)&&(identical(other.name, _this.name) || other.name == _this.name));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as ProposalStop;
  return Object.hash(runtimeType,_this.attractionId,_this.name);
}

@override
String toString() {
  final _this = this as ProposalStop;
  return 'ProposalStop(attractionId: ${_this.attractionId}, name: ${_this.name})';
}


}

/// @nodoc
abstract mixin class $ProposalStopCopyWith<$Res>  {
  factory $ProposalStopCopyWith(ProposalStop value, $Res Function(ProposalStop) _then) = _$ProposalStopCopyWithImpl;
@useResult
$Res call({
@JsonKey(name: 'attraction_id') String attractionId, String name
});




}
/// @nodoc
class _$ProposalStopCopyWithImpl<$Res>
    implements $ProposalStopCopyWith<$Res> {
  _$ProposalStopCopyWithImpl(this._self, this._then);

  final ProposalStop _self;
  final $Res Function(ProposalStop) _then;

/// Create a copy of ProposalStop
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? attractionId = null,Object? name = null,}) {
  return _then(ProposalStop(
attractionId: null == attractionId ? _self.attractionId : attractionId // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [ProposalStop].
extension ProposalStopPatterns on ProposalStop {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _ProposalStop value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _ProposalStop() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _ProposalStop value)  $default,){
final _that = this;
switch (_that) {
case _ProposalStop():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _ProposalStop value)?  $default,){
final _that = this;
switch (_that) {
case _ProposalStop() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function(@JsonKey(name: 'attraction_id')  String attractionId,  String name)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _ProposalStop() when $default != null:
return $default(_that.attractionId,_that.name);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function(@JsonKey(name: 'attraction_id')  String attractionId,  String name)  $default,) {final _that = this;
switch (_that) {
case _ProposalStop():
return $default(_that.attractionId,_that.name);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function(@JsonKey(name: 'attraction_id')  String attractionId,  String name)?  $default,) {final _that = this;
switch (_that) {
case _ProposalStop() when $default != null:
return $default(_that.attractionId,_that.name);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _ProposalStop implements ProposalStop {
  const _ProposalStop({@JsonKey(name: 'attraction_id') required this.attractionId, required this.name});
  factory _ProposalStop.fromJson(Map<String, dynamic> json) => _$ProposalStopFromJson(json);

@override@JsonKey(name: 'attraction_id') final  String attractionId;
@override final  String name;

/// Create a copy of ProposalStop
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$ProposalStopCopyWith<_ProposalStop> get copyWith => __$ProposalStopCopyWithImpl<_ProposalStop>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$ProposalStopToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _ProposalStop&&(identical(other.attractionId, attractionId) || other.attractionId == attractionId)&&(identical(other.name, name) || other.name == name));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,attractionId,name);
}

@override
String toString() {
    return 'ProposalStop(attractionId: $attractionId, name: $name)';
}


}

/// @nodoc
abstract mixin class _$ProposalStopCopyWith<$Res> implements $ProposalStopCopyWith<$Res> {
  factory _$ProposalStopCopyWith(_ProposalStop value, $Res Function(_ProposalStop) _then) = __$ProposalStopCopyWithImpl;
@override @useResult
$Res call({
@JsonKey(name: 'attraction_id') String attractionId, String name
});




}
/// @nodoc
class __$ProposalStopCopyWithImpl<$Res>
    implements _$ProposalStopCopyWith<$Res> {
  __$ProposalStopCopyWithImpl(this._self, this._then);

  final _ProposalStop _self;
  final $Res Function(_ProposalStop) _then;

/// Create a copy of ProposalStop
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? attractionId = null,Object? name = null,}) {
  return _then(_ProposalStop(
attractionId: null == attractionId ? _self.attractionId : attractionId // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}


/// @nodoc
mixin _$Itinerary {

 List<ItineraryDay> get days;
/// Create a copy of Itinerary
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$ItineraryCopyWith<Itinerary> get copyWith => _$ItineraryCopyWithImpl<Itinerary>(this as Itinerary, _$identity);

  /// Serializes this Itinerary to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as Itinerary;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is Itinerary&&const DeepCollectionEquality().equals(other.days, _this.days));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as Itinerary;
  return Object.hash(runtimeType,const DeepCollectionEquality().hash(_this.days));
}

@override
String toString() {
  final _this = this as Itinerary;
  return 'Itinerary(days: ${_this.days})';
}


}

/// @nodoc
abstract mixin class $ItineraryCopyWith<$Res>  {
  factory $ItineraryCopyWith(Itinerary value, $Res Function(Itinerary) _then) = _$ItineraryCopyWithImpl;
@useResult
$Res call({
 List<ItineraryDay> days
});




}
/// @nodoc
class _$ItineraryCopyWithImpl<$Res>
    implements $ItineraryCopyWith<$Res> {
  _$ItineraryCopyWithImpl(this._self, this._then);

  final Itinerary _self;
  final $Res Function(Itinerary) _then;

/// Create a copy of Itinerary
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? days = null,}) {
  return _then(Itinerary(
days: null == days ? _self.days : days // ignore: cast_nullable_to_non_nullable
as List<ItineraryDay>,
  ));
}

}


/// Adds pattern-matching-related methods to [Itinerary].
extension ItineraryPatterns on Itinerary {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _Itinerary value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Itinerary() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _Itinerary value)  $default,){
final _that = this;
switch (_that) {
case _Itinerary():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _Itinerary value)?  $default,){
final _that = this;
switch (_that) {
case _Itinerary() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( List<ItineraryDay> days)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Itinerary() when $default != null:
return $default(_that.days);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( List<ItineraryDay> days)  $default,) {final _that = this;
switch (_that) {
case _Itinerary():
return $default(_that.days);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( List<ItineraryDay> days)?  $default,) {final _that = this;
switch (_that) {
case _Itinerary() when $default != null:
return $default(_that.days);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _Itinerary implements Itinerary {
  const _Itinerary({required  List<ItineraryDay> days}): _days = days;
  factory _Itinerary.fromJson(Map<String, dynamic> json) => _$ItineraryFromJson(json);

 final  List<ItineraryDay> _days;
@override List<ItineraryDay> get days {
  if (_days is EqualUnmodifiableListView) return _days;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_days);
}


/// Create a copy of Itinerary
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$ItineraryCopyWith<_Itinerary> get copyWith => __$ItineraryCopyWithImpl<_Itinerary>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$ItineraryToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _Itinerary&&const DeepCollectionEquality().equals(other.days, _days));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,const DeepCollectionEquality().hash(_days));
}

@override
String toString() {
    return 'Itinerary(days: $days)';
}


}

/// @nodoc
abstract mixin class _$ItineraryCopyWith<$Res> implements $ItineraryCopyWith<$Res> {
  factory _$ItineraryCopyWith(_Itinerary value, $Res Function(_Itinerary) _then) = __$ItineraryCopyWithImpl;
@override @useResult
$Res call({
 List<ItineraryDay> days
});




}
/// @nodoc
class __$ItineraryCopyWithImpl<$Res>
    implements _$ItineraryCopyWith<$Res> {
  __$ItineraryCopyWithImpl(this._self, this._then);

  final _Itinerary _self;
  final $Res Function(_Itinerary) _then;

/// Create a copy of Itinerary
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? days = null,}) {
  return _then(_Itinerary(
days: null == days ? _self._days : days // ignore: cast_nullable_to_non_nullable
as List<ItineraryDay>,
  ));
}


}


/// @nodoc
mixin _$ItineraryDay {

 int get dayNumber; String get city; List<ItineraryStop> get stops;
/// Create a copy of ItineraryDay
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$ItineraryDayCopyWith<ItineraryDay> get copyWith => _$ItineraryDayCopyWithImpl<ItineraryDay>(this as ItineraryDay, _$identity);

  /// Serializes this ItineraryDay to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as ItineraryDay;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ItineraryDay&&(identical(other.dayNumber, _this.dayNumber) || other.dayNumber == _this.dayNumber)&&(identical(other.city, _this.city) || other.city == _this.city)&&const DeepCollectionEquality().equals(other.stops, _this.stops));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as ItineraryDay;
  return Object.hash(runtimeType,_this.dayNumber,_this.city,const DeepCollectionEquality().hash(_this.stops));
}

@override
String toString() {
  final _this = this as ItineraryDay;
  return 'ItineraryDay(dayNumber: ${_this.dayNumber}, city: ${_this.city}, stops: ${_this.stops})';
}


}

/// @nodoc
abstract mixin class $ItineraryDayCopyWith<$Res>  {
  factory $ItineraryDayCopyWith(ItineraryDay value, $Res Function(ItineraryDay) _then) = _$ItineraryDayCopyWithImpl;
@useResult
$Res call({
 int dayNumber, String city, List<ItineraryStop> stops
});




}
/// @nodoc
class _$ItineraryDayCopyWithImpl<$Res>
    implements $ItineraryDayCopyWith<$Res> {
  _$ItineraryDayCopyWithImpl(this._self, this._then);

  final ItineraryDay _self;
  final $Res Function(ItineraryDay) _then;

/// Create a copy of ItineraryDay
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? dayNumber = null,Object? city = null,Object? stops = null,}) {
  return _then(ItineraryDay(
dayNumber: null == dayNumber ? _self.dayNumber : dayNumber // ignore: cast_nullable_to_non_nullable
as int,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,stops: null == stops ? _self.stops : stops // ignore: cast_nullable_to_non_nullable
as List<ItineraryStop>,
  ));
}

}


/// Adds pattern-matching-related methods to [ItineraryDay].
extension ItineraryDayPatterns on ItineraryDay {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _ItineraryDay value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _ItineraryDay() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _ItineraryDay value)  $default,){
final _that = this;
switch (_that) {
case _ItineraryDay():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _ItineraryDay value)?  $default,){
final _that = this;
switch (_that) {
case _ItineraryDay() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( int dayNumber,  String city,  List<ItineraryStop> stops)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _ItineraryDay() when $default != null:
return $default(_that.dayNumber,_that.city,_that.stops);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( int dayNumber,  String city,  List<ItineraryStop> stops)  $default,) {final _that = this;
switch (_that) {
case _ItineraryDay():
return $default(_that.dayNumber,_that.city,_that.stops);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( int dayNumber,  String city,  List<ItineraryStop> stops)?  $default,) {final _that = this;
switch (_that) {
case _ItineraryDay() when $default != null:
return $default(_that.dayNumber,_that.city,_that.stops);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _ItineraryDay implements ItineraryDay {
  const _ItineraryDay({required this.dayNumber, required this.city, required  List<ItineraryStop> stops}): _stops = stops;
  factory _ItineraryDay.fromJson(Map<String, dynamic> json) => _$ItineraryDayFromJson(json);

@override final  int dayNumber;
@override final  String city;
 final  List<ItineraryStop> _stops;
@override List<ItineraryStop> get stops {
  if (_stops is EqualUnmodifiableListView) return _stops;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_stops);
}


/// Create a copy of ItineraryDay
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$ItineraryDayCopyWith<_ItineraryDay> get copyWith => __$ItineraryDayCopyWithImpl<_ItineraryDay>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$ItineraryDayToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _ItineraryDay&&(identical(other.dayNumber, dayNumber) || other.dayNumber == dayNumber)&&(identical(other.city, city) || other.city == city)&&const DeepCollectionEquality().equals(other.stops, _stops));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,dayNumber,city,const DeepCollectionEquality().hash(_stops));
}

@override
String toString() {
    return 'ItineraryDay(dayNumber: $dayNumber, city: $city, stops: $stops)';
}


}

/// @nodoc
abstract mixin class _$ItineraryDayCopyWith<$Res> implements $ItineraryDayCopyWith<$Res> {
  factory _$ItineraryDayCopyWith(_ItineraryDay value, $Res Function(_ItineraryDay) _then) = __$ItineraryDayCopyWithImpl;
@override @useResult
$Res call({
 int dayNumber, String city, List<ItineraryStop> stops
});




}
/// @nodoc
class __$ItineraryDayCopyWithImpl<$Res>
    implements _$ItineraryDayCopyWith<$Res> {
  __$ItineraryDayCopyWithImpl(this._self, this._then);

  final _ItineraryDay _self;
  final $Res Function(_ItineraryDay) _then;

/// Create a copy of ItineraryDay
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? dayNumber = null,Object? city = null,Object? stops = null,}) {
  return _then(_ItineraryDay(
dayNumber: null == dayNumber ? _self.dayNumber : dayNumber // ignore: cast_nullable_to_non_nullable
as int,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,stops: null == stops ? _self._stops : stops // ignore: cast_nullable_to_non_nullable
as List<ItineraryStop>,
  ));
}


}


/// @nodoc
mixin _$ItineraryStop {

 String get attractionId; String get attractionName;
/// Create a copy of ItineraryStop
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$ItineraryStopCopyWith<ItineraryStop> get copyWith => _$ItineraryStopCopyWithImpl<ItineraryStop>(this as ItineraryStop, _$identity);

  /// Serializes this ItineraryStop to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as ItineraryStop;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is ItineraryStop&&(identical(other.attractionId, _this.attractionId) || other.attractionId == _this.attractionId)&&(identical(other.attractionName, _this.attractionName) || other.attractionName == _this.attractionName));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as ItineraryStop;
  return Object.hash(runtimeType,_this.attractionId,_this.attractionName);
}

@override
String toString() {
  final _this = this as ItineraryStop;
  return 'ItineraryStop(attractionId: ${_this.attractionId}, attractionName: ${_this.attractionName})';
}


}

/// @nodoc
abstract mixin class $ItineraryStopCopyWith<$Res>  {
  factory $ItineraryStopCopyWith(ItineraryStop value, $Res Function(ItineraryStop) _then) = _$ItineraryStopCopyWithImpl;
@useResult
$Res call({
 String attractionId, String attractionName
});




}
/// @nodoc
class _$ItineraryStopCopyWithImpl<$Res>
    implements $ItineraryStopCopyWith<$Res> {
  _$ItineraryStopCopyWithImpl(this._self, this._then);

  final ItineraryStop _self;
  final $Res Function(ItineraryStop) _then;

/// Create a copy of ItineraryStop
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? attractionId = null,Object? attractionName = null,}) {
  return _then(ItineraryStop(
attractionId: null == attractionId ? _self.attractionId : attractionId // ignore: cast_nullable_to_non_nullable
as String,attractionName: null == attractionName ? _self.attractionName : attractionName // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [ItineraryStop].
extension ItineraryStopPatterns on ItineraryStop {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _ItineraryStop value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _ItineraryStop() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _ItineraryStop value)  $default,){
final _that = this;
switch (_that) {
case _ItineraryStop():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _ItineraryStop value)?  $default,){
final _that = this;
switch (_that) {
case _ItineraryStop() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String attractionId,  String attractionName)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _ItineraryStop() when $default != null:
return $default(_that.attractionId,_that.attractionName);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String attractionId,  String attractionName)  $default,) {final _that = this;
switch (_that) {
case _ItineraryStop():
return $default(_that.attractionId,_that.attractionName);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String attractionId,  String attractionName)?  $default,) {final _that = this;
switch (_that) {
case _ItineraryStop() when $default != null:
return $default(_that.attractionId,_that.attractionName);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _ItineraryStop implements ItineraryStop {
  const _ItineraryStop({required this.attractionId, required this.attractionName});
  factory _ItineraryStop.fromJson(Map<String, dynamic> json) => _$ItineraryStopFromJson(json);

@override final  String attractionId;
@override final  String attractionName;

/// Create a copy of ItineraryStop
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$ItineraryStopCopyWith<_ItineraryStop> get copyWith => __$ItineraryStopCopyWithImpl<_ItineraryStop>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$ItineraryStopToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _ItineraryStop&&(identical(other.attractionId, attractionId) || other.attractionId == attractionId)&&(identical(other.attractionName, attractionName) || other.attractionName == attractionName));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,attractionId,attractionName);
}

@override
String toString() {
    return 'ItineraryStop(attractionId: $attractionId, attractionName: $attractionName)';
}


}

/// @nodoc
abstract mixin class _$ItineraryStopCopyWith<$Res> implements $ItineraryStopCopyWith<$Res> {
  factory _$ItineraryStopCopyWith(_ItineraryStop value, $Res Function(_ItineraryStop) _then) = __$ItineraryStopCopyWithImpl;
@override @useResult
$Res call({
 String attractionId, String attractionName
});




}
/// @nodoc
class __$ItineraryStopCopyWithImpl<$Res>
    implements _$ItineraryStopCopyWith<$Res> {
  __$ItineraryStopCopyWithImpl(this._self, this._then);

  final _ItineraryStop _self;
  final $Res Function(_ItineraryStop) _then;

/// Create a copy of ItineraryStop
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? attractionId = null,Object? attractionName = null,}) {
  return _then(_ItineraryStop(
attractionId: null == attractionId ? _self.attractionId : attractionId // ignore: cast_nullable_to_non_nullable
as String,attractionName: null == attractionName ? _self.attractionName : attractionName // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}


/// @nodoc
mixin _$TripHistoryEntry {

 String get at; String get action; String get actor; String? get fromStatus; String? get toStatus; String? get reason;
/// Create a copy of TripHistoryEntry
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripHistoryEntryCopyWith<TripHistoryEntry> get copyWith => _$TripHistoryEntryCopyWithImpl<TripHistoryEntry>(this as TripHistoryEntry, _$identity);

  /// Serializes this TripHistoryEntry to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TripHistoryEntry;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripHistoryEntry&&(identical(other.at, _this.at) || other.at == _this.at)&&(identical(other.action, _this.action) || other.action == _this.action)&&(identical(other.actor, _this.actor) || other.actor == _this.actor)&&(identical(other.fromStatus, _this.fromStatus) || other.fromStatus == _this.fromStatus)&&(identical(other.toStatus, _this.toStatus) || other.toStatus == _this.toStatus)&&(identical(other.reason, _this.reason) || other.reason == _this.reason));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TripHistoryEntry;
  return Object.hash(runtimeType,_this.at,_this.action,_this.actor,_this.fromStatus,_this.toStatus,_this.reason);
}

@override
String toString() {
  final _this = this as TripHistoryEntry;
  return 'TripHistoryEntry(at: ${_this.at}, action: ${_this.action}, actor: ${_this.actor}, fromStatus: ${_this.fromStatus}, toStatus: ${_this.toStatus}, reason: ${_this.reason})';
}


}

/// @nodoc
abstract mixin class $TripHistoryEntryCopyWith<$Res>  {
  factory $TripHistoryEntryCopyWith(TripHistoryEntry value, $Res Function(TripHistoryEntry) _then) = _$TripHistoryEntryCopyWithImpl;
@useResult
$Res call({
 String at, String action, String actor, String? fromStatus, String? toStatus, String? reason
});




}
/// @nodoc
class _$TripHistoryEntryCopyWithImpl<$Res>
    implements $TripHistoryEntryCopyWith<$Res> {
  _$TripHistoryEntryCopyWithImpl(this._self, this._then);

  final TripHistoryEntry _self;
  final $Res Function(TripHistoryEntry) _then;

/// Create a copy of TripHistoryEntry
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? at = null,Object? action = null,Object? actor = null,Object? fromStatus = freezed,Object? toStatus = freezed,Object? reason = freezed,}) {
  return _then(TripHistoryEntry(
at: null == at ? _self.at : at // ignore: cast_nullable_to_non_nullable
as String,action: null == action ? _self.action : action // ignore: cast_nullable_to_non_nullable
as String,actor: null == actor ? _self.actor : actor // ignore: cast_nullable_to_non_nullable
as String,fromStatus: freezed == fromStatus ? _self.fromStatus : fromStatus // ignore: cast_nullable_to_non_nullable
as String?,toStatus: freezed == toStatus ? _self.toStatus : toStatus // ignore: cast_nullable_to_non_nullable
as String?,reason: freezed == reason ? _self.reason : reason // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}

}


/// Adds pattern-matching-related methods to [TripHistoryEntry].
extension TripHistoryEntryPatterns on TripHistoryEntry {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripHistoryEntry value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripHistoryEntry() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripHistoryEntry value)  $default,){
final _that = this;
switch (_that) {
case _TripHistoryEntry():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripHistoryEntry value)?  $default,){
final _that = this;
switch (_that) {
case _TripHistoryEntry() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String at,  String action,  String actor,  String? fromStatus,  String? toStatus,  String? reason)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripHistoryEntry() when $default != null:
return $default(_that.at,_that.action,_that.actor,_that.fromStatus,_that.toStatus,_that.reason);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String at,  String action,  String actor,  String? fromStatus,  String? toStatus,  String? reason)  $default,) {final _that = this;
switch (_that) {
case _TripHistoryEntry():
return $default(_that.at,_that.action,_that.actor,_that.fromStatus,_that.toStatus,_that.reason);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String at,  String action,  String actor,  String? fromStatus,  String? toStatus,  String? reason)?  $default,) {final _that = this;
switch (_that) {
case _TripHistoryEntry() when $default != null:
return $default(_that.at,_that.action,_that.actor,_that.fromStatus,_that.toStatus,_that.reason);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TripHistoryEntry implements TripHistoryEntry {
  const _TripHistoryEntry({required this.at, required this.action, required this.actor, this.fromStatus, this.toStatus, this.reason});
  factory _TripHistoryEntry.fromJson(Map<String, dynamic> json) => _$TripHistoryEntryFromJson(json);

@override final  String at;
@override final  String action;
@override final  String actor;
@override final  String? fromStatus;
@override final  String? toStatus;
@override final  String? reason;

/// Create a copy of TripHistoryEntry
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripHistoryEntryCopyWith<_TripHistoryEntry> get copyWith => __$TripHistoryEntryCopyWithImpl<_TripHistoryEntry>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TripHistoryEntryToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripHistoryEntry&&(identical(other.at, at) || other.at == at)&&(identical(other.action, action) || other.action == action)&&(identical(other.actor, actor) || other.actor == actor)&&(identical(other.fromStatus, fromStatus) || other.fromStatus == fromStatus)&&(identical(other.toStatus, toStatus) || other.toStatus == toStatus)&&(identical(other.reason, reason) || other.reason == reason));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,at,action,actor,fromStatus,toStatus,reason);
}

@override
String toString() {
    return 'TripHistoryEntry(at: $at, action: $action, actor: $actor, fromStatus: $fromStatus, toStatus: $toStatus, reason: $reason)';
}


}

/// @nodoc
abstract mixin class _$TripHistoryEntryCopyWith<$Res> implements $TripHistoryEntryCopyWith<$Res> {
  factory _$TripHistoryEntryCopyWith(_TripHistoryEntry value, $Res Function(_TripHistoryEntry) _then) = __$TripHistoryEntryCopyWithImpl;
@override @useResult
$Res call({
 String at, String action, String actor, String? fromStatus, String? toStatus, String? reason
});




}
/// @nodoc
class __$TripHistoryEntryCopyWithImpl<$Res>
    implements _$TripHistoryEntryCopyWith<$Res> {
  __$TripHistoryEntryCopyWithImpl(this._self, this._then);

  final _TripHistoryEntry _self;
  final $Res Function(_TripHistoryEntry) _then;

/// Create a copy of TripHistoryEntry
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? at = null,Object? action = null,Object? actor = null,Object? fromStatus = freezed,Object? toStatus = freezed,Object? reason = freezed,}) {
  return _then(_TripHistoryEntry(
at: null == at ? _self.at : at // ignore: cast_nullable_to_non_nullable
as String,action: null == action ? _self.action : action // ignore: cast_nullable_to_non_nullable
as String,actor: null == actor ? _self.actor : actor // ignore: cast_nullable_to_non_nullable
as String,fromStatus: freezed == fromStatus ? _self.fromStatus : fromStatus // ignore: cast_nullable_to_non_nullable
as String?,toStatus: freezed == toStatus ? _self.toStatus : toStatus // ignore: cast_nullable_to_non_nullable
as String?,reason: freezed == reason ? _self.reason : reason // ignore: cast_nullable_to_non_nullable
as String?,
  ));
}


}


/// @nodoc
mixin _$CancellationInfo {

 bool get canCancel; String get cancelUntil; int get cutoffDays; String? get closedReason; String get operatorContact;
/// Create a copy of CancellationInfo
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$CancellationInfoCopyWith<CancellationInfo> get copyWith => _$CancellationInfoCopyWithImpl<CancellationInfo>(this as CancellationInfo, _$identity);

  /// Serializes this CancellationInfo to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as CancellationInfo;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is CancellationInfo&&(identical(other.canCancel, _this.canCancel) || other.canCancel == _this.canCancel)&&(identical(other.cancelUntil, _this.cancelUntil) || other.cancelUntil == _this.cancelUntil)&&(identical(other.cutoffDays, _this.cutoffDays) || other.cutoffDays == _this.cutoffDays)&&(identical(other.closedReason, _this.closedReason) || other.closedReason == _this.closedReason)&&(identical(other.operatorContact, _this.operatorContact) || other.operatorContact == _this.operatorContact));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as CancellationInfo;
  return Object.hash(runtimeType,_this.canCancel,_this.cancelUntil,_this.cutoffDays,_this.closedReason,_this.operatorContact);
}

@override
String toString() {
  final _this = this as CancellationInfo;
  return 'CancellationInfo(canCancel: ${_this.canCancel}, cancelUntil: ${_this.cancelUntil}, cutoffDays: ${_this.cutoffDays}, closedReason: ${_this.closedReason}, operatorContact: ${_this.operatorContact})';
}


}

/// @nodoc
abstract mixin class $CancellationInfoCopyWith<$Res>  {
  factory $CancellationInfoCopyWith(CancellationInfo value, $Res Function(CancellationInfo) _then) = _$CancellationInfoCopyWithImpl;
@useResult
$Res call({
 bool canCancel, String cancelUntil, int cutoffDays, String? closedReason, String operatorContact
});




}
/// @nodoc
class _$CancellationInfoCopyWithImpl<$Res>
    implements $CancellationInfoCopyWith<$Res> {
  _$CancellationInfoCopyWithImpl(this._self, this._then);

  final CancellationInfo _self;
  final $Res Function(CancellationInfo) _then;

/// Create a copy of CancellationInfo
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? canCancel = null,Object? cancelUntil = null,Object? cutoffDays = null,Object? closedReason = freezed,Object? operatorContact = null,}) {
  return _then(CancellationInfo(
canCancel: null == canCancel ? _self.canCancel : canCancel // ignore: cast_nullable_to_non_nullable
as bool,cancelUntil: null == cancelUntil ? _self.cancelUntil : cancelUntil // ignore: cast_nullable_to_non_nullable
as String,cutoffDays: null == cutoffDays ? _self.cutoffDays : cutoffDays // ignore: cast_nullable_to_non_nullable
as int,closedReason: freezed == closedReason ? _self.closedReason : closedReason // ignore: cast_nullable_to_non_nullable
as String?,operatorContact: null == operatorContact ? _self.operatorContact : operatorContact // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [CancellationInfo].
extension CancellationInfoPatterns on CancellationInfo {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _CancellationInfo value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _CancellationInfo() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _CancellationInfo value)  $default,){
final _that = this;
switch (_that) {
case _CancellationInfo():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _CancellationInfo value)?  $default,){
final _that = this;
switch (_that) {
case _CancellationInfo() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( bool canCancel,  String cancelUntil,  int cutoffDays,  String? closedReason,  String operatorContact)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _CancellationInfo() when $default != null:
return $default(_that.canCancel,_that.cancelUntil,_that.cutoffDays,_that.closedReason,_that.operatorContact);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( bool canCancel,  String cancelUntil,  int cutoffDays,  String? closedReason,  String operatorContact)  $default,) {final _that = this;
switch (_that) {
case _CancellationInfo():
return $default(_that.canCancel,_that.cancelUntil,_that.cutoffDays,_that.closedReason,_that.operatorContact);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( bool canCancel,  String cancelUntil,  int cutoffDays,  String? closedReason,  String operatorContact)?  $default,) {final _that = this;
switch (_that) {
case _CancellationInfo() when $default != null:
return $default(_that.canCancel,_that.cancelUntil,_that.cutoffDays,_that.closedReason,_that.operatorContact);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _CancellationInfo implements CancellationInfo {
  const _CancellationInfo({required this.canCancel, required this.cancelUntil, required this.cutoffDays, this.closedReason, required this.operatorContact});
  factory _CancellationInfo.fromJson(Map<String, dynamic> json) => _$CancellationInfoFromJson(json);

@override final  bool canCancel;
@override final  String cancelUntil;
@override final  int cutoffDays;
@override final  String? closedReason;
@override final  String operatorContact;

/// Create a copy of CancellationInfo
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$CancellationInfoCopyWith<_CancellationInfo> get copyWith => __$CancellationInfoCopyWithImpl<_CancellationInfo>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$CancellationInfoToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _CancellationInfo&&(identical(other.canCancel, canCancel) || other.canCancel == canCancel)&&(identical(other.cancelUntil, cancelUntil) || other.cancelUntil == cancelUntil)&&(identical(other.cutoffDays, cutoffDays) || other.cutoffDays == cutoffDays)&&(identical(other.closedReason, closedReason) || other.closedReason == closedReason)&&(identical(other.operatorContact, operatorContact) || other.operatorContact == operatorContact));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,canCancel,cancelUntil,cutoffDays,closedReason,operatorContact);
}

@override
String toString() {
    return 'CancellationInfo(canCancel: $canCancel, cancelUntil: $cancelUntil, cutoffDays: $cutoffDays, closedReason: $closedReason, operatorContact: $operatorContact)';
}


}

/// @nodoc
abstract mixin class _$CancellationInfoCopyWith<$Res> implements $CancellationInfoCopyWith<$Res> {
  factory _$CancellationInfoCopyWith(_CancellationInfo value, $Res Function(_CancellationInfo) _then) = __$CancellationInfoCopyWithImpl;
@override @useResult
$Res call({
 bool canCancel, String cancelUntil, int cutoffDays, String? closedReason, String operatorContact
});




}
/// @nodoc
class __$CancellationInfoCopyWithImpl<$Res>
    implements _$CancellationInfoCopyWith<$Res> {
  __$CancellationInfoCopyWithImpl(this._self, this._then);

  final _CancellationInfo _self;
  final $Res Function(_CancellationInfo) _then;

/// Create a copy of CancellationInfo
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? canCancel = null,Object? cancelUntil = null,Object? cutoffDays = null,Object? closedReason = freezed,Object? operatorContact = null,}) {
  return _then(_CancellationInfo(
canCancel: null == canCancel ? _self.canCancel : canCancel // ignore: cast_nullable_to_non_nullable
as bool,cancelUntil: null == cancelUntil ? _self.cancelUntil : cancelUntil // ignore: cast_nullable_to_non_nullable
as String,cutoffDays: null == cutoffDays ? _self.cutoffDays : cutoffDays // ignore: cast_nullable_to_non_nullable
as int,closedReason: freezed == closedReason ? _self.closedReason : closedReason // ignore: cast_nullable_to_non_nullable
as String?,operatorContact: null == operatorContact ? _self.operatorContact : operatorContact // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}


/// @nodoc
mixin _$TripVoucher {

 String get id; String get type; String? get hotelId; String? get hotelName; String? get night; int get rooms; String get code; String get qrPayload;
/// Create a copy of TripVoucher
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripVoucherCopyWith<TripVoucher> get copyWith => _$TripVoucherCopyWithImpl<TripVoucher>(this as TripVoucher, _$identity);

  /// Serializes this TripVoucher to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TripVoucher;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripVoucher&&(identical(other.id, _this.id) || other.id == _this.id)&&(identical(other.type, _this.type) || other.type == _this.type)&&(identical(other.hotelId, _this.hotelId) || other.hotelId == _this.hotelId)&&(identical(other.hotelName, _this.hotelName) || other.hotelName == _this.hotelName)&&(identical(other.night, _this.night) || other.night == _this.night)&&(identical(other.rooms, _this.rooms) || other.rooms == _this.rooms)&&(identical(other.code, _this.code) || other.code == _this.code)&&(identical(other.qrPayload, _this.qrPayload) || other.qrPayload == _this.qrPayload));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TripVoucher;
  return Object.hash(runtimeType,_this.id,_this.type,_this.hotelId,_this.hotelName,_this.night,_this.rooms,_this.code,_this.qrPayload);
}

@override
String toString() {
  final _this = this as TripVoucher;
  return 'TripVoucher(id: ${_this.id}, type: ${_this.type}, hotelId: ${_this.hotelId}, hotelName: ${_this.hotelName}, night: ${_this.night}, rooms: ${_this.rooms}, code: ${_this.code}, qrPayload: ${_this.qrPayload})';
}


}

/// @nodoc
abstract mixin class $TripVoucherCopyWith<$Res>  {
  factory $TripVoucherCopyWith(TripVoucher value, $Res Function(TripVoucher) _then) = _$TripVoucherCopyWithImpl;
@useResult
$Res call({
 String id, String type, String? hotelId, String? hotelName, String? night, int rooms, String code, String qrPayload
});




}
/// @nodoc
class _$TripVoucherCopyWithImpl<$Res>
    implements $TripVoucherCopyWith<$Res> {
  _$TripVoucherCopyWithImpl(this._self, this._then);

  final TripVoucher _self;
  final $Res Function(TripVoucher) _then;

/// Create a copy of TripVoucher
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? id = null,Object? type = null,Object? hotelId = freezed,Object? hotelName = freezed,Object? night = freezed,Object? rooms = null,Object? code = null,Object? qrPayload = null,}) {
  return _then(TripVoucher(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,type: null == type ? _self.type : type // ignore: cast_nullable_to_non_nullable
as String,hotelId: freezed == hotelId ? _self.hotelId : hotelId // ignore: cast_nullable_to_non_nullable
as String?,hotelName: freezed == hotelName ? _self.hotelName : hotelName // ignore: cast_nullable_to_non_nullable
as String?,night: freezed == night ? _self.night : night // ignore: cast_nullable_to_non_nullable
as String?,rooms: null == rooms ? _self.rooms : rooms // ignore: cast_nullable_to_non_nullable
as int,code: null == code ? _self.code : code // ignore: cast_nullable_to_non_nullable
as String,qrPayload: null == qrPayload ? _self.qrPayload : qrPayload // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [TripVoucher].
extension TripVoucherPatterns on TripVoucher {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripVoucher value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripVoucher() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripVoucher value)  $default,){
final _that = this;
switch (_that) {
case _TripVoucher():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripVoucher value)?  $default,){
final _that = this;
switch (_that) {
case _TripVoucher() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String id,  String type,  String? hotelId,  String? hotelName,  String? night,  int rooms,  String code,  String qrPayload)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripVoucher() when $default != null:
return $default(_that.id,_that.type,_that.hotelId,_that.hotelName,_that.night,_that.rooms,_that.code,_that.qrPayload);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String id,  String type,  String? hotelId,  String? hotelName,  String? night,  int rooms,  String code,  String qrPayload)  $default,) {final _that = this;
switch (_that) {
case _TripVoucher():
return $default(_that.id,_that.type,_that.hotelId,_that.hotelName,_that.night,_that.rooms,_that.code,_that.qrPayload);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String id,  String type,  String? hotelId,  String? hotelName,  String? night,  int rooms,  String code,  String qrPayload)?  $default,) {final _that = this;
switch (_that) {
case _TripVoucher() when $default != null:
return $default(_that.id,_that.type,_that.hotelId,_that.hotelName,_that.night,_that.rooms,_that.code,_that.qrPayload);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TripVoucher implements TripVoucher {
  const _TripVoucher({required this.id, required this.type, this.hotelId, this.hotelName, this.night, required this.rooms, required this.code, required this.qrPayload});
  factory _TripVoucher.fromJson(Map<String, dynamic> json) => _$TripVoucherFromJson(json);

@override final  String id;
@override final  String type;
@override final  String? hotelId;
@override final  String? hotelName;
@override final  String? night;
@override final  int rooms;
@override final  String code;
@override final  String qrPayload;

/// Create a copy of TripVoucher
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripVoucherCopyWith<_TripVoucher> get copyWith => __$TripVoucherCopyWithImpl<_TripVoucher>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TripVoucherToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripVoucher&&(identical(other.id, id) || other.id == id)&&(identical(other.type, type) || other.type == type)&&(identical(other.hotelId, hotelId) || other.hotelId == hotelId)&&(identical(other.hotelName, hotelName) || other.hotelName == hotelName)&&(identical(other.night, night) || other.night == night)&&(identical(other.rooms, rooms) || other.rooms == rooms)&&(identical(other.code, code) || other.code == code)&&(identical(other.qrPayload, qrPayload) || other.qrPayload == qrPayload));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,id,type,hotelId,hotelName,night,rooms,code,qrPayload);
}

@override
String toString() {
    return 'TripVoucher(id: $id, type: $type, hotelId: $hotelId, hotelName: $hotelName, night: $night, rooms: $rooms, code: $code, qrPayload: $qrPayload)';
}


}

/// @nodoc
abstract mixin class _$TripVoucherCopyWith<$Res> implements $TripVoucherCopyWith<$Res> {
  factory _$TripVoucherCopyWith(_TripVoucher value, $Res Function(_TripVoucher) _then) = __$TripVoucherCopyWithImpl;
@override @useResult
$Res call({
 String id, String type, String? hotelId, String? hotelName, String? night, int rooms, String code, String qrPayload
});




}
/// @nodoc
class __$TripVoucherCopyWithImpl<$Res>
    implements _$TripVoucherCopyWith<$Res> {
  __$TripVoucherCopyWithImpl(this._self, this._then);

  final _TripVoucher _self;
  final $Res Function(_TripVoucher) _then;

/// Create a copy of TripVoucher
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? id = null,Object? type = null,Object? hotelId = freezed,Object? hotelName = freezed,Object? night = freezed,Object? rooms = null,Object? code = null,Object? qrPayload = null,}) {
  return _then(_TripVoucher(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,type: null == type ? _self.type : type // ignore: cast_nullable_to_non_nullable
as String,hotelId: freezed == hotelId ? _self.hotelId : hotelId // ignore: cast_nullable_to_non_nullable
as String?,hotelName: freezed == hotelName ? _self.hotelName : hotelName // ignore: cast_nullable_to_non_nullable
as String?,night: freezed == night ? _self.night : night // ignore: cast_nullable_to_non_nullable
as String?,rooms: null == rooms ? _self.rooms : rooms // ignore: cast_nullable_to_non_nullable
as int,code: null == code ? _self.code : code // ignore: cast_nullable_to_non_nullable
as String,qrPayload: null == qrPayload ? _self.qrPayload : qrPayload // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}

// dart format on
