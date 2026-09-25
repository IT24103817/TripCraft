// GENERATED CODE - DO NOT MODIFY BY HAND
// coverage:ignore-file
// ignore_for_file: type=lint, type=warning, deprecated_member_use, deprecated_member_use_from_same_package
// ignore_for_file: unused_element, deprecated_member_use, deprecated_member_use_from_same_package, use_function_type_syntax_for_parameters, unnecessary_const, avoid_init_to_null, invalid_override_different_default_values_named, prefer_expression_function_bodies, annotate_overrides, invalid_annotation_target, unnecessary_question_mark

part of 'quotation_models.dart';

// **************************************************************************
// FreezedGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// dart format off
T _$identity<T>(T value) => value;

/// @nodoc
mixin _$Quotation {

 List<QuotationLine> get lines;@JsonKey(name: 'subtotal_lkr') double get subtotalLkr;@JsonKey(name: 'margin_pct') double get marginPct;@JsonKey(name: 'margin_lkr') double get marginLkr;@JsonKey(name: 'total_lkr') double get totalLkr;@JsonKey(name: 'fx_rate') double get fxRate;@JsonKey(name: 'fx_as_of') String get fxAsOf;@JsonKey(name: 'fx_stale') bool get fxStale;@JsonKey(name: 'total_usd') double get totalUsd;
/// Create a copy of Quotation
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$QuotationCopyWith<Quotation> get copyWith => _$QuotationCopyWithImpl<Quotation>(this as Quotation, _$identity);

  /// Serializes this Quotation to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as Quotation;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is Quotation&&const DeepCollectionEquality().equals(other.lines, _this.lines)&&(identical(other.subtotalLkr, _this.subtotalLkr) || other.subtotalLkr == _this.subtotalLkr)&&(identical(other.marginPct, _this.marginPct) || other.marginPct == _this.marginPct)&&(identical(other.marginLkr, _this.marginLkr) || other.marginLkr == _this.marginLkr)&&(identical(other.totalLkr, _this.totalLkr) || other.totalLkr == _this.totalLkr)&&(identical(other.fxRate, _this.fxRate) || other.fxRate == _this.fxRate)&&(identical(other.fxAsOf, _this.fxAsOf) || other.fxAsOf == _this.fxAsOf)&&(identical(other.fxStale, _this.fxStale) || other.fxStale == _this.fxStale)&&(identical(other.totalUsd, _this.totalUsd) || other.totalUsd == _this.totalUsd));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as Quotation;
  return Object.hash(runtimeType,const DeepCollectionEquality().hash(_this.lines),_this.subtotalLkr,_this.marginPct,_this.marginLkr,_this.totalLkr,_this.fxRate,_this.fxAsOf,_this.fxStale,_this.totalUsd);
}

@override
String toString() {
  final _this = this as Quotation;
  return 'Quotation(lines: ${_this.lines}, subtotalLkr: ${_this.subtotalLkr}, marginPct: ${_this.marginPct}, marginLkr: ${_this.marginLkr}, totalLkr: ${_this.totalLkr}, fxRate: ${_this.fxRate}, fxAsOf: ${_this.fxAsOf}, fxStale: ${_this.fxStale}, totalUsd: ${_this.totalUsd})';
}


}

/// @nodoc
abstract mixin class $QuotationCopyWith<$Res>  {
  factory $QuotationCopyWith(Quotation value, $Res Function(Quotation) _then) = _$QuotationCopyWithImpl;
@useResult
$Res call({
 List<QuotationLine> lines,@JsonKey(name: 'subtotal_lkr') double subtotalLkr,@JsonKey(name: 'margin_pct') double marginPct,@JsonKey(name: 'margin_lkr') double marginLkr,@JsonKey(name: 'total_lkr') double totalLkr,@JsonKey(name: 'fx_rate') double fxRate,@JsonKey(name: 'fx_as_of') String fxAsOf,@JsonKey(name: 'fx_stale') bool fxStale,@JsonKey(name: 'total_usd') double totalUsd
});




}
/// @nodoc
class _$QuotationCopyWithImpl<$Res>
    implements $QuotationCopyWith<$Res> {
  _$QuotationCopyWithImpl(this._self, this._then);

  final Quotation _self;
  final $Res Function(Quotation) _then;

/// Create a copy of Quotation
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? lines = null,Object? subtotalLkr = null,Object? marginPct = null,Object? marginLkr = null,Object? totalLkr = null,Object? fxRate = null,Object? fxAsOf = null,Object? fxStale = null,Object? totalUsd = null,}) {
  return _then(Quotation(
lines: null == lines ? _self.lines : lines // ignore: cast_nullable_to_non_nullable
as List<QuotationLine>,subtotalLkr: null == subtotalLkr ? _self.subtotalLkr : subtotalLkr // ignore: cast_nullable_to_non_nullable
as double,marginPct: null == marginPct ? _self.marginPct : marginPct // ignore: cast_nullable_to_non_nullable
as double,marginLkr: null == marginLkr ? _self.marginLkr : marginLkr // ignore: cast_nullable_to_non_nullable
as double,totalLkr: null == totalLkr ? _self.totalLkr : totalLkr // ignore: cast_nullable_to_non_nullable
as double,fxRate: null == fxRate ? _self.fxRate : fxRate // ignore: cast_nullable_to_non_nullable
as double,fxAsOf: null == fxAsOf ? _self.fxAsOf : fxAsOf // ignore: cast_nullable_to_non_nullable
as String,fxStale: null == fxStale ? _self.fxStale : fxStale // ignore: cast_nullable_to_non_nullable
as bool,totalUsd: null == totalUsd ? _self.totalUsd : totalUsd // ignore: cast_nullable_to_non_nullable
as double,
  ));
}

}


