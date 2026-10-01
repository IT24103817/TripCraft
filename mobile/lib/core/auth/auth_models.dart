import 'package:freezed_annotation/freezed_annotation.dart';

part 'auth_models.freezed.dart';
part 'auth_models.g.dart';

/// UserDto from the API. role is "Tourist", "Guide", "OperationsManager" or "Admin".
/// [mustChangePassword] is true after a manager created the account (or reset its password): the app then
/// forces the Change password screen before anything else.
@freezed
abstract class AppUser with _$AppUser {
  const factory AppUser({
    required String id,
    required String email,
    required String fullName,
    required String role,
    required bool isActive,
    @Default(false) bool mustChangePassword,
  }) = _AppUser;

  factory AppUser.fromJson(Map<String, dynamic> json) =>
      _$AppUserFromJson(json);
}

/// POST /api/auth/login response.
@freezed
abstract class AuthResponse with _$AuthResponse {
  const factory AuthResponse({
    required String accessToken,
    required DateTime expiresAt,
    required AppUser user,
  }) = _AuthResponse;

  factory AuthResponse.fromJson(Map<String, dynamic> json) =>
      _$AuthResponseFromJson(json);
}
