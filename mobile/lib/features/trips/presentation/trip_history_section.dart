import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/statuses.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/section_card.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';

/// Tourist-friendly names for the audit actions of a trip. Unknown actions are shown as they are.
const _actionLabels = {
  'TripRequestCreated': 'Request submitted',
  'TripRequestUpdated': 'Details edited',
  'TripRequestStatusChanged': 'Status changed',
  'AgentWorkflowStarted': 'Planning started',
  'AgentWorkflowFailedSafely': 'Planning could not start',
  'AgentProposalReceived': 'Plan received from the agents',
  'PassportPhotoUploaded': 'Passport photo added',
  'QuotationApproved': 'Quotation sent to you',
  'QuotationRevisionRequested': 'Operator asked for changes',
  'QuotationRepriced': 'Quotation re-priced',
  'QuotationAccepted': 'You accepted the quotation',
  'QuotationDeclined': 'You declined the quotation',
  'QuotationRejected': 'Quotation rejected',
  'TripConfirmed': 'Trip confirmed',
  'TripRequestCancelled': 'Trip cancelled',
  'StopCheckedIn': 'Checked in at a stop',
  'GuideChangeRequested': 'Guide asked to be replaced',
  'GuideChangeResolved': 'New guide assigned',
};

/// Spec section 8 "status tracking and history": every recorded change of the trip, oldest first.
class TripHistorySection extends ConsumerWidget {
  const TripHistorySection({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SectionCard(
      title: 'History',
      child: AsyncView<List<TripHistoryEntry>>(
        value: ref.watch(tripHistoryProvider(tripId)),
        onRetry: () => ref.invalidate(tripHistoryProvider(tripId)),
        isEmpty: (entries) => entries.isEmpty,
        empty: const Text('No history yet.'),
        data: (entries) => Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [for (final entry in entries) _HistoryTile(entry: entry)],
        ),
      ),
    );
  }
}

class _HistoryTile extends StatelessWidget {
  const _HistoryTile({required this.entry});

  final TripHistoryEntry entry;

  @override
  Widget build(BuildContext context) {
    final change = entry.toStatus == null
        ? ''
        : ' · ${entry.fromStatus == null ? '' : '${statusLabel(entry.fromStatus!)} → '}${statusLabel(entry.toStatus!)}';
    return ListTile(
      dense: true,
      contentPadding: EdgeInsets.zero,
      leading: const Icon(Icons.history, size: 20),
      title: Text('${_actionLabels[entry.action] ?? entry.action}$change'),
      // The API's human-readable reason (e.g. why the status changed) goes under the date.
      subtitle: Text(
        '${formatDateTime(entry.at)} · ${_actorLabel(entry.actor)}'
        '${entry.reason == null ? '' : '\n${entry.reason}'}',
      ),
    );
  }

  static String _actorLabel(String actor) => switch (actor) {
    'OperationsManager' => 'by the operator',
    'Tourist' => 'by you',
    'Guide' => 'by your guide',
    _ => 'by the system',
  };
}
