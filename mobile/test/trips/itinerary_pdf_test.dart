import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/features/trips/data/pdf_sharer.dart';
import 'package:tripcraft_mobile/features/trips/presentation/itinerary_pdf_card.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_detail_screen.dart';

import '../helpers.dart';
import 'trip_fakes.dart';

/// Records what would be handed to the phone's share sheet.
class FakePdfSharer implements PdfSharer {
  List<int>? bytes;
  String? fileName;

  @override
  Future<void> share(List<int> bytes, String fileName) async {
    this.bytes = bytes;
    this.fileName = fileName;
  }
}

void main() {
  late MockApiClient api;
  late FakePdfSharer sharer;
  const pdfPath = '/api/trips/trip-1/itinerary.pdf';
  final download = find.text('Download itinerary PDF');

  setUp(() {
    api = MockApiClient();
    sharer = FakePdfSharer();
  });

  Future<void> pumpTrip(WidgetTester tester, String status) async {
    stubTrip(api, status, workflow: {'id': 'wf-1', 'status': 'Approved'});
    await pumpScreen(
      tester,
      TripDetailScreen(
        tripId: 'trip-1',
        decisionPanel: (tripId, _) => Text('decision-panel-$tripId'),
      ),
      api: api,
      overrides: [pdfSharerProvider.overrideWithValue(sharer)],
    );
    await tester.pumpAndSettle();
  }

  test('offered from QuotationSent on', () {
    expect(itineraryPdfStatuses, {
      'QuotationSent',
      'ClientAccepted',
      'Confirmed',
      'InProgress',
      'Completed',
    });
  });

  for (final status in itineraryPdfStatuses) {
    testWidgets('$status shows Download itinerary PDF', (tester) async {
      await pumpTrip(tester, status);
      await scrollTo(tester, download);

      expect(download, findsOneWidget);
    });
  }

  for (final status in [
    'Submitted',
    'Planning',
    'NeedsOperator',
    'Cancelled',
  ]) {
    testWidgets('$status has no PDF button', (tester) async {
      await pumpTrip(tester, status);
      // History comes after where the button would be.
      await scrollTo(tester, find.text('History'));

      expect(download, findsNothing);
    });
  }

  testWidgets('tapping downloads the PDF and hands it to the phone', (
    tester,
  ) async {
    when(() => api.getBytes(pdfPath)).thenAnswer((_) async => [0x25, 0x50]);
    await pumpTrip(tester, 'Confirmed');
    await scrollTo(tester, download);

    await tester.tap(download);
    await tester.pumpAndSettle();

    verify(() => api.getBytes(pdfPath)).called(1);
    expect(sharer.bytes, [0x25, 0x50]);
    expect(sharer.fileName, 'tripcraft-itinerary-trip-1.pdf');
  });

  testWidgets('an API error is shown in a snackbar', (tester) async {
    when(() => api.getBytes(pdfPath)).thenThrow(
      const UserFacingException(
        'The itinerary PDF is available once a quotation has been sent.',
        statusCode: 409,
      ),
    );
    await pumpTrip(tester, 'QuotationSent');
    await scrollTo(tester, download);

    await tester.tap(download);
    await tester.pumpAndSettle();

    expect(
      find.text(
        'The itinerary PDF is available once a quotation has been sent.',
      ),
      findsOneWidget,
    );
    expect(sharer.bytes, isNull);
  });
}
