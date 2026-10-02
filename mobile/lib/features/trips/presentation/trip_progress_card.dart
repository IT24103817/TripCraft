import 'package:flutter/material.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_timeline.dart';

/// The main path of a trip in the v1.1 lifecycle (docs/API-V11.md), as the tourist sees it.
/// The quotation goes straight to the tourist: there is no operator review step.
const tripTimelineSteps = [
  'Submitted',
  'Planning',
  'QuotationSent',
  'ClientAccepted',
  'Confirmed',
  'InProgress',
  'Completed',
];

/// The side states: off the main path, waiting for the operator.
const tripSideStates = {'ClientDeclined', 'NeedsOperator'};

/// Where a trip status sits on the timeline. The side states stay on the step they came from:
/// ClientDeclined on Quotation sent (the tourist declined it), NeedsOperator on Planning (the agents could not finish).
String timelineStatus(String tripStatus) => switch (tripStatus) {
  'ClientDeclined' => 'QuotationSent',
  'NeedsOperator' => 'Planning',
  _ => tripStatus,
};

/// One short sentence per status: what happens next.
String whatHappensNext(String tripStatus) => switch (tripStatus) {
  'Submitted' => 'We have your request. Our AI agents start planning it next.',
  'Planning' => 'Our AI agents are drafting your itinerary and price. Your quote is sent to you as soon as it is ready.',
  'QuotationSent' =>
    'Your quote is ready. Review it, then accept or decline it.',
  'ClientAccepted' => 'You accepted the quote. The operator is now confirming your guide, vehicle and hotels.',
  'ClientDeclined' =>
    'You declined this quote — the operator will replan or contact you.',
  'NeedsOperator' =>
    'Our team is looking at your trip and will send you a quote.',
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
    final sideState = tripSideStates.contains(status);
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
