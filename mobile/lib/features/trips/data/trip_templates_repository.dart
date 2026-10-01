import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/api/api_providers.dart';
import 'template_models.dart';

part 'trip_templates_repository.g.dart';

/// The mood packages on the tourist's home screen (v1.1).
class TripTemplatesRepository {
  TripTemplatesRepository(this._api);

  final ApiClient _api;

  /// GET /api/trip-templates: every active package, without its itinerary.
  Future<List<TripTemplate>> templates() async {
    final json = await _api.get('/api/trip-templates') as List<dynamic>;
    return json
        .map((e) => TripTemplate.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  /// GET /api/trip-templates/{id}: one package with its day-by-day itinerary.
  Future<TripTemplate> template(String id) async => TripTemplate.fromJson(
    await _api.get('/api/trip-templates/$id') as Map<String, dynamic>,
  );

  /// POST /api/trip-templates/{id}/book ("Book as is"): creates the trip and starts planning it.
  Future<BookTemplateResult> book(
    String templateId,
    BookTemplateRequest request,
  ) async => BookTemplateResult.fromJson(
    await _api.post(
      '/api/trip-templates/$templateId/book',
      body: request.toJson(),
    ) as Map<String, dynamic>,
  );
}

@riverpod
TripTemplatesRepository tripTemplatesRepository(Ref ref) =>
    TripTemplatesRepository(ref.watch(apiClientProvider));
