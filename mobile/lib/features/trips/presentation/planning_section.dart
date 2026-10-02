import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import '../data/trips_repository.dart';
import 'trip_detail_rules.dart';

/// "Planning": start planning, a spinner while the agents work, then a link to the quotation.
/// When the agents could not finish (NeedsOperator) the operator takes over; the tourist only waits.
class PlanningSection extends ConsumerWidget {
  const PlanningSection({
    super.key,
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
    // At NeedsOperator the workflow's "Planning failed" chip would only worry the tourist.
    final showChip = current != null && trip.status != 'NeedsOperator';
    return SectionCard(
      title: 'Planning',
      trailing: showChip ? StatusChip(status: current.status) : null,
      child: AsyncView<TripWorkflow?>(
        value: workflow,
        onRetry: () => ref.invalidate(tripWorkflowProvider(trip.id)),
        data: (loaded) => _body(context, loaded),
      ),
    );
  }

  Widget _body(BuildContext context, TripWorkflow? w) {
    final mayStart = canStartPlanningStatuses.contains(trip.status);
    if (trip.status == 'NeedsOperator') {
      return const Text(
        'The agents could not finish this plan, so our team is preparing your quote by hand.',
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
      return const Text('The agents have finished. Your quote is on its way.');
    }
    return Align(
      alignment: Alignment.centerLeft,
      child: OutlinedButton.icon(
        icon: const Icon(Icons.receipt_long),
        label: const Text('View quotation'),
        onPressed: () async {
          await context.push(Routes.quotation(trip.id));
          await onBack();
        },
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
