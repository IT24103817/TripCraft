import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/resources/data/guide_models.dart';
import 'package:tripcraft_mobile/features/resources/presentation/trip_day_screen.dart';

import '../helpers.dart';

/// A GET /api/guides/me/schedule body with one trip; [vehicle] adds the vehicle fields.
GuideSchedule fakeSchedule(Map<String, dynamic> vehicle) =>
    GuideSchedule.fromJson({
      'guideId': 'g1',
      'guideName': 'Nimal Perera',
      'trips': [
        {
          'tripRequestId': 't1',
          'objective': 'Kandy and Ella',
          'startDate': '2026-10-10',
          'endDate': '2026-10-11',
          'pax': 4,
          'status': 'Confirmed',
          ...vehicle,
          'days': [
            {
              'dayNumber': 1,
              'date': '2026-10-10',
              'city': 'Kandy',
              'stops': [
                {
                  'stopId': 's1',
                  'sequence': 1,
                  'attractionName': 'Temple of the Tooth',
                  'latitude': 7.29,
                  'longitude': 80.64,
                },
              ],
            },
          ],
        },
      ],
    });

Future<void> pumpDay(WidgetTester tester, GuideSchedule schedule) {
  final trip = schedule.trips.single;
  return pumpScreen(
    tester,
    TripDayScreen(stops: trip.days.single.guideStops, trip: trip),
    api: MockApiClient(),
  );
}

void main() {
  testWidgets(
    'the day screen shows the trip vehicle registration, type and seats',
    (tester) async {
      await pumpDay(
        tester,
        fakeSchedule({
          'vehicleRegistrationNo': 'CAB-1234',
          'vehicleType': 'Van',
          'vehicleSeats': 9,
        }),
      );

      expect(find.text('Vehicle'), findsOneWidget);
      expect(find.text('CAB-1234'), findsOneWidget);
      expect(find.text('Van'), findsOneWidget);
      expect(find.text('9'), findsOneWidget);
      expect(find.text('1. Temple of the Tooth'), findsOneWidget);
    },
  );

  testWidgets('an unknown vehicle shows a dash for each value', (tester) async {
    // An older API response without the vehicle fields still parses.
    await pumpDay(tester, fakeSchedule({}));

    expect(find.text('Vehicle'), findsOneWidget);
    expect(find.text('—'), findsNWidgets(3));
  });
}