/// Adds pattern-matching-related methods to [Quotation].
extension QuotationPatterns on Quotation {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _Quotation value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _Quotation() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _Quotation value)  $default,){
final _that = this;
switch (_that) {
case _Quotation():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _Quotation value)?  $default,){
final _that = this;
switch (_that) {
case _Quotation() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( List<QuotationLine> lines, @JsonKey(name: 'subtotal_lkr')  double subtotalLkr, @JsonKey(name: 'margin_pct')  double marginPct, @JsonKey(name: 'margin_lkr')  double marginLkr, @JsonKey(name: 'total_lkr')  double totalLkr, @JsonKey(name: 'fx_rate')  double fxRate, @JsonKey(name: 'fx_as_of')  String fxAsOf, @JsonKey(name: 'fx_stale')  bool fxStale, @JsonKey(name: 'total_usd')  double totalUsd)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _Quotation() when $default != null:
return $default(_that.lines,_that.subtotalLkr,_that.marginPct,_that.marginLkr,_that.totalLkr,_that.fxRate,_that.fxAsOf,_that.fxStale,_that.totalUsd);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( List<QuotationLine> lines, @JsonKey(name: 'subtotal_lkr')  double subtotalLkr, @JsonKey(name: 'margin_pct')  double marginPct, @JsonKey(name: 'margin_lkr')  double marginLkr, @JsonKey(name: 'total_lkr')  double totalLkr, @JsonKey(name: 'fx_rate')  double fxRate, @JsonKey(name: 'fx_as_of')  String fxAsOf, @JsonKey(name: 'fx_stale')  bool fxStale, @JsonKey(name: 'total_usd')  double totalUsd)  $default,) {final _that = this;
switch (_that) {
case _Quotation():
return $default(_that.lines,_that.subtotalLkr,_that.marginPct,_that.marginLkr,_that.totalLkr,_that.fxRate,_that.fxAsOf,_that.fxStale,_that.totalUsd);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( List<QuotationLine> lines, @JsonKey(name: 'subtotal_lkr')  double subtotalLkr, @JsonKey(name: 'margin_pct')  double marginPct, @JsonKey(name: 'margin_lkr')  double marginLkr, @JsonKey(name: 'total_lkr')  double totalLkr, @JsonKey(name: 'fx_rate')  double fxRate, @JsonKey(name: 'fx_as_of')  String fxAsOf, @JsonKey(name: 'fx_stale')  bool fxStale, @JsonKey(name: 'total_usd')  double totalUsd)?  $default,) {final _that = this;
switch (_that) {
case _Quotation() when $default != null:
return $default(_that.lines,_that.subtotalLkr,_that.marginPct,_that.marginLkr,_that.totalLkr,_that.fxRate,_that.fxAsOf,_that.fxStale,_that.totalUsd);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _Quotation implements Quotation {
  const _Quotation({ List<QuotationLine> lines = const <QuotationLine>[], @JsonKey(name: 'subtotal_lkr') required this.subtotalLkr, @JsonKey(name: 'margin_pct') required this.marginPct, @JsonKey(name: 'margin_lkr') required this.marginLkr, @JsonKey(name: 'total_lkr') required this.totalLkr, @JsonKey(name: 'fx_rate') required this.fxRate, @JsonKey(name: 'fx_as_of') required this.fxAsOf, @JsonKey(name: 'fx_stale') this.fxStale = false, @JsonKey(name: 'total_usd') required this.totalUsd}): _lines = lines;
  factory _Quotation.fromJson(Map<String, dynamic> json) => _$QuotationFromJson(json);

 final  List<QuotationLine> _lines;
@override@JsonKey() List<QuotationLine> get lines {
  if (_lines is EqualUnmodifiableListView) return _lines;
  // ignore: implicit_dynamic_type
  return EqualUnmodifiableListView(_lines);
}

@override@JsonKey(name: 'subtotal_lkr') final  double subtotalLkr;
@override@JsonKey(name: 'margin_pct') final  double marginPct;
@override@JsonKey(name: 'margin_lkr') final  double marginLkr;
@override@JsonKey(name: 'total_lkr') final  double totalLkr;
@override@JsonKey(name: 'fx_rate') final  double fxRate;
@override@JsonKey(name: 'fx_as_of') final  String fxAsOf;
@override@JsonKey(name: 'fx_stale') final  bool fxStale;
@override@JsonKey(name: 'total_usd') final  double totalUsd;

/// Create a copy of Quotation
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$QuotationCopyWith<_Quotation> get copyWith => __$QuotationCopyWithImpl<_Quotation>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$QuotationToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _Quotation&&const DeepCollectionEquality().equals(other.lines, _lines)&&(identical(other.subtotalLkr, subtotalLkr) || other.subtotalLkr == subtotalLkr)&&(identical(other.marginPct, marginPct) || other.marginPct == marginPct)&&(identical(other.marginLkr, marginLkr) || other.marginLkr == marginLkr)&&(identical(other.totalLkr, totalLkr) || other.totalLkr == totalLkr)&&(identical(other.fxRate, fxRate) || other.fxRate == fxRate)&&(identical(other.fxAsOf, fxAsOf) || other.fxAsOf == fxAsOf)&&(identical(other.fxStale, fxStale) || other.fxStale == fxStale)&&(identical(other.totalUsd, totalUsd) || other.totalUsd == totalUsd));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,const DeepCollectionEquality().hash(_lines),subtotalLkr,marginPct,marginLkr,totalLkr,fxRate,fxAsOf,fxStale,totalUsd);
}

@override
String toString() {
    return 'Quotation(lines: $lines, subtotalLkr: $subtotalLkr, marginPct: $marginPct, marginLkr: $marginLkr, totalLkr: $totalLkr, fxRate: $fxRate, fxAsOf: $fxAsOf, fxStale: $fxStale, totalUsd: $totalUsd)';
}


}

/// @nodoc
abstract mixin class _$QuotationCopyWith<$Res> implements $QuotationCopyWith<$Res> {
  factory _$QuotationCopyWith(_Quotation value, $Res Function(_Quotation) _then) = __$QuotationCopyWithImpl;
@override @useResult
$Res call({
 List<QuotationLine> lines,@JsonKey(name: 'subtotal_lkr') double subtotalLkr,@JsonKey(name: 'margin_pct') double marginPct,@JsonKey(name: 'margin_lkr') double marginLkr,@JsonKey(name: 'total_lkr') double totalLkr,@JsonKey(name: 'fx_rate') double fxRate,@JsonKey(name: 'fx_as_of') String fxAsOf,@JsonKey(name: 'fx_stale') bool fxStale,@JsonKey(name: 'total_usd') double totalUsd
});




}
/// @nodoc
class __$QuotationCopyWithImpl<$Res>
    implements _$QuotationCopyWith<$Res> {
  __$QuotationCopyWithImpl(this._self, this._then);

  final _Quotation _self;
  final $Res Function(_Quotation) _then;

/// Create a copy of Quotation
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? lines = null,Object? subtotalLkr = null,Object? marginPct = null,Object? marginLkr = null,Object? totalLkr = null,Object? fxRate = null,Object? fxAsOf = null,Object? fxStale = null,Object? totalUsd = null,}) {
  return _then(_Quotation(
lines: null == lines ? _self._lines : lines // ignore: cast_nullable_to_non_nullable
as List<QuotationLine>,subtotalLkr: null == subtotalLkr ? _self.subtotalLkr : subtotalLkr // ignore: cast_nullable_to_non_nullable
as double,marginPct: null == marginPct ? _self.marginPct : marginPct // ignore: cast_nullable_to_non_nullable
as double,marginLkr: null == marginLkr ? _self.marginLkr : marginLkr // ignore: cast_nullable_to_non_nullable
as double,totalLkr: null == totalLkr ? _self.totalLkr : totalLkr // ignore: cast_nullable_to_non_nullable
as double,fxRate: null == fxRate ? _self.fxRate : fxRate // ignore: cast_nullable_to_non_nullable
as double,fxAsOf: null == fxAsOf ? _self.fxAsOf : fxAsOf // ignore: cast_nullable_to_non_nullable
as String,fxStale: null == fxStale ? _self.fxStale : fxStale // ignore: cast_nullable_to_non_nullable
as bool,totalUsd: null == totalUsd ? _self.totalUsd : totalUsd // ignore: cast_nullable_to_non_nullable
as double,
  ));
}


}


