import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/features/trips/presentation/cancel_trip_section.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_detail_screen.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_progress_card.dart';

import '../helpers.dart';

void main() {
  late MockApiClient api;

  void givenTrip(
    String status, {
    Map<String, dynamic>? workflow,
    Map<String, dynamic>? cancellation,
  }) {
    when(() => api.get('/api/trip-requests/trip-1'))
        .thenAnswer((_) async => tripJson(status: status));
    when(() => api.get('/api/trip-requests/trip-1/workflow'))
        .thenAnswer((_) async {
          if (workflow == null) {
            throw const UserFacingException(
              'We could not find that.',
              statusCode: 404,
            );
          }
          return workflow;
        });
    when(() => api.get('/api/trip-requests/trip-1/itinerary')).thenThrow(
      const UserFacingException('We could not find that.', statusCode: 404),
    );
    when(() => api.get('/api/trip-requests/trip-1/cancellation'))
        .thenAnswer((_) async => cancellation ?? cancellationJson());
    when(() => api.get('/api/trips/trip-1/vouchers'))
        .thenAnswer((_) async => <Object>[]);
    when(() => api.get('/api/trip-requests/trip-1/history')).thenAnswer(
      (_) async => [
        {
          'at': '2026-09-26T04:12:54Z',
          'action': 'TripRequestCreated',
          'actor': 'Tourist',
          'toStatus': 'Submitted',
        },
        {
          'at': '2026-09-26T04:13:43Z',
          'action': 'TripRequestStatusChanged',
          'actor': 'System',
          'fromStatus': 'Planning',
          'toStatus': 'Submitted',
        },
      ],
    );
  }

  setUp(() => api = MockApiClient());

  testWidgets(
    'PendingReview highlights that step and says the operator is checking',
    (tester) async {
      usePhoneSize(tester, phoneSizes.currentValue!);
      givenTrip(
        'PendingReview',
        workflow: {
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
        },
      );

      await pumpScreen(
        tester,
        const TripDetailScreen(tripId: 'trip-1'),
        api: api,
      );
      await tester.pumpAndSettle();

      expect(find.text(whatHappensNext('PendingReview')), findsOneWidget);
      expect(find.byKey(const ValueKey('step-Submitted-done')), findsOneWidget);
      expect(find.byKey(const ValueKey('step-Planning-done')), findsOneWidget);
      expect(
        find.byKey(const ValueKey('step-PendingReview-current')),
        findsOneWidget,
      );
      // The rest of the timeline is below the fold on a small phone.
      await tester.scrollUntilVisible(
        find.byKey(const ValueKey('step-Completed-todo')),
        200,
        scrollable: find.byType(Scrollable).first,
      );
      expect(
        find.byKey(const ValueKey('step-QuotationSent-todo')),
        findsOneWidget,
      );
      // The tourist cannot see the price until the operator sends it.
      expect(find.text('View quotation'), findsNothing);
      await tester.scrollUntilVisible(
        find.text('Day 1 — Kandy'),
        200,
        scrollable: find.byType(Scrollable).first,
      );
      expect(find.text('Day 1 — Kandy'), findsOneWidget);
    },
    variant: phoneSizes,
  );

  testWidgets('Confirmed: earlier steps done, later steps still to come', (
    tester,
  ) async {
    givenTrip('Confirmed', workflow: {'id': 'wf-1', 'status': 'Completed'});

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();

    expect(find.text(whatHappensNext('Confirmed')), findsOneWidget);
    await tester.scrollUntilVisible(
      find.byKey(const ValueKey('step-Completed-todo')),
      200,
      scrollable: find.byType(Scrollable).first,
    );
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
    givenTrip('Submitted');

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();

    expect(
      find.byKey(const ValueKey('step-Submitted-current')),
      findsOneWidget,
    );
    await tester.scrollUntilVisible(
      find.text('Start planning'),
      200,
      scrollable: find.byType(Scrollable).first,
    );
    expect(find.text('Start planning'), findsOneWidget);
  });

  testWidgets(
    'a FailedSafely trip shows the reason, hides its days and offers Try again',
    (tester) async {
      givenTrip(
        'FailedSafely',
        workflow: {
          'id': 'wf-1',
          'status': 'FailedSafely',
          'errorSummary': 'Agents failed safely: resources: tool returned 503',
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
        },
      );

      await pumpScreen(
        tester,
        const TripDetailScreen(tripId: 'trip-1'),
        api: api,
      );
      await tester.pumpAndSettle();

      expect(find.text(whatHappensNext('FailedSafely')), findsOneWidget);
      // FailedSafely is still on the Planning step of the timeline.
      expect(
        find.byKey(const ValueKey('step-Planning-current')),
        findsOneWidget,
      );
      await tester.scrollUntilVisible(
        find.text('Try again'),
        200,
        scrollable: find.byType(Scrollable).first,
      );
      expect(find.textContaining('Planning could not finish:'), findsOneWidget);
      expect(find.text('Try again'), findsOneWidget);
      final empty = find.text(
        'Your day-by-day plan appears here once the agents have drafted it.',
      );
      await tester.scrollUntilVisible(
        empty,
        200,
        scrollable: find.byType(Scrollable).first,
      );
      expect(find.text('Day 1 — Kandy'), findsNothing);
      expect(empty, findsOneWidget);

      // Only the tourist may start planning, so Try again calls start-planning again.
      when(() => api.post('/api/trip-requests/trip-1/start-planning'))
          .thenAnswer(
            (_) async => {
              'workflowId': 'wf-2',
              'workflowStatus': 'Planning',
              'tripStatus': 'Planning',
            },
          );
      await tester.scrollUntilVisible(
        find.text('Try again'),
        -200,
        scrollable: find.byType(Scrollable).first,
      );
      await tester.tap(find.text('Try again'));
      await tester.pumpAndSettle();
      verify(() => api.post('/api/trip-requests/trip-1/start-planning'))
          .called(1);
    },
  );

  testWidgets('shows the trip history oldest first with who made each change', (
    tester,
  ) async {
    givenTrip('Submitted');

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();
    await tester.scrollUntilVisible(
      find.text('History'),
      200,
      scrollable: find.byType(Scrollable).first,
    );
    await tester.scrollUntilVisible(
      find.textContaining('Status changed'),
      200,
      scrollable: find.byType(Scrollable).first,
    );

    expect(find.textContaining('Request submitted'), findsOneWidget);
    expect(find.textContaining('by you'), findsOneWidget);
    expect(find.textContaining('Status changed'), findsOneWidget);
    expect(find.textContaining('by the system'), findsOneWidget);
  });

  Future<void> scrollToCancellation(WidgetTester tester) =>
      tester.scrollUntilVisible(
        find.text('Cancellation'),
        200,
        scrollable: find.byType(Scrollable).first,
      );

  testWidgets('before the cut-off a trip is cancelled with a required reason', (
    tester,
  ) async {
    givenTrip('Confirmed', workflow: {'id': 'wf-1', 'status': 'Completed'});
    when(
      () => api.post(
        '/api/trip-requests/trip-1/cancel',
        body: any(named: 'body'),
      ),
    ).thenAnswer((_) async => tripJson(status: 'Cancelled'));

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();
    await scrollToCancellation(tester);
    await tester.scrollUntilVisible(
      find.text('Cancel trip'),
      200,
      scrollable: find.byType(Scrollable).first,
    );
    expect(
      find.text(
        'You can cancel this trip until 7 Oct 2026 (3 days before it starts).',
      ),
      findsOneWidget,
    );

    // Bring the whole button on screen (the last card is at the bottom edge).
    await tester.drag(find.byType(Scrollable).first, const Offset(0, -300));
    await tester.pumpAndSettle();
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

  testWidgets(
    'after the cut-off the closed reason and the operator contact are shown',
    (tester) async {
      givenTrip(
        'Confirmed',
        workflow: {'id': 'wf-1', 'status': 'Completed'},
        cancellation: cancellationJson(
          canCancel: false,
          closedReason: 'Cancellation closed on 07 Oct 2026, 3 days before the start. Please contact the operator: operations@tripcraft.test.',
        ),
      );

      await pumpScreen(
        tester,
        const TripDetailScreen(tripId: 'trip-1'),
        api: api,
      );
      await tester.pumpAndSettle();
      await tester.scrollUntilVisible(
        find.text('Contact operator'),
        200,
        scrollable: find.byType(Scrollable).first,
      );

      expect(
        find.textContaining('Cancellation closed on 07 Oct 2026'),
        findsOneWidget,
      );
      expect(find.text('operations@tripcraft.test'), findsOneWidget);
      expect(find.text('Contact operator'), findsOneWidget);
      expect(find.text('Cancel trip'), findsNothing);
    },
  );

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

  testWidgets('a cancelled or completed trip has no Cancellation section', (
    tester,
  ) async {
    givenTrip('Cancelled');

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();
    expect(find.text(whatHappensNext('Cancelled')), findsOneWidget);
    await tester.scrollUntilVisible(
      find.text('History'),
      200,
      scrollable: find.byType(Scrollable).first,
    );

    expect(find.text('Cancellation'), findsNothing);
    verifyNever(() => api.get('/api/trip-requests/trip-1/cancellation'));
  });

  testWidgets('RevisionRequested says a new version is coming', (tester) async {
    givenTrip(
      'RevisionRequested',
      workflow: {'id': 'wf-1', 'status': 'PendingApproval'},
    );

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();

    expect(
      find.text('The operator asked for changes; a new version is coming.'),
      findsOneWidget,
    );
    expect(
      find.byKey(const ValueKey('step-PendingReview-current')),
      findsOneWidget,
    );
  });

  testWidgets('QuotationSent offers Review quotation', (tester) async {
    givenTrip('QuotationSent', workflow: {'id': 'wf-1', 'status': 'Approved'});

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();
    await tester.scrollUntilVisible(
      find.text('Review quotation'),
      200,
      scrollable: find.byType(Scrollable).first,
    );

    expect(find.text(whatHappensNext('QuotationSent')), findsOneWidget);
    expect(
      find.widgetWithText(FilledButton, 'Review quotation'),
      findsOneWidget,
    );
  });

  testWidgets(
    'a workflow that fails to load shows the error with Retry, not "not started"',
    (tester) async {
      givenTrip('Planning');
      when(() => api.get('/api/trip-requests/trip-1/workflow')).thenThrow(
        const UserFacingException(
          'The TripCraft server had a problem. Please try again in a moment.',
          statusCode: 500,
        ),
      );

      await pumpScreen(
        tester,
        const TripDetailScreen(tripId: 'trip-1'),
        api: api,
      );
      await tester.pumpAndSettle();
      await tester.scrollUntilVisible(
        find.text('Retry'),
        200,
        scrollable: find.byType(Scrollable).first,
      );
      await tester.pumpAndSettle();

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
      expect(
        find.textContaining('The operator is reviewing your plan'),
        findsOneWidget,
      );
    },
  );

  testWidgets('the itinerary map shows one marker per stop with coordinates', (
    tester,
  ) async {
    givenTrip(
      'PendingReview',
      workflow: {
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
                'stops': [
                  {'attraction_id': 'a1', 'name': 'Temple of the Tooth'},
                  {'attraction_id': 'a2', 'name': 'Peradeniya Gardens'},
                ],
              },
            ],
          },
        },
      },
    );
    when(() => api.get('/api/attractions/a1')).thenAnswer(
      (_) async => {
        'id': 'a1',
        'name': 'Temple of the Tooth',
        'city': 'Kandy',
        'latitude': 7.2936,
        'longitude': 80.6413,
      },
    );
    when(() => api.get('/api/attractions/a2')).thenAnswer(
      (_) async => {
        'id': 'a2',
        'name': 'Peradeniya Gardens',
        'city': 'Kandy',
        'latitude': 7.2687,
        'longitude': 80.5966,
      },
    );

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();
    await tester.scrollUntilVisible(
      find.byType(MarkerLayer),
      200,
      scrollable: find.byType(Scrollable).first,
    );

    final layer = tester.widget<MarkerLayer>(find.byType(MarkerLayer));
    expect(layer.markers, hasLength(2));
    expect(
      layer.markers.map((m) => (m.point.latitude, m.point.longitude)),
      containsAll([(7.2936, 80.6413), (7.2687, 80.5966)]),
    );
  });

  test('side states sit on the step they came from', () {
    expect(timelineStatus('RevisionRequested'), 'PendingReview');
    expect(timelineStatus('FailedSafely'), 'Planning');
    expect(timelineStatus('QuotationSent'), 'QuotationSent');
  });
}
