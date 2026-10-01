import '../../../shared/utils/clock.dart';
import '../data/guide_models.dart';

/// The trip the guide's home shows first. [isToday] is true when it is running today ("Today"),
/// false when it is the next one to start ("Next trip").
class GuideFocus {
  const GuideFocus({required this.trip, required this.isToday});

  final GuideTrip trip;
  final bool isToday;
}

/// Statuses of a trip the guide still has to run.
const _activeStatuses = {'Confirmed', 'InProgress'};

/// The trip running today (today is between its first and last day), else the next confirmed trip.
/// Null when the guide has nothing today or later.
GuideFocus? focusTrip(List<GuideTrip> trips, DateTime today) {
  final day = dateOnly(today);
  for (final trip in trips) {
    final start = DateTime.parse(trip.startDate);
    final end = DateTime.parse(trip.endDate);
    final running = !day.isBefore(start) && !day.isAfter(end);
    if (_activeStatuses.contains(trip.status) && running) {
      return GuideFocus(trip: trip, isToday: true);
    }
  }
  final upcoming = [
    for (final trip in trips)
      if (trip.status == 'Confirmed' &&
          DateTime.parse(trip.startDate).isAfter(day))
        trip,
  ]..sort((a, b) => a.startDate.compareTo(b.startDate));
  return upcoming.isEmpty
      ? null
      : GuideFocus(trip: upcoming.first, isToday: false);
}

/// The day of [trip] that falls on [today], or null.
GuideDay? dayOn(GuideTrip trip, DateTime today) {
  final day = dateOnly(today);
  for (final d in trip.days) {
    if (DateTime.parse(d.date) == day) return d;
  }
  return null;
}
