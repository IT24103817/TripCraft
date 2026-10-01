import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/auth/auth_notifier.dart';
import '../../../core/auth/profile_button.dart';
import '../../../core/router/routes.dart';
import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/clock.dart';
import '../../../shared/utils/reload.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../../../shared/widgets/primary_button.dart';
import '../application/greeting.dart';
import '../application/template_providers.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import 'packages_row.dart';
import 'trip_card.dart';

/// How many trips the home screen lists before "See all".
const homeTripsShown = 5;

/// The tourist's first tab: a greeting, "Plan a new trip", the mood packages and their latest trips.
class TouristHomeScreen extends ConsumerWidget {
  const TouristHomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final user = ref.watch(authNotifierProvider).value;
    final now = ref.watch(clockProvider)();
    final trips = ref.watch(myTripsProvider());

    Future<void> refresh() async {
      ref.invalidate(tripTemplatesProvider);
      ref.invalidate(myTripsProvider);
      await waitForReload(ref.read(myTripsProvider().future));
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('TripCraft'),
        actions: const [ProfileButton()],
      ),
      body: RefreshIndicator(
        onRefresh: refresh,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Text(
              greetingFor(now, user?.fullName ?? ''),
              style: theme.textTheme.headlineSmall,
            ),
            const SizedBox(height: 4),
            Text(
              'Where would you like to go in Sri Lanka?',
              style: theme.textTheme.bodyMedium?.copyWith(
                color: AppColors.muted,
              ),
            ),
            const SizedBox(height: 16),
            PrimaryButton(
              label: 'Plan a new trip',
              icon: Icons.add,
              onPressed: () => context.push(Routes.newTrip),
            ),
            const SizedBox(height: 24),
            Text(
              'Trips picked for your mood',
              style: theme.textTheme.titleLarge,
            ),
            const SizedBox(height: 12),
            const PackagesRow(),
            const SizedBox(height: 24),
            Row(
              children: [
                Expanded(
                  child: Text('My trips', style: theme.textTheme.titleLarge),
                ),
                TextButton(
                  onPressed: () => context.go(Routes.trips),
                  child: const Text('See all'),
                ),
              ],
            ),
            const SizedBox(height: 8),
            AsyncView<List<TripRequest>>(
              value: trips,
              onRetry: () => ref.invalidate(myTripsProvider),
              isEmpty: (items) => items.isEmpty,
              empty: const EmptyState(
                icon: Icons.luggage_outlined,
                title: 'No trips yet',
                message: 'Pick a package above or plan your own trip.',
              ),
              data: (items) => Column(
                children: [
                  for (final trip in items.take(homeTripsShown)) ...[
                    TripCard(trip: trip),
                    const SizedBox(height: 8),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
