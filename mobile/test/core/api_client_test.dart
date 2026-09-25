import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/core/api/api_client.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';

/// Answers every request with a fixed status and JSON body, and remembers the last request.
class FakeAdapter implements HttpClientAdapter {
  FakeAdapter(this.status, [this.body = const {}]);

  final int status;
  final Object body;
  RequestOptions? lastRequest;

  @override
  Future<ResponseBody> fetch(
    RequestOptions options,
    Stream<Uint8List>? requestStream,
    Future<void>? cancelFuture,
  ) async {
    lastRequest = options;
    return ResponseBody.fromString(
      jsonEncode(body),
      status,
      headers: {
        Headers.contentTypeHeader: ['application/json'],
      },
    );
  }

  @override
  void close({bool force = false}) {}
}

void main() {
  late InMemorySessionStorage storage;
  late int unauthorizedCalls;

  ApiClient clientWith(HttpClientAdapter adapter) {
    final dio = Dio(BaseOptions(baseUrl: 'http://api.test'))
      ..httpClientAdapter = adapter;
    return ApiClient(
      dio,
      storage: storage,
      onUnauthorized: () => unauthorizedCalls++,
    );
  }

  setUp(() {
    storage = InMemorySessionStorage()
      ..token = 'jwt-123'
      ..user = {'id': 'u1'};
    unauthorizedCalls = 0;
  });

  test('adds the JWT from storage to every request', () async {
    final adapter = FakeAdapter(200, {'ok': true});

    await clientWith(adapter).get('/api/trip-requests');

    expect(adapter.lastRequest!.headers['Authorization'], 'Bearer jwt-123');
  });

  test(
    'a 401 clears the session, triggers logout and throws a friendly error',
    () async {
      final client = clientWith(FakeAdapter(401, {'title': 'Unauthorized'}));

      final error = await client
          .get('/api/trip-requests')
          .then<Object?>((_) => null, onError: (Object e) => e);

      expect(
        error,
        isA<UserFacingException>().having(
          (e) => e.isUnauthorized,
          'isUnauthorized',
          isTrue,
        ),
      );
      expect(
        (error! as UserFacingException).message,
        'Your session has expired. Please log in again.',
      );
      expect(storage.token, isNull);
      expect(unauthorizedCalls, 1);
    },
  );

  test('a 401 from login is a wrong password, not a logout', () async {
    final client = clientWith(FakeAdapter(401));

    await expectLater(
      client.post('/api/auth/login', body: {}),
      throwsA(isA<UserFacingException>()),
    );

    expect(storage.token, 'jwt-123');
    expect(unauthorizedCalls, 0);
  });

  test('a 400 ProblemDetails shows the first field error', () async {
    final client = clientWith(
      FakeAdapter(400, {
        'title': 'Validation failed',
        'errors': {
          'Pax': ['\'Pax\' must be between 1 and 50.'],
        },
      }),
    );

    await expectLater(
      client.post('/api/trip-requests', body: {}),
      throwsA(
        isA<UserFacingException>().having(
          (e) => e.message,
          'message',
          "'Pax' must be between 1 and 50.",
        ),
      ),
    );
  });

  test('server and network failures become friendly sentences', () {
    expect(
      mapDioError(
        DioException(
          requestOptions: RequestOptions(),
          response: Response(requestOptions: RequestOptions(), statusCode: 500),
        ),
      ).message,
      'The TripCraft server had a problem. Please try again in a moment.',
    );
    expect(
      mapDioError(
        DioException(
          requestOptions: RequestOptions(),
          type: DioExceptionType.connectionError,
        ),
      ).message,
      'Cannot reach TripCraft. Check your internet connection.',
    );
  });
}
