import 'package:flutter_riverpod/flutter_riverpod.dart';

/// The current time, as a function so tests can fix "now" (e.g. the greeting or "day 2 of 4").
final clockProvider = Provider<DateTime Function()>((ref) => DateTime.now);

/// [value] without the time of day, so dates compare by day.
DateTime dateOnly(DateTime value) =>
    DateTime(value.year, value.month, value.day);
