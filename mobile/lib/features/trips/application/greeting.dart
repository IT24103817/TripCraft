import 'whats_next.dart';

/// "Good morning, Amal": the time of day and the user's first name.
String greetingFor(DateTime now, String fullName) {
  final part = now.hour < 12
      ? 'morning'
      : (now.hour < 18 ? 'afternoon' : 'evening');
  final name = firstName(fullName);
  return name.isEmpty ? 'Good $part' : 'Good $part, $name';
}
