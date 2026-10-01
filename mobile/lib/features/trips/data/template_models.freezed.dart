// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint, type=warning, deprecated_member_use, deprecated_member_use_from_same_package
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'template_models.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$TripTemplate {

 String get id; String get slug; String get name; String get moodTag; String get summary; String get objective; int get days; List<String> get cities; Map<String, dynamic> get preferences; int get pricedForPax; double get fromPriceLkr; double get fromPriceUsd; List<TemplateDay>? get itinerary;
/// Create a copy of TripTemplate
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripTemplateCopyWith<TripTemplate> get copyWith => _$TripTemplateCopyWithImpl<TripTemplate>(this as TripTemplate, _$identity);

  /// Serializes this TripTemplate to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TripTemplate;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripTemplate&&(identical(other.id, _this.id) || other.id == _this.id)&&(identical(other.slug, _this.slug) || other.slug == _this.slug)&&(identical(other.name, _this.name) || other.name == _this.name)&&(identical(other.moodTag, _this.moodTag) || other.moodTag == _this.moodTag)&&(identical(other.summary, _this.summary) || other.summary == _this.summary)&&(identical(other.objective, _this.objective) || other.objective == _this.objective)&&(identical(other.days, _this.days) || other.days == _this.days)&&const DeepCollectionEquality().equals(other.cities, _this.cities)&&const DeepCollectionEquality().equals(other.preferences, _this.preferences)&&(identical(other.pricedForPax, _this.pricedForPax) || other.pricedForPax == _this.pricedForPax)&&(identical(other.fromPriceLkr, _this.fromPriceLkr) || other.fromPriceLkr == _this.fromPriceLkr)&&(identical(other.fromPriceUsd, _this.fromPriceUsd) || other.fromPriceUsd == _this.fromPriceUsd)&&const DeepCollectionEquality().equals(other.itinerary, _this.itinerary));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TripTemplate;
  return Object.hash(runtimeType,_this.id,_this.slug,_this.name,_this.moodTag,_this.summary,_this.objective,_this.days,const DeepCollectionEquality().hash(_this.cities),const DeepCollectionEquality().hash(_this.preferences),_this.pricedForPax,_this.fromPriceLkr,_this.fromPriceUsd,const DeepCollectionEquality().hash(_this.itinerary));
}

@override
String toString() {
  final _this = this as TripTemplate;
  return 'TripTemplate(id: ${_this.id}, slug: ${_this.slug}, name: ${_this.name}, moodTag: ${_this.moodTag}, summary: ${_this.summary}, objective: ${_this.objective}, days: ${_this.days}, cities: ${_this.cities}, preferences: ${_this.preferences}, pricedForPax: ${_this.pricedForPax}, fromPriceLkr: ${_this.fromPriceLkr}, fromPriceUsd: ${_this.fromPriceUsd}, itinerary: ${_this.itinerary})';
}


}

/// @nodoc
abstract mixin class $TripTemplateCopyWith<$Res>  {
  factory $TripTemplateCopyWith(TripTemplate value, $Res Function(TripTemplate) _then) = _$TripTemplateCopyWithImpl;
@useResult
$Res call({
 String id, String slug, String name, String moodTag, String summary, String objective, int days, List<String> cities, Map<String, dynamic> preferences, int pricedForPax, double fromPriceLkr, double fromPriceUsd, List<TemplateDay>? itinerary
});




}
/// @nodoc
class _$TripTemplateCopyWithImpl<$Res>
    implements $TripTemplateCopyWith<$Res> {
  _$TripTemplateCopyWithImpl(this._self, this._then);

  final TripTemplate _self;
  final $Res Function(TripTemplate) _then;

/// Create a copy of TripTemplate
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? id = null,Object? slug = null,Object? name = null,Object? moodTag = null,Object? summary = null,Object? objective = null,Object? days = null,Object? cities = null,Object? preferences = null,Object? pricedForPax = null,Object? fromPriceLkr = null,Object? fromPriceUsd = null,Object? itinerary = freezed,}) {
  return _then(TripTemplate(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,slug: null == slug ? _self.slug : slug // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,moodTag: null == moodTag ? _self.moodTag : moodTag // ignore: cast_nullable_to_non_nullable
as String,summary: null == summary ? _self.summary : summary // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,days: null == days ? _self.days : days // ignore: cast_nullable_to_non_nullable
as int,cities: null == cities ? _self.cities : cities // ignore: cast_nullable_to_non_nullable
as List<String>,preferences: null == preferences ? _self.preferences : preferences // ignore: cast_nullable_to_non_nullable
as Map<String, dynamic>,pricedForPax: null == pricedForPax ? _self.pricedForPax : pricedForPax // ignore: cast_nullable_to_non_nullable
as int,fromPriceLkr: null == fromPriceLkr ? _self.fromPriceLkr : fromPriceLkr // ignore: cast_nullable_to_non_nullable
as double,fromPriceUsd: null == fromPriceUsd ? _self.fromPriceUsd : fromPriceUsd // ignore: cast_nullable_to_non_nullable
as double,itinerary: freezed == itinerary ? _self.itinerary : itinerary // ignore: cast_nullable_to_non_nullable
as List<TemplateDay>?,
  ));
}

}


