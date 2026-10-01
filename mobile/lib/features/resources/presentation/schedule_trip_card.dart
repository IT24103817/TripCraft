import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/routes.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/section_card.dart';
import '../../../shared/widgets/status_chip.dart';
import '../data/guide_models.dart';
import 'request_replacement_button.dart';
import 'trip_day_screen.dart';

/// One trip of the guide's schedule: dates, status, travellers, vehicle and its days (tap a day to check in).
class ScheduleTripCard extends StatelessWidget {
  const ScheduleTripCard({super.key, required this.trip});

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
