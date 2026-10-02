import '../data/guide_models.dart';

/// True when [trip] matches the guide's search [query]: its objective, a day's city, a stop name or the
/// vehicle registration contains the query (case-insensitive). An empty query matches every trip.
bool matchesScheduleSearch(GuideTrip trip, String query) {
  final needle = query.trim().toLowerCase();
  if (needle.isEmpty) return true;

  final haystack = <String>[
    trip.objective,
    trip.vehicleRegistrationNo ?? '',
    for (final day in trip.days) day.city,
    for (final day in trip.days)
      for (final stop in day.stops) stop.attractionName,
  ];
  return haystack.any((text) => text.toLowerCase().contains(needle));
}