/// @nodoc
mixin _$QuotationLine {

@JsonKey(name: 'line_type') String get lineType; String get description; double get qty;@JsonKey(name: 'unit_lkr') double get unitLkr;@JsonKey(name: 'amount_lkr') double get amountLkr;
/// Create a copy of QuotationLine
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$QuotationLineCopyWith<QuotationLine> get copyWith => _$QuotationLineCopyWithImpl<QuotationLine>(this as QuotationLine, _$identity);

  /// Serializes this QuotationLine to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as QuotationLine;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is QuotationLine&&(identical(other.lineType, _this.lineType) || other.lineType == _this.lineType)&&(identical(other.description, _this.description) || other.description == _this.description)&&(identical(other.qty, _this.qty) || other.qty == _this.qty)&&(identical(other.unitLkr, _this.unitLkr) || other.unitLkr == _this.unitLkr)&&(identical(other.amountLkr, _this.amountLkr) || other.amountLkr == _this.amountLkr));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as QuotationLine;
  return Object.hash(runtimeType,_this.lineType,_this.description,_this.qty,_this.unitLkr,_this.amountLkr);
}

@override
String toString() {
  final _this = this as QuotationLine;
  return 'QuotationLine(lineType: ${_this.lineType}, description: ${_this.description}, qty: ${_this.qty}, unitLkr: ${_this.unitLkr}, amountLkr: ${_this.amountLkr})';
}


}

