/// Every path in the app. Features navigate by these paths, so they never import each other's screens.
class Routes {
  const Routes._();

  static const splash = '/splash';
  static const login = '/login';
  static const register = '/register';
  static const notSupported = '/not-supported';

  /// Forced after a login with mustChangePassword (a guide's temporary password).
  static const changePassword = '/change-password';

  // Tourist and guide
  static const alerts = '/alerts';

  // Tourist
  static const home = '/home';
  static const packages = '/packages';
  static String package(String templateId) => '/packages/$templateId';
  static const trips = '/trips';
  static const newTrip = '/trips/new';
  static String trip(String id) => '/trips/$id';
  static String quotation(String tripId) => '/trips/$tripId/quotation';

  // Guide
  static const schedule = '/schedule';
  static const tripDay = '/schedule/day';
  static const scan = '/scan';

  /// The scanner opened from one trip: a scanned trip voucher must belong to it.
  static String scanForTrip(String tripId) => '/scan?trip=$tripId';

  static const publicPaths = {login, register};
}

/// Landing page per role (PLAN.md section 2): staff use the React app, not this one.
String homeForRole(String role) => switch (role) {
  'Tourist' => Routes.home,
  'Guide' => Routes.schedule,
  _ => Routes.notSupported,
};
