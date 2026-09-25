import 'dart:convert';

import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Where the JWT and the signed-in user live between app starts. Tests use [InMemorySessionStorage].
abstract class SessionStorage {
  Future<String?> readToken();
  Future<Map<String, dynamic>?> readUser();
  Future<void> writeSession({
    required String token,
    required Map<String, dynamic> user,
  });
  Future<void> clear();

  /// Nationality typed at registration, used to pre-fill the trip form
  /// (the API's register endpoint does not take a nationality).
  Future<String?> readNationality();
  Future<void> writeNationality(String nationality);
}

/// Android Keystore / iOS Keychain backed storage (PLAN.md section 6: JWT in flutter_secure_storage).
class SecureSessionStorage implements SessionStorage {
  SecureSessionStorage([FlutterSecureStorage? storage])
    : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;
  static const _tokenKey = 'auth.token';
  static const _userKey = 'auth.user';
  static const _nationalityKey = 'profile.nationality';

  @override
  Future<String?> readToken() => _storage.read(key: _tokenKey);

  @override
  Future<Map<String, dynamic>?> readUser() async {
    final raw = await _storage.read(key: _userKey);
    return raw == null ? null : jsonDecode(raw) as Map<String, dynamic>;
  }

  @override
  Future<void> writeSession({
    required String token,
    required Map<String, dynamic> user,
  }) async {
    await _storage.write(key: _tokenKey, value: token);
    await _storage.write(key: _userKey, value: jsonEncode(user));
  }

  @override
  Future<void> clear() async {
    await _storage.delete(key: _tokenKey);
    await _storage.delete(key: _userKey);
  }

  @override
  Future<String?> readNationality() => _storage.read(key: _nationalityKey);

  @override
  Future<void> writeNationality(String nationality) =>
      _storage.write(key: _nationalityKey, value: nationality);
}

/// Keeps everything in memory. Used by tests.
class InMemorySessionStorage implements SessionStorage {
  String? token;
  Map<String, dynamic>? user;
  String? nationality;

  @override
  Future<String?> readToken() async => token;

  @override
  Future<Map<String, dynamic>?> readUser() async => user;

  @override
  Future<void> writeSession({
    required String token,
    required Map<String, dynamic> user,
  }) async {
    this.token = token;
    this.user = user;
  }

  @override
  Future<void> clear() async {
    token = null;
    user = null;
  }

  @override
  Future<String?> readNationality() async => nationality;

  @override
  Future<void> writeNationality(String nationality) async =>
      this.nationality = nationality;
}
