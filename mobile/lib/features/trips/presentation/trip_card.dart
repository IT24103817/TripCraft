import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/clock.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/status_chip.dart';
import '../application/assignment_providers.dart';
import '../application/whats_next.dart';
import '../data/trip_models.dart';

/// A trip in a list: what it is, when, its status pill and one line about what happens next.
class TripCard extends ConsumerWidget {
  const TripCard({super.key, required this.trip});

  final TripRequest trip;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    // Only a confirmed trip has a guide to name, and only a completed one can be rated.
    final guideName = trip.status == 'Confirmed'
        ? ref.watch(tripAssignmentProvider(trip.id)).value?.guideName
        : null;
    final rated =
        trip.status == 'Completed' &&
        ref.watch(guideRatingProvider(trip.id)).value != null;
    final next = whatsNextLine(
      trip,
      today: ref.watch(clockProvider)(),
      guideName: guideName,
      guideRated: rated,
    );

    return Card(
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: () => context.push(Routes.trip(trip.id)),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    child: Text(
                      trip.objective,
                      style: theme.textTheme.titleMedium,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  const SizedBox(width: 8),
                  StatusChip(status: trip.status),
                ],
              ),
              const SizedBox(height: 4),
              Text(
                '${formatDate(trip.startDate)} – ${formatDate(trip.endDate)} · ${trip.pax} travellers',
                style: theme.textTheme.bodySmall,
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  const Icon(
                    Icons.arrow_forward,
                    size: 16,
                    color: AppColors.brand,
                  ),
                  const SizedBox(width: 6),
                  Expanded(
                    child: Text(
                      next,
                      key: ValueKey('whats-next-${trip.id}'),
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: AppColors.brand,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
