import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/resources/data/check_in.dart';
import 'package:tripcraft_mobile/features/resources/presentation/check_in_panel.dart';

import '../helpers.dart';

/// Puts the guide a fixed distance from any stop.
class FakeLocation implements LocationService {
  FakeLocation(this.distance);
  final double distance;

  @override
  Future<LocationFix> currentPosition() async => const LocationFix(7.29, 80.64);

  @override
  double distanceMeters(
    LocationFix from,
    double toLatitude,
    double toLongitude,
  ) => distance;
}

const temple = GuideStop(
  id: 's1',
  name: 'Temple of the Tooth',
  latitude: 7.2936,
  longitude: 80.6413,
);

Future<void> locateAt(
  WidgetTester tester,
  double distance, {
  MockApiClient? api,
  GuideStop stop = temple,
}) async {
  await pumpScreen(
    tester,
    Scaffold(
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: CheckInPanel(stop: stop),
      ),
    ),
    api: api ?? MockApiClient(),
    overrides: [
      locationServiceProvider.overrideWithValue(FakeLocation(distance)),
    ],
  );
  await tester.tap(find.text('Find my location'));
  await tester.pumpAndSettle();
}

FilledButton checkInButton(WidgetTester tester) => tester.widget<FilledButton>(
  find.descendant(
    of: find.bySemanticsLabel('Check in by GPS'),
    matching: find.byType(FilledButton),
  ),
);

void main() {
  testWidgets('Check in is disabled when more than 500 m away', (tester) async {
    await locateAt(tester, 820);

    expect(
      find.text(
        'You are 820 m from Temple of the Tooth. Get within 500 m to check in.',
      ),
      findsOneWidget,
    );
    expect(checkInButton(tester).onPressed, isNull);
  });

  testWidgets('Check in is enabled within 500 m', (tester) async {
    await locateAt(tester, 120);

    expect(
      find.text('You are 120 m from Temple of the Tooth.'),
      findsOneWidget,
    );
    expect(checkInButton(tester).onPressed, isNotNull);
  });

  testWidgets(
    'Check in posts the GPS fix to the API and shows the new trip status',
    (tester) async {
      final api = MockApiClient();
      when(() => api.post('/api/check-ins', body: any(named: 'body')))
          .thenAnswer(
            (_) async => {
              'stopId': 's1',
              'distanceMeters': 120,
              'checkedInAt': '2026-10-10T05:00:00Z',
              'tripStatus': 'InProgress',
            },
          );
      await locateAt(tester, 120, api: api);

      await tester.tap(find.text('Check in by GPS'));
      await tester.pumpAndSettle();

      final body =
          verify(
                () =>
                    api.post('/api/check-ins', body: captureAny(named: 'body')),
              ).captured.single
              as Map<String, dynamic>;
      expect(body, {
        'itineraryStopId': 's1',
        'latitude': 7.29,
        'longitude': 80.64,
      });
      expect(
        find.text('Checked in at Temple of the Tooth. Trip is in progress.'),
        findsOneWidget,
      );
      expect(find.textContaining('Checked in 10'), findsOneWidget);
    },
  );

  testWidgets('a stop already checked in shows when, with no button', (
    tester,
  ) async {
    await pumpScreen(
      tester,
      const Scaffold(
        body: CheckInPanel(
          stop: GuideStop(
            id: 's1',
            name: 'Temple of the Tooth',
            latitude: 7.2936,
            longitude: 80.6413,
            checkedInAt: '2026-10-10T05:00:00Z',
          ),
        ),
      ),
      api: MockApiClient(),
    );

    expect(find.textContaining('Checked in'), findsOneWidget);
    expect(find.text('Find my location'), findsNothing);
  });

  test('the 500 m rule includes the boundary', () {
    expect(CheckInRule.canCheckIn(500), isTrue);
    expect(CheckInRule.canCheckIn(500.1), isFalse);
  });
}