/// Adds pattern-matching-related methods to [TripTemplate].
extension TripTemplatePatterns on TripTemplate {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripTemplate value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripTemplate() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripTemplate value)  $default,){
final _that = this;
switch (_that) {
case _TripTemplate():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripTemplate value)?  $default,){
final _that = this;
switch (_that) {
case _TripTemplate() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String id,  String slug,  String name,  String moodTag,  String summary,  String objective,  int days,  List<String> cities,  Map<String, dynamic> preferences,  int pricedForPax,  double fromPriceLkr,  double fromPriceUsd,  List<TemplateDay>? itinerary)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripTemplate() when $default != null:
return $default(_that.id,_that.slug,_that.name,_that.moodTag,_that.summary,_that.objective,_that.days,_that.cities,_that.preferences,_that.pricedForPax,_that.fromPriceLkr,_that.fromPriceUsd,_that.itinerary);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String id,  String slug,  String name,  String moodTag,  String summary,  String objective,  int days,  List<String> cities,  Map<String, dynamic> preferences,  int pricedForPax,  double fromPriceLkr,  double fromPriceUsd,  List<TemplateDay>? itinerary)  $default,) {final _that = this;
switch (_that) {
case _TripTemplate():
return $default(_that.id,_that.slug,_that.name,_that.moodTag,_that.summary,_that.objective,_that.days,_that.cities,_that.preferences,_that.pricedForPax,_that.fromPriceLkr,_that.fromPriceUsd,_that.itinerary);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String id,  String slug,  String name,  String moodTag,  String summary,  String objective,  int days,  List<String> cities,  Map<String, dynamic> preferences,  int pricedForPax,  double fromPriceLkr,  double fromPriceUsd,  List<TemplateDay>? itinerary)?  $default,) {final _that = this;
switch (_that) {
case _TripTemplate() when $default != null:
return $default(_that.id,_that.slug,_that.name,_that.moodTag,_that.summary,_that.objective,_that.days,_that.cities,_that.preferences,_that.pricedForPax,_that.fromPriceLkr,_that.fromPriceUsd,_that.itinerary);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TripTemplate implements TripTemplate {
  const _TripTemplate({required this.id, required this.slug, required this.name, required this.moodTag, required this.summary, required this.objective, required this.days,  List<String> cities = const <String>[],  Map<String, dynamic> preferences = const <String, dynamic>{}, this.pricedForPax = 2, required this.fromPriceLkr, required this.fromPriceUsd,  List<TemplateDay>? itinerary}): _cities = cities,_preferences = preferences,_itinerary = itinerary;
  factory _TripTemplate.fromJson(Map<String, dynamic> json) => _$TripTemplateFromJson(json);

@override final  String id;
@override final  String slug;
@override final  String name;
@override final  String moodTag;
@override final  String summary;
@override final  String objective;
@override final  int days;
 final  List<String> _cities;
@override@JsonKey() List<String> get cities {
  if (_cities is EqualUnmodifiableListView) return _cities;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_cities);
}

 final  Map<String, dynamic> _preferences;
@override@JsonKey() Map<String, dynamic> get preferences {
  if (_preferences is EqualUnmodifiableMapView) return _preferences;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableMapView(_preferences);
}

@override@JsonKey() final  int pricedForPax;
@override final  double fromPriceLkr;
@override final  double fromPriceUsd;
 final  List<TemplateDay>? _itinerary;
@override List<TemplateDay>? get itinerary {
  final value = _itinerary;
  if (value == null) return null;
  if (_itinerary is EqualUnmodifiableListView) return _itinerary;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(value);
}


/// Create a copy of TripTemplate
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripTemplateCopyWith<_TripTemplate> get copyWith => __$TripTemplateCopyWithImpl<_TripTemplate>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TripTemplateToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripTemplate&&(identical(other.id, id) || other.id == id)&&(identical(other.slug, slug) || other.slug == slug)&&(identical(other.name, name) || other.name == name)&&(identical(other.moodTag, moodTag) || other.moodTag == moodTag)&&(identical(other.summary, summary) || other.summary == summary)&&(identical(other.objective, objective) || other.objective == objective)&&(identical(other.days, days) || other.days == days)&&const DeepCollectionEquality().equals(other.cities, _cities)&&const DeepCollectionEquality().equals(other.preferences, _preferences)&&(identical(other.pricedForPax, pricedForPax) || other.pricedForPax == pricedForPax)&&(identical(other.fromPriceLkr, fromPriceLkr) || other.fromPriceLkr == fromPriceLkr)&&(identical(other.fromPriceUsd, fromPriceUsd) || other.fromPriceUsd == fromPriceUsd)&&const DeepCollectionEquality().equals(other.itinerary, _itinerary));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,id,slug,name,moodTag,summary,objective,days,const DeepCollectionEquality().hash(_cities),const DeepCollectionEquality().hash(_preferences),pricedForPax,fromPriceLkr,fromPriceUsd,const DeepCollectionEquality().hash(_itinerary));
}

