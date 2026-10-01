import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/auth/profile_button.dart';
import '../../../core/router/routes.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/statuses.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../data/guide_models.dart';
import '../data/resources_repository.dart';
import 'request_replacement_button.dart';
import 'trip_day_screen.dart';

/// The guide's schedule (Component B): only trips they are held for, with search and a status filter.
class ScheduleScreen extends ConsumerStatefulWidget {
  const ScheduleScreen({super.key});

  @override
  ConsumerState<ScheduleScreen> createState() => _ScheduleScreenState();
}

class _ScheduleScreenState extends ConsumerState<ScheduleScreen> {
  static const _filters = ['', 'Confirmed', 'InProgress', 'Completed'];
  String _search = '';
  String _status = '';

  List<GuideTrip> _visible(GuideSchedule schedule) => [
    for (final trip in schedule.trips)
      if ((_status.isEmpty || trip.status == _status) &&
          trip.objective.toLowerCase().contains(_search.toLowerCase()))
        trip,
  ];

  @override
  Widget build(BuildContext context) {
    final schedule = ref.watch(myScheduleProvider);
    return Scaffold(
      appBar: AppBar(
        title: const Text('My schedule'),
        actions: const [ProfileButton()],
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 0),
            child: TextField(
              decoration: const InputDecoration(
                prefixIcon: Icon(Icons.search),
                hintText: 'Search your trips',
              ),
              onChanged: (text) => setState(() => _search = text.trim()),
            ),
          ),
          SizedBox(
            height: 52,
            child: ListView(
              scrollDirection: Axis.horizontal,
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              children: [
                for (final status in _filters)
                  Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 4),
                    child: ChoiceChip(
                      label: Text(status.isEmpty ? 'All' : statusLabel(status)),
                      selected: _status == status,
                      onSelected: (_) => setState(() => _status = status),
                    ),
                  ),
              ],
            ),
          ),
          Expanded(
            child: RefreshIndicator(
              onRefresh: () => ref.refresh(myScheduleProvider.future),
              child: AsyncView<GuideSchedule>(
                value: schedule,
                onRetry: () => ref.invalidate(myScheduleProvider),
                isEmpty: (s) => _visible(s).isEmpty,
                empty: ListView(
                  children: const [
                    SizedBox(height: 48),
                    EmptyState(
                      title: 'No trips assigned',
                      message: 'Trips appear here once an operator approves one with you as the guide.',
                    ),
                  ],
                ),
                data: (s) => ListView(
                  padding: const EdgeInsets.all(16),
                  children: [
                    for (final trip in _visible(s)) _TripCard(trip: trip),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _TripCard extends StatelessWidget {
  const _TripCard({required this.trip});

  final GuideTrip trip;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: SectionCard(
        title: '${formatDate(trip.startDate)} – ${formatDate(trip.endDate)}',
        trailing: StatusChip(status: trip.status),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(trip.objective),
            const SizedBox(height: 4),
            Text(
              '${trip.pax} travellers · vehicle ${trip.vehicleRegistrationNo ?? 'not assigned'}',
            ),
            for (final day in trip.days)
              ListTile(
                contentPadding: EdgeInsets.zero,
                leading: CircleAvatar(child: Text('${day.dayNumber}')),
                title: Text('${day.city} · ${formatDate(day.date)}'),
                subtitle: Text(
                  '${day.stops.length} stops'
                  '${day.hotelName == null ? '' : ' · ${day.hotelName}'}'
                  ' · ${day.stops.where((s) => s.checkedInAt != null).length} checked in',
                ),
                trailing: const Icon(Icons.chevron_right),
                onTap: () => context.push(
                  Routes.tripDay,
                  extra: TripDayArgs(trip: trip, day: day),
                ),
              ),
            // A guide can only ask to be replaced before the trip starts (API: trip Confirmed).
            if (trip.status == 'Confirmed')
              Align(
                alignment: Alignment.centerRight,
                child: RequestReplacementButton(tripId: trip.tripRequestId),
              ),
          ],
        ),
      ),
    );
  }
}
