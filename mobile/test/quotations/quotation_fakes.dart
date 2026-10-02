import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/quotations/data/quotation_models.dart';
import 'package:tripcraft_mobile/features/quotations/data/quotations_repository.dart';

final quotation = Quotation.fromJson({
  'lines': [
    {
      'line_type': 'guide',
      'description': 'Guide',
      'qty': 5,
      'unit_lkr': 6000,
      'amount_lkr': 30000,
    },
    {
      'line_type': 'room',
      'description': 'Standard double',
      'qty': 8,
      'unit_lkr': 12000,
      'amount_lkr': 96000,
    },
  ],
  'subtotal_lkr': 162800,
  'margin_pct': 15,
  'margin_lkr': 24420,
  'total_lkr': 187220,
  'fx_rate': 300,
  'fx_as_of': '2026-10-01T00:00:00Z',
  'fx_stale': false,
  'total_usd': 624.07,
});

/// The same quotation, still USD 120 over budget after the agents' cheapest plan (sent anyway).
final overBudgetQuotation = quotation.copyWith(
  bestAvailablePrice: true,
  overBudgetUsd: 120,
  budgetNote: 'Best price we can offer — USD 120 above your budget',
);

/// Serves a fixed quotation view and records the tourist's decision instead of calling the API.
class FakeQuotationsRepository extends Fake implements QuotationsRepository {
  FakeQuotationsRepository(this.tripStatus, {this.served, this.version = 1});

  String tripStatus;

  /// The quotation to serve; the plain [quotation] when null.
  final Quotation? served;
  final int version;
  String? acceptedId;
  (String, String)? declined;
  Object? error;

  QuotationDecision _decision(String id, String decision, String status) {
    final failure = error;
    if (failure != null) throw failure;
    tripStatus = status;
    return QuotationDecision(
      quotationId: id,
      tripRequestId: 'trip-1',
      decision: decision,
      tripStatus: status,
    );
  }

  @override
  Future<QuotationView> quotationFor(String tripId) async => QuotationView(
    tripStatus: tripStatus,
    workflowStatus: 'Approved',
    quotation: served ?? quotation,
    quotationId: 'q1',
    version: version,
    quotationStatus: 'Approved',
  );

  @override
  Future<QuotationDecision> accept(String quotationId) async {
    acceptedId = quotationId;
    return _decision(quotationId, 'Accepted', 'ClientAccepted');
  }

  @override
  Future<QuotationDecision> decline(String quotationId, String reason) async {
    declined = (quotationId, reason);
    return _decision(quotationId, 'Declined', 'ClientDeclined');
  }
}