@override
String toString() {
    return 'TripTemplate(id: $id, slug: $slug, name: $name, moodTag: $moodTag, summary: $summary, objective: $objective, days: $days, cities: $cities, preferences: $preferences, pricedForPax: $pricedForPax, fromPriceLkr: $fromPriceLkr, fromPriceUsd: $fromPriceUsd, itinerary: $itinerary)';
}


}

/// @nodoc
abstract mixin class _$TripTemplateCopyWith<$Res> implements $TripTemplateCopyWith<$Res> {
  factory _$TripTemplateCopyWith(_TripTemplate value, $Res Function(_TripTemplate) _then) = __$TripTemplateCopyWithImpl;
@override @useResult
$Res call({
 String id, String slug, String name, String moodTag, String summary, String objective, int days, List<String> cities, Map<String, dynamic> preferences, int pricedForPax, double fromPriceLkr, double fromPriceUsd, List<TemplateDay>? itinerary
});




}
/// @nodoc
class __$TripTemplateCopyWithImpl<$Res>
    implements _$TripTemplateCopyWith<$Res> {
  __$TripTemplateCopyWithImpl(this._self, this._then);

  final _TripTemplate _self;
  final $Res Function(_TripTemplate) _then;

/// Create a copy of TripTemplate
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? id = null,Object? slug = null,Object? name = null,Object? moodTag = null,Object? summary = null,Object? objective = null,Object? days = null,Object? cities = null,Object? preferences = null,Object? pricedForPax = null,Object? fromPriceLkr = null,Object? fromPriceUsd = null,Object? itinerary = freezed,}) {
  return _then(_TripTemplate(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,slug: null == slug ? _self.slug : slug // ignore: cast_nullable_to_non_nullable
as String,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,moodTag: null == moodTag ? _self.moodTag : moodTag // ignore: cast_nullable_to_non_nullable
as String,summary: null == summary ? _self.summary : summary // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,days: null == days ? _self.days : days // ignore: cast_nullable_to_non_nullable
as int,cities: null == cities ? _self._cities : cities // ignore: cast_nullable_to_non_nullable
as List<String>,preferences: null == preferences ? _self._preferences : preferences // ignore: cast_nullable_to_non_nullable
as Map<String, dynamic>,pricedForPax: null == pricedForPax ? _self.pricedForPax : pricedForPax // ignore: cast_nullable_to_non_nullable
as int,fromPriceLkr: null == fromPriceLkr ? _self.fromPriceLkr : fromPriceLkr // ignore: cast_nullable_to_non_nullable
as double,fromPriceUsd: null == fromPriceUsd ? _self.fromPriceUsd : fromPriceUsd // ignore: cast_nullable_to_non_nullable
as double,itinerary: freezed == itinerary ? _self._itinerary : itinerary // ignore: cast_nullable_to_non_nullable
as List<TemplateDay>?,
  ));
}


}


/// @nodoc
mixin _$TemplateDay {

 int get day; String get city; List<TemplateStop> get stops;
/// Create a copy of TemplateDay
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TemplateDayCopyWith<TemplateDay> get copyWith => _$TemplateDayCopyWithImpl<TemplateDay>(this as TemplateDay, _$identity);

  /// Serializes this TemplateDay to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TemplateDay;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TemplateDay&&(identical(other.day, _this.day) || other.day == _this.day)&&(identical(other.city, _this.city) || other.city == _this.city)&&const DeepCollectionEquality().equals(other.stops, _this.stops));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TemplateDay;
  return Object.hash(runtimeType,_this.day,_this.city,const DeepCollectionEquality().hash(_this.stops));
}

@override
String toString() {
  final _this = this as TemplateDay;
  return 'TemplateDay(day: ${_this.day}, city: ${_this.city}, stops: ${_this.stops})';
}


}

