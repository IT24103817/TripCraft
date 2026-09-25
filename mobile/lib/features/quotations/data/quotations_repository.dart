import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/api/api_providers.dart';
import '../../../core/api/paged_result.dart';
import 'quotation_models.dart';

part 'quotations_repository.g.dart';

class QuotationsRepository {
  QuotationsRepository(this._api);

  final ApiClient _api;

  /// The quotation the agents proposed, read from GET /api/trip-requests/{id}/workflow
  /// (finalOutcome.proposal.quotation). Student C's quotation endpoints will replace this read.
  Future<QuotationView> quotationFor(String tripId) async {
    final workflow = await _api.get(
      '/api/trip-requests/$tripId/workflow',
    ) as Map<String, dynamic>;
    final outcome = workflow['finalOutcome'] as Map<String, dynamic>?;
    final proposal = outcome?['proposal'] as Map<String, dynamic>?;
    final quotation = proposal?['quotation'] as Map<String, dynamic>?;
    return QuotationView(
      workflowStatus: workflow['status'] as String,
      quotation: quotation == null ? null : Quotation.fromJson(quotation),
    );
  }

  /// Every trip of the signed-in tourist with its status (GET /api/trip-requests only returns their own).
  Future<List<TripStatusItem>> tripStatuses() async {
    final json = await _api.get(
      '/api/trip-requests',
      query: {'pageSize': 100, 'sort': '-createdAt'},
    );
    return PagedResult.fromJson(
      json as Map<String, dynamic>,
      TripStatusItem.fromJson,
    ).items;
  }
}

@riverpod
QuotationsRepository quotationsRepository(Ref ref) =>
    QuotationsRepository(ref.watch(apiClientProvider));
