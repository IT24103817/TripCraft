import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:qr_flutter/qr_flutter.dart';
import 'package:tripcraft_mobile/features/trips/application/trips_providers.dart';
import 'package:tripcraft_mobile/features/trips/data/trip_models.dart';
import 'package:tripcraft_mobile/features/trips/presentation/vouchers_section.dart';

import '../helpers.dart';

const tripPayload = 'TRIPCRAFT-VOUCHER:1.aaa.bbb.T.-.sig1';
const nightOnePayload = 'TRIPCRAFT-VOUCHER:1.ccc.bbb.H.20261010.sig2';
const nightTwoPayload = 'TRIPCRAFT-VOUCHER:1.ddd.bbb.H.20261011.sig3';

Map<String, dynamic> voucherJson({
  required String type,
  required String payload,
  String? hotelName,
  String? night,
  int rooms = 0,
}) => {
  'id': 'v-$payload',
  'type': type,
  'hotelId': hotelName == null ? null : 'h1',
  'hotelName': hotelName,
  'night': night,
  'rooms': rooms,
  'code': payload.substring('TRIPCRAFT-VOUCHER:'.length),
  'qrPayload': payload,
};

void main() {
  test('the trip voucher comes first, then hotel nights by date', () {
    final sorted = sortVouchers([
      TripVoucher.fromJson(
        voucherJson(
          type: 'HotelNight',
          payload: nightTwoPayload,
          hotelName: 'Ella Rock',
          night: '2026-10-11',
          rooms: 2,
        ),
      ),
      TripVoucher.fromJson(
        voucherJson(
          type: 'HotelNight',
          payload: nightOnePayload,
          hotelName: 'Kandy Hills',
          night: '2026-10-10',
          rooms: 2,
        ),
      ),
      TripVoucher.fromJson(voucherJson(type: 'Trip', payload: tripPayload)),
    ]);

    expect(sorted.map((v) => v.qrPayload), [
      tripPayload,
      nightOnePayload,
      nightTwoPayload,
    ]);
  });

  testWidgets(
    'each voucher is a QR code of its payload, hotel nights with hotel, night and rooms',
    (tester) async {
      final api = MockApiClient();
      when(() => api.get('/api/trips/trip-1/vouchers')).thenAnswer(
        (_) async => [
          voucherJson(
            type: 'HotelNight',
            payload: nightOnePayload,
            hotelName: 'Kandy Hills',
            night: '2026-10-10',
            rooms: 2,
          ),
          voucherJson(type: 'Trip', payload: tripPayload),
        ],
      );

      await pumpScreen(
        tester,
        const Scaffold(
          body: SingleChildScrollView(child: VouchersSection(tripId: 'trip-1')),
        ),
        api: api,
      );
      await tester.pumpAndSettle();

      expect(find.byType(QrImageView), findsNWidgets(2));
      expect(find.byKey(const ValueKey('qr-$tripPayload')), findsOneWidget);
      expect(find.byKey(const ValueKey('qr-$nightOnePayload')), findsOneWidget);
      expect(find.text('Trip voucher'), findsOneWidget);
      expect(find.text('Kandy Hills'), findsOneWidget);
      expect(find.text('Night of 10 Oct 2026 · 2 rooms'), findsOneWidget);
      // The trip voucher is shown above the hotel night.
      expect(
        tester.getTopLeft(find.text('Trip voucher')).dy,
        lessThan(tester.getTopLeft(find.text('Kandy Hills')).dy),
      );
    },
  );

  testWidgets('no vouchers yet shows a hint', (tester) async {
    final api = MockApiClient();
    when(() => api.get('/api/trips/trip-1/vouchers'))
        .thenAnswer((_) async => <Object>[]);

    await pumpScreen(
      tester,
      const Scaffold(body: VouchersSection(tripId: 'trip-1')),
      api: api,
    );
    await tester.pumpAndSettle();

    expect(
      find.text('Your vouchers appear here once the trip is confirmed.'),
      findsOneWidget,
    );
  });
}