/// @nodoc
abstract mixin class $QuotationLineCopyWith<$Res>  {
  factory $QuotationLineCopyWith(QuotationLine value, $Res Function(QuotationLine) _then) = _$QuotationLineCopyWithImpl;
@useResult
$Res call({
@JsonKey(name: 'line_type') String lineType, String description, double qty,@JsonKey(name: 'unit_lkr') double unitLkr,@JsonKey(name: 'amount_lkr') double amountLkr
});




}
/// @nodoc
class _$QuotationLineCopyWithImpl<$Res>
    implements $QuotationLineCopyWith<$Res> {
  _$QuotationLineCopyWithImpl(this._self, this._then);

  final QuotationLine _self;
  final $Res Function(QuotationLine) _then;

/// Create a copy of QuotationLine
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? lineType = null,Object? description = null,Object? qty = null,Object? unitLkr = null,Object? amountLkr = null,}) {
  return _then(QuotationLine(
lineType: null == lineType ? _self.lineType : lineType // ignore: cast_nullable_to_non_nullable
as String,description: null == description ? _self.description : description // ignore: cast_nullable_to_non_nullable
as String,qty: null == qty ? _self.qty : qty // ignore: cast_nullable_to_non_nullable
as double,unitLkr: null == unitLkr ? _self.unitLkr : unitLkr // ignore: cast_nullable_to_non_nullable
as double,amountLkr: null == amountLkr ? _self.amountLkr : amountLkr // ignore: cast_nullable_to_non_nullable
as double,
  ));
}

}


