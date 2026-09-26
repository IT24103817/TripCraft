import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/resources/presentation/schedule_screen.dart';

import '../helpers.dart';

Map<String, dynamic> trip(String objective, String status) => {
  'tripRequestId': 't-$objective',
  'objective': objective,
  'startDate': '2026-10-10',
  'endDate': '2026-10-11',
  'pax': 4,
  'status': status,
  'vehicleRegistrationNo': 'CAB-1234',
  'days': [
    {
      'dayNumber': 1,
      'date': '2026-10-10',
      'city': 'Kandy',
      'hotelName': 'Kandy Hills',
      'stops': [
        {
          'stopId': 's1',
          'sequence': 1,
          'attractionName': 'Temple of the Tooth',
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

void main() {
  late MockApiClient api;
  setUp(() => api = MockApiClient());

  testWidgets(
    'lists the guide\'s trips with vehicle, hotel and check-in progress',
    (tester) async {
      when(() => api.get('/api/guides/me/schedule')).thenAnswer(
        (_) async => {
          'guideId': 'g1',
          'guideName': 'Nimal Perera',
          'trips': [trip('Kandy and Ella', 'Confirmed')],
        },
      );

      await pumpScreen(tester, const ScheduleScreen(), api: api);
      await tester.pumpAndSettle();

      expect(find.text('Kandy and Ella'), findsOneWidget);
      expect(find.text('4 travellers · vehicle CAB-1234'), findsOneWidget);
      expect(find.text('2 stops · Kandy Hills · 1 checked in'), findsOneWidget);
    },
  );

  testWidgets('search and the status filter narrow the list', (tester) async {
    when(() => api.get('/api/guides/me/schedule')).thenAnswer(
      (_) async => {
        'guideId': 'g1',
        'guideName': 'Nimal Perera',
        'trips': [
          trip('Kandy and Ella', 'Confirmed'),
          trip('Galle coast', 'Completed'),
        ],
      },
    );

    await pumpScreen(tester, const ScheduleScreen(), api: api);
    await tester.pumpAndSettle();
    await tester.tap(find.widgetWithText(ChoiceChip, 'Completed'));
    await tester.pumpAndSettle();
    expect(find.text('Galle coast'), findsOneWidget);
    expect(find.text('Kandy and Ella'), findsNothing);

    await tester.tap(find.widgetWithText(ChoiceChip, 'All'));
    await tester.enterText(find.byType(TextField), 'kandy');
    await tester.pumpAndSettle();
    expect(find.text('Kandy and Ella'), findsOneWidget);
    expect(find.text('Galle coast'), findsNothing);
  });

  testWidgets('no assigned trips shows the empty state', (tester) async {
    when(() => api.get('/api/guides/me/schedule')).thenAnswer(
      (_) async => {'guideId': 'g1', 'guideName': 'Nimal Perera', 'trips': []},
    );

    await pumpScreen(tester, const ScheduleScreen(), api: api);
    await tester.pumpAndSettle();

    expect(find.text('No trips assigned'), findsOneWidget);
  });
}
