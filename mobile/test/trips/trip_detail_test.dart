import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_progress_card.dart';

import '../helpers.dart';
import 'trip_fakes.dart';

/// The Overview tab of the trip screen: timeline, what happens next, planning, decisions and history.
void main() {
  late MockApiClient api;
  setUp(() => api = MockApiClient());

  final kandyProposal = {
    'id': 'wf-1',
    'status': 'PendingApproval',
    'finalOutcome': {
      'proposal': {
        'days': [
          {
            'day': 1,
            'date': '2026-10-10',
            'city': 'Kandy',
            'transport': 'road',
            'stops': <Object>[],
          },
        ],
      },
    },
  };

  testWidgets('the screen has Overview, Itinerary and Vouchers tabs', (
    tester,
  ) async {
    stubTrip(api, 'Submitted');
    await pumpTripDetail(tester, api);

    expect(find.widgetWithText(Tab, 'Overview'), findsOneWidget);
    expect(find.widgetWithText(Tab, 'Itinerary'), findsOneWidget);
    expect(find.widgetWithText(Tab, 'Vouchers'), findsOneWidget);
  });

  testWidgets('Planning highlights that step; the quote is not visible yet', (
    tester,
  ) async {
    usePhoneSize(tester, phoneSizes.currentValue!);
    stubTrip(api, 'Planning', workflow: kandyProposal);
    await pumpTripDetail(tester, api);

    expect(find.text(whatHappensNext('Planning')), findsOneWidget);
    expect(find.byKey(const ValueKey('step-Submitted-done')), findsOneWidget);
    expect(find.byKey(const ValueKey('step-Planning-current')), findsOneWidget);
    await scrollTo(tester, find.byKey(const ValueKey('step-Completed-todo')));
    expect(
      find.byKey(const ValueKey('step-QuotationSent-todo')),
      findsOneWidget,
    );
    // The tourist cannot see the price or decide until the quote is sent.
    expect(find.text('View quotation'), findsNothing);
    expect(find.text('decision-panel-trip-1'), findsNothing);
  }, variant: phoneSizes);

  testWidgets('Confirmed: earlier steps done, later steps still to come', (
    tester,
  ) async {
    stubTrip(api, 'Confirmed', workflow: {'id': 'wf-1', 'status': 'Completed'});
    await pumpTripDetail(tester, api);

    expect(find.text(whatHappensNext('Confirmed')), findsOneWidget);
    await scrollTo(tester, find.byKey(const ValueKey('step-Completed-todo')));
    expect(
      find.byKey(const ValueKey('step-ClientAccepted-done')),
      findsOneWidget,
    );
    expect(
      find.byKey(const ValueKey('step-Confirmed-current')),
      findsOneWidget,
    );
    expect(find.byKey(const ValueKey('step-InProgress-todo')), findsOneWidget);
  });

  testWidgets('a Submitted trip without a workflow offers Start planning', (
    tester,
  ) async {
    stubTrip(api, 'Submitted');
    await pumpTripDetail(tester, api);

    expect(
      find.byKey(const ValueKey('step-Submitted-current')),
      findsOneWidget,
    );
    await scrollTo(tester, find.text('Start planning'));
    expect(find.text('Start planning'), findsOneWidget);
  });

  testWidgets(
    'a NeedsOperator trip waits for the team: no Try again, no failed days',
    (tester) async {
      stubTrip(
        api,
        'NeedsOperator',
        workflow: {
          ...kandyProposal,
          'status': 'FailedSafely',
          'errorSummary': 'Agents failed safely: resources: tool returned 503',
        },
      );
      await pumpTripDetail(tester, api);

      expect(find.text(whatHappensNext('NeedsOperator')), findsOneWidget);
      // NeedsOperator stays on the Planning step of the timeline.
      expect(
        find.byKey(const ValueKey('step-Planning-current')),
        findsOneWidget,
      );
      await scrollTo(tester, find.text('History'));
      expect(
        find.textContaining('our team is preparing your quote by hand'),
        findsOneWidget,
      );
      // Retrying is the operator's action now; the tourist never sees the internal error.
      expect(find.text('Try again'), findsNothing);
      expect(find.text('Start planning'), findsNothing);
      expect(find.textContaining('tool returned 503'), findsNothing);
      expect(find.text('Planning failed'), findsNothing);
      verifyNever(() => api.post('/api/trip-requests/trip-1/start-planning'));

      // The failed plan's days were never checked, so the Itinerary tab does not show them.
      await openTab(tester, 'Itinerary');
      expect(find.text('Day 1 — Kandy'), findsNothing);
      expect(find.text('No itinerary yet'), findsOneWidget);
    },
  );

  testWidgets('shows the trip history oldest first with who made each change', (
    tester,
  ) async {
    stubTrip(api, 'Submitted');
    await pumpTripDetail(tester, api);
    await scrollTo(tester, find.textContaining('Status changed'));

    expect(find.textContaining('Request submitted'), findsOneWidget);
    expect(find.textContaining('by you'), findsOneWidget);
    expect(find.textContaining('Status changed'), findsOneWidget);
    expect(find.textContaining('by the system'), findsOneWidget);
  });

  testWidgets('ClientDeclined says the operator will replan or contact you', (
    tester,
  ) async {
    stubTrip(
      api,
      'ClientDeclined',
      workflow: {'id': 'wf-1', 'status': 'Approved'},
    );
    await pumpTripDetail(tester, api);

    expect(
      find.text(
        'You declined this quote — the operator will replan or contact you.',
      ),
      findsOneWidget,
    );
    expect(
      find.byKey(const ValueKey('step-QuotationSent-current')),
      findsOneWidget,
    );
    // The decision is made: no Accept / Decline panel.
    expect(find.text('decision-panel-trip-1'), findsNothing);
  });

  testWidgets(
    'QuotationSent shows the Accept / Decline panel and a link to the quotation',
    (tester) async {
      stubTrip(
        api,
        'QuotationSent',
        workflow: {'id': 'wf-1', 'status': 'Approved'},
      );
      await pumpTripDetail(tester, api);

      expect(find.text(whatHappensNext('QuotationSent')), findsOneWidget);
      await scrollTo(tester, find.text('decision-panel-trip-1'));
      expect(find.text('decision-panel-trip-1'), findsOneWidget);
      await scrollTo(tester, find.text('View quotation'));
      expect(
        find.widgetWithText(OutlinedButton, 'View quotation'),
        findsOneWidget,
      );
    },
  );

  testWidgets(
    'a workflow that fails to load shows the error with Retry, not "not started"',
    (tester) async {
      stubTrip(api, 'Planning');
      when(() => api.get('/api/trip-requests/trip-1/workflow')).thenThrow(
        const UserFacingException(
          'The TripCraft server had a problem. Please try again in a moment.',
          statusCode: 500,
        ),
      );
      await pumpTripDetail(tester, api);
      await scrollTo(tester, find.text('Retry'));

      expect(
        find.text(
          'The TripCraft server had a problem. Please try again in a moment.',
        ),
        findsOneWidget,
      );
      expect(find.text('Planning has not started yet.'), findsNothing);
      expect(find.text('Start planning'), findsNothing);

      // Retry loads the workflow again.
      when(() => api.get('/api/trip-requests/trip-1/workflow'))
          .thenAnswer((_) async => {'id': 'wf-1', 'status': 'PendingApproval'});
      await tester.tap(find.text('Retry'));
      await tester.pumpAndSettle();
      expect(find.text('Retry'), findsNothing);
      expect(find.textContaining('Your quote is on its way'), findsOneWidget);
    },
  );

  testWidgets('a trip that fails to load shows the error with Retry', (
    tester,
  ) async {
    stubTrip(api, 'Submitted');
    when(() => api.get('/api/trip-requests/trip-1')).thenThrow(Exception('x'));
    await pumpTripDetail(tester, api);

    expect(find.text('Something went wrong'), findsOneWidget);
    expect(find.text('Retry'), findsOneWidget);
  });
}
