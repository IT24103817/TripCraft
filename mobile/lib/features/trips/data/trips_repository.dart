import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/api/api_providers.dart';
import '../../../core/api/paged_result.dart';
import '../../../core/api/user_facing_exception.dart';
import 'trip_models.dart';
import 'weather.dart';

part 'trips_repository.g.dart';

/// Every trip-request call a Tourist makes. The API only ever returns the caller's own trips.
class TripsRepository {
  TripsRepository(this._api);

  final ApiClient _api;

  /// GET /api/trip-requests (a tourist only sees their own; this is the "mine" list).
  Future<List<TripRequest>> myTrips({
    String search = '',
    String status = '',
  }) async {
    final json = await _api.get(
      '/api/trip-requests',
      query: {
        'search': search,
        'status': status,
        'sort': '-createdAt',
        'pageSize': 100,
      },
    );
    return PagedResult.fromJson(
      json as Map<String, dynamic>,
      TripRequest.fromJson,
    ).items;
  }

  Future<TripRequest> trip(String id) async => TripRequest.fromJson(
    await _api.get('/api/trip-requests/$id') as Map<String, dynamic>,
  );

  Future<TripRequest> create(CreateTripRequest request) async =>
      TripRequest.fromJson(
        await _api.post('/api/trip-requests', body: request.toJson())
            as Map<String, dynamic>,
      );

  /// POST /api/trip-requests/{id}/passport-photo as multipart field "file" (JPEG/PNG, ≤ 5 MB).
  Future<void> uploadPassportPhoto(String tripId, String filePath) async {
    final form = FormData.fromMap({
      'file': await MultipartFile.fromFile(filePath),
    });
    await _api.postMultipart('/api/trip-requests/$tripId/passport-photo', form);
  }

  /// POST /api/trip-requests/{id}/cancel {reason}. 409 after the cut-off (the message names the operator contact).
  Future<TripRequest> cancel(String tripId, String reason) async =>
      TripRequest.fromJson(
        await _api.post(
          '/api/trip-requests/$tripId/cancel',
          body: {'reason': reason.trim()},
        ) as Map<String, dynamic>,
      );

  /// GET /api/trip-requests/{id}/cancellation: may the tourist cancel now, and until when.
  Future<CancellationInfo> cancellationInfo(String tripId) async =>
      CancellationInfo.fromJson(
        await _api.get('/api/trip-requests/$tripId/cancellation')
            as Map<String, dynamic>,
      );

  /// GET /api/trips/{id}/vouchers: the trip voucher and one per hotel night (issued when the trip is confirmed).
  Future<List<TripVoucher>> vouchers(String tripId) async {
    final json = await _api.get('/api/trips/$tripId/vouchers') as List<dynamic>;
    return json
        .map((e) => TripVoucher.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  /// GET /api/attractions/cities: the only cities a trip may visit.
  Future<List<String>> cities() async {
    final json = await _api.get('/api/attractions/cities') as List<dynamic>;
    return json.map((city) => '$city').toList();
  }

  /// GET /api/trip-requests/{id}/history: audited events, oldest first.
  Future<List<TripHistoryEntry>> history(String tripId) async {
    final json =
        await _api.get('/api/trip-requests/$tripId/history') as List<dynamic>;
    return json
        .map((e) => TripHistoryEntry.fromJson(e as Map<String, dynamic>))
        .toList();
  }

  Future<StartPlanningResult> startPlanning(String tripId) async =>
      StartPlanningResult.fromJson(
        await _api.post('/api/trip-requests/$tripId/start-planning')
            as Map<String, dynamic>,
      );

  /// GET /api/trip-requests/{id}/workflow. Null before planning has started (404).
  Future<TripWorkflow?> workflow(String tripId) async {
    try {
      return TripWorkflow.fromJson(
        await _api.get('/api/trip-requests/$tripId/workflow')
            as Map<String, dynamic>,
      );
    } on UserFacingException catch (e) {
      if (e.statusCode == 404) return null;
      rethrow;
    }
  }

  /// The saved itinerary (GET /api/trip-requests/{id}/itinerary). Empty before one exists (404).
  /// The weather comes from each day's notes ("By train; weather: Light rain, 24°C").
  Future<List<TripDay>> savedItinerary(String tripId) async {
    try {
      final itinerary = Itinerary.fromJson(
        await _api.get('/api/trip-requests/$tripId/itinerary')
            as Map<String, dynamic>,
      );
      return [
        for (final d in itinerary.days)
          TripDay(
            day: d.dayNumber,
            city: d.city,
            weather: weatherFromNotes(d.notes),
            stops: [
              for (final s in d.stops)
                TripStop(
                  attractionId: s.attractionId,
                  name: s.attractionName,
                  latitude: s.latitude,
                  longitude: s.longitude,
                ),
            ],
          ),
      ];
    } on UserFacingException catch (e) {
      if (e.statusCode == 404) return const [];
      rethrow;
    }
  }

  Future<Attraction> attraction(String id) async => Attraction.fromJson(
    await _api.get('/api/attractions/$id') as Map<String, dynamic>,
  );
}

@riverpod
TripsRepository tripsRepository(Ref ref) =>
    TripsRepository(ref.watch(apiClientProvider));
