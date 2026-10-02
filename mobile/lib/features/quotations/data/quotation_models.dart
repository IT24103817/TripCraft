import 'package:freezed_annotation/freezed_annotation.dart';

part 'quotation_models.freezed.dart';
part 'quotation_models.g.dart';

/// The quotation inside the agents' proposal (finalOutcome.proposal.quotation), snake_case from the agent.
/// [bestAvailablePrice] is true when the agents could not get under the budget even with the cheapest plan:
/// the quote is sent anyway, [overBudgetUsd] says by how much and [budgetNote] is the sentence to show.
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
    @JsonKey(name: 'best_available_price')
    @Default(false)
    bool bestAvailablePrice,
    @JsonKey(name: 'over_budget_usd') double? overBudgetUsd,
    @JsonKey(name: 'budget_note') String? budgetNote,
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

/// What the tourist sees: the quotation plus the trip and workflow status it belongs to, and (once the quotation
/// is stored by the API) its id, version (2 or more after the operator edited and resent it), status
/// (Pending, Approved = sent, Declined, ...) and when the tourist accepted it.
@freezed
abstract class QuotationView with _$QuotationView {
  const factory QuotationView({
    required String tripStatus,
    required String workflowStatus,
    Quotation? quotation,
    String? quotationId,
    int? version,
    String? quotationStatus,
    String? acceptedAt,
  }) = _QuotationView;
}

/// Response of POST /api/quotations/{id}/accept and /decline (QuotationDecisionResponse).
@freezed
abstract class QuotationDecision with _$QuotationDecision {
  const factory QuotationDecision({
    required String quotationId,
    required String tripRequestId,
    required String decision,
    required String tripStatus,
  }) = _QuotationDecision;

  factory QuotationDecision.fromJson(Map<String, dynamic> json) =>
      _$QuotationDecisionFromJson(json);
}
