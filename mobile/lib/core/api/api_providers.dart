import 'package:dio/dio.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../auth/auth_notifier.dart';
import '../config.dart';
import '../storage/session_storage.dart';
import 'api_client.dart';

part 'api_providers.g.dart';

@Riverpod(keepAlive: true)
SessionStorage sessionStorage(Ref ref) => SecureSessionStorage();

/// The app-wide API client. A 401 tells the auth notifier the session is over; the router then shows /login.
@Riverpod(keepAlive: true)
ApiClient apiClient(Ref ref) {
  final dio = Dio(
    BaseOptions(
      baseUrl: AppConfig.apiUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 20),
    ),
  );
  return ApiClient(
    dio,
    storage: ref.watch(sessionStorageProvider),
    onUnauthorized: () =>
        ref.read(authNotifierProvider.notifier).sessionExpired(),
  );
}
