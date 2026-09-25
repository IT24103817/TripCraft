import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../shared/widgets/empty_state.dart';
import 'auth_notifier.dart';

/// Shown for a moment while the saved session is read.
class SplashScreen extends StatelessWidget {
  const SplashScreen({super.key});

  @override
  Widget build(BuildContext context) => const Scaffold(
    body: Center(
      child: CircularProgressIndicator(semanticsLabel: 'Starting TripCraft'),
    ),
  );
}

/// Operations Managers and Admins use the web dashboard (PLAN.md section 2).
class NotSupportedScreen extends ConsumerWidget {
  const NotSupportedScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Scaffold(
      body: EmptyState(
        icon: Icons.desktop_windows_outlined,
        title: 'Please use the TripCraft web dashboard',
        message: 'This app is for tourists and guides. Staff accounts work on the website.',
        action: OutlinedButton(
          onPressed: () => ref.read(authNotifierProvider.notifier).logout(),
          child: const Text('Log out'),
        ),
      ),
    );
  }
}
