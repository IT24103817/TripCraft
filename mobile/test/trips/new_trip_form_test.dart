import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/trips/presentation/new_trip_screen.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_form_rules.dart';

import '../helpers.dart';

void main() {
  testWidgets('blocks submit without dates and with 0 travellers', (
    tester,
  ) async {
    usePhoneSize(tester, phoneSizes.currentValue!);
    final api = MockApiClient();
    when(() => api.get('/api/attractions/cities'))
        .thenAnswer((_) async => demoCities);
    await pumpScreen(
      tester,
      NewTripScreen(today: DateTime(2026, 9, 26)),
      api: api,
    );

    await tester.enterText(
      find.widgetWithText(TextFormField, 'Travellers'),
      '0',
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'What would you like to do?'),
      '5 days in Kandy and Ella',
    );
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Budget (USD)'),
      '1500',
    );
    final form = find
        .byType(Scrollable)
        .first; // the form's ListView (text fields have their own)
    await tester.scrollUntilVisible(
      find.text('Submit trip request'),
      200,
      scrollable: form,
    );
    await tester.tap(find.text('Submit trip request'));
    await tester.pump();
    await tester.fling(
      form,
      const Offset(0, 3000),
      3000,
    ); // back to the top where the errors are
    await tester.pumpAndSettle();

    expect(find.text('Choose your travel dates.'), findsOneWidget);
    expect(find.text('Choose at least one city.'), findsOneWidget);
    expect(find.text('At least 1 traveller.'), findsOneWidget);
    verifyNever(() => api.post(any(), body: any(named: 'body')));
  }, variant: phoneSizes);

  test('date rules match the API (past start, over 30 days)', () {
    final today = DateTime(2026, 9, 26);
    expect(TripFormRules.dates(null, today), 'Choose your travel dates.');
    expect(
      TripFormRules.dates(
        DateTimeRange(start: DateTime(2026, 9, 20), end: DateTime(2026, 9, 28)),
        today,
      ),
      'Start date cannot be in the past.',
    );
    expect(
      TripFormRules.dates(
        DateTimeRange(start: DateTime(2026, 10, 1), end: DateTime(2026, 11, 5)),
        today,
      ),
      'Trips can be at most 30 days.',
    );
    expect(
      TripFormRules.dates(
        DateTimeRange(
          start: DateTime(2026, 10, 10),
          end: DateTime(2026, 10, 14),
        ),
        today,
      ),
      isNull,
    );
  });

  test('city rules: at least one, at most 10, at most one per trip day', () {
    final fiveDays = DateTimeRange(
      start: DateTime(2026, 10, 10),
      end: DateTime(2026, 10, 14),
    );
    expect(TripFormRules.cities([], fiveDays), 'Choose at least one city.');
    expect(TripFormRules.cities(['Kandy', 'Ella'], fiveDays), isNull);
    expect(TripFormRules.cities(['Kandy', 'Ella'], null), isNull);
    expect(
      TripFormRules.cities(
        ['Kandy', 'Ella', 'Galle'],
        DateTimeRange(
          start: DateTime(2026, 10, 10),
          end: DateTime(2026, 10, 11),
        ),
      ),
      'At most one city per day: your trip has 2 days.',
    );
    expect(
      TripFormRules.cities(List.generate(11, (i) => 'City $i'), null),
      'Choose at most 10 cities.',
    );
  });

  test('pax, budget and passport rules', () {
    expect(TripFormRules.pax('0'), 'At least 1 traveller.');
    expect(TripFormRules.pax('51'), 'At most 50 travellers.');
    expect(TripFormRules.pax('4'), isNull);
    expect(TripFormRules.budget('0'), 'Budget must be more than 0.');
    expect(
      TripFormRules.passportNumber('N12'),
      'Passport number must be 6–20 letters or digits.',
    );
    expect(TripFormRules.passportNumber('N1234567'), isNull);
  });
}