/// @nodoc
abstract mixin class $TemplateDayCopyWith<$Res>  {
  factory $TemplateDayCopyWith(TemplateDay value, $Res Function(TemplateDay) _then) = _$TemplateDayCopyWithImpl;
@useResult
$Res call({
 int day, String city, List<TemplateStop> stops
});




}
/// @nodoc
class _$TemplateDayCopyWithImpl<$Res>
    implements $TemplateDayCopyWith<$Res> {
  _$TemplateDayCopyWithImpl(this._self, this._then);

  final TemplateDay _self;
  final $Res Function(TemplateDay) _then;

/// Create a copy of TemplateDay
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? day = null,Object? city = null,Object? stops = null,}) {
  return _then(TemplateDay(
day: null == day ? _self.day : day // ignore: cast_nullable_to_non_nullable
as int,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,stops: null == stops ? _self.stops : stops // ignore: cast_nullable_to_non_nullable
as List<TemplateStop>,
  ));
}

}


/// Adds pattern-matching-related methods to [TemplateDay].
extension TemplateDayPatterns on TemplateDay {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TemplateDay value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TemplateDay() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TemplateDay value)  $default,){
final _that = this;
switch (_that) {
case _TemplateDay():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TemplateDay value)?  $default,){
final _that = this;
switch (_that) {
case _TemplateDay() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( int day,  String city,  List<TemplateStop> stops)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TemplateDay() when $default != null:
return $default(_that.day,_that.city,_that.stops);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( int day,  String city,  List<TemplateStop> stops)  $default,) {final _that = this;
switch (_that) {
case _TemplateDay():
return $default(_that.day,_that.city,_that.stops);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( int day,  String city,  List<TemplateStop> stops)?  $default,) {final _that = this;
switch (_that) {
case _TemplateDay() when $default != null:
return $default(_that.day,_that.city,_that.stops);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TemplateDay implements TemplateDay {
  const _TemplateDay({required this.day, required this.city,  List<TemplateStop> stops = const <TemplateStop>[]}): _stops = stops;
  factory _TemplateDay.fromJson(Map<String, dynamic> json) => _$TemplateDayFromJson(json);

@override final  int day;
@override final  String city;
 final  List<TemplateStop> _stops;
@override@JsonKey() List<TemplateStop> get stops {
  if (_stops is EqualUnmodifiableListView) return _stops;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_stops);
}


/// Create a copy of TemplateDay
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TemplateDayCopyWith<_TemplateDay> get copyWith => __$TemplateDayCopyWithImpl<_TemplateDay>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TemplateDayToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TemplateDay&&(identical(other.day, day) || other.day == day)&&(identical(other.city, city) || other.city == city)&&const DeepCollectionEquality().equals(other.stops, _stops));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,day,city,const DeepCollectionEquality().hash(_stops));
}

@override
String toString() {
    return 'TemplateDay(day: $day, city: $city, stops: $stops)';
}


}

/// @nodoc
abstract mixin class _$TemplateDayCopyWith<$Res> implements $TemplateDayCopyWith<$Res> {
  factory _$TemplateDayCopyWith(_TemplateDay value, $Res Function(_TemplateDay) _then) = __$TemplateDayCopyWithImpl;
@override @useResult
$Res call({
 int day, String city, List<TemplateStop> stops
});




}
/// @nodoc
class __$TemplateDayCopyWithImpl<$Res>
    implements _$TemplateDayCopyWith<$Res> {
  __$TemplateDayCopyWithImpl(this._self, this._then);

  final _TemplateDay _self;
  final $Res Function(_TemplateDay) _then;

/// Create a copy of TemplateDay
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? day = null,Object? city = null,Object? stops = null,}) {
  return _then(_TemplateDay(
day: null == day ? _self.day : day // ignore: cast_nullable_to_non_nullable
as int,city: null == city ? _self.city : city // ignore: cast_nullable_to_non_nullable
as String,stops: null == stops ? _self._stops : stops // ignore: cast_nullable_to_non_nullable
as List<TemplateStop>,
  ));
}


}


/// @nodoc
mixin _$TemplateStop {

 String? get attractionId; String get name; double get entryFeeLkr; double? get latitude; double? get longitude;
/// Create a copy of TemplateStop
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TemplateStopCopyWith<TemplateStop> get copyWith => _$TemplateStopCopyWithImpl<TemplateStop>(this as TemplateStop, _$identity);

  /// Serializes this TemplateStop to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TemplateStop;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TemplateStop&&(identical(other.attractionId, _this.attractionId) || other.attractionId == _this.attractionId)&&(identical(other.name, _this.name) || other.name == _this.name)&&(identical(other.entryFeeLkr, _this.entryFeeLkr) || other.entryFeeLkr == _this.entryFeeLkr)&&(identical(other.latitude, _this.latitude) || other.latitude == _this.latitude)&&(identical(other.longitude, _this.longitude) || other.longitude == _this.longitude));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TemplateStop;
  return Object.hash(runtimeType,_this.attractionId,_this.name,_this.entryFeeLkr,_this.latitude,_this.longitude);
}