/// Adds pattern-matching-related methods to [QuotationLine].
extension QuotationLinePatterns on QuotationLine {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _QuotationLine value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _QuotationLine() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _QuotationLine value)  $default,){
final _that = this;
switch (_that) {
case _QuotationLine():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _QuotationLine value)?  $default,){
final _that = this;
switch (_that) {
case _QuotationLine() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function(@JsonKey(name: 'line_type')  String lineType,  String description,  double qty, @JsonKey(name: 'unit_lkr')  double unitLkr, @JsonKey(name: 'amount_lkr')  double amountLkr)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _QuotationLine() when $default != null:
return $default(_that.lineType,_that.description,_that.qty,_that.unitLkr,_that.amountLkr);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function(@JsonKey(name: 'line_type')  String lineType,  String description,  double qty, @JsonKey(name: 'unit_lkr')  double unitLkr, @JsonKey(name: 'amount_lkr')  double amountLkr)  $default,) {final _that = this;
switch (_that) {
case _QuotationLine():
return $default(_that.lineType,_that.description,_that.qty,_that.unitLkr,_that.amountLkr);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function(@JsonKey(name: 'line_type')  String lineType,  String description,  double qty, @JsonKey(name: 'unit_lkr')  double unitLkr, @JsonKey(name: 'amount_lkr')  double amountLkr)?  $default,) {final _that = this;
switch (_that) {
case _QuotationLine() when $default != null:
return $default(_that.lineType,_that.description,_that.qty,_that.unitLkr,_that.amountLkr);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _QuotationLine implements QuotationLine {
  const _QuotationLine({@JsonKey(name: 'line_type') required this.lineType, required this.description, required this.qty, @JsonKey(name: 'unit_lkr') required this.unitLkr, @JsonKey(name: 'amount_lkr') required this.amountLkr});
  factory _QuotationLine.fromJson(Map<String, dynamic> json) => _$QuotationLineFromJson(json);

@override@JsonKey(name: 'line_type') final  String lineType;
@override final  String description;
@override final  double qty;
@override@JsonKey(name: 'unit_lkr') final  double unitLkr;
@override@JsonKey(name: 'amount_lkr') final  double amountLkr;

/// Create a copy of QuotationLine
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$QuotationLineCopyWith<_QuotationLine> get copyWith => __$QuotationLineCopyWithImpl<_QuotationLine>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$QuotationLineToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _QuotationLine&&(identical(other.lineType, lineType) || other.lineType == lineType)&&(identical(other.description, description) || other.description == description)&&(identical(other.qty, qty) || other.qty == qty)&&(identical(other.unitLkr, unitLkr) || other.unitLkr == unitLkr)&&(identical(other.amountLkr, amountLkr) || other.amountLkr == amountLkr));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,lineType,description,qty,unitLkr,amountLkr);
}

@override
String toString() {
    return 'QuotationLine(lineType: $lineType, description: $description, qty: $qty, unitLkr: $unitLkr, amountLkr: $amountLkr)';
}


}

/// @nodoc
abstract mixin class _$QuotationLineCopyWith<$Res> implements $QuotationLineCopyWith<$Res> {
  factory _$QuotationLineCopyWith(_QuotationLine value, $Res Function(_QuotationLine) _then) = __$QuotationLineCopyWithImpl;
@override @useResult
$Res call({
@JsonKey(name: 'line_type') String lineType, String description, double qty,@JsonKey(name: 'unit_lkr') double unitLkr,@JsonKey(name: 'amount_lkr') double amountLkr
});




}
/// @nodoc
class __$QuotationLineCopyWithImpl<$Res>
    implements _$QuotationLineCopyWith<$Res> {
  __$QuotationLineCopyWithImpl(this._self, this._then);

  final _QuotationLine _self;
  final $Res Function(_QuotationLine) _then;

/// Create a copy of QuotationLine
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? lineType = null,Object? description = null,Object? qty = null,Object? unitLkr = null,Object? amountLkr = null,}) {
  return _then(_QuotationLine(
lineType: null == lineType ? _self.lineType : lineType // ignore: cast_nullable_to_non_nullable
as String,description: null == description ? _self.description : description // ignore: cast_nullable_to_non_nullable
as String,qty: null == qty ? _self.qty : qty // ignore: cast_nullable_to_non_nullable
as double,unitLkr: null == unitLkr ? _self.unitLkr : unitLkr // ignore: cast_nullable_to_non_nullable
as double,amountLkr: null == amountLkr ? _self.amountLkr : amountLkr // ignore: cast_nullable_to_non_nullable
as double,
  ));
}


}

/// @nodoc
mixin _$QuotationView {

 String get workflowStatus; Quotation? get quotation;
/// Create a copy of QuotationView
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$QuotationViewCopyWith<QuotationView> get copyWith => _$QuotationViewCopyWithImpl<QuotationView>(this as QuotationView, _$identity);



@override
bool operator ==(Object other) {
  final _this = this as QuotationView;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is QuotationView&&(identical(other.workflowStatus, _this.workflowStatus) || other.workflowStatus == _this.workflowStatus)&&(identical(other.quotation, _this.quotation) || other.quotation == _this.quotation));
}


@override
int get hashCode {
  final _this = this as QuotationView;
  return Object.hash(runtimeType,_this.workflowStatus,_this.quotation);
}

@override
String toString() {
  final _this = this as QuotationView;
  return 'QuotationView(workflowStatus: ${_this.workflowStatus}, quotation: ${_this.quotation})';
}


}

