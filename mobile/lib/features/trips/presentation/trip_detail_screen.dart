import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import '../data/trips_repository.dart';
import 'cancel_trip_section.dart';
import 'trip_history_section.dart';
import 'trip_map.dart';
import 'trip_progress_card.dart';
import 'vouchers_section.dart';

/// From QuotationSent on, the tourist can open the quotation (before that the operator is still reviewing it).
const quotationVisibleStatuses = {
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
};

/// Vouchers exist once the operator has confirmed the trip.
const voucherStatuses = {'Confirmed', 'InProgress', 'Completed'};

/// A finished or cancelled trip has nothing left to cancel.
const noCancelStatuses = {'Completed', 'Cancelled'};

/// Only the tourist may (re)start planning: a new request, or after planning failed safely.
const canStartPlanningStatuses = {'Submitted', 'FailedSafely'};

class TripDetailScreen extends ConsumerWidget {
  const TripDetailScreen({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final trip = ref.watch(tripDetailProvider(tripId));
    final workflow = ref.watch(tripWorkflowProvider(tripId));

    // When the workflow moves on (e.g. Planning -> PendingApproval), refresh the trip status too.
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
      await ref.read(tripDetailProvider(tripId).future);
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Trip request')),
      body: RefreshIndicator(
        onRefresh: refresh,
        child: AsyncView<TripRequest>(
          value: trip,
          onRetry: () => ref.invalidate(tripDetailProvider(tripId)),
          data: (t) => ListView(
            padding: const EdgeInsets.all(16),
            children: [
              _Summary(trip: t),
              const SizedBox(height: 12),
              TripProgressCard(status: t.status),
              const SizedBox(height: 12),
              _WorkflowSection(trip: t, workflow: workflow, onBack: refresh),
              const SizedBox(height: 12),
              if (voucherStatuses.contains(t.status)) ...[
                VouchersSection(tripId: tripId),
                const SizedBox(height: 12),
              ],
              _ItinerarySection(tripId: tripId, workflow: workflow.value),
              const SizedBox(height: 12),
              TripHistorySection(tripId: tripId),
              if (!noCancelStatuses.contains(t.status)) ...[
                const SizedBox(height: 12),
                CancelTripSection(tripId: tripId),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

class _Summary extends StatelessWidget {
  const _Summary({required this.trip});

  final TripRequest trip;

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Your request',
      trailing: StatusChip(status: trip.status),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(trip.objective),
          const SizedBox(height: 8),
          if (trip.cities.isNotEmpty)
            Text('Cities: ${trip.cities.join(' → ')}'),
          Text('${formatDate(trip.startDate)} – ${formatDate(trip.endDate)}'),
          Text('${trip.pax} travellers · budget ${formatUsd(trip.budgetUsd)}'),
        ],
      ),
    );
  }
}

class _WorkflowSection extends ConsumerWidget {
  const _WorkflowSection({
    required this.trip,
    required this.workflow,
    required this.onBack,
  });

  final TripRequest trip;
  final AsyncValue<TripWorkflow?> workflow;

  /// Reloads the trip after the quotation screen closes (the tourist may have accepted or declined).
  final Future<void> Function() onBack;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // A failed load (e.g. no connection) shows the error with Retry, never "not started".
    final current = workflow.hasError ? null : workflow.value;
    return SectionCard(
      title: 'Planning',
      trailing: current == null ? null : StatusChip(status: current.status),
      child: AsyncView<TripWorkflow?>(
        value: workflow,
        onRetry: () => ref.invalidate(tripWorkflowProvider(trip.id)),
        data: (loaded) => _body(context, loaded),
      ),
    );
  }

  Widget _body(BuildContext context, TripWorkflow? w) {
    final mayStart = canStartPlanningStatuses.contains(trip.status);
    if (trip.status == 'FailedSafely' || w?.status == 'FailedSafely') {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Planning could not finish: ${w?.errorSummary ?? 'unknown reason'}.',
          ),
          // Only the tourist may start planning (API rule), so the retry lives here.
          if (mayStart) ...[
            const SizedBox(height: 8),
            _StartPlanningButton(tripId: trip.id, label: 'Try again'),
          ],
        ],
      );
    }
    if (w == null) {
      return mayStart
          ? _StartPlanningButton(tripId: trip.id, label: 'Start planning')
          : const Text('Planning has not started yet.');
    }
    if (w.status == 'Planning') {
      return const Row(
        children: [
          SizedBox(
            width: 20,
            height: 20,
            child: CircularProgressIndicator(strokeWidth: 2),
          ),
          SizedBox(width: 12),
          Expanded(
            child: Text(
              'Our AI agents are planning your trip. This page updates every 10 seconds.',
            ),
          ),
        ],
      );
    }
    if (!quotationVisibleStatuses.contains(trip.status)) {
      return const Text(
        'The agents have finished. The operator is reviewing your plan before sending you the quotation.',
      );
    }

    Future<void> openQuotation() async {
      await context.push(Routes.quotation(trip.id));
      await onBack();
    }

    // At QuotationSent the tourist has to decide, so the button stands out.
    return Align(
      alignment: Alignment.centerLeft,
      child: trip.status == 'QuotationSent'
          ? FilledButton.icon(
              icon: const Icon(Icons.receipt_long),
              label: const Text('Review quotation'),
              onPressed: openQuotation,
            )
          : OutlinedButton.icon(
              icon: const Icon(Icons.receipt_long),
              label: const Text('View quotation'),
              onPressed: openQuotation,
            ),
    );
  }
}

