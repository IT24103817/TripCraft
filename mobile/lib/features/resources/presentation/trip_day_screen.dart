import 'package:flutter/material.dart';

import '../../../shared/widgets/empty_state.dart';
import '../../../shared/widgets/section_card.dart';
import '../data/check_in.dart';
import 'check_in_panel.dart';

export '../data/check_in.dart' show GuideStop;

/// One day of a guide's trip: the stops, each with GPS check-in. Opened from the schedule with the stops.
class TripDayScreen extends StatelessWidget {
  const TripDayScreen({super.key, required this.stops});

  final List<GuideStop> stops;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Today\'s stops')),
      body: stops.isEmpty
          ? const EmptyState(title: 'No stops for this day')
          : ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: stops.length,
              separatorBuilder: (_, _) => const SizedBox(height: 12),
              itemBuilder: (_, i) => SectionCard(
                title: '${i + 1}. ${stops[i].name}',
                child: CheckInPanel(stop: stops[i]),
              ),
            ),
    );
  }
}
