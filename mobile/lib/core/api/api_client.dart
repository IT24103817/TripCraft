import 'package:dio/dio.dart';

import '../storage/session_storage.dart';
import 'user_facing_exception.dart';

/// The only HTTP client in the app; it talks to the ASP.NET Core API and nothing else.
/// - adds `Authorization: Bearer <jwt>` from secure storage to every request;
/// - on a 401 (expired or revoked token) clears the session and calls [onUnauthorized] (router goes to /login);
/// - turns every failure into a [UserFacingException].
class ApiClient {
  ApiClient(
    this._dio, {
    required SessionStorage storage,
    required void Function() onUnauthorized,
  }) {
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) async {
          final token = await storage.readToken();
          if (token != null) options.headers['Authorization'] = 'Bearer $token';
          handler.next(options);
        },
        onError: (error, handler) async {
          // A 401 on login just means a wrong password, not an expired session.
          final isLogin = error.requestOptions.path.endsWith('/api/auth/login');
          if (error.response?.statusCode == 401 && !isLogin) {
            await storage.clear();
            onUnauthorized();
          }
          handler.next(error);
        },
      ),
    );
  }

  final Dio _dio;

  Future<dynamic> get(String path, {Map<String, dynamic>? query}) =>
      _send(() => _dio.get<dynamic>(path, queryParameters: _clean(query)));

  /// A file download (e.g. a PDF): the raw bytes instead of JSON.
  Future<List<int>> getBytes(String path) async {
    final data = await _send(
      () => _dio.get<List<int>>(
        path,
        options: Options(responseType: ResponseType.bytes),
      ),
    );
    return data as List<int>;
  }

  Future<dynamic> post(String path, {Object? body}) =>
      _send(() => _dio.post<dynamic>(path, data: body));

  Future<dynamic> postMultipart(String path, FormData form) =>
      _send(() => _dio.post<dynamic>(path, data: form));

  Future<dynamic> _send(Future<Response<dynamic>> Function() request) async {
    try {
      return (await request()).data;
    } on DioException catch (error) {
      throw mapDioError(error);
    }
  }

  static Map<String, dynamic>? _clean(Map<String, dynamic>? query) =>
      query == null
      ? null
      : (Map.of(query)..removeWhere((_, v) => v == null || v == ''));
}