@override
String toString() {
  final _this = this as TemplateStop;
  return 'TemplateStop(attractionId: ${_this.attractionId}, name: ${_this.name}, entryFeeLkr: ${_this.entryFeeLkr}, latitude: ${_this.latitude}, longitude: ${_this.longitude})';
}


}

/// @nodoc
abstract mixin class $TemplateStopCopyWith<$Res>  {
  factory $TemplateStopCopyWith(TemplateStop value, $Res Function(TemplateStop) _then) = _$TemplateStopCopyWithImpl;
@useResult
$Res call({
 String? attractionId, String name, double entryFeeLkr, double? latitude, double? longitude
});




}
/// @nodoc
class _$TemplateStopCopyWithImpl<$Res>
    implements $TemplateStopCopyWith<$Res> {
  _$TemplateStopCopyWithImpl(this._self, this._then);

  final TemplateStop _self;
  final $Res Function(TemplateStop) _then;

/// Create a copy of TemplateStop
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? attractionId = freezed,Object? name = null,Object? entryFeeLkr = null,Object? latitude = freezed,Object? longitude = freezed,}) {
  return _then(TemplateStop(
attractionId: freezed == attractionId ? _self.attractionId : attractionId // ignore: cast_nullable_to_non_nullable
as String?,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,entryFeeLkr: null == entryFeeLkr ? _self.entryFeeLkr : entryFeeLkr // ignore: cast_nullable_to_non_nullable
as double,latitude: freezed == latitude ? _self.latitude : latitude // ignore: cast_nullable_to_non_nullable
as double?,longitude: freezed == longitude ? _self.longitude : longitude // ignore: cast_nullable_to_non_nullable
as double?,
  ));
}

}


/// Adds pattern-matching-related methods to [TemplateStop].
extension TemplateStopPatterns on TemplateStop {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TemplateStop value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TemplateStop() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TemplateStop value)  $default,){
final _that = this;
switch (_that) {
case _TemplateStop():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TemplateStop value)?  $default,){
final _that = this;
switch (_that) {
case _TemplateStop() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String? attractionId,  String name,  double entryFeeLkr,  double? latitude,  double? longitude)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TemplateStop() when $default != null:
return $default(_that.attractionId,_that.name,_that.entryFeeLkr,_that.latitude,_that.longitude);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String? attractionId,  String name,  double entryFeeLkr,  double? latitude,  double? longitude)  $default,) {final _that = this;
switch (_that) {
case _TemplateStop():
return $default(_that.attractionId,_that.name,_that.entryFeeLkr,_that.latitude,_that.longitude);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String? attractionId,  String name,  double entryFeeLkr,  double? latitude,  double? longitude)?  $default,) {final _that = this;
switch (_that) {
case _TemplateStop() when $default != null:
return $default(_that.attractionId,_that.name,_that.entryFeeLkr,_that.latitude,_that.longitude);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TemplateStop implements TemplateStop {
  const _TemplateStop({this.attractionId, required this.name, this.entryFeeLkr = 0, this.latitude, this.longitude});
  factory _TemplateStop.fromJson(Map<String, dynamic> json) => _$TemplateStopFromJson(json);

@override final  String? attractionId;
@override final  String name;
@override@JsonKey() final  double entryFeeLkr;
@override final  double? latitude;
@override final  double? longitude;

/// Create a copy of TemplateStop
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TemplateStopCopyWith<_TemplateStop> get copyWith => __$TemplateStopCopyWithImpl<_TemplateStop>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TemplateStopToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TemplateStop&&(identical(other.attractionId, attractionId) || other.attractionId == attractionId)&&(identical(other.name, name) || other.name == name)&&(identical(other.entryFeeLkr, entryFeeLkr) || other.entryFeeLkr == entryFeeLkr)&&(identical(other.latitude, latitude) || other.latitude == latitude)&&(identical(other.longitude, longitude) || other.longitude == longitude));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,attractionId,name,entryFeeLkr,latitude,longitude);
}

@override
String toString() {
    return 'TemplateStop(attractionId: $attractionId, name: $name, entryFeeLkr: $entryFeeLkr, latitude: $latitude, longitude: $longitude)';
}


}

