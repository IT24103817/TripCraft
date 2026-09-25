import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../features/quotations/presentation/notifications_screen.dart';
import '../../features/quotations/presentation/quotation_screen.dart';
import '../../features/resources/presentation/qr_scan_screen.dart';
import '../../features/resources/presentation/schedule_screen.dart';
import '../../features/resources/presentation/trip_day_screen.dart';
import '../../features/trips/presentation/my_trips_screen.dart';
import '../../features/trips/presentation/new_trip_screen.dart';
import '../../features/trips/presentation/trip_detail_screen.dart';
import '../auth/auth_notifier.dart';
import '../auth/login_screen.dart';
import '../auth/register_screen.dart';
import '../auth/simple_screens.dart';
import 'auth_redirect.dart';
import 'role_shell.dart';
import 'routes.dart';

part 'app_router.g.dart';

/// The router is the app's composition root: the only place that knows every feature's screens.
@Riverpod(keepAlive: true)
GoRouter appRouter(Ref ref) {
  // GoRouter re-runs redirect whenever this notifier fires, i.e. on every sign-in/out.
  final authChanged = ValueNotifier<int>(0);
  ref.listen(authNotifierProvider, (_, _) => authChanged.value++);
  ref.onDispose(authChanged.dispose);

  return GoRouter(
    initialLocation: Routes.splash,
    refreshListenable: authChanged,
    redirect: (context, state) =>
        authRedirect(ref.read(authNotifierProvider), state.matchedLocation),
    routes: [
      GoRoute(path: Routes.splash, builder: (_, _) => const SplashScreen()),
      GoRoute(path: Routes.login, builder: (_, _) => const LoginScreen()),
      GoRoute(path: Routes.register, builder: (_, _) => const RegisterScreen()),
      GoRoute(
        path: Routes.notSupported,
        builder: (_, _) => const NotSupportedScreen(),
      ),
      ShellRoute(
        builder: (context, state, child) =>
            RoleShell(location: state.matchedLocation, child: child),
        routes: [
          GoRoute(path: Routes.trips, builder: (_, _) => const MyTripsScreen()),
          GoRoute(
            path: Routes.newTrip,
            builder: (_, _) => const NewTripScreen(),
          ),
          GoRoute(
            path: '/trips/:id',
            builder: (_, s) =>
                TripDetailScreen(tripId: s.pathParameters['id']!),
          ),
          GoRoute(
            path: '/trips/:id/quotation',
            builder: (_, s) => QuotationScreen(tripId: s.pathParameters['id']!),
          ),
          GoRoute(
            path: Routes.alerts,
            builder: (_, _) => const NotificationsScreen(),
          ),
          GoRoute(
            path: Routes.schedule,
            builder: (_, _) => const ScheduleScreen(),
          ),
          GoRoute(
            path: Routes.tripDay,
            builder: (_, s) =>
                TripDayScreen(stops: (s.extra as List<GuideStop>?) ?? const []),
          ),
          GoRoute(path: Routes.scan, builder: (_, _) => const QrScanScreen()),
        ],
      ),
    ],
  );
}
