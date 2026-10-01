import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../data/trip_models.dart';
import 'assignment_card.dart';
import 'cancel_trip_section.dart';
import 'itinerary_pdf_card.dart';
import 'planning_section.dart';
import 'rate_guide_card.dart';
import 'trip_detail_rules.dart';
import 'trip_history_section.dart';
import 'trip_progress_card.dart';

/// The "Overview" tab, top to bottom: the request, where it is and what happens next, the decision to make
/// (at QuotationSent), planning, the itinerary PDF, the guide and vehicle, the guide rating, the history and cancellation.
class TripOverviewTab extends StatelessWidget {
  const TripOverviewTab({
    super.key,
    required this.trip,
    required this.workflow,
    required this.onRefresh,
    this.decisionPanel,
  });

  final TripRequest trip;
  final AsyncValue<TripWorkflow?> workflow;
  final Future<void> Function() onRefresh;
  final DecisionPanelBuilder? decisionPanel;

  @override
  Widget build(BuildContext context) {
    final status = trip.status;
    // Each card is followed by the same gap.
    final cards = <Widget>[
      _Summary(trip: trip),
      TripProgressCard(status: status),
      if (status == 'QuotationSent' && decisionPanel != null)
        decisionPanel!(trip.id, onRefresh),
      PlanningSection(trip: trip, workflow: workflow, onBack: onRefresh),
      if (itineraryPdfStatuses.contains(status))
        ItineraryPdfCard(tripId: trip.id),
      if (voucherStatuses.contains(status)) AssignmentCard(tripId: trip.id),
      if (status == 'Completed') RateGuideCard(tripId: trip.id),
      TripHistorySection(tripId: trip.id),
      if (!noCancelStatuses.contains(status))
        CancelTripSection(tripId: trip.id),
    ];
    return ListView.separated(
      padding: const EdgeInsets.all(16),
      itemCount: cards.length,
      separatorBuilder: (_, _) => const SizedBox(height: 12),
      itemBuilder: (_, i) => cards[i],
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
