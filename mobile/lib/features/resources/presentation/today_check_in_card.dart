import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/widgets/section_card.dart';
import '../data/guide_models.dart';
import 'check_in_panel.dart';

/// "Check in today": scan the tourist's trip voucher (checks in the next stop), or check in at each of
/// today's stops by GPS (within 500 m).
class TodayCheckInCard extends StatelessWidget {
  const TodayCheckInCard({super.key, required this.trip, required this.day});

  final GuideTrip trip;

  /// Today's day of the trip; null when nothing is planned today.
  final GuideDay? day;

  @override
  Widget build(BuildContext context) {
    final stops = day?.guideStops ?? const [];
    final theme = Theme.of(context);
    return SectionCard(
      title: 'Check in today',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          FilledButton.icon(
            icon: const Icon(Icons.qr_code_scanner),
            label: const Text('Scan voucher'),
            onPressed: () =>
                context.push(Routes.scanForTrip(trip.tripRequestId)),
          ),
          const SizedBox(height: 8),
          Text(
            'Or check in by GPS at each stop:',
            style: theme.textTheme.bodySmall,
          ),
          if (stops.isEmpty) const Text('No stops are planned for today.'),
          for (var i = 0; i < stops.length; i++) ...[
            const SizedBox(height: 12),
            Text(
              '${i + 1}. ${stops[i].name}',
              style: theme.textTheme.titleSmall,
            ),
            const SizedBox(height: 4),
            CheckInPanel(key: ValueKey(stops[i].id), stop: stops[i]),
          ],
        ],
      ),
    );
  }
}
