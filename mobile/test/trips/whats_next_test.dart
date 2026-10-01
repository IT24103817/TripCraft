import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/trips/application/greeting.dart';
import 'package:tripcraft_mobile/features/trips/application/whats_next.dart';
import 'package:tripcraft_mobile/features/trips/data/trip_models.dart';

import '../helpers.dart';

/// A trip from 10 to 14 October 2026 in [status].
TripRequest trip(String status) =>
    TripRequest.fromJson(tripJson(status: status));

void main() {
  final today = DateTime(2026, 10, 2, 9);

  group('whatsNextLine', () {
    test('one line per status', () {
      final expected = {
        'Submitted': 'Ready to plan',
        'Planning': 'Our planner agents are building your trip',
        'PendingReview': 'Operator is reviewing your plan',
        'RevisionRequested':
            'Operator asked for changes; a new version is coming',
        'QuotationSent': 'Waiting for your acceptance',
        'ClientAccepted': 'Operator is confirming',
        'Confirmed': 'Starts 10 Oct',
        'Completed': 'Rate your guide',
        'FailedSafely': 'Planning failed — tap to try again',
        'Cancelled': 'Cancelled',
      };
      for (final entry in expected.entries) {
        expect(
          whatsNextLine(trip(entry.key), today: today),
          entry.value,
          reason: entry.key,
        );
      }
    });

    test('Confirmed names the guide by first name when known', () {
      expect(
        whatsNextLine(
          trip('Confirmed'),
          today: today,
          guideName: 'Nimal Perera',
        ),
        'Guide: Nimal, starts 10 Oct',
      );
      expect(
        whatsNextLine(trip('Confirmed'), today: today, guideName: '  '),
        'Starts 10 Oct',
      );
    });

    test('InProgress counts the day of the tour, kept within the trip', () {
      String onDay(DateTime day) =>
          whatsNextLine(trip('InProgress'), today: day);
      expect(onDay(DateTime(2026, 10, 10, 7)), 'On tour — day 1 of 5');
      expect(onDay(DateTime(2026, 10, 12, 23)), 'On tour — day 3 of 5');
      expect(onDay(DateTime(2026, 10, 14)), 'On tour — day 5 of 5');
      expect(onDay(DateTime(2026, 10, 20)), 'On tour — day 5 of 5');
      expect(onDay(DateTime(2026, 10, 1)), 'On tour — day 1 of 5');
    });

    test('Completed says "Completed" once the guide is rated', () {
      expect(
        whatsNextLine(trip('Completed'), today: today, guideRated: true),
        'Completed',
      );
    });
  });

  group('greetingFor', () {
    test('morning, afternoon and evening with the first name', () {
      expect(
        greetingFor(DateTime(2026, 10, 2, 6), 'Amal Perera'),
        'Good morning, Amal',
      );
      expect(
        greetingFor(DateTime(2026, 10, 2, 12), 'Amal Perera'),
        'Good afternoon, Amal',
      );
      expect(
        greetingFor(DateTime(2026, 10, 2, 18), 'Amal Perera'),
        'Good evening, Amal',
      );
    });

    test('without a name, just the time of day', () {
      expect(greetingFor(DateTime(2026, 10, 2, 9), ''), 'Good morning');
    });
  });
}
