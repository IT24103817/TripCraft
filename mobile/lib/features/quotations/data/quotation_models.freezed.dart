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

 String get tripStatus; String get workflowStatus; Quotation? get quotation; String? get quotationId; String? get quotationStatus; String? get acceptedAt;
/// Create a copy of QuotationView
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$QuotationViewCopyWith<QuotationView> get copyWith => _$QuotationViewCopyWithImpl<QuotationView>(this as QuotationView, _$identity);



@override
bool operator ==(Object other) {
  final _this = this as QuotationView;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is QuotationView&&(identical(other.tripStatus, _this.tripStatus) || other.tripStatus == _this.tripStatus)&&(identical(other.workflowStatus, _this.workflowStatus) || other.workflowStatus == _this.workflowStatus)&&(identical(other.quotation, _this.quotation) || other.quotation == _this.quotation)&&(identical(other.quotationId, _this.quotationId) || other.quotationId == _this.quotationId)&&(identical(other.quotationStatus, _this.quotationStatus) || other.quotationStatus == _this.quotationStatus)&&(identical(other.acceptedAt, _this.acceptedAt) || other.acceptedAt == _this.acceptedAt));
}


@override
int get hashCode {
  final _this = this as QuotationView;
  return Object.hash(runtimeType,_this.tripStatus,_this.workflowStatus,_this.quotation,_this.quotationId,_this.quotationStatus,_this.acceptedAt);
}

@override
String toString() {
  final _this = this as QuotationView;
  return 'QuotationView(tripStatus: ${_this.tripStatus}, workflowStatus: ${_this.workflowStatus}, quotation: ${_this.quotation}, quotationId: ${_this.quotationId}, quotationStatus: ${_this.quotationStatus}, acceptedAt: ${_this.acceptedAt})';
}


}

/// @nodoc
abstract mixin class $QuotationViewCopyWith<$Res>  {
  factory $QuotationViewCopyWith(QuotationView value, $Res Function(QuotationView) _then) = _$QuotationViewCopyWithImpl;
@useResult
$Res call({
 String tripStatus, String workflowStatus, Quotation? quotation, String? quotationId, String? quotationStatus, String? acceptedAt
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
@pragma('vm:prefer-inline') @override $Res call({Object? tripStatus = null,Object? workflowStatus = null,Object? quotation = freezed,Object? quotationId = freezed,Object? quotationStatus = freezed,Object? acceptedAt = freezed,}) {
  return _then(QuotationView(
tripStatus: null == tripStatus ? _self.tripStatus : tripStatus // ignore: cast_nullable_to_non_nullable
as String,workflowStatus: null == workflowStatus ? _self.workflowStatus : workflowStatus // ignore: cast_nullable_to_non_nullable
as String,quotation: freezed == quotation ? _self.quotation : quotation // ignore: cast_nullable_to_non_nullable
as Quotation?,quotationId: freezed == quotationId ? _self.quotationId : quotationId // ignore: cast_nullable_to_non_nullable
as String?,quotationStatus: freezed == quotationStatus ? _self.quotationStatus : quotationStatus // ignore: cast_nullable_to_non_nullable
as String?,acceptedAt: freezed == acceptedAt ? _self.acceptedAt : acceptedAt // ignore: cast_nullable_to_non_nullable
as String?,
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String tripStatus,  String workflowStatus,  Quotation? quotation,  String? quotationId,  String? quotationStatus,  String? acceptedAt)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _QuotationView() when $default != null:
return $default(_that.tripStatus,_that.workflowStatus,_that.quotation,_that.quotationId,_that.quotationStatus,_that.acceptedAt);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String tripStatus,  String workflowStatus,  Quotation? quotation,  String? quotationId,  String? quotationStatus,  String? acceptedAt)  $default,) {final _that = this;
switch (_that) {
case _QuotationView():
return $default(_that.tripStatus,_that.workflowStatus,_that.quotation,_that.quotationId,_that.quotationStatus,_that.acceptedAt);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String tripStatus,  String workflowStatus,  Quotation? quotation,  String? quotationId,  String? quotationStatus,  String? acceptedAt)?  $default,) {final _that = this;
switch (_that) {
case _QuotationView() when $default != null:
return $default(_that.tripStatus,_that.workflowStatus,_that.quotation,_that.quotationId,_that.quotationStatus,_that.acceptedAt);case _:
  return null;

}
}

}

/// @nodoc


class _QuotationView implements QuotationView {
  const _QuotationView({required this.tripStatus, required this.workflowStatus, this.quotation, this.quotationId, this.quotationStatus, this.acceptedAt});
  

@override final  String tripStatus;
@override final  String workflowStatus;
@override final  Quotation? quotation;
@override final  String? quotationId;
@override final  String? quotationStatus;
@override final  String? acceptedAt;

/// Create a copy of QuotationView
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$QuotationViewCopyWith<_QuotationView> get copyWith => __$QuotationViewCopyWithImpl<_QuotationView>(this, _$identity);



@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _QuotationView&&(identical(other.tripStatus, tripStatus) || other.tripStatus == tripStatus)&&(identical(other.workflowStatus, workflowStatus) || other.workflowStatus == workflowStatus)&&(identical(other.quotation, quotation) || other.quotation == quotation)&&(identical(other.quotationId, quotationId) || other.quotationId == quotationId)&&(identical(other.quotationStatus, quotationStatus) || other.quotationStatus == quotationStatus)&&(identical(other.acceptedAt, acceptedAt) || other.acceptedAt == acceptedAt));
}


@override
int get hashCode {
    return Object.hash(runtimeType,tripStatus,workflowStatus,quotation,quotationId,quotationStatus,acceptedAt);
}

@override
String toString() {
    return 'QuotationView(tripStatus: $tripStatus, workflowStatus: $workflowStatus, quotation: $quotation, quotationId: $quotationId, quotationStatus: $quotationStatus, acceptedAt: $acceptedAt)';
}


}

