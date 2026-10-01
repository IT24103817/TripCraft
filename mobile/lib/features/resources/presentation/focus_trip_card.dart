import 'package:flutter/material.dart';

import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../application/guide_focus.dart';
import 'request_replacement_button.dart';

/// The first card of the guide's home: "Today" with the trip running today, or "Next trip" with its start date.
class FocusTripCard extends StatelessWidget {
  const FocusTripCard({super.key, required this.focus, required this.today});

  final GuideFocus focus;
  final DateTime today;

  @override
  Widget build(BuildContext context) {
    final trip = focus.trip;
    final day = dayOn(trip, today);
    final when = focus.isToday
        ? (day == null
              ? 'Running today'
              : 'Day ${day.dayNumber} of ${trip.days.length} · ${day.city}')
        : 'Starts ${formatDate(trip.startDate)}';
    return SectionCard(
      key: const ValueKey('focus-trip'),
      title: focus.isToday ? 'Today' : 'Next trip',
      trailing: StatusChip(status: trip.status),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(trip.objective, style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 4),
          Text(when),
          Text(
            '${trip.pax} travellers · ${formatDate(trip.startDate)} – ${formatDate(trip.endDate)}',
            style: Theme.of(context).textTheme.bodySmall,
          ),
          // A guide can only ask to be replaced before the trip starts (API: trip Confirmed).
          if (trip.status == 'Confirmed')
            Align(
              alignment: Alignment.centerLeft,
              child: RequestReplacementButton(tripId: trip.tripRequestId),
            ),
        ],
      ),
    );
  }
}
