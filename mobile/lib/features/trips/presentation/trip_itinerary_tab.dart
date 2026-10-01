import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/widgets/async_view.dart';
import '../../../shared/widgets/empty_state.dart';
import '../../../shared/widgets/section_card.dart';
import '../application/trips_providers.dart';
import '../data/trip_models.dart';
import '../data/weather.dart';
import 'trip_map.dart';

/// The "Itinerary" tab: each day with its weather icon and stops, then a map of the stops joined by the route.
/// Days come from the agents' proposal when there is one, otherwise from the saved itinerary.
class TripItineraryTab extends ConsumerWidget {
  const TripItineraryTab({
    super.key,
    required this.tripId,
    required this.workflow,
  });

  final String tripId;
  final TripWorkflow? workflow;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final fromProposal = proposalDays(workflow);
    final days = fromProposal.isEmpty
        ? ref.watch(savedItineraryProvider(tripId))
        : AsyncData(fromProposal);

    return AsyncView<List<TripDay>>(
      value: days,
      onRetry: () => ref.invalidate(savedItineraryProvider(tripId)),
      isEmpty: (list) => list.isEmpty,
      // A ListView so pull-to-refresh also works on the empty tab.
      empty: ListView(
        children: const [
          SizedBox(height: 48),
          EmptyState(
            icon: Icons.map_outlined,
            title: 'No itinerary yet',
            message: 'Your day-by-day plan appears here once the agents have drafted it.',
          ),
        ],
      ),
      data: (list) => ListView(
        padding: const EdgeInsets.all(16),
        children: [
          SectionCard(
            title: 'Day by day',
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [for (final day in list) _DayTile(day: day)],
            ),
          ),
          const SizedBox(height: 12),
          _RouteMap(days: list),
        ],
      ),
    );
  }
}

class _DayTile extends StatelessWidget {
  const _DayTile({required this.day});

  final TripDay day;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final kind = weatherKindOf(day.weather);
    final details = [
      if (day.date != null) formatDate(day.date),
      if (day.transport != null) 'by ${day.transport}',
      if (day.weather != null) day.weather!,
    ].join(' · ');
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Day ${day.day} — ${day.city}',
                  style: theme.textTheme.titleSmall,
                ),
                if (details.isNotEmpty)
                  Text(details, style: theme.textTheme.bodySmall),
                for (final stop in day.stops)
                  Padding(
                    padding: const EdgeInsets.only(left: 8, top: 2),
                    child: Text('• ${stop.name}'),
                  ),
              ],
            ),
          ),
          if (kind != null)
            Icon(
              weatherIcon(kind),
              key: ValueKey('weather-${day.day}-${kind.name}'),
              color: AppColors.muted,
              semanticLabel: day.weather,
            ),
        ],
      ),
    );
  }
}

/// The icon for each kind of weather.
IconData weatherIcon(WeatherKind kind) => switch (kind) {
  WeatherKind.sun => Icons.wb_sunny_outlined,
  WeatherKind.cloud => Icons.cloud_outlined,
  WeatherKind.rain => Icons.water_drop_outlined,
  WeatherKind.storm => Icons.thunderstorm_outlined,
};

/// The map of all stops. The saved itinerary already has each stop's position; the agents' proposal does not,
/// so then the positions are loaded per attraction. The map is optional: without positions it is left out.
class _RouteMap extends ConsumerWidget {
  const _RouteMap({required this.days});

  final List<TripDay> days;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final stops = days.expand((d) => d.stops).toList();
    if (stops.isEmpty) return const SizedBox.shrink();

    final allKnown = stops.every(
      (s) => s.latitude != null && s.longitude != null,
    );
    final List<Attraction>? located;
    if (allKnown) {
      located = [
        for (final day in days)
          for (final s in day.stops)
            Attraction(
              id: s.attractionId,
              name: s.name,
              city: day.city,
              latitude: s.latitude!,
              longitude: s.longitude!,
            ),
      ];
    } else {
      final ids = stops.map((s) => s.attractionId).join(',');
      located = ref.watch(stopLocationsProvider(ids)).value;
    }
    if (located == null || located.isEmpty) return const SizedBox.shrink();
    return SectionCard(
      title: 'Route',
      child: TripMap(stops: located),
    );
  }
}
