import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/resources/application/guide_focus.dart';
import 'package:tripcraft_mobile/features/resources/data/guide_models.dart';
import 'package:tripcraft_mobile/features/resources/presentation/guide_home_screen.dart';

import '../helpers.dart';

/// One trip of GET /api/guides/me/schedule, two days long from [start].
Map<String, dynamic> trip(
  String objective,
  String status, {
  String start = '2026-10-10',
  String end = '2026-10-11',
  String city = 'Kandy',
  String vehicle = 'CAB-1234',
  String firstStop = 'Temple of the Tooth',
}) => {
  'tripRequestId': 't-$objective',
  'objective': objective,
  'startDate': start,
  'endDate': end,
  'pax': 4,
  'status': status,
  'vehicleRegistrationNo': vehicle,
  'vehicleType': 'Van',
  'vehicleSeats': 9,
  'days': [
    {
      'dayNumber': 1,
      'date': start,
      'city': city,
      'hotelName': 'Kandy Hills',
      'stops': [
        {
          'stopId': 's1',
          'sequence': 1,
          'attractionName': firstStop,
          'latitude': 7.29,
          'longitude': 80.64,
          'checkedInAt': '2026-10-10T05:00:00Z',
        },
        {
          'stopId': 's2',
          'sequence': 2,
          'attractionName': 'Peradeniya Gardens',
          'latitude': 7.27,
          'longitude': 80.59,
        },
      ],
    },
  ],
};

Map<String, dynamic> schedule(List<Map<String, dynamic>> trips) => {
  'guideId': 'g1',
  'guideName': 'Nimal Perera',
  'trips': trips,
};

