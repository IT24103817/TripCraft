import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/quotations/application/notifications_poller.dart';
import '../../features/quotations/application/quotation_providers.dart';
import '../../features/trips/application/trips_providers.dart';
import '../auth/auth_notifier.dart';
import 'routes.dart';

class _Tab {
  const _Tab(this.path, this.label, this.icon);
  final String path;
  final String label;
  final IconData icon;
}

const _touristTabs = [
  _Tab(Routes.home, 'Home', Icons.home_outlined),
  _Tab(Routes.trips, 'My trips', Icons.luggage_outlined),
  _Tab(Routes.newTrip, 'New trip', Icons.add_circle_outline),
  _Tab(Routes.alerts, 'Alerts', Icons.notifications_outlined),
];

const _guideTabs = [
  _Tab(Routes.schedule, 'Home', Icons.home_outlined),
  _Tab(Routes.scan, 'Scan voucher', Icons.qr_code_scanner),
  _Tab(Routes.alerts, 'Alerts', Icons.notifications_outlined),
];

/// Bottom navigation for the signed-in role. While the app is in the foreground it also polls the
/// notifications (phone notifications plus the unread badge on Alerts).
class RoleShell extends ConsumerStatefulWidget {
  const RoleShell({super.key, required this.location, required this.child});

  final String location;
  final Widget child;

  @override
  ConsumerState<RoleShell> createState() => _RoleShellState();
}

class _RoleShellState extends ConsumerState<RoleShell> {
  late final AppLifecycleListener _lifecycle;

  @override
  void initState() {
    super.initState();
    final poller = ref.read(notificationsPollerProvider.notifier);
    poller.start();
    // Poll only while the app is on screen; check straight away when it comes back.
    _lifecycle = AppLifecycleListener(
      onShow: poller.start,
      onHide: poller.stop,
    );
  }

  @override
  void dispose() {
    _lifecycle.dispose();
    super.dispose();
  }

  /// A new notification about a trip means its status, quote or vouchers changed on the server: reload them.
  void _reloadTrip(String tripId) {
    ref.invalidate(tripDetailProvider(tripId));
    ref.invalidate(tripWorkflowProvider(tripId));
    ref.invalidate(tripHistoryProvider(tripId));
    ref.invalidate(tripVouchersProvider(tripId));
    ref.invalidate(quotationViewProvider(tripId));
    ref.invalidate(myTripsProvider);
  }

  @override
  Widget build(BuildContext context) {
    ref.listen(notificationsPollerProvider, (previous, next) {
      if (previous == null) return;
      tripsWithNewNotifications(previous, next).forEach(_reloadTrip);
    });
    final role = ref.watch(authNotifierProvider).value?.role;
    final unread = ref.watch(
      notificationsPollerProvider.select((list) => list.unreadCount),
    );
    final tabs = role == 'Guide' ? _guideTabs : _touristTabs;
    // The most specific tab whose path starts the location ("/trips/new" beats "/trips").
    final sorted = [...tabs]
      ..sort((a, b) => b.path.length.compareTo(a.path.length));
    final current = sorted.firstWhere(
      (t) => widget.location.startsWith(t.path),
      orElse: () => tabs.first,
    );

    return Scaffold(
      body: widget.child,
      bottomNavigationBar: NavigationBar(
        selectedIndex: tabs.indexOf(current),
        onDestinationSelected: (i) => context.go(tabs[i].path),
        destinations: [
          for (final t in tabs)
            NavigationDestination(
              icon: t.path == Routes.alerts && unread > 0
                  ? Badge.count(count: unread, child: Icon(t.icon))
                  : Icon(t.icon),
              label: t.label,
            ),
        ],
      ),
    );
  }
}
