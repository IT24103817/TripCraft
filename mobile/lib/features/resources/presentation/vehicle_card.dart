import 'package:flutter/material.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/widgets/section_card.dart';
import '../data/guide_models.dart';

/// The vehicle held for the guide's trip (registration, type and seats). "—" when a value is unknown.
class VehicleCard extends StatelessWidget {
  const VehicleCard({super.key, required this.trip});

  final GuideTrip trip;

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Vehicle',
      child: Column(
        children: [
          _Row(label: 'Registration', value: trip.vehicleRegistrationNo),
          _Row(label: 'Type', value: trip.vehicleType),
          _Row(label: 'Seats', value: trip.vehicleSeats?.toString()),
        ],
      ),
    );
  }
}

class _Row extends StatelessWidget {
  const _Row({required this.label, required this.value});

  final String label;
  final String? value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 4),
      child: Row(
        children: [
          Expanded(
            child: Text(
              label,
              style: Theme.of(context).textTheme.bodyMedium
                  ?.copyWith(color: AppColors.muted),
            ),
          ),
          Text(value ?? '—'),
        ],
      ),
    );
  }
}