/// @nodoc
abstract mixin class _$TemplateStopCopyWith<$Res> implements $TemplateStopCopyWith<$Res> {
  factory _$TemplateStopCopyWith(_TemplateStop value, $Res Function(_TemplateStop) _then) = __$TemplateStopCopyWithImpl;
@override @useResult
$Res call({
 String? attractionId, String name, double entryFeeLkr, double? latitude, double? longitude
});




}
/// @nodoc
class __$TemplateStopCopyWithImpl<$Res>
    implements _$TemplateStopCopyWith<$Res> {
  __$TemplateStopCopyWithImpl(this._self, this._then);

  final _TemplateStop _self;
  final $Res Function(_TemplateStop) _then;

/// Create a copy of TemplateStop
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? attractionId = freezed,Object? name = null,Object? entryFeeLkr = null,Object? latitude = freezed,Object? longitude = freezed,}) {
  return _then(_TemplateStop(
attractionId: freezed == attractionId ? _self.attractionId : attractionId // ignore: cast_nullable_to_non_nullable
as String?,name: null == name ? _self.name : name // ignore: cast_nullable_to_non_nullable
as String,entryFeeLkr: null == entryFeeLkr ? _self.entryFeeLkr : entryFeeLkr // ignore: cast_nullable_to_non_nullable
as double,latitude: freezed == latitude ? _self.latitude : latitude // ignore: cast_nullable_to_non_nullable
as double?,longitude: freezed == longitude ? _self.longitude : longitude // ignore: cast_nullable_to_non_nullable
as double?,
  ));
}


}


/// @nodoc
mixin _$BookTemplateRequest {

 String get startDate; int get pax; double get budgetUsd; String get nationality; String get passportNumber;
/// Create a copy of BookTemplateRequest
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$BookTemplateRequestCopyWith<BookTemplateRequest> get copyWith => _$BookTemplateRequestCopyWithImpl<BookTemplateRequest>(this as BookTemplateRequest, _$identity);

  /// Serializes this BookTemplateRequest to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as BookTemplateRequest;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is BookTemplateRequest&&(identical(other.startDate, _this.startDate) || other.startDate == _this.startDate)&&(identical(other.pax, _this.pax) || other.pax == _this.pax)&&(identical(other.budgetUsd, _this.budgetUsd) || other.budgetUsd == _this.budgetUsd)&&(identical(other.nationality, _this.nationality) || other.nationality == _this.nationality)&&(identical(other.passportNumber, _this.passportNumber) || other.passportNumber == _this.passportNumber));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as BookTemplateRequest;
  return Object.hash(runtimeType,_this.startDate,_this.pax,_this.budgetUsd,_this.nationality,_this.passportNumber);
}

@override
String toString() {
  final _this = this as BookTemplateRequest;
  return 'BookTemplateRequest(startDate: ${_this.startDate}, pax: ${_this.pax}, budgetUsd: ${_this.budgetUsd}, nationality: ${_this.nationality}, passportNumber: ${_this.passportNumber})';
}


}

/// @nodoc
abstract mixin class $BookTemplateRequestCopyWith<$Res>  {
  factory $BookTemplateRequestCopyWith(BookTemplateRequest value, $Res Function(BookTemplateRequest) _then) = _$BookTemplateRequestCopyWithImpl;
@useResult
$Res call({
 String startDate, int pax, double budgetUsd, String nationality, String passportNumber
});




}
/// @nodoc
class _$BookTemplateRequestCopyWithImpl<$Res>
    implements $BookTemplateRequestCopyWith<$Res> {
  _$BookTemplateRequestCopyWithImpl(this._self, this._then);

  final BookTemplateRequest _self;
  final $Res Function(BookTemplateRequest) _then;

/// Create a copy of BookTemplateRequest
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? startDate = null,Object? pax = null,Object? budgetUsd = null,Object? nationality = null,Object? passportNumber = null,}) {
  return _then(BookTemplateRequest(
startDate: null == startDate ? _self.startDate : startDate // ignore: cast_nullable_to_non_nullable
as String,pax: null == pax ? _self.pax : pax // ignore: cast_nullable_to_non_nullable
as int,budgetUsd: null == budgetUsd ? _self.budgetUsd : budgetUsd // ignore: cast_nullable_to_non_nullable
as double,nationality: null == nationality ? _self.nationality : nationality // ignore: cast_nullable_to_non_nullable
as String,passportNumber: null == passportNumber ? _self.passportNumber : passportNumber // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [BookTemplateRequest].
extension BookTemplateRequestPatterns on BookTemplateRequest {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _BookTemplateRequest value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _BookTemplateRequest() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _BookTemplateRequest value)  $default,){
final _that = this;
switch (_that) {
case _BookTemplateRequest():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _BookTemplateRequest value)?  $default,){
final _that = this;
switch (_that) {
case _BookTemplateRequest() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String startDate,  int pax,  double budgetUsd,  String nationality,  String passportNumber)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _BookTemplateRequest() when $default != null:
return $default(_that.startDate,_that.pax,_that.budgetUsd,_that.nationality,_that.passportNumber);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String startDate,  int pax,  double budgetUsd,  String nationality,  String passportNumber)  $default,) {final _that = this;
switch (_that) {
case _BookTemplateRequest():
return $default(_that.startDate,_that.pax,_that.budgetUsd,_that.nationality,_that.passportNumber);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String startDate,  int pax,  double budgetUsd,  String nationality,  String passportNumber)?  $default,) {final _that = this;
switch (_that) {
case _BookTemplateRequest() when $default != null:
return $default(_that.startDate,_that.pax,_that.budgetUsd,_that.nationality,_that.passportNumber);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _BookTemplateRequest implements BookTemplateRequest {
  const _BookTemplateRequest({required this.startDate, required this.pax, required this.budgetUsd, required this.nationality, required this.passportNumber});
  factory _BookTemplateRequest.fromJson(Map<String, dynamic> json) => _$BookTemplateRequestFromJson(json);

@override final  String startDate;
@override final  int pax;
@override final  double budgetUsd;
@override final  String nationality;
@override final  String passportNumber;

/// Create a copy of BookTemplateRequest
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$BookTemplateRequestCopyWith<_BookTemplateRequest> get copyWith => __$BookTemplateRequestCopyWithImpl<_BookTemplateRequest>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$BookTemplateRequestToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _BookTemplateRequest&&(identical(other.startDate, startDate) || other.startDate == startDate)&&(identical(other.pax, pax) || other.pax == pax)&&(identical(other.budgetUsd, budgetUsd) || other.budgetUsd == budgetUsd)&&(identical(other.nationality, nationality) || other.nationality == nationality)&&(identical(other.passportNumber, passportNumber) || other.passportNumber == passportNumber));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,startDate,pax,budgetUsd,nationality,passportNumber);
}

@override
String toString() {
    return 'BookTemplateRequest(startDate: $startDate, pax: $pax, budgetUsd: $budgetUsd, nationality: $nationality, passportNumber: $passportNumber)';
}


}