/// @nodoc
abstract mixin class $QuotationViewCopyWith<$Res>  {
  factory $QuotationViewCopyWith(QuotationView value, $Res Function(QuotationView) _then) = _$QuotationViewCopyWithImpl;
@useResult
$Res call({
 String workflowStatus, Quotation? quotation
});


$QuotationCopyWith<$Res>? get quotation;

}
/// @nodoc
class _$QuotationViewCopyWithImpl<$Res>
    implements $QuotationViewCopyWith<$Res> {
  _$QuotationViewCopyWithImpl(this._self, this._then);

  final QuotationView _self;
  final $Res Function(QuotationView) _then;

/// Create a copy of QuotationView
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? workflowStatus = null,Object? quotation = freezed,}) {
  return _then(QuotationView(
workflowStatus: null == workflowStatus ? _self.workflowStatus : workflowStatus // ignore: cast_nullable_to_non_nullable
as String,quotation: freezed == quotation ? _self.quotation : quotation // ignore: cast_nullable_to_non_nullable
as Quotation?,
  ));
}
/// Create a copy of QuotationView
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$QuotationCopyWith<$Res>? get quotation {
    if (_self.quotation == null) {
    return null;
  }

  return $QuotationCopyWith<$Res>(_self.quotation!, (value) {
    return _then(_self.copyWith(quotation: value));
  });
}
}


/// Adds pattern-matching-related methods to [QuotationView].
extension QuotationViewPatterns on QuotationView {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _QuotationView value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _QuotationView() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _QuotationView value)  $default,){
final _that = this;
switch (_that) {
case _QuotationView():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _QuotationView value)?  $default,){
final _that = this;
switch (_that) {
case _QuotationView() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String workflowStatus,  Quotation? quotation)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _QuotationView() when $default != null:
return $default(_that.workflowStatus,_that.quotation);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String workflowStatus,  Quotation? quotation)  $default,) {final _that = this;
switch (_that) {
case _QuotationView():
return $default(_that.workflowStatus,_that.quotation);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String workflowStatus,  Quotation? quotation)?  $default,) {final _that = this;
switch (_that) {
case _QuotationView() when $default != null:
return $default(_that.workflowStatus,_that.quotation);case _:
  return null;

}
}

}

/// @nodoc


class _QuotationView implements QuotationView {
  const _QuotationView({required this.workflowStatus, this.quotation});
  

@override final  String workflowStatus;
@override final  Quotation? quotation;

/// Create a copy of QuotationView
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$QuotationViewCopyWith<_QuotationView> get copyWith => __$QuotationViewCopyWithImpl<_QuotationView>(this, _$identity);



@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _QuotationView&&(identical(other.workflowStatus, workflowStatus) || other.workflowStatus == workflowStatus)&&(identical(other.quotation, quotation) || other.quotation == quotation));
}


@override
int get hashCode {
    return Object.hash(runtimeType,workflowStatus,quotation);
}

@override
String toString() {
    return 'QuotationView(workflowStatus: $workflowStatus, quotation: $quotation)';
}


}

/// @nodoc
abstract mixin class _$QuotationViewCopyWith<$Res> implements $QuotationViewCopyWith<$Res> {
  factory _$QuotationViewCopyWith(_QuotationView value, $Res Function(_QuotationView) _then) = __$QuotationViewCopyWithImpl;
@override @useResult
$Res call({
 String workflowStatus, Quotation? quotation
});


@override $QuotationCopyWith<$Res>? get quotation;

}
/// @nodoc
class __$QuotationViewCopyWithImpl<$Res>
    implements _$QuotationViewCopyWith<$Res> {
  __$QuotationViewCopyWithImpl(this._self, this._then);

  final _QuotationView _self;
  final $Res Function(_QuotationView) _then;

/// Create a copy of QuotationView
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? workflowStatus = null,Object? quotation = freezed,}) {
  return _then(_QuotationView(
workflowStatus: null == workflowStatus ? _self.workflowStatus : workflowStatus // ignore: cast_nullable_to_non_nullable
as String,quotation: freezed == quotation ? _self.quotation : quotation // ignore: cast_nullable_to_non_nullable
as Quotation?,
  ));
}

/// Create a copy of QuotationView
/// with the given fields replaced by the non-null parameter values.
@override
@pragma('vm:prefer-inline')
$QuotationCopyWith<$Res>? get quotation {
    if (_self.quotation == null) {
    return null;
  }

  return $QuotationCopyWith<$Res>(_self.quotation!, (value) {
    return _then(_self.copyWith(quotation: value));
  });
}
}


/// @nodoc
mixin _$TripStatusItem {

 String get id; String get objective; String get status;
/// Create a copy of TripStatusItem
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$TripStatusItemCopyWith<TripStatusItem> get copyWith => _$TripStatusItemCopyWithImpl<TripStatusItem>(this as TripStatusItem, _$identity);

  /// Serializes this TripStatusItem to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as TripStatusItem;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is TripStatusItem&&(identical(other.id, _this.id) || other.id == _this.id)&&(identical(other.objective, _this.objective) || other.objective == _this.objective)&&(identical(other.status, _this.status) || other.status == _this.status));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as TripStatusItem;
  return Object.hash(runtimeType,_this.id,_this.objective,_this.status);
}