class _ItinerarySection extends ConsumerWidget {
  const _ItinerarySection({required this.tripId, required this.workflow});

  final String tripId;
  final TripWorkflow? workflow;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final fromProposal = proposalDays(workflow);
    final saved = fromProposal.isEmpty
        ? ref.watch(savedItineraryProvider(tripId))
        : AsyncData(fromProposal);

    return SectionCard(
      title: 'Itinerary',
      child: AsyncView<List<TripDay>>(
        value: saved,
        onRetry: () => ref.invalidate(savedItineraryProvider(tripId)),
        isEmpty: (days) => days.isEmpty,
        empty: const Text(
          'Your day-by-day plan appears here once the agents have drafted it.',
        ),
        data: (days) {
          final ids = days
              .expand((d) => d.stops)
              .map((s) => s.attractionId)
              .join(',');
          final locations = ref.watch(stopLocationsProvider(ids));
          return Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              for (final day in days) _DayTile(day: day),
              const SizedBox(height: 8),
              // The map is optional: if coordinates cannot be loaded the itinerary still shows.
              if (locations.value case final stops? when stops.isNotEmpty)
                TripMap(stops: stops),
            ],
          );
        },
      ),
    );
  }
}

class _DayTile extends StatelessWidget {
  const _DayTile({required this.day});

  final TripDay day;

  @override
  Widget build(BuildContext context) {
    final details = [
      if (day.date != null) formatDate(day.date),
      if (day.transport != null) 'by ${day.transport}',
      if (day.weather != null) day.weather!,
    ].join(' · ');
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Day ${day.day} — ${day.city}',
            style: Theme.of(context).textTheme.titleSmall,
          ),
          if (details.isNotEmpty)
            Text(details, style: Theme.of(context).textTheme.bodySmall),
          for (final stop in day.stops)
            Padding(
              padding: const EdgeInsets.only(left: 8, top: 2),
              child: Text('• ${stop.name}'),
            ),
        ],
      ),
    );
  }
}

/// Calls start-planning, then reloads the trip and its workflow. Errors are shown as a snackbar.
class _StartPlanningButton extends ConsumerWidget {
  const _StartPlanningButton({required this.tripId, required this.label});

  final String tripId;
  final String label;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return FilledButton(
      onPressed: () async {
        final messenger = ScaffoldMessenger.of(context);
        try {
          await ref.read(tripsRepositoryProvider).startPlanning(tripId);
          ref.invalidate(tripWorkflowProvider(tripId));
          ref.invalidate(tripDetailProvider(tripId));
        } catch (error) {
          messenger.showSnackBar(
            SnackBar(content: Text(friendlyMessage(error))),
          );
        }
      },
      child: Text(label),
    );
  }
}
