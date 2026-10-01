import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/trips/data/template_models.dart';
import 'package:tripcraft_mobile/features/trips/data/trip_templates_repository.dart';
import 'package:tripcraft_mobile/features/trips/presentation/new_trip_screen.dart';
import 'package:tripcraft_mobile/features/trips/presentation/package_detail_screen.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_prefill.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';

import '../helpers.dart';
import 'template_fakes.dart';

/// "Book as is" and "Customize with the planner" on a package.
void main() {
  final today = DateTime(2026, 10, 2, 9);
  late FakeTripTemplatesRepository templates;
  late GoRouter router;

  /// The package screen inside a router, so booking can move on to the new trip.
  Future<void> pumpPackage(WidgetTester tester) async {
    usePhoneSize(tester, const Size(412, 915));
    templates = FakeTripTemplatesRepository();
    final api = MockApiClient();
    when(() => api.get('/api/attractions/cities'))
        .thenAnswer((_) async => demoCities);
    router = GoRouter(
      initialLocation: '/packages/tpl-1',
      routes: [
        GoRoute(
          path: '/packages/:id',
          builder: (_, s) =>
              PackageDetailScreen(templateId: s.pathParameters['id']!),
        ),
        GoRoute(
          path: '/trips/new',
          builder: (_, s) =>
              NewTripScreen(prefill: s.extra as TripPrefill?, today: today),
        ),
        GoRoute(
          path: '/trips/:id',
          builder: (_, s) =>
              Scaffold(body: Text('trip ${s.pathParameters['id']}')),
        ),
      ],
    );
    await tester.pumpWidget(
      ProviderScope(
        retry: (_, _) => null,
        overrides: [
          apiClientProvider.overrideWithValue(api),
          sessionStorageProvider.overrideWithValue(InMemorySessionStorage()),
          tripTemplatesRepositoryProvider.overrideWithValue(templates),
          fixedClock(today),
        ],
        child: MaterialApp.router(theme: buildAppTheme(), routerConfig: router),
      ),
    );
    await tester.pumpAndSettle();
  }

  Future<void> tapButton(WidgetTester tester, String label) async {
    final button = find.text(label);
    await tester.scrollUntilVisible(
      button,
      200,
      scrollable: verticalScrollable(),
    );
    await tester.tap(button);
    await tester.pumpAndSettle();
  }

  /// Picks 10 October in the open date picker.
  Future<void> pickOctoberTenth(WidgetTester tester) async {
    await tester.tap(find.text('10'));
    await tester.tap(find.text('OK'));
    await tester.pumpAndSettle();
  }

  testWidgets('the package shows its summary and day-by-day plan', (
    tester,
  ) async {
    await pumpPackage(tester);

    expect(find.text('Hill-country escape'), findsWidgets);
    expect(find.text('Cool & green'), findsOneWidget);
    expect(find.text('4 days · Kandy → Nuwara Eliya → Ella'), findsOneWidget);
    expect(find.text('From USD 640 for 2 travellers'), findsOneWidget);
    await tester.scrollUntilVisible(
      find.text('Day 2 — Ella'),
      200,
      scrollable: verticalScrollable(),
    );
    expect(find.text('• Nine Arches Bridge'), findsOneWidget);
  });

  testWidgets('Book as is validates the sheet, then posts the right body', (
    tester,
  ) async {
    await pumpPackage(tester);
    await tapButton(tester, 'Book as is');
    expect(find.byType(BottomSheet), findsOneWidget);

    // The budget starts at the from-price for two.
    expect(find.widgetWithText(TextFormField, '640'), findsOneWidget);

    // Nothing chosen yet: the errors show and nothing is sent.
    await tester.tap(find.text('Book this trip'));
    await tester.pumpAndSettle();
    expect(find.text('Choose a start date.'), findsOneWidget);
    expect(find.text('Nationality is required.'), findsOneWidget);
    expect(find.text('Passport number is required.'), findsOneWidget);
    expect(templates.booked, isNull);

    await tester.tap(find.text('Choose a start date'));
    await tester.pumpAndSettle();
    await pickOctoberTenth(tester);
    // The package is 4 days long, so it ends on 13 October.
    expect(find.text('10 Oct 2026 – 13 Oct 2026'), findsOneWidget);

    // One more traveller: the budget follows (USD 320 a person).
    await tester.tap(find.byTooltip('One traveller more'));
    await tester.pump();
    expect(find.widgetWithText(TextFormField, '960'), findsOneWidget);

    await tester.enterText(
      find.widgetWithText(TextFormField, 'Nationality'),
      'Australian',
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Passport number'),
      'N1234567',
    );
    await tester.tap(find.text('Book this trip'));
    await tester.pumpAndSettle();

    expect(
      templates.booked,
      const BookTemplateRequest(
        startDate: '2026-10-10',
        pax: 3,
        budgetUsd: 960,
        nationality: 'Australian',
        passportNumber: 'N1234567',
      ),
    );
    expect(templates.booked!.toJson(), {
      'startDate': '2026-10-10',
      'pax': 3,
      'budgetUsd': 960.0,
      'nationality': 'Australian',
      'passportNumber': 'N1234567',
    });
    expect(router.state.matchedLocation, '/trips/trip-9');
    expect(find.text('Planning started'), findsOneWidget);
  });

  testWidgets('Customize opens the new-trip form filled in from the package', (
    tester,
  ) async {
    await pumpPackage(tester);
    await tapButton(tester, 'Customize with the planner');

    expect(router.state.matchedLocation, '/trips/new');
    expect(find.text('Customize your trip'), findsOneWidget);
    expect(
      find.text(
        '4 relaxed days through the hill country: Kandy, Nuwara Eliya and Ella.',
      ),
      findsOneWidget,
    );
    // The cities in the package's order.
    expect(find.text('1. Kandy'), findsOneWidget);
    expect(find.text('2. Nuwara Eliya'), findsOneWidget);
    expect(find.text('3. Ella'), findsOneWidget);
    expect(find.text('Route: Kandy → Nuwara Eliya → Ella'), findsOneWidget);

    bool chipSelected(String label) => tester
        .widget<FilterChip>(find.widgetWithText(FilterChip, label))
        .selected;
    // language "en" and pace "relaxed" map to chips; transport "any" does not.
    expect(chipSelected('English-speaking guide'), isTrue);
    expect(chipSelected('Relaxed pace'), isTrue);
    expect(chipSelected('Hill-country train'), isFalse);
    expect(find.widgetWithText(TextFormField, '640'), findsOneWidget);

    // Choosing a start date sets the end date from the package length.
    await tester.tap(find.text('Choose a start date'));
    await tester.pumpAndSettle();
    await pickOctoberTenth(tester);
    expect(find.text('10 Oct 2026 – 13 Oct 2026'), findsOneWidget);
  });

  test('the suggested budget scales the from-price by travellers', () {
    final template = TripTemplate.fromJson(templateJson());
    expect(suggestedBudgetUsd(template, 2), 640);
    expect(suggestedBudgetUsd(template, 1), 320);
    expect(suggestedBudgetUsd(template, 5), 1600);
  });
}