void main() {
  late MockApiClient api;
  setUp(() => api = MockApiClient());

  Future<void> pumpHome(
    WidgetTester tester,
    List<Map<String, dynamic>> trips, {
    required DateTime now,
  }) async {
    when(() => api.get('/api/guides/me/schedule'))
        .thenAnswer((_) async => schedule(trips));
    await pumpScreen(
      tester,
      const GuideHomeScreen(),
      api: api,
      overrides: [fixedClock(now)],
    );
    await tester.pumpAndSettle();
  }

  group('focusTrip', () {
    List<GuideTrip> trips(List<Map<String, dynamic>> json) =>
        GuideSchedule.fromJson(schedule(json)).trips;

    test('the trip running today comes before a later one', () {
      final focus = focusTrip(
        trips([
          trip(
            'Galle coast',
            'Confirmed',
            start: '2026-10-20',
            end: '2026-10-21',
          ),
          trip('Kandy and Ella', 'InProgress'),
        ]),
        DateTime(2026, 10, 11, 15),
      );
      expect(focus?.trip.objective, 'Kandy and Ella');
      expect(focus?.isToday, isTrue);
    });

    test('with nothing today, the next confirmed trip', () {
      final focus = focusTrip(
        trips([
          trip(
            'Galle coast',
            'Confirmed',
            start: '2026-10-20',
            end: '2026-10-21',
          ),
          trip('Kandy and Ella', 'Confirmed'),
        ]),
        DateTime(2026, 10, 2),
      );
      expect(focus?.trip.objective, 'Kandy and Ella');
      expect(focus?.isToday, isFalse);
    });

    test('nothing ahead: no focus trip', () {
      expect(
        focusTrip(trips([trip('Kandy', 'Completed')]), DateTime(2026, 10, 2)),
        isNull,
      );
    });
  });

  testWidgets(
    'shows today\'s trip first, then its vehicle, check-in and the rest of the schedule',
    (tester) async {
      usePhoneSize(tester, phoneSizes.currentValue!);
      await pumpHome(tester, [
        trip(
          'Galle coast',
          'Confirmed',
          start: '2026-10-20',
          end: '2026-10-21',
        ),
        trip('Kandy and Ella', 'Confirmed'),
      ], now: DateTime(2026, 10, 10, 8));

      expect(find.text('Today'), findsOneWidget);
      expect(find.text('Kandy and Ella'), findsOneWidget);
      expect(find.text('Day 1 of 1 · Kandy'), findsOneWidget);
      expect(find.text('Vehicle'), findsOneWidget);
      expect(find.text('CAB-1234'), findsOneWidget);
      expect(find.text('Van'), findsOneWidget);
      expect(
        tester.getTopLeft(find.text('Today')).dy,
        lessThan(tester.getTopLeft(find.text('Vehicle')).dy),
      );

      await tester.scrollUntilVisible(
        find.text('Scan voucher'),
        200,
        scrollable: verticalScrollable(),
      );
      expect(find.text('Check in today'), findsOneWidget);
      await tester.scrollUntilVisible(
        find.text('2. Peradeniya Gardens'),
        200,
        scrollable: verticalScrollable(),
      );
      // GPS check-in is offered next to the voucher scan; a checked-in stop says so.
      expect(find.text('Find my location'), findsOneWidget);
      expect(find.textContaining('Checked in'), findsOneWidget);

      await tester.scrollUntilVisible(
        find.text('Galle coast'),
        200,
        scrollable: verticalScrollable(),
      );
      expect(find.text('Your schedule'), findsOneWidget);
    },
    variant: phoneSizes,
  );

  testWidgets('before the trip starts it is the "Next trip" with its date', (
    tester,
  ) async {
    await pumpHome(tester, [
      trip('Kandy and Ella', 'Confirmed'),
    ], now: DateTime(2026, 10, 2, 9));

    expect(find.text('Next trip'), findsOneWidget);
    expect(find.text('Starts 10 Oct 2026'), findsOneWidget);
    expect(find.text('Vehicle'), findsOneWidget);
    expect(find.text('Check in today'), findsNothing);
    expect(find.text('Request a change'), findsOneWidget);
  });

  testWidgets('the rest of the schedule shows hotel and check-in progress', (
    tester,
  ) async {
    await pumpHome(tester, [
      trip('Kandy and Ella', 'Completed'),
    ], now: DateTime(2026, 10, 20));

    expect(find.text('Today'), findsNothing);
    expect(find.text('Next trip'), findsNothing);
    expect(find.text('Kandy and Ella'), findsOneWidget);
    expect(find.text('4 travellers · vehicle CAB-1234'), findsOneWidget);
    expect(find.text('2 stops · Kandy Hills · 1 checked in'), findsOneWidget);
  });

  testWidgets('the status filter narrows the rest of the schedule', (
    tester,
  ) async {
    await pumpHome(tester, [
      trip(
        'Kandy and Ella',
        'InProgress',
        start: '2026-09-01',
        end: '2026-09-02',
      ),
      trip('Galle coast', 'Completed', start: '2026-09-05', end: '2026-09-06'),
    ], now: DateTime(2026, 10, 20));

    await tester.tap(find.widgetWithText(ChoiceChip, 'Completed'));
    await tester.pumpAndSettle();
    expect(find.text('Galle coast'), findsOneWidget);
    expect(find.text('Kandy and Ella'), findsNothing);

    await tester.tap(find.widgetWithText(ChoiceChip, 'All'));
    await tester.pumpAndSettle();
    expect(find.text('Kandy and Ella'), findsOneWidget);
  });

  group('schedule search', () {
    // Three past trips, so none of them is the focus trip and all are in "Your schedule".
    final pastTrips = [
      trip(
        'Kandy and Ella',
        'InProgress',
        start: '2026-09-01',
        end: '2026-09-02',
      ),
      trip(
        'Galle coast',
        'Completed',
        start: '2026-09-05',
        end: '2026-09-06',
        city: 'Galle',
        vehicle: 'WP-7788',
        firstStop: 'Galle Fort',
      ),
      trip(
        'Cultural triangle',
        'Completed',
        start: '2026-09-10',
        end: '2026-09-11',
        city: 'Sigiriya',
        vehicle: 'NC-4455',
        firstStop: 'Sigiriya Rock',
      ),
    ];

    // A tall screen, so all three schedule cards are built at once.
    Future<void> pumpTall(WidgetTester tester) async {
      usePhoneSize(tester, const Size(412, 2400));
      await pumpHome(tester, pastTrips, now: DateTime(2026, 10, 20));
    }

    Future<void> search(WidgetTester tester, String text) async {
      await tester.enterText(find.byType(TextField), text);
      await tester.pumpAndSettle();
    }

    testWidgets('typing filters the list by objective, city, stop or vehicle', (
      tester,
    ) async {
      await pumpTall(tester);
      expect(find.text('Kandy and Ella'), findsOneWidget);
      expect(find.text('Galle coast'), findsOneWidget);
      expect(find.text('Cultural triangle'), findsOneWidget);

      await search(tester, 'GALLE');
      expect(find.text('Galle coast'), findsOneWidget);
      expect(find.text('Kandy and Ella'), findsNothing);
      expect(find.text('Cultural triangle'), findsNothing);

      await search(tester, 'sigiriya rock');
      expect(find.text('Cultural triangle'), findsOneWidget);
      expect(find.text('Galle coast'), findsNothing);

      await search(tester, 'wp-77');
      expect(find.text('Galle coast'), findsOneWidget);
      expect(find.text('Cultural triangle'), findsNothing);

      await search(tester, 'ella');
      expect(find.text('Kandy and Ella'), findsOneWidget);
      expect(find.text('Galle coast'), findsNothing);
    });

    testWidgets('no match shows the empty state; clearing restores the list', (
      tester,
    ) async {
      await pumpTall(tester);

      await search(tester, 'Jaffna');
      expect(find.text('No trips match your search'), findsOneWidget);
      expect(find.text('Kandy and Ella'), findsNothing);

      await tester.tap(find.byTooltip('Clear search'));
      await tester.pumpAndSettle();
      expect(find.text('No trips match your search'), findsNothing);
      expect(find.text('Kandy and Ella'), findsOneWidget);
      expect(find.text('Galle coast'), findsOneWidget);
      expect(find.text('Cultural triangle'), findsOneWidget);
      expect(find.byTooltip('Clear search'), findsNothing);
    });

    testWidgets('search and the status filter work together', (tester) async {
      await pumpTall(tester);

      // "Kandy" is the city of the first trip only.
      await search(tester, 'kandy');
      // The status chips scroll sideways on a phone.
      final completed = find.widgetWithText(ChoiceChip, 'Completed');
      await tester.scrollUntilVisible(
        completed,
        100,
        scrollable: find
            .ancestor(
              of: find.widgetWithText(ChoiceChip, 'All'),
              matching: find.byType(Scrollable),
            )
            .first,
      );
      await tester.ensureVisible(completed);
      await tester.pumpAndSettle();
      await tester.tap(completed);
      await tester.pumpAndSettle();
      expect(find.text('No trips match your search'), findsOneWidget);

      await search(tester, 'galle');
      expect(find.text('Galle coast'), findsOneWidget);
      expect(find.text('Cultural triangle'), findsNothing);

      await search(tester, '');
      expect(find.text('Galle coast'), findsOneWidget);
      expect(find.text('Cultural triangle'), findsOneWidget);
      expect(find.text('Kandy and Ella'), findsNothing);
    });
  });

  testWidgets('no assigned trips shows the empty state', (tester) async {
    await pumpHome(tester, [], now: DateTime(2026, 10, 2));

    expect(find.text('No trips assigned'), findsOneWidget);
  });

  testWidgets('a failed load shows the error with Retry', (tester) async {
    when(() => api.get('/api/guides/me/schedule')).thenThrow(Exception('x'));
    await pumpScreen(tester, const GuideHomeScreen(), api: api);
    await tester.pumpAndSettle();

    expect(find.text('Something went wrong'), findsOneWidget);
    expect(find.text('Retry'), findsOneWidget);
  });
}
