import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../application/assignment_providers.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import 'trip_detail_rules.dart';
import 'trip_itinerary_tab.dart';
import 'trip_overview_tab.dart';
import 'vouchers_section.dart';

export 'trip_detail_rules.dart';

/// One trip in three tabs: Overview (status, decisions, guide, history, cancel), Itinerary (days, weather,
/// map) and Vouchers (QR codes). Every tab can be pulled down to reload the trip.
class TripDetailScreen extends ConsumerWidget {
  const TripDetailScreen({super.key, required this.tripId, this.decisionPanel});

  final String tripId;

  /// The Accept / Decline panel at QuotationSent, passed in by the router.
  final DecisionPanelBuilder? decisionPanel;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final trip = ref.watch(tripDetailProvider(tripId));
    final workflow = ref.watch(tripWorkflowProvider(tripId));

    // When the workflow moves on (e.g. Planning -> Approved: the quote was sent), refresh the trip status too.
    ref.listen(tripWorkflowProvider(tripId), (previous, next) {
      if (previous?.value?.status != next.value?.status) {
        ref.invalidate(tripDetailProvider(tripId));
        ref.invalidate(tripHistoryProvider(tripId));
        ref.invalidate(cancellationInfoProvider(tripId));
      }
    });

    Future<void> refresh() async {
      ref.invalidate(tripWorkflowProvider(tripId));
      ref.invalidate(savedItineraryProvider(tripId));
      ref.invalidate(tripDetailProvider(tripId));
      ref.invalidate(tripHistoryProvider(tripId));
      ref.invalidate(cancellationInfoProvider(tripId));
      ref.invalidate(tripVouchersProvider(tripId));
      ref.invalidate(tripAssignmentProvider(tripId));
      ref.invalidate(guideRatingProvider(tripId));
      ref.invalidate(myTripsProvider);
      await ref.read(tripDetailProvider(tripId).future);
    }

    return DefaultTabController(
      length: 3,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Trip request'),
          // 48 dp high tabs: the design system's minimum touch target.
          bottom: const TabBar(
            tabs: [
              Tab(text: 'Overview', height: 48),
              Tab(text: 'Itinerary', height: 48),
              Tab(text: 'Vouchers', height: 48),
            ],
          ),
        ),
        body: AsyncView<TripRequest>(
          value: trip,
          onRetry: () => ref.invalidate(tripDetailProvider(tripId)),
          data: (t) => TabBarView(
            children: [
              RefreshIndicator(
                onRefresh: refresh,
                child: TripOverviewTab(
                  trip: t,
                  workflow: workflow,
                  onRefresh: refresh,
                  decisionPanel: decisionPanel,
                ),
              ),
              RefreshIndicator(
                onRefresh: refresh,
                child: TripItineraryTab(
                  tripId: tripId,
                  workflow: workflow.value,
                ),
              ),
              RefreshIndicator(
                onRefresh: refresh,
                child: _VouchersTab(trip: t),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

/// The "Vouchers" tab: the QR vouchers once the trip is confirmed (trip voucher first).
class _VouchersTab extends StatelessWidget {
  const _VouchersTab({required this.trip});

  final TripRequest trip;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        if (voucherStatuses.contains(trip.status))
          VouchersSection(tripId: trip.id)
        else
          const EmptyState(
            icon: Icons.qr_code_2,
            title: 'No vouchers yet',
            message: 'Your vouchers appear here once the trip is confirmed.',
          ),
      ],
    );
  }
}
