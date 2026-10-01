import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/trips/data/trip_templates_repository.dart';
import 'package:tripcraft_mobile/features/trips/presentation/package_hero.dart';
import 'package:tripcraft_mobile/features/trips/presentation/tourist_home_screen.dart';

import '../helpers.dart';
import 'template_fakes.dart';
import 'trip_fakes.dart';

/// A trip of GET /api/trip-requests with its own dates.
Map<String, dynamic> datedTrip(String id, String status, String objective) => {
  ...tripJson(id: id, status: status, objective: objective),
  'startDate': '2026-10-10',
  'endDate': '2026-10-14',
};

void main() {
  late MockApiClient api;
  setUp(() => api = MockApiClient());

  /// Signs Amal Perera in (a Tourist) and opens the home screen at [now].
  Future<void> pumpHome(
    WidgetTester tester, {
    required DateTime now,
    List<Map<String, dynamic>> trips = const [],
    List<Map<String, dynamic>>? templates,
  }) async {
    when(() => api.get('/api/trip-requests', query: any(named: 'query')))
        .thenAnswer((_) async => pagedJson(trips));
    final storage = InMemorySessionStorage()
      ..token = 'jwt'
      ..user = {
        'id': 'u1',
        'email': 'amal@example.com',
        'fullName': 'Amal Perera',
        'role': 'Tourist',
        'isActive': true,
      };
    await pumpScreen(
      tester,
      const TouristHomeScreen(),
      api: api,
      storage: storage,
      overrides: [
        fixedClock(now),
        tripTemplatesRepositoryProvider.overrideWithValue(
          FakeTripTemplatesRepository(list: templates),
        ),
      ],
    );
    await tester.pumpAndSettle();
  }

  testWidgets('greets the tourist by first name and time of day', (
    tester,
  ) async {
    await pumpHome(tester, now: DateTime(2026, 10, 2, 8, 30));

    expect(find.text('Good morning, Amal'), findsOneWidget);
    expect(find.text('Plan a new trip'), findsOneWidget);
  });

  testWidgets('shows the mood packages from the API as cards', (tester) async {
    usePhoneSize(tester, phoneSizes.currentValue!);
    await pumpHome(
      tester,
      now: DateTime(2026, 10, 2, 19),
      templates: [
        templateJson(),
        templateJson(
          id: 'tpl-2',
          slug: 'beach-and-heritage',
          name: 'Beach and heritage',
        ),
      ],
    );

    expect(find.text('Good evening, Amal'), findsOneWidget);
    expect(find.text('Trips picked for your mood'), findsOneWidget);
    expect(find.text('Hill-country escape'), findsOneWidget);
    expect(find.text('Cool & green'), findsWidgets);
    expect(find.text('4 days · Kandy, Nuwara Eliya, Ella'), findsWidgets);
    expect(find.text('From USD 640 for 2'), findsWidgets);
    expect(find.byType(PackageHero), findsWidgets);
  }, variant: phoneSizes);

  testWidgets('a package without a photo gets the placeholder', (tester) async {
    await pumpHome(
      tester,
      now: DateTime(2026, 10, 2, 9),
      templates: [templateJson(slug: 'no-such-photo')],
    );
    // Loading the missing asset fails outside the fake clock.
    await tester.runAsync(
      () => Future<void>.delayed(const Duration(milliseconds: 100)),
    );
    await tester.pump();

    expect(
      find.byKey(const ValueKey('package-hero-placeholder')),
      findsOneWidget,
    );
  });

  testWidgets('My trips shows each trip with what happens next', (
    tester,
  ) async {
    when(() => api.get('/api/trip-requests/t-conf/assignment')).thenAnswer(
      (_) async => {'tripRequestId': 't-conf', 'guideName': 'Nimal Perera'},
    );
    when(() => api.get('/api/trip-requests/t-done/guide-rating'))
        .thenThrow(notFound);
    await pumpHome(
      tester,
      now: DateTime(2026, 10, 11, 14),
      trips: [
        datedTrip('t-quote', 'QuotationSent', 'Kandy by train'),
        datedTrip('t-conf', 'Confirmed', 'Galle beaches'),
        datedTrip('t-tour', 'InProgress', 'Ella hikes'),
        datedTrip('t-done', 'Completed', 'Sigiriya'),
      ],
    );

    Future<String> line(String id) async {
      final finder = find.byKey(ValueKey('whats-next-$id'));
      await tester.scrollUntilVisible(
        finder,
        200,
        scrollable: verticalScrollable(),
      );
      return tester.widget<Text>(finder).data!;
    }

    expect(await line('t-quote'), 'Waiting for your acceptance');
    expect(await line('t-conf'), 'Guide: Nimal, starts 10 Oct');
    expect(await line('t-tour'), 'On tour — day 2 of 5');
    expect(await line('t-done'), 'Rate your guide');
  });

  testWidgets('no trips yet: an empty state under the packages', (
    tester,
  ) async {
    await pumpHome(tester, now: DateTime(2026, 10, 2, 9));
    await tester.scrollUntilVisible(
      find.text('No trips yet'),
      200,
      scrollable: verticalScrollable(),
    );

    expect(find.text('No trips yet'), findsOneWidget);
  });

  testWidgets('trips that fail to load show Retry', (tester) async {
    await pumpHome(tester, now: DateTime(2026, 10, 2, 9));
    when(() => api.get('/api/trip-requests', query: any(named: 'query')))
        .thenThrow(Exception('offline'));
    // Pull to refresh reloads the trips, which now fail.
    await tester.fling(verticalScrollable(), const Offset(0, 400), 1000);
    await tester.pumpAndSettle();
    await tester.scrollUntilVisible(
      find.text('Retry'),
      200,
      scrollable: verticalScrollable(),
    );

    expect(find.text('Retry'), findsOneWidget);
  });
}
