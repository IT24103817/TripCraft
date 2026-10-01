import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/core/api/api_client.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/trips/data/trips_repository.dart';

/// Answers every request with [status], [bytes] and [contentType], and remembers the request.
class BytesAdapter implements HttpClientAdapter {
  BytesAdapter(this.status, this.bytes, this.contentType);

  final int status;
  final List<int> bytes;
  final String contentType;
  RequestOptions? lastRequest;

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    lastRequest = options;
    return ResponseBody.fromBytes(
      bytes,
      status,
      headers: {
        Headers.contentTypeHeader: [contentType],
      },
    );
  }

  @override
  void close({bool force = false}) {}
}

/// A real ApiClient over [adapter], signed in with a token.
TripsRepository repositoryWith(BytesAdapter adapter) {
  final dio = Dio(BaseOptions(baseUrl: 'http://api.test'))
    ..httpClientAdapter = adapter;
  final storage = InMemorySessionStorage()..token = 'jwt-123';
  return TripsRepository(
    ApiClient(dio, storage: storage, onUnauthorized: () {}),
  );
}

void main() {
  // "%PDF-1.7" and a few more bytes.
  final pdf = [0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37, 0x0A, 0xFF];

  test('downloads the PDF with GET, as bytes, with the token', () async {
    final adapter = BytesAdapter(200, pdf, 'application/pdf');

    final bytes = await repositoryWith(adapter).itineraryPdf('trip-1');

    final request = adapter.lastRequest!;
    expect(request.method, 'GET');
    expect(request.path, '/api/trips/trip-1/itinerary.pdf');
    expect(request.responseType, ResponseType.bytes);
    expect(request.headers['Authorization'], 'Bearer jwt-123');
    expect(bytes, pdf);
  });

  test('a 409 before the quotation was sent shows the API\'s detail', () async {
    final problem = utf8.encode(
      jsonEncode({
        'status': 409,
        'title': 'Conflict',
        'detail':
            'The itinerary PDF is available once a quotation has been sent.',
      }),
    );
    final adapter = BytesAdapter(409, problem, 'application/problem+json');

    await expectLater(
      repositoryWith(adapter).itineraryPdf('trip-1'),
      throwsA(
        isA<UserFacingException>()
            .having((e) => e.statusCode, 'statusCode', 409)
            .having(
              (e) => e.message,
              'message',
              'The itinerary PDF is available once a quotation has been sent.',
            ),
      ),
    );
  });
}
