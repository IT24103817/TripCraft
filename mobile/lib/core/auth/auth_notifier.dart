import 'package:riverpod_annotation/riverpod_annotation.dart';

import 'auth_models.dart';
import 'auth_repository.dart';

part 'auth_notifier.g.dart';

/// Who is signed in. AsyncLoading while the saved session is read at start-up;
/// data(null) = signed out; data(user) = signed in. The router listens to this.
@Riverpod(keepAlive: true, name: 'authNotifierProvider')
class AuthNotifier extends _$AuthNotifier {
  @override
  Future<AppUser?> build() => ref.read(authRepositoryProvider).restore();

  /// Throws a UserFacingException on failure (the login screen shows it); the state only changes on success.
  Future<void> login(String email, String password) async {
    final user = await ref.read(authRepositoryProvider).login(email, password);
    state = AsyncData(user);
  }

  /// Throws a UserFacingException (e.g. wrong current password); on success the router leaves the
  /// Change password screen because mustChangePassword is now false.
  Future<void> changePassword(
    String currentPassword,
    String newPassword,
  ) async {
    final user = await ref
        .read(authRepositoryProvider)
        .changePassword(
          currentPassword: currentPassword,
          newPassword: newPassword,
        );
    state = AsyncData(user);
  }

  Future<void> logout() async {
    await ref.read(authRepositoryProvider).logout();
    state = const AsyncData(null);
  }

  /// Called by the ApiClient after a 401: storage is already cleared.
  void sessionExpired() => state = const AsyncData(null);
}
