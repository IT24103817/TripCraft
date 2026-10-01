import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/trips/presentation/cancel_trip_section.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_progress_card.dart';

import '../helpers.dart';
import 'trip_fakes.dart';

/// The cancellation rule on the Overview tab: open until N days before the start, then closed with a contact.
void main() {
  late MockApiClient api;
  setUp(() => api = MockApiClient());

  testWidgets('open: says until when, and cancelling needs a reason', (
    tester,
  ) async {
    stubTrip(api, 'Confirmed', workflow: {'id': 'wf-1', 'status': 'Completed'});
    when(
      () => api.post(
        '/api/trip-requests/trip-1/cancel',
        body: any(named: 'body'),
      ),
    ).thenAnswer((_) async => tripJson(status: 'Cancelled'));
    await pumpTripDetail(tester, api);
    await scrollTo(tester, find.text('Cancel trip'));

    expect(
      find.text('You can cancel until 7 Oct 2026 (3 days before the start).'),
      findsOneWidget,
    );

    await tester.tap(find.text('Cancel trip'));
    await tester.pumpAndSettle();
    expect(find.text('Cancel this trip?'), findsOneWidget);

    // The reason is required.
    await tester.tap(find.widgetWithText(FilledButton, 'Cancel trip'));
    await tester.pumpAndSettle();
    expect(find.text('Please give a reason.'), findsOneWidget);
    verifyNever(() => api.post(any(), body: any(named: 'body')));

    await tester.enterText(
      find.widgetWithText(TextFormField, 'Reason'),
      'Family emergency',
    );
    await tester.tap(find.widgetWithText(FilledButton, 'Cancel trip'));
    await tester.pumpAndSettle();

    final body =
        verify(
              () => api.post(
                '/api/trip-requests/trip-1/cancel',
                body: captureAny(named: 'body'),
              ),
            ).captured.single
            as Map<String, dynamic>;
    expect(body, {'reason': 'Family emergency'});
    expect(find.text('Trip cancelled.'), findsOneWidget);
  });

  testWidgets('closed: shows the reason and the operator contact', (
    tester,
  ) async {
    stubTrip(
      api,
      'Confirmed',
      workflow: {'id': 'wf-1', 'status': 'Completed'},
      cancellation: cancellationJson(
        canCancel: false,
        closedReason: 'Cancellation closed on 07 Oct 2026, 3 days before the start. Please contact the operator: operations@tripcraft.test.',
      ),
    );
    await pumpTripDetail(tester, api);
    await scrollTo(tester, find.text('Contact operator'));

    expect(
      find.textContaining('Cancellation closed on 07 Oct 2026'),
      findsOneWidget,
    );
    expect(find.text('operations@tripcraft.test'), findsOneWidget);
    expect(find.text('Contact operator'), findsOneWidget);
    expect(find.text('Cancel trip'), findsNothing);
  });

  testWidgets('a cancelled trip has no Cancellation section', (tester) async {
    stubTrip(api, 'Cancelled');
    await pumpTripDetail(tester, api);
    expect(find.text(whatHappensNext('Cancelled')), findsOneWidget);
    await scrollTo(tester, find.text('History'));

    expect(find.text('Cancellation'), findsNothing);
    verifyNever(() => api.get('/api/trip-requests/trip-1/cancellation'));
  });

  test('the operator contact becomes a mailto or tel link', () {
    expect(
      operatorContactUri('operations@tripcraft.test').toString(),
      'mailto:operations@tripcraft.test',
    );
    expect(
      operatorContactUri('+94 11 234 5678').toString(),
      'tel:+94112345678',
    );
    expect(operatorContactUri('the front desk'), isNull);
  });
}
