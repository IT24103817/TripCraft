import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/api/api_providers.dart';
import 'guide_models.dart';

part 'resources_repository.g.dart';

/// Every Resource Management call a Guide makes (Component B).
class ResourcesRepository {
  ResourcesRepository(this._api);

  final ApiClient _api;

  /// GET /api/guides/me/schedule — only the trips this guide is held for.
  Future<GuideSchedule> mySchedule() async => GuideSchedule.fromJson(
    await _api.get('/api/guides/me/schedule') as Map<String, dynamic>,
  );

  /// POST /api/check-ins — the API checks the 500 m rule again and moves the trip to InProgress / Completed.
  Future<CheckInResult> checkIn(
    String stopId,
    double latitude,
    double longitude,
  ) async => CheckInResult.fromJson(
    await _api.post(
      '/api/check-ins',
      body: {
        'itineraryStopId': stopId,
        'latitude': latitude,
        'longitude': longitude,
      },
    ) as Map<String, dynamic>,
  );

  /// GET /api/hotels/{id} — hotel details for a scanned voucher.
  Future<HotelInfo> hotel(String id) async => HotelInfo.fromJson(
    await _api.get('/api/hotels/$id') as Map<String, dynamic>,
  );
}

@riverpod
ResourcesRepository resourcesRepository(Ref ref) =>
    ResourcesRepository(ref.watch(apiClientProvider));

@riverpod
Future<GuideSchedule> mySchedule(Ref ref) =>
    ref.watch(resourcesRepositoryProvider).mySchedule();

/// A voucher QR holds the hotel id, optionally prefixed with `TRIPCRAFT-HOTEL:`. Null if it is not a hotel voucher.
String? hotelIdFromVoucher(String code) {
  final value = code.trim().replaceFirst(
    RegExp(r'^TRIPCRAFT-HOTEL:', caseSensitive: false),
    '',
  );
  final uuid = RegExp(
    r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$',
  );
  return uuid.hasMatch(value) ? value.toLowerCase() : null;
}
