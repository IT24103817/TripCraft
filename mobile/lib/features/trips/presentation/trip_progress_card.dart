import 'package:flutter/material.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_timeline.dart';

/// The main path of a trip in the v1.1 lifecycle (docs/API-V11.md), as the tourist sees it.
const tripTimelineSteps = [
  'Submitted',
  'Planning',
  'PendingReview',
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
];

/// Where a trip status sits on the timeline. The side states stay on the step they came from:
/// RevisionRequested is still with the operator (Pending review), FailedSafely is still Planning.
String timelineStatus(String tripStatus) => switch (tripStatus) {
  'RevisionRequested' => 'PendingReview',
  'FailedSafely' => 'Planning',
  _ => tripStatus,
};

/// One short sentence per status: what happens next.
String whatHappensNext(String tripStatus) => switch (tripStatus) {
  'Submitted' => 'We have your request. Our AI agents start planning it next.',
  'Planning' => 'Our AI agents are drafting your itinerary and price. This takes a minute or two.',
  'FailedSafely' =>
    'Planning could not finish. Tap Try again to plan your trip again.',
  'PendingReview' => 'An operator is checking your itinerary and price. We will notify you when your quotation is ready.',
  'RevisionRequested' =>
    'The operator asked for changes; a new version is coming.',
  'QuotationSent' =>
    'Your quotation is ready. Review it, then accept or decline it.',
  'ClientAccepted' => 'You accepted the quotation. The operator is now booking your guide, vehicle and hotels.',
  'Confirmed' => 'Your trip is booked. Show your trip voucher to your guide on the first day.',
  'InProgress' => 'Enjoy your trip! Your guide checks you in at each stop.',
  'Completed' =>
    'Your trip is complete. Thank you for travelling with TripCraft.',
  'Cancelled' => 'This trip was cancelled. Nothing else will happen.',
  _ => 'We will let you know when something changes.',
};

/// "Progress": the status timeline and the next-step sentence. A cancelled trip has no timeline.
class TripProgressCard extends StatelessWidget {
  const TripProgressCard({super.key, required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final sideState = status == 'RevisionRequested' || status == 'FailedSafely';
    return SectionCard(
      title: 'Progress',
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Semantics(
            liveRegion: true,
            child: Container(
              key: const ValueKey('what-happens-next'),
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: sideState
                    ? AppColors.warning.withValues(alpha: 0.1)
                    : AppColors.brandSoft,
                borderRadius: BorderRadius.circular(AppRadius.control),
              ),
              child: Text(whatHappensNext(status)),
            ),
          ),
          if (status != 'Cancelled') ...[
            const SizedBox(height: 16),
            StatusTimeline(
              steps: tripTimelineSteps,
              current: timelineStatus(status),
            ),
          ],
        ],
      ),
    );
  }
}