/// @nodoc
abstract mixin class _$QuotationViewCopyWith<$Res> implements $QuotationViewCopyWith<$Res> {
  factory _$QuotationViewCopyWith(_QuotationView value, $Res Function(_QuotationView) _then) = __$QuotationViewCopyWithImpl;
@override @useResult
$Res call({
 String tripStatus, String workflowStatus, Quotation? quotation, String? quotationId, String? quotationStatus, String? acceptedAt
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
@override @pragma('vm:prefer-inline') $Res call({Object? tripStatus = null,Object? workflowStatus = null,Object? quotation = freezed,Object? quotationId = freezed,Object? quotationStatus = freezed,Object? acceptedAt = freezed,}) {
  return _then(_QuotationView(
tripStatus: null == tripStatus ? _self.tripStatus : tripStatus // ignore: cast_nullable_to_non_nullable
as String,workflowStatus: null == workflowStatus ? _self.workflowStatus : workflowStatus // ignore: cast_nullable_to_non_nullable
as String,quotation: freezed == quotation ? _self.quotation : quotation // ignore: cast_nullable_to_non_nullable
as Quotation?,quotationId: freezed == quotationId ? _self.quotationId : quotationId // ignore: cast_nullable_to_non_nullable
as String?,quotationStatus: freezed == quotationStatus ? _self.quotationStatus : quotationStatus // ignore: cast_nullable_to_non_nullable
as String?,acceptedAt: freezed == acceptedAt ? _self.acceptedAt : acceptedAt // ignore: cast_nullable_to_non_nullable
as String?,
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
mixin _$QuotationDecision {

 String get quotationId; String get tripRequestId; String get decision; String get tripStatus;
/// Create a copy of QuotationDecision
/// with the given fields replaced by the non-null parameter values.
@JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
$QuotationDecisionCopyWith<QuotationDecision> get copyWith => _$QuotationDecisionCopyWithImpl<QuotationDecision>(this as QuotationDecision, _$identity);

  /// Serializes this QuotationDecision to a JSON map.
  Map<String, dynamic> toJson();


@override
bool operator ==(Object other) {
  final _this = this as QuotationDecision;
  return identical(this, other) || (other.runtimeType == runtimeType&&other is QuotationDecision&&(identical(other.quotationId, _this.quotationId) || other.quotationId == _this.quotationId)&&(identical(other.tripRequestId, _this.tripRequestId) || other.tripRequestId == _this.tripRequestId)&&(identical(other.decision, _this.decision) || other.decision == _this.decision)&&(identical(other.tripStatus, _this.tripStatus) || other.tripStatus == _this.tripStatus));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
  final _this = this as QuotationDecision;
  return Object.hash(runtimeType,_this.quotationId,_this.tripRequestId,_this.decision,_this.tripStatus);
}

@override
String toString() {
  final _this = this as QuotationDecision;
  return 'QuotationDecision(quotationId: ${_this.quotationId}, tripRequestId: ${_this.tripRequestId}, decision: ${_this.decision}, tripStatus: ${_this.tripStatus})';
}


}

/// @nodoc
abstract mixin class $QuotationDecisionCopyWith<$Res>  {
  factory $QuotationDecisionCopyWith(QuotationDecision value, $Res Function(QuotationDecision) _then) = _$QuotationDecisionCopyWithImpl;
@useResult
$Res call({
 String quotationId, String tripRequestId, String decision, String tripStatus
});




}
/// @nodoc
class _$QuotationDecisionCopyWithImpl<$Res>
    implements $QuotationDecisionCopyWith<$Res> {
  _$QuotationDecisionCopyWithImpl(this._self, this._then);

  final QuotationDecision _self;
  final $Res Function(QuotationDecision) _then;

/// Create a copy of QuotationDecision
/// with the given fields replaced by the non-null parameter values.
@pragma('vm:prefer-inline') @override $Res call({Object? quotationId = null,Object? tripRequestId = null,Object? decision = null,Object? tripStatus = null,}) {
  return _then(QuotationDecision(
quotationId: null == quotationId ? _self.quotationId : quotationId // ignore: cast_nullable_to_non_nullable
as String,tripRequestId: null == tripRequestId ? _self.tripRequestId : tripRequestId // ignore: cast_nullable_to_non_nullable
as String,decision: null == decision ? _self.decision : decision // ignore: cast_nullable_to_non_nullable
as String,tripStatus: null == tripStatus ? _self.tripStatus : tripStatus // ignore: cast_nullable_to_non_nullable
as String,
  ));
}

}


/// Adds pattern-matching-related methods to [QuotationDecision].
extension QuotationDecisionPatterns on QuotationDecision {
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

@optionalTypeArgs TResult maybeMap<TResult extends Object?>(TResult Function( _QuotationDecision value)?  $default,{required TResult orElse(),}){
final _that = this;
switch (_that) {
case _QuotationDecision() when $default != null:
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

@optionalTypeArgs TResult map<TResult extends Object?>(TResult Function( _QuotationDecision value)  $default,){
final _that = this;
switch (_that) {
case _QuotationDecision():
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

@optionalTypeArgs TResult? mapOrNull<TResult extends Object?>(TResult? Function( _QuotationDecision value)?  $default,){
final _that = this;
switch (_that) {
case _QuotationDecision() when $default != null:
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

@optionalTypeArgs TResult maybeWhen<TResult extends Object?>(TResult Function( String quotationId,  String tripRequestId,  String decision,  String tripStatus)?  $default,{required TResult orElse(),}) {final _that = this;
switch (_that) {
case _QuotationDecision() when $default != null:
return $default(_that.quotationId,_that.tripRequestId,_that.decision,_that.tripStatus);case _:
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

@optionalTypeArgs TResult when<TResult extends Object?>(TResult Function( String quotationId,  String tripRequestId,  String decision,  String tripStatus)  $default,) {final _that = this;
switch (_that) {
case _QuotationDecision():
return $default(_that.quotationId,_that.tripRequestId,_that.decision,_that.tripStatus);case _:
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

@optionalTypeArgs TResult? whenOrNull<TResult extends Object?>(TResult? Function( String quotationId,  String tripRequestId,  String decision,  String tripStatus)?  $default,) {final _that = this;
switch (_that) {
case _QuotationDecision() when $default != null:
return $default(_that.quotationId,_that.tripRequestId,_that.decision,_that.tripStatus);case _:
  return null;

}
}

}

/// @nodoc
@JsonSerializable()

class _QuotationDecision implements QuotationDecision {
  const _QuotationDecision({required this.quotationId, required this.tripRequestId, required this.decision, required this.tripStatus});
  factory _QuotationDecision.fromJson(Map<String, dynamic> json) => _$QuotationDecisionFromJson(json);

@override final  String quotationId;
@override final  String tripRequestId;
@override final  String decision;
@override final  String tripStatus;

/// Create a copy of QuotationDecision
/// with the given fields replaced by the non-null parameter values.
@override @JsonKey(includeFromJson: false, includeToJson: false)
@pragma('vm:prefer-inline')
_$QuotationDecisionCopyWith<_QuotationDecision> get copyWith => __$QuotationDecisionCopyWithImpl<_QuotationDecision>(this, _$identity);

@override
Map<String, dynamic> toJson() {
  return _$QuotationDecisionToJson(this, );
}

@override
bool operator ==(Object other) {
    return identical(this, other) || (other.runtimeType == runtimeType&&other is _QuotationDecision&&(identical(other.quotationId, quotationId) || other.quotationId == quotationId)&&(identical(other.tripRequestId, tripRequestId) || other.tripRequestId == tripRequestId)&&(identical(other.decision, decision) || other.decision == decision)&&(identical(other.tripStatus, tripStatus) || other.tripStatus == tripStatus));
}

@JsonKey(includeFromJson: false, includeToJson: false)
@override
int get hashCode {
    return Object.hash(runtimeType,quotationId,tripRequestId,decision,tripStatus);
}

@override
String toString() {
    return 'QuotationDecision(quotationId: $quotationId, tripRequestId: $tripRequestId, decision: $decision, tripStatus: $tripStatus)';
}


}

/// @nodoc
abstract mixin class _$QuotationDecisionCopyWith<$Res> implements $QuotationDecisionCopyWith<$Res> {
  factory _$QuotationDecisionCopyWith(_QuotationDecision value, $Res Function(_QuotationDecision) _then) = __$QuotationDecisionCopyWithImpl;
@override @useResult
$Res call({
 String quotationId, String tripRequestId, String decision, String tripStatus
});




}
/// @nodoc
class __$QuotationDecisionCopyWithImpl<$Res>
    implements _$QuotationDecisionCopyWith<$Res> {
  __$QuotationDecisionCopyWithImpl(this._self, this._then);

  final _QuotationDecision _self;
  final $Res Function(_QuotationDecision) _then;

/// Create a copy of QuotationDecision
/// with the given fields replaced by the non-null parameter values.
@override @pragma('vm:prefer-inline') $Res call({Object? quotationId = null,Object? tripRequestId = null,Object? decision = null,Object? tripStatus = null,}) {
  return _then(_QuotationDecision(
quotationId: null == quotationId ? _self.quotationId : quotationId // ignore: cast_nullable_to_non_nullable
as String,tripRequestId: null == tripRequestId ? _self.tripRequestId : tripRequestId // ignore: cast_nullable_to_non_nullable
as String,decision: null == decision ? _self.decision : decision // ignore: cast_nullable_to_non_nullable
as String,tripStatus: null == tripStatus ? _self.tripStatus : tripStatus // ignore: cast_nullable_to_non_nullable
as String,
  ));
}


}

// dart format on
