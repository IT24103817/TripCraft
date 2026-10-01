import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../auth/auth_models.dart';
import 'routes.dart';

/// Pure redirect rule used by GoRouter (unit-tested without widgets). Returns null to stay.
/// - while the saved session is being read: /splash;
/// - signed out: only /login and /register;
/// - signed in with mustChangePassword: only /change-password (it cannot be skipped);
/// - signed in: /login, /register and /splash go home; each role stays in its own area.
String? authRedirect(AsyncValue<AppUser?> auth, String location) {
  if (auth.isLoading && !auth.hasValue) {
    return location == Routes.splash ? null : Routes.splash;
  }

  final user = auth.value;
  if (user == null) {
    return Routes.publicPaths.contains(location) ? null : Routes.login;
  }

  final home = homeForRole(user.role);
  if (home == Routes.notSupported) return location == home ? null : home;

  if (user.mustChangePassword) {
    return location == Routes.changePassword ? null : Routes.changePassword;
  }
  if (Routes.publicPaths.contains(location) ||
      location == Routes.splash ||
      location == Routes.changePassword) {
    return home;
  }

  final touristArea = location.startsWith(Routes.trips);
  final guideArea =
      location.startsWith(Routes.schedule) || location.startsWith(Routes.scan);
  if (user.role == 'Tourist' && guideArea) return home;
  if (user.role == 'Guide' && touristArea) return home;
  if (location == Routes.notSupported) return home;
  return null;
}
