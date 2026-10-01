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

  /// POST /api/check-ins (GPS) — the API checks the 500 m rule again and moves the trip to InProgress / Completed.
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

  /// POST /api/check-ins with a scanned trip voucher: the API checks in the next stop of today.
  /// [tripRequestId] is the trip the guide opened the scanner from, if any (the voucher must belong to it).
  Future<CheckInResult> checkInWithVoucher(
    String voucherCode, {
    String? tripRequestId,
  }) async => CheckInResult.fromJson(
    await _api.post(
      '/api/check-ins',
      // "?tripRequestId" leaves the key out when it is null.
      body: {'voucherCode': voucherCode, 'tripRequestId': ?tripRequestId},
    ) as Map<String, dynamic>,
  );

  /// POST /api/trip-requests/{id}/guide-change-requests {reason}: the guide asks the operator for a replacement.
  /// 409 unless the trip is Confirmed, or when a request is already open.
  Future<void> requestReplacement(String tripId, String reason) => _api.post(
    '/api/trip-requests/$tripId/guide-change-requests',
    body: {'reason': reason.trim()},
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

/// What a scanned QR code is.
enum ScannedKind { tripVoucher, hotel, unknown }

/// The QR prefix of every signed TripCraft voucher (VoucherSigner.QrPrefix in the API).
const voucherQrPrefix = 'TRIPCRAFT-VOUCHER:';

/// "TRIPCRAFT-VOUCHER:..." is a signed voucher (the guide checks in with it); the older "TRIPCRAFT-HOTEL:{id}"
/// or a bare hotel id is a hotel lookup; anything else is not ours.
ScannedKind kindOfScan(String code) {
  final value = code.trim();
  if (value.toUpperCase().startsWith(voucherQrPrefix) &&
      value.length > voucherQrPrefix.length) {
    return ScannedKind.tripVoucher;
  }
  if (hotelIdFromVoucher(value) != null) return ScannedKind.hotel;
  return ScannedKind.unknown;
}

/// The voucher code inside a scanned "TRIPCRAFT-VOUCHER:{code}" QR (the API accepts it with or without the prefix).
String voucherCodeFromScan(String code) =>
    code.trim().substring(voucherQrPrefix.length);

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
