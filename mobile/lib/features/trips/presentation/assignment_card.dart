import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/section_card.dart';
import '../application/assignment_providers.dart';
import '../data/assignment_models.dart';

/// "Your guide and vehicle" of a confirmed trip (GET /api/trip-requests/{id}/assignment).
class AssignmentCard extends ConsumerWidget {
  const AssignmentCard({super.key, required this.tripId});

  final String tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SectionCard(
      title: 'Your guide and vehicle',
      child: AsyncView<TripAssignment>(
        value: ref.watch(tripAssignmentProvider(tripId)),
        onRetry: () => ref.invalidate(tripAssignmentProvider(tripId)),
        isEmpty: (a) => a.guideName == null && a.vehicleRegistrationNo == null,
        empty: const Text(
          'Your guide and vehicle appear here once the operator confirms the trip.',
        ),
        data: (a) => Column(
          children: [
            if (a.guideName != null)
              ListTile(
                contentPadding: EdgeInsets.zero,
                leading: const Icon(Icons.person_outline),
                title: Text(a.guideName!),
                subtitle: Text(a.guidePhone ?? 'Your guide'),
                trailing: a.guidePhone == null
                    ? null
                    : IconButton(
                        tooltip: 'Call your guide',
                        icon: const Icon(Icons.phone_outlined),
                        onPressed: () =>
                            launchUrl(Uri(scheme: 'tel', path: a.guidePhone)),
                      ),
              ),
            if (a.vehicleRegistrationNo != null)
              ListTile(
                contentPadding: EdgeInsets.zero,
                leading: const Icon(Icons.directions_car_outlined),
                title: Text(a.vehicleRegistrationNo!),
                subtitle: Text(vehicleLine(a)),
              ),
          ],
        ),
      ),
    );
  }
}

/// "Van · 9 seats", leaving out what is unknown.
String vehicleLine(TripAssignment a) => [
  ?a.vehicleType,
  if (a.vehicleSeats != null) '${a.vehicleSeats} seats',
].join(' · ');
