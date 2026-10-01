import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/widgets/empty_state.dart';
import '../../../shared/widgets/section_card.dart';
import '../data/check_in.dart';
import '../data/guide_models.dart';
import 'check_in_panel.dart';
import 'vehicle_card.dart';

export '../data/check_in.dart' show GuideStop;

/// What the schedule passes to the day screen (through go_router's `extra`): the trip and the chosen day.
class TripDayArgs {
  const TripDayArgs({required this.trip, required this.day});

  final GuideTrip trip;
  final GuideDay day;
}

/// One day of a guide's trip: the trip's vehicle, a button to check in by scanning the tourist's trip voucher,
/// then the stops, each with GPS check-in as the alternative.
/// Opened from the schedule with the stops and the trip.
class TripDayScreen extends StatelessWidget {
  const TripDayScreen({super.key, required this.stops, this.trip});

  final List<GuideStop> stops;

  /// The trip this day belongs to, for the vehicle card. Null hides the card.
  final GuideTrip? trip;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Today\'s stops')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          if (trip != null) ...[
            VehicleCard(trip: trip!),
            const SizedBox(height: 12),
            OutlinedButton.icon(
              icon: const Icon(Icons.qr_code_scanner),
              label: const Text('Scan trip voucher to check in'),
              onPressed: () =>
                  context.push(Routes.scanForTrip(trip!.tripRequestId)),
            ),
            const SizedBox(height: 12),
          ],
          if (stops.isEmpty) const EmptyState(title: 'No stops for this day'),
          for (var i = 0; i < stops.length; i++) ...[
            SectionCard(
              title: '${i + 1}. ${stops[i].name}',
              child: CheckInPanel(stop: stops[i]),
            ),
            const SizedBox(height: 12),
          ],
        ],
      ),
    );
  }
}