@override
String toString() {
  final _this = this as TripStatusItem;
  return 'TripStatusItem(id: ${_this.id}, objective: ${_this.objective}, status: ${_this.status})';
}


}

/// @nodoc
abstract mixin class $TripStatusItemCopyWith<$Res>  {
  factory $TripStatusItemCopyWith(TripStatusItem value, $Res Function(TripStatusItem) _then) = _$TripStatusItemCopyWithImpl;
@useResult
$Res call({
 String id, String objective, String status
});




}
/// @nodoc
class _$TripStatusItemCopyWithImpl<$Res>
    implements $TripStatusItemCopyWith<$Res> {
  _$TripStatusItemCopyWithImpl(this._self, this._then);

  final TripStatusItem _self;
  final $Res Function(TripStatusItem) _then;

/// Create a copy of TripStatusItem
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? id = null,Object? objective = null,Object? status = null,}) {
  return _then(TripStatusItem(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [TripStatusItem].
extension TripStatusItemPatterns on TripStatusItem {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _TripStatusItem value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _TripStatusItem() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _TripStatusItem value)  $default,){
final _that = this;
switch (_that) {
case _TripStatusItem():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _TripStatusItem value)?  $default,){
final _that = this;
switch (_that) {
case _TripStatusItem() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String id,  String objective,  String status)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _TripStatusItem() when $default != null:
return $default(_that.id,_that.objective,_that.status);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String id,  String objective,  String status)  $default,) {final _that = this;
switch (_that) {
case _TripStatusItem():
return $default(_that.id,_that.objective,_that.status);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String id,  String objective,  String status)?  $default,) {final _that = this;
switch (_that) {
case _TripStatusItem() when $default != null:
return $default(_that.id,_that.objective,_that.status);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _TripStatusItem implements TripStatusItem {
  const _TripStatusItem({required this.id, required this.objective, required this.status});
  factory _TripStatusItem.fromJson(Map<String, dynamic> json) => _$TripStatusItemFromJson(json);

@override final  String id;
@override final  String objective;
@override final  String status;

/// Create a copy of TripStatusItem
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$TripStatusItemCopyWith<_TripStatusItem> get copyWith => __$TripStatusItemCopyWithImpl<_TripStatusItem>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$TripStatusItemToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _TripStatusItem&&(identical(other.id, id) || other.id == id)&&(identical(other.objective, objective) || other.objective == objective)&&(identical(other.status, status) || other.status == status));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,id,objective,status);
}

@override
String toString() {
    return 'TripStatusItem(id: $id, objective: $objective, status: $status)';
}


}

/// @nodoc
abstract mixin class _$TripStatusItemCopyWith<$Res> implements $TripStatusItemCopyWith<$Res> {
  factory _$TripStatusItemCopyWith(_TripStatusItem value, $Res Function(_TripStatusItem) _then) = __$TripStatusItemCopyWithImpl;
@override @useResult
$Res call({
 String id, String objective, String status
});




}
/// @nodoc
class __$TripStatusItemCopyWithImpl<$Res>
    implements _$TripStatusItemCopyWith<$Res> {
  __$TripStatusItemCopyWithImpl(this._self, this._then);

  final _TripStatusItem _self;
  final $Res Function(_TripStatusItem) _then;

/// Create a copy of TripStatusItem
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? id = null,Object? objective = null,Object? status = null,}) {
  return _then(_TripStatusItem(
id: null == id ? _self.id : id // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,status: null == status ? _self.status : status // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}

/// @nodoc
mixin _$StatusChange {

 String get tripId; String get objective; String get from; String get to; DateTime get at;
/// Create a copy of StatusChange
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$StatusChangeCopyWith<StatusChange> get copyWith => _$StatusChangeCopyWithImpl<StatusChange>(this as StatusChange, _$identity);



@override
bool operator ==(Object other) {
  final _this = this as StatusChange;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is StatusChange&&(identical(other.tripId, _this.tripId) || other.tripId == _this.tripId)&&(identical(other.objective, _this.objective) || other.objective == _this.objective)&&(identical(other.from, _this.from) || other.from == _this.from)&&(identical(other.to, _this.to) || other.to == _this.to)&&(identical(other.at, _this.at) || other.at == _this.at));
}


@override
int get hashCode {
  final _this = this as StatusChange;
  return Object.hash(runtimeType,_this.tripId,_this.objective,_this.from,_this.to,_this.at);
}

@override
String toString() {
  final _this = this as StatusChange;
  return 'StatusChange(tripId: ${_this.tripId}, objective: ${_this.objective}, from: ${_this.from}, to: ${_this.to}, at: ${_this.at})';
}


}

/// @nodoc
abstract mixin class $StatusChangeCopyWith<$Res>  {
  factory $StatusChangeCopyWith(StatusChange value, $Res Function(StatusChange) _then) = _$StatusChangeCopyWithImpl;
@useResult
$Res call({
 String tripId, String objective, String from, String to, DateTime at
});




}
/// @nodoc
class _$StatusChangeCopyWithImpl<$Res>
    implements $StatusChangeCopyWith<$Res> {
  _$StatusChangeCopyWithImpl(this._self, this._then);

  final StatusChange _self;
  final $Res Function(StatusChange) _then;

/// Create a copy of StatusChange
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? tripId = null,Object? objective = null,Object? from = null,Object? to = null,Object? at = null,}) {
  return _then(StatusChange(
tripId: null == tripId ? _self.tripId : tripId // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,from: null == from ? _self.from : from // ignore: cast_nullable_to_non_nullable
as String,to: null == to ? _self.to : to // ignore: cast_nullable_to_non_nullable
as String,at: null == at ? _self.at : at // ignore: cast_nullable_to_non_nullable
as DateTime,
  ));
}

}


