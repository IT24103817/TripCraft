import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/api/api_providers.dart';
import '../../../core/api/user_facing_exception.dart';
import 'assignment_models.dart';

part 'trip_assignment_repository.g.dart';

/// Who is booked for a trip, and the tourist's rating of the guide afterwards (v1.1).
class TripAssignmentRepository {
  TripAssignmentRepository(this._api);

  final ApiClient _api;

  /// GET /api/trip-requests/{id}/assignment (all nulls before the trip is confirmed).
  Future<TripAssignment> assignment(String tripId) async =>
      TripAssignment.fromJson(
        await _api.get('/api/trip-requests/$tripId/assignment')
            as Map<String, dynamic>,
      );

  /// GET /api/trip-requests/{id}/guide-rating. Null when the guide is not rated yet (404).
  Future<GuideRating?> guideRating(String tripId) async {
    try {
      return GuideRating.fromJson(
        await _api.get('/api/trip-requests/$tripId/guide-rating')
            as Map<String, dynamic>,
      );
    } on UserFacingException catch (e) {
      if (e.statusCode == 404) return null;
      rethrow;
    }
  }

  /// POST /api/trip-requests/{id}/guide-rating {stars, comment?}: once, after the trip is Completed (409 otherwise).
  Future<GuideRating> rateGuide(
    String tripId, {
    required int stars,
    String? comment,
  }) async {
    final text = comment?.trim() ?? '';
    return GuideRating.fromJson(
      await _api.post(
        '/api/trip-requests/$tripId/guide-rating',
        body: {'stars': stars, 'comment': text.isEmpty ? null : text},
      ) as Map<String, dynamic>,
    );
  }
}

@riverpod
TripAssignmentRepository tripAssignmentRepository(Ref ref) =>
    TripAssignmentRepository(ref.watch(apiClientProvider));
