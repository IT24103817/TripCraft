import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/auth/profile_button.dart';
import '../../../shared/utils/clock.dart';
import '../../../shared/utils/reload.dart';
import '../../../shared/utils/statuses.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../application/guide_focus.dart';
import '../data/guide_models.dart';
import '../data/resources_repository.dart';
import 'focus_trip_card.dart';
import 'schedule_trip_card.dart';
import 'today_check_in_card.dart';
import 'vehicle_card.dart';

/// The guide's home: today's trip first (or the next one), its vehicle, the check-in panel for today's stops,
/// then the rest of the schedule with a status filter.
class GuideHomeScreen extends ConsumerStatefulWidget {
  const GuideHomeScreen({super.key});

  @override
  ConsumerState<GuideHomeScreen> createState() => _GuideHomeScreenState();
}

class _GuideHomeScreenState extends ConsumerState<GuideHomeScreen> {
  static const _filters = ['', 'Confirmed', 'InProgress', 'Completed'];
  String _status = '';

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('My schedule'),
        actions: const [ProfileButton()],
      ),
      body: RefreshIndicator(
        onRefresh: () => waitForReload(ref.refresh(myScheduleProvider.future)),
        child: AsyncView<GuideSchedule>(
          value: ref.watch(myScheduleProvider),
          onRetry: () => ref.invalidate(myScheduleProvider),
          isEmpty: (s) => s.trips.isEmpty,
          empty: ListView(
            children: const [
              SizedBox(height: 48),
              EmptyState(
                title: 'No trips assigned',
                message: 'Trips appear here once an operator approves one with you as the guide.',
              ),
            ],
          ),
          data: _body,
        ),
      ),
    );
  }

  Widget _body(GuideSchedule schedule) {
    final today = ref.watch(clockProvider)();
    final focus = focusTrip(schedule.trips, today);
    final rest = [
      for (final trip in schedule.trips)
        if (trip != focus?.trip && (_status.isEmpty || trip.status == _status))
          trip,
    ];
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        if (focus != null) ...[
          FocusTripCard(focus: focus, today: today),
          const SizedBox(height: 12),
          VehicleCard(trip: focus.trip),
          const SizedBox(height: 12),
          if (focus.isToday) ...[
            TodayCheckInCard(trip: focus.trip, day: dayOn(focus.trip, today)),
            const SizedBox(height: 12),
          ],
        ],
        Text('Your schedule', style: Theme.of(context).textTheme.titleLarge),
        SizedBox(
          height: 52,
          child: ListView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(vertical: 8),
            children: [
              for (final status in _filters)
                Padding(
                  padding: const EdgeInsets.only(right: 8),
                  child: ChoiceChip(
                    label: Text(status.isEmpty ? 'All' : statusLabel(status)),
                    selected: _status == status,
                    onSelected: (_) => setState(() => _status = status),
                  ),
                ),
            ],
          ),
        ),
        if (rest.isEmpty)
          const Padding(
            padding: EdgeInsets.symmetric(vertical: 16),
            child: Text('No other trips.'),
          ),
        for (final trip in rest) ScheduleTripCard(trip: trip),
      ],
    );
  }
}
