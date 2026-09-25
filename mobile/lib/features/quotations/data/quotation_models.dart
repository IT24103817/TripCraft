import 'package:freezed_annotation/freezed_annotation.dart';

part 'quotation_models.freezed.dart';
part 'quotation_models.g.dart';

/// The quotation inside the agents' proposal (finalOutcome.proposal.quotation), snake_case from the agent.
@freezed
abstract class Quotation with _$Quotation {
  const factory Quotation({
    @Default(<QuotationLine>[]) List<QuotationLine> lines,
    @JsonKey(name: 'subtotal_lkr') required double subtotalLkr,
    @JsonKey(name: 'margin_pct') required double marginPct,
    @JsonKey(name: 'margin_lkr') required double marginLkr,
    @JsonKey(name: 'total_lkr') required double totalLkr,
    @JsonKey(name: 'fx_rate') required double fxRate,
    @JsonKey(name: 'fx_as_of') required String fxAsOf,
    @JsonKey(name: 'fx_stale') @Default(false) bool fxStale,
    @JsonKey(name: 'total_usd') required double totalUsd,
  }) = _Quotation;

  factory Quotation.fromJson(Map<String, dynamic> json) =>
      _$QuotationFromJson(json);
}

@freezed
abstract class QuotationLine with _$QuotationLine {
  const factory QuotationLine({
    @JsonKey(name: 'line_type') required String lineType,
    required String description,
    required double qty,
    @JsonKey(name: 'unit_lkr') required double unitLkr,
    @JsonKey(name: 'amount_lkr') required double amountLkr,
  }) = _QuotationLine;

  factory QuotationLine.fromJson(Map<String, dynamic> json) =>
      _$QuotationLineFromJson(json);
}

/// What the tourist sees: the quotation plus the workflow status it belongs to.
@freezed
abstract class QuotationView with _$QuotationView {
  const factory QuotationView({
    required String workflowStatus,
    Quotation? quotation,
  }) = _QuotationView;
}

/// One trip in the status history (from GET /api/trip-requests).
@freezed
abstract class TripStatusItem with _$TripStatusItem {
  const factory TripStatusItem({
    required String id,
    required String objective,
    required String status,
  }) = _TripStatusItem;

  factory TripStatusItem.fromJson(Map<String, dynamic> json) =>
      _$TripStatusItemFromJson(json);
}

/// A detected status change, shown in the notifications history.
@freezed
abstract class StatusChange with _$StatusChange {
  const factory StatusChange({
    required String tripId,
    required String objective,
    required String from,
    required String to,
    required DateTime at,
  }) = _StatusChange;
}