/// Adds pattern-matching-related methods to [StatusChange].
extension StatusChangePatterns on StatusChange {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _StatusChange value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _StatusChange() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _StatusChange value)  $default,){
final _that = this;
switch (_that) {
case _StatusChange():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _StatusChange value)?  $default,){
final _that = this;
switch (_that) {
case _StatusChange() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String tripId,  String objective,  String from,  String to,  DateTime at)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _StatusChange() when $default != null:
return $default(_that.tripId,_that.objective,_that.from,_that.to,_that.at);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String tripId,  String objective,  String from,  String to,  DateTime at)  $default,) {final _that = this;
switch (_that) {
case _StatusChange():
return $default(_that.tripId,_that.objective,_that.from,_that.to,_that.at);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String tripId,  String objective,  String from,  String to,  DateTime at)?  $default,) {final _that = this;
switch (_that) {
case _StatusChange() when $default != null:
return $default(_that.tripId,_that.objective,_that.from,_that.to,_that.at);case _:
  return null;

}
}

}

/// @nodoc


class _StatusChange implements StatusChange {
  const _StatusChange({required this.tripId, required this.objective, required this.from, required this.to, required this.at});
  

@override final  String tripId;
@override final  String objective;
@override final  String from;
@override final  String to;
@override final  DateTime at;

/// Create a copy of StatusChange
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$StatusChangeCopyWith<_StatusChange> get copyWith => __$StatusChangeCopyWithImpl<_StatusChange>(this, _$identity);



@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _StatusChange&&(identical(other.tripId, tripId) || other.tripId == tripId)&&(identical(other.objective, objective) || other.objective == objective)&&(identical(other.from, from) || other.from == from)&&(identical(other.to, to) || other.to == to)&&(identical(other.at, at) || other.at == at));
}


@override
int get hashCode {
    return Object.hash(runtimeType,tripId,objective,from,to,at);
}

@override
String toString() {
    return 'StatusChange(tripId: $tripId, objective: $objective, from: $from, to: $to, at: $at)';
}


}

/// @nodoc
abstract mixin class _$StatusChangeCopyWith<$Res> implements $StatusChangeCopyWith<$Res> {
  factory _$StatusChangeCopyWith(_StatusChange value, $Res Function(_StatusChange) _then) = __$StatusChangeCopyWithImpl;
@override @useResult
$Res call({
 String tripId, String objective, String from, String to, DateTime at
});




}
/// @nodoc
class __$StatusChangeCopyWithImpl<$Res>
    implements _$StatusChangeCopyWith<$Res> {
  __$StatusChangeCopyWithImpl(this._self, this._then);

  final _StatusChange _self;
  final $Res Function(_StatusChange) _then;

/// Create a copy of StatusChange
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? tripId = null,Object? objective = null,Object? from = null,Object? to = null,Object? at = null,}) {
  return _then(_StatusChange(
tripId: null == tripId ? _self.tripId : tripId // ignore: cast_nullable_to_non_nullable
as String,objective: null == objective ? _self.objective : objective // ignore: cast_nullable_to_non_nullable
as String,from: null == from ? _self.from : from // ignore: cast_nullable_to_non_nullable
as String,to: null == to ? _self.to : to // ignore: cast_nullable_to_non_nullable
as String,at: null == at ? _self.at : at // ignore: cast_nullable_to_non_nullable
as DateTime,
  ));
}


}

// dart format on
