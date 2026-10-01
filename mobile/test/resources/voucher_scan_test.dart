import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/features/resources/data/resources_repository.dart';
import 'package:tripcraft_mobile/features/resources/presentation/qr_scan_screen.dart';

import '../helpers.dart';

const signedCode =
    '1.0f8fad5bd9cb469fa16570867728950e.7c9e6679742540de944be07fc1f90ae7.T.-.abcdefghijklmnopqrstuv';
const tripVoucherQr = 'TRIPCRAFT-VOUCHER:$signedCode';
const kandyHills = '00000000-0000-0000-0000-00000000c001';

void main() {
  group('scanned code parsing', () {
    test('a TRIPCRAFT-VOUCHER code is a trip voucher; its code is sent without the prefix', () {
      expect(kindOfScan(tripVoucherQr), ScannedKind.tripVoucher);
      expect(kindOfScan('  $tripVoucherQr\n'), ScannedKind.tripVoucher);
      expect(voucherCodeFromScan(tripVoucherQr), signedCode);
    });

    test(
      'the legacy TRIPCRAFT-HOTEL code and a bare hotel id are hotel lookups',
      () {
        expect(kindOfScan('TRIPCRAFT-HOTEL:$kandyHills'), ScannedKind.hotel);
        expect(kindOfScan(kandyHills), ScannedKind.hotel);
      },
    );

    test('anything else is not ours', () {
      expect(kindOfScan('https://example.com'), ScannedKind.unknown);
      expect(kindOfScan('TRIPCRAFT-VOUCHER:'), ScannedKind.unknown);
      expect(kindOfScan(''), ScannedKind.unknown);
    });
  });

  Future<void> pumpPanel(
    WidgetTester tester,
    MockApiClient api,
    String? code, {
    String? tripId,
  }) => pumpScreen(
    tester,
    Scaffold(
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: ScanResultPanel(code: code, tripId: tripId),
      ),
    ),
    api: api,
  );

  testWidgets(
    'a scanned trip voucher posts {voucherCode, tripRequestId} and shows the stop and new status',
    (tester) async {
      final api = MockApiClient();
      when(() => api.post('/api/check-ins', body: any(named: 'body')))
          .thenAnswer(
            (_) async => {
              'stopId': 's1',
              'distanceMeters': null,
              'checkedInAt': '2026-10-10T05:00:00Z',
              'tripStatus': 'InProgress',
              'method': 'Voucher',
              'stopName': 'Temple of the Tooth',
            },
          );
      await pumpPanel(tester, api, tripVoucherQr, tripId: 'trip-1');
      expect(find.text('Trip voucher scanned.'), findsOneWidget);

      await tester.tap(find.text('Check in with this voucher'));
      await tester.pumpAndSettle();

      final body =
          verify(
                () =>
                    api.post('/api/check-ins', body: captureAny(named: 'body')),
              ).captured.single
              as Map<String, dynamic>;
      expect(body, {'voucherCode': signedCode, 'tripRequestId': 'trip-1'});
      expect(find.text('Checked in at Temple of the Tooth'), findsOneWidget);
      expect(find.text('Trip is now in progress.'), findsOneWidget);
    },
  );

  testWidgets('from the Scan tab no trip id is sent', (tester) async {
    final api = MockApiClient();
    when(() => api.post('/api/check-ins', body: any(named: 'body'))).thenAnswer(
      (_) async => {
        'stopId': 's2',
        'checkedInAt': '2026-10-10T07:00:00Z',
        'tripStatus': 'Completed',
        'method': 'Voucher',
        'stopName': 'Peradeniya Gardens',
      },
    );
    await pumpPanel(tester, api, tripVoucherQr);

    await tester.tap(find.text('Check in with this voucher'));
    await tester.pumpAndSettle();

    verify(() => api.post('/api/check-ins', body: {'voucherCode': signedCode}))
        .called(1);
    expect(find.text('Trip is now completed.'), findsOneWidget);
  });

  testWidgets('a rejected voucher shows the API message', (tester) async {
    final api = MockApiClient();
    when(() => api.post('/api/check-ins', body: any(named: 'body'))).thenThrow(
      const UserFacingException(
        'This is a hotel voucher. Scan the tourist\'s trip voucher.',
        statusCode: 400,
      ),
    );
    await pumpPanel(tester, api, tripVoucherQr);

    await tester.tap(find.text('Check in with this voucher'));
    await tester.pumpAndSettle();

    expect(
      find.text('This is a hotel voucher. Scan the tourist\'s trip voucher.'),
      findsOneWidget,
    );
  });

  testWidgets('the legacy hotel QR still offers the hotel lookup', (
    tester,
  ) async {
    await pumpPanel(tester, MockApiClient(), 'TRIPCRAFT-HOTEL:$kandyHills');

    expect(find.text('Look up hotel'), findsOneWidget);
    expect(find.text('Check in with this voucher'), findsNothing);
  });

  testWidgets('nothing scanned yet, or an unknown code', (tester) async {
    await pumpPanel(tester, MockApiClient(), null);
    expect(
      find.text('Point the camera at the tourist\'s trip voucher.'),
      findsOneWidget,
    );

    await pumpPanel(tester, MockApiClient(), 'hello');
    expect(
      find.text('This QR code is not a TripCraft voucher.'),
      findsOneWidget,
    );
  });
}
