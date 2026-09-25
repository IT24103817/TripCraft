// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'quotation_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_Quotation _$QuotationFromJson(Map<String, dynamic> json) => _Quotation(
  lines:
      (json['lines'] as List<dynamic>?)
          ?.map((e) => QuotationLine.fromJson(e as Map<String, dynamic>))
          .toList() ??
      const <QuotationLine>[],
  subtotalLkr: (json['subtotal_lkr'] as num).toDouble(),
  marginPct: (json['margin_pct'] as num).toDouble(),
  marginLkr: (json['margin_lkr'] as num).toDouble(),
  totalLkr: (json['total_lkr'] as num).toDouble(),
  fxRate: (json['fx_rate'] as num).toDouble(),
  fxAsOf: json['fx_as_of'] as String,
  fxStale: json['fx_stale'] as bool? ?? false,
  totalUsd: (json['total_usd'] as num).toDouble(),
);

Map<String, dynamic> _$QuotationToJson(_Quotation instance) =>
    <String, dynamic>{
      'lines': instance.lines,
      'subtotal_lkr': instance.subtotalLkr,
      'margin_pct': instance.marginPct,
      'margin_lkr': instance.marginLkr,
      'total_lkr': instance.totalLkr,
      'fx_rate': instance.fxRate,
      'fx_as_of': instance.fxAsOf,
      'fx_stale': instance.fxStale,
      'total_usd': instance.totalUsd,
    };

_QuotationLine _$QuotationLineFromJson(Map<String, dynamic> json) =>
    _QuotationLine(
      lineType: json['line_type'] as String,
      description: json['description'] as String,
      qty: (json['qty'] as num).toDouble(),
      unitLkr: (json['unit_lkr'] as num).toDouble(),
      amountLkr: (json['amount_lkr'] as num).toDouble(),
    );

Map<String, dynamic> _$QuotationLineToJson(_QuotationLine instance) =>
    <String, dynamic>{
      'line_type': instance.lineType,
      'description': instance.description,
      'qty': instance.qty,
      'unit_lkr': instance.unitLkr,
      'amount_lkr': instance.amountLkr,
    };

_TripStatusItem _$TripStatusItemFromJson(Map<String, dynamic> json) =>
    _TripStatusItem(
      id: json['id'] as String,
      objective: json['objective'] as String,
      status: json['status'] as String,
    );

Map<String, dynamic> _$TripStatusItemToJson(_TripStatusItem instance) =>
    <String, dynamic>{
      'id': instance.id,
      'objective': instance.objective,
      'status': instance.status,
    };