/// @nodoc
abstract mixin class _$BookTemplateRequestCopyWith<$Res> implements $BookTemplateRequestCopyWith<$Res> {
  factory _$BookTemplateRequestCopyWith(_BookTemplateRequest value, $Res Function(_BookTemplateRequest) _then) = __$BookTemplateRequestCopyWithImpl;
@override @useResult
$Res call({
 String startDate, int pax, double budgetUsd, String nationality, String passportNumber
});




}
/// @nodoc
class __$BookTemplateRequestCopyWithImpl<$Res>
    implements _$BookTemplateRequestCopyWith<$Res> {
  __$BookTemplateRequestCopyWithImpl(this._self, this._then);

  final _BookTemplateRequest _self;
  final $Res Function(_BookTemplateRequest) _then;

/// Create a copy of BookTemplateRequest
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? startDate = null,Object? pax = null,Object? budgetUsd = null,Object? nationality = null,Object? passportNumber = null,}) {
  return _then(_BookTemplateRequest(
startDate: null == startDate ? _self.startDate : startDate // ignore: cast_nullable_to_non_nullable
as String,pax: null == pax ? _self.pax : pax // ignore: cast_nullable_to_non_nullable
as int,budgetUsd: null == budgetUsd ? _self.budgetUsd : budgetUsd // ignore: cast_nullable_to_non_nullable
as double,nationality: null == nationality ? _self.nationality : nationality // ignore: cast_nullable_to_non_nullable
as String,passportNumber: null == passportNumber ? _self.passportNumber : passportNumber // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}


/// @nodoc
mixin _$BookTemplateResult {

 TripRequest get trip; StartPlanningResult get planning;
/// Create a copy of BookTemplateResult
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$BookTemplateResultCopyWith<BookTemplateResult> get copyWith => _$BookTemplateResultCopyWithImpl<BookTemplateResult>(this as BookTemplateResult, _$identity);

  /// Serializes this BookTemplateResult to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as BookTemplateResult;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is BookTemplateResult&&(identical(other.trip, _this.trip) || other.trip == _this.trip)&&(identical(other.planning, _this.planning) || other.planning == _this.planning));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as BookTemplateResult;
  return Object.hash(runtimeType,_this.trip,_this.planning);
}

@override
String toString() {
  final _this = this as BookTemplateResult;
  return 'BookTemplateResult(trip: ${_this.trip}, planning: ${_this.planning})';
}


}

/// @nodoc
abstract mixin class $BookTemplateResultCopyWith<$Res>  {
  factory $BookTemplateResultCopyWith(BookTemplateResult value, $Res Function(BookTemplateResult) _then) = _$BookTemplateResultCopyWithImpl;
@useResult
$Res call({
 TripRequest trip, StartPlanningResult planning
});


$TripRequestCopyWith<$Res> get trip;$StartPlanningResultCopyWith<$Res> get planning;

}
/// @nodoc
class _$BookTemplateResultCopyWithImpl<$Res>
    implements $BookTemplateResultCopyWith<$Res> {
  _$BookTemplateResultCopyWithImpl(this._self, this._then);

  final BookTemplateResult _self;
  final $Res Function(BookTemplateResult) _then;

/// Create a copy of BookTemplateResult
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? trip = null,Object? planning = null,}) {
  return _then(BookTemplateResult(
trip: null == trip ? _self.trip : trip // ignore: cast_nullable_to_non_nullable
as TripRequest,planning: null == planning ? _self.planning : planning // ignore: cast_nullable_to_non_nullable
as StartPlanningResult,
  ));
}
/// Create a copy of BookTemplateResult
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$TripRequestCopyWith<$Res> get trip {
  
  return $TripRequestCopyWith<$Res>(_self.trip, (value) {
    return _then(_self.copyWith(trip: value));
  });
}/// Create a copy of BookTemplateResult
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$StartPlanningResultCopyWith<$Res> get planning {
  
  return $StartPlanningResultCopyWith<$Res>(_self.planning, (value) {
    return _then(_self.copyWith(planning: value));
  });
}
}


