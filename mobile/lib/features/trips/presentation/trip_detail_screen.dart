import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../../../shared/widgets/status_timeline.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import '../data/trips_repository.dart';
import 'trip_map.dart';

/// The main path a tourist sees (PLAN.md section 6, steps 1, 3, 8 and 11).
const tripTimelineSteps = [
  'Submitted',
  'Planning',
  'PendingApproval',
  'Confirmed',
];

/// Maps the trip status onto the tourist's timeline. Approved is shown as Confirmed (they happen together).
String timelineStatus(String tripStatus) =>
    tripStatus == 'Approved' ? 'Confirmed' : tripStatus;

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
      }
    });

    Future<void> refresh() async {
      ref.invalidate(tripWorkflowProvider(tripId));
      ref.invalidate(savedItineraryProvider(tripId));
      ref.invalidate(tripDetailProvider(tripId));
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
              if (t.status == 'PendingApproval') ...[
                const _AwaitingApproval(),
                const SizedBox(height: 12),
              ],
              SectionCard(
                title: 'Progress',
                child: StatusTimeline(
                  steps: tripTimelineSteps,
                  current: timelineStatus(t.status),
                ),
              ),
              const SizedBox(height: 12),
              _WorkflowSection(trip: t, workflow: workflow),
              const SizedBox(height: 12),
              _ItinerarySection(tripId: tripId, workflow: workflow.value),
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
          Text('${formatDate(trip.startDate)} – ${formatDate(trip.endDate)}'),
          Text('${trip.pax} travellers · budget ${formatUsd(trip.budgetUsd)}'),
        ],
      ),
    );
  }
}

class _AwaitingApproval extends StatelessWidget {
  const _AwaitingApproval();

  @override
  Widget build(BuildContext context) {
    return Semantics(
      liveRegion: true,
      child: Card(
        color: AppColors.warning.withValues(alpha: 0.1),
        child: const ListTile(
          leading: Icon(Icons.hourglass_top, color: AppColors.warning),
          title: Text('Awaiting operator approval'),
          subtitle: Text(
            'Your itinerary and quotation are ready. An operator is checking them now.',
          ),
        ),
      ),
    );
  }
}

class _WorkflowSection extends ConsumerWidget {
  const _WorkflowSection({required this.trip, required this.workflow});

  final TripRequest trip;
  final AsyncValue<TripWorkflow?> workflow;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final w = workflow.value;
    final Widget body;
    if (workflow.isLoading && w == null) {
      body = const LinearProgressIndicator();
    } else if (w == null) {
      body = trip.status == 'Submitted'
          ? FilledButton(
              onPressed: () async {
                final messenger = ScaffoldMessenger.of(context);
                try {
                  await ref
                      .read(tripsRepositoryProvider)
                      .startPlanning(trip.id);
                  ref.invalidate(tripWorkflowProvider(trip.id));
                  ref.invalidate(tripDetailProvider(trip.id));
                } catch (error) {
                  messenger.showSnackBar(
                    SnackBar(content: Text(friendlyMessage(error))),
                  );
                }
              },
              child: const Text('Start planning'),
            )
          : const Text('Planning has not started yet.');
    } else if (w.status == 'Planning') {
      body = const Row(
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
    } else if (w.status == 'FailedSafely') {
      body = Text(
        'Planning could not finish: ${w.errorSummary ?? 'unknown reason'}. Our team will follow up.',
      );
    } else {
      body = Align(
        alignment: Alignment.centerLeft,
        child: OutlinedButton.icon(
          icon: const Icon(Icons.receipt_long),
          label: const Text('View quotation'),
          onPressed: () => context.push(Routes.quotation(trip.id)),
        ),
      );
    }
    return SectionCard(
      title: 'Planning',
      trailing: w == null ? null : StatusChip(status: w.status),
      child: body,
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
