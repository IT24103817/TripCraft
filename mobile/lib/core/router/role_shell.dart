import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../features/quotations/application/status_watcher.dart';
import '../auth/auth_notifier.dart';
import 'routes.dart';

class _Tab {
  const _Tab(this.path, this.label, this.icon);
  final String path;
  final String label;
  final IconData icon;
}

const _touristTabs = [
  _Tab(Routes.trips, 'My trips', Icons.luggage_outlined),
  _Tab(Routes.newTrip, 'New trip', Icons.add_circle_outline),
  _Tab(Routes.alerts, 'Alerts', Icons.notifications_outlined),
];

const _guideTabs = [
  _Tab(Routes.schedule, 'Schedule', Icons.event_note_outlined),
  _Tab(Routes.scan, 'Scan voucher', Icons.qr_code_scanner),
];

/// Bottom navigation for the signed-in role. Tourists also get the background status watcher.
class RoleShell extends ConsumerStatefulWidget {
  const RoleShell({super.key, required this.location, required this.child});

  final String location;
  final Widget child;

  @override
  ConsumerState<RoleShell> createState() => _RoleShellState();
}

class _RoleShellState extends ConsumerState<RoleShell> {
  @override
  void initState() {
    super.initState();
    if (ref.read(authNotifierProvider).value?.role == 'Tourist') {
      ref.read(statusWatcherProvider.notifier).start();
    }
  }

  @override
  Widget build(BuildContext context) {
    final role = ref.watch(authNotifierProvider).value?.role;
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
            NavigationDestination(icon: Icon(t.icon), label: t.label),
        ],
      ),
    );
  }
}
