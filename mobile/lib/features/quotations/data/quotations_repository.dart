import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/api/api_providers.dart';
import 'quotation_models.dart';

part 'quotations_repository.g.dart';

class QuotationsRepository {
  QuotationsRepository(this._api);

  final ApiClient _api;

  /// The trip's quotation: the stored one (GET /api/quotations/{id}) when it exists, otherwise the one inside
  /// the agents' proposal (GET /api/trip-requests/{id}/workflow, finalOutcome.proposal.quotation).
  /// The trip status (GET /api/trip-requests/{id}) decides whether the tourist may accept or decline.
  Future<QuotationView> quotationFor(String tripId) async {
    final trip =
        await _api.get('/api/trip-requests/$tripId') as Map<String, dynamic>;
    final workflow = await _api.get(
      '/api/trip-requests/$tripId/workflow',
    ) as Map<String, dynamic>;
    final outcome = workflow['finalOutcome'] as Map<String, dynamic>?;
    final proposal = outcome?['proposal'] as Map<String, dynamic>?;
    final proposed = proposal?['quotation'] as Map<String, dynamic>?;
    final quotationId = proposal?['quotationId'] as String?;
    final view = QuotationView(
      tripStatus: trip['status'] as String,
      workflowStatus: workflow['status'] as String,
      quotation: proposed == null ? null : Quotation.fromJson(proposed),
      quotationId: quotationId,
    );
    if (quotationId == null) return view;

    final stored =
        await _api.get('/api/quotations/$quotationId') as Map<String, dynamic>;
    return view.copyWith(
      quotation: Quotation.fromJson(_toProposalShape(stored)),
      quotationStatus: stored['status'] as String?,
      acceptedAt: stored['acceptedAt'] as String?,
    );
  }

  /// POST /api/quotations/{id}/accept — only while the trip is QuotationSent (409 otherwise).
  Future<QuotationDecision> accept(String quotationId) async =>
      QuotationDecision.fromJson(
        await _api.post('/api/quotations/$quotationId/accept')
            as Map<String, dynamic>,
      );

  /// POST /api/quotations/{id}/decline {reason}: the trip goes back to the operator (PendingReview).
  Future<QuotationDecision> decline(String quotationId, String reason) async =>
      QuotationDecision.fromJson(
        await _api.post(
          '/api/quotations/$quotationId/decline',
          body: {'reason': reason.trim()},
        ) as Map<String, dynamic>,
      );

  /// The API's camelCase quotation in the agent's snake_case shape that [Quotation] reads.
  static Map<String, dynamic> _toProposalShape(Map<String, dynamic> q) => {
    'lines': [
      for (final l in q['lines'] as List<dynamic>)
        {
          'line_type': l['lineType'],
          'description': l['description'],
          'qty': l['qty'],
          'unit_lkr': l['unitLkr'],
          'amount_lkr': l['amountLkr'],
        },
    ],
    'subtotal_lkr': q['subtotalLkr'],
    'margin_pct': q['marginPct'],
    'margin_lkr': q['marginLkr'],
    'total_lkr': q['totalLkr'],
    'fx_rate': q['fxRate'],
    'fx_as_of': q['fxAsOf'],
    'fx_stale': q['fxStale'],
    'total_usd': q['totalUsd'],
  };
}

@riverpod
QuotationsRepository quotationsRepository(Ref ref) =>
    QuotationsRepository(ref.watch(apiClientProvider));
