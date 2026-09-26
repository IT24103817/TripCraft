import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/resources/data/resources_repository.dart';
import 'package:tripcraft_mobile/features/resources/presentation/voucher_lookup.dart';

import '../helpers.dart';

const kandyHills = '00000000-0000-0000-0000-00000000c001';

void main() {
  test(
    'a voucher is the hotel id, optionally with the TRIPCRAFT-HOTEL prefix',
    () {
      expect(hotelIdFromVoucher(kandyHills), kandyHills);
      expect(hotelIdFromVoucher('TRIPCRAFT-HOTEL:$kandyHills'), kandyHills);
      expect(hotelIdFromVoucher('https://example.com'), isNull);
    },
  );

  testWidgets('looks the scanned hotel up through the API', (tester) async {
    final api = MockApiClient();
    when(() => api.get('/api/hotels/$kandyHills')).thenAnswer(
      (_) async => {
        'id': kandyHills,
        'name': 'Kandy Hills',
        'city': 'Kandy',
        'starRating': 4,
        'roomTypes': [
          {'name': 'Standard Double', 'capacity': 2},
        ],
      },
    );

    await pumpScreen(
      tester,
      const Scaffold(body: VoucherLookup(code: 'TRIPCRAFT-HOTEL:$kandyHills')),
      api: api,
    );
    await tester.tap(find.text('Look up hotel'));
    await tester.pumpAndSettle();

    expect(find.text('Kandy Hills · ★★★★'), findsOneWidget);
    expect(find.textContaining('Standard Double (sleeps 2)'), findsOneWidget);
  });

  testWidgets('a QR code that is not a voucher cannot be looked up', (
    tester,
  ) async {
    await pumpScreen(
      tester,
      const Scaffold(body: VoucherLookup(code: 'hello')),
      api: MockApiClient(),
    );

    expect(
      find.text('This QR code is not a TripCraft hotel voucher.'),
      findsOneWidget,
    );
    final button = tester.widget<FilledButton>(find.byType(FilledButton));
    expect(button.onPressed, isNull);
  });
}