/// Adds pattern-matching-related methods to [BookTemplateResult].
extension BookTemplateResultPatterns on BookTemplateResult {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _BookTemplateResult value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _BookTemplateResult() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _BookTemplateResult value)  $default,){
final _that = this;
switch (_that) {
case _BookTemplateResult():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _BookTemplateResult value)?  $default,){
final _that = this;
switch (_that) {
case _BookTemplateResult() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( TripRequest trip,  StartPlanningResult planning)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _BookTemplateResult() when $default != null:
return $default(_that.trip,_that.planning);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( TripRequest trip,  StartPlanningResult planning)  $default,) {final _that = this;
switch (_that) {
case _BookTemplateResult():
return $default(_that.trip,_that.planning);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( TripRequest trip,  StartPlanningResult planning)?  $default,) {final _that = this;
switch (_that) {
case _BookTemplateResult() when $default != null:
return $default(_that.trip,_that.planning);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _BookTemplateResult implements BookTemplateResult {
  const _BookTemplateResult({required this.trip, required this.planning});
  factory _BookTemplateResult.fromJson(Map<String, dynamic> json) => _$BookTemplateResultFromJson(json);

@override final  TripRequest trip;
@override final  StartPlanningResult planning;

/// Create a copy of BookTemplateResult
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$BookTemplateResultCopyWith<_BookTemplateResult> get copyWith => __$BookTemplateResultCopyWithImpl<_BookTemplateResult>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$BookTemplateResultToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _BookTemplateResult&&(identical(other.trip, trip) || other.trip == trip)&&(identical(other.planning, planning) || other.planning == planning));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,trip,planning);
}

@override
String toString() {
    return 'BookTemplateResult(trip: $trip, planning: $planning)';
}


}

/// @nodoc
abstract mixin class _$BookTemplateResultCopyWith<$Res> implements $BookTemplateResultCopyWith<$Res> {
  factory _$BookTemplateResultCopyWith(_BookTemplateResult value, $Res Function(_BookTemplateResult) _then) = __$BookTemplateResultCopyWithImpl;
@override @useResult
$Res call({
 TripRequest trip, StartPlanningResult planning
});


@override $TripRequestCopyWith<$Res> get trip;@override $StartPlanningResultCopyWith<$Res> get planning;

}
/// @nodoc
class __$BookTemplateResultCopyWithImpl<$Res>
    implements _$BookTemplateResultCopyWith<$Res> {
  __$BookTemplateResultCopyWithImpl(this._self, this._then);

  final _BookTemplateResult _self;
  final $Res Function(_BookTemplateResult) _then;

/// Create a copy of BookTemplateResult
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? trip = null,Object? planning = null,}) {
  return _then(_BookTemplateResult(
trip: null == trip ? _self.trip : trip // ignore: cast_nullable_to_non_nullable
as TripRequest,planning: null == planning ? _self.planning : planning // ignore: cast_nullable_to_non_nullable
as StartPlanningResult,
  ));
}

/// Create a copy of BookTemplateResult
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$TripRequestCopyWith<$Res> get trip {
  
  return $TripRequestCopyWith<$Res>(_self.trip, (value) {
    return _then(_self.copyWith(trip: value));
  });
}/// Create a copy of BookTemplateResult
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$StartPlanningResultCopyWith<$Res> get planning {
  
  return $StartPlanningResultCopyWith<$Res>(_self.planning, (value) {
    return _then(_self.copyWith(planning: value));
  });
}
}

// dart format on
