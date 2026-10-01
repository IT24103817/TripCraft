import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../api/api_client.dart';
import '../api/api_providers.dart';
import '../storage/session_storage.dart';
import 'auth_models.dart';

part 'auth_repository.g.dart';

/// Login, registration and the stored session.
class AuthRepository {
  AuthRepository(this._api, this._storage);

  final ApiClient _api;
  final SessionStorage _storage;

  /// POST /api/auth/login, then keeps the JWT and user in secure storage.
  Future<AppUser> login(String email, String password) async {
    final json = await _api.post(
      '/api/auth/login',
      body: {'email': email.trim(), 'password': password},
    );
    final response = AuthResponse.fromJson(json as Map<String, dynamic>);
    await _storage.writeSession(
      token: response.accessToken,
      user: response.user.toJson(),
    );
    return response.user;
  }

  /// POST /api/auth/register always creates a Tourist. The API takes no nationality at registration,
  /// so it is kept on the phone to pre-fill the first trip request.
  Future<void> register({
    required String fullName,
    required String email,
    required String password,
    required String nationality,
  }) async {
    await _api.post(
      '/api/auth/register',
      body: {
        'fullName': fullName.trim(),
        'email': email.trim(),
        'password': password,
      },
    );
    await _storage.writeNationality(nationality.trim());
  }

  /// POST /api/auth/change-password. The API clears mustChangePassword and returns the user; the JWT stays
  /// valid, so only the stored user is replaced.
  Future<AppUser> changePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    final json = await _api.post(
      '/api/auth/change-password',
      body: {'currentPassword': currentPassword, 'newPassword': newPassword},
    );
    final user = AppUser.fromJson(json as Map<String, dynamic>);
    final token = await _storage.readToken();
    if (token != null) {
      await _storage.writeSession(token: token, user: user.toJson());
    }
    return user;
  }

  /// The user saved by the last login, if a token is still stored.
  Future<AppUser?> restore() async {
    final token = await _storage.readToken();
    final user = await _storage.readUser();
    return token == null || user == null ? null : AppUser.fromJson(user);
  }

  Future<String?> savedNationality() => _storage.readNationality();

  Future<void> logout() => _storage.clear();
}

@Riverpod(keepAlive: true)
AuthRepository authRepository(Ref ref) => AuthRepository(
  ref.watch(apiClientProvider),
  ref.watch(sessionStorageProvider),
);
