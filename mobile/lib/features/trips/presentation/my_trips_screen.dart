import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/auth/profile_button.dart';
import '../../../core/router/routes.dart';
import '../../../shared/utils/reload.dart';
import '../../../shared/utils/statuses.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import 'trip_card.dart';

/// The tourist's trip requests: search, status filter, pull to refresh and the four screen states.
/// Each card shows the status and what happens next.
class MyTripsScreen extends ConsumerStatefulWidget {
  const MyTripsScreen({super.key});

  @override
  ConsumerState<MyTripsScreen> createState() => _MyTripsScreenState();
}

class _MyTripsScreenState extends ConsumerState<MyTripsScreen> {
  String _search = '';
  String _status = '';
  Timer? _debounce;

  @override
  void dispose() {
    _debounce?.cancel();
    super.dispose();
  }

  void _onSearch(String text) {
    _debounce?.cancel();
    _debounce = Timer(
      const Duration(milliseconds: 400),
      () => setState(() => _search = text.trim()),
    );
  }

  @override
  Widget build(BuildContext context) {
    final provider = myTripsProvider(search: _search, status: _status);
    final trips = ref.watch(provider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('My trips'),
        actions: const [ProfileButton()],
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
            child: TextField(
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.search),
                hintText: 'Search your trips',
              ),
              onChanged: _onSearch,
            ),
          ),
          SizedBox(
            height: 52,
            child: ListView(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              children: [
                for (final status in ['', ...tripStatuses])
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 4),
                    child: ChoiceChip(
                      label: Text(status.isEmpty ? 'All' : statusLabel(status)),
                      selected: _status == status,
                      onSelected: (_) => setState(() => _status = status),
                    ),
                  ),
              ],
            ),
          ),
          Expanded(
            child: RefreshIndicator(
              onRefresh: () => waitForReload(ref.refresh(provider.future)),
              child: AsyncView<List<TripRequest>>(
                value: trips,
                onRetry: () => ref.invalidate(provider),
                isEmpty: (items) => items.isEmpty,
                empty: ListView(
                  // Scrollable so pull-to-refresh still works on an empty list.
                  children: [
                    const SizedBox(height: 48),
                    EmptyState(
                      title: _search.isEmpty && _status.isEmpty
                          ? 'No trip requests yet'
                          : 'No trips match',
                      message: 'Plan your first Sri Lanka trip in a minute.',
                      action: FilledButton(
                        onPressed: () => context.go(Routes.newTrip),
                        child: const Text('Plan a trip'),
                      ),
                    ),
                  ],
                ),
                data: (items) => ListView.separated(
                  padding: const EdgeInsets.all(16),
                  itemCount: items.length,
                  separatorBuilder: (_, _) => const SizedBox(height: 8),
                  itemBuilder: (_, i) => TripCard(trip: items[i]),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
