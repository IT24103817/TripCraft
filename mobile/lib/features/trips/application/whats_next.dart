import '../../../shared/utils/clock.dart';
import '../../../shared/utils/formatters.dart';
import '../data/trip_models.dart';

/// The one line under a trip card that says what happens next, e.g. "Guide: Nimal, starts 10 Oct".
/// [guideName] comes from the assignment (Confirmed trips) and [guideRated] from the guide rating
/// (Completed trips); [today] decides "day N of M" while the trip is running.
String whatsNextLine(
  TripRequest trip, {
  required DateTime today,
  String? guideName,
  bool guideRated = false,
}) {
  final starts = formatShortDate(trip.startDate);
  return switch (trip.status) {
    'Submitted' => 'Ready to plan',
    'Planning' => 'Our planner agents are building your trip',
    'QuotationSent' => 'Your quote is ready — accept or decline',
    'ClientAccepted' => 'Operator is confirming',
    'ClientDeclined' => 'The operator is reviewing your feedback',
    'NeedsOperator' => 'Our team is preparing your quote',
    'Confirmed' =>
      guideName == null || guideName.trim().isEmpty
          ? 'Starts $starts'
          : 'Guide: ${firstName(guideName)}, starts $starts',
    'InProgress' => _onTour(trip, today),
    'Completed' => guideRated ? 'Completed' : 'Rate your guide',
    'Cancelled' => 'Cancelled',
    _ => 'We will let you know when something changes',
  };
}

/// "On tour — day 2 of 4". Today is kept between the first and the last day.
String _onTour(TripRequest trip, DateTime today) {
  final start = DateTime.parse(trip.startDate);
  final end = DateTime.parse(trip.endDate);
  final total = end.difference(start).inDays + 1;
  final day = (dateOnly(today).difference(start).inDays + 1).clamp(1, total);
  return 'On tour — day $day of $total';
}

/// "Nimal Perera" -> "Nimal".
String firstName(String fullName) => fullName.trim().split(' ').first;
