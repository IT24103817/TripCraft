import 'package:flutter/widgets.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_detail_screen.dart';

import '../helpers.dart';

void main() {
  late MockApiClient api;

  void givenTrip(String status, {Map<String, dynamic>? workflow}) {
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
  }

  setUp(() => api = MockApiClient());

  testWidgets(
    'PendingApproval highlights that step and says it awaits the operator',
    (tester) async {
      usePhoneSize(tester, phoneSizes.currentValue!);
      givenTrip(
        'PendingApproval',
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

      expect(find.text('Awaiting operator approval'), findsOneWidget);
      expect(find.byKey(const ValueKey('step-Submitted-done')), findsOneWidget);
      expect(find.byKey(const ValueKey('step-Planning-done')), findsOneWidget);
      expect(
        find.byKey(const ValueKey('step-PendingApproval-current')),
        findsOneWidget,
      );
      expect(find.byKey(const ValueKey('step-Confirmed-todo')), findsOneWidget);
      // The planning card and the itinerary are below the fold on a small phone.
      await tester.scrollUntilVisible(
        find.text('View quotation'),
        200,
        scrollable: find.byType(Scrollable).first,
      );
      expect(find.text('View quotation'), findsOneWidget);
      await tester.scrollUntilVisible(
        find.text('Day 1 — Kandy'),
        200,
        scrollable: find.byType(Scrollable).first,
      );
      expect(find.text('Day 1 — Kandy'), findsOneWidget);
    },
    variant: phoneSizes,
  );

  testWidgets('Confirmed is the last step and there is no approval banner', (
    tester,
  ) async {
    givenTrip('Confirmed', workflow: {'id': 'wf-1', 'status': 'Completed'});

    await pumpScreen(
      tester,
      const TripDetailScreen(tripId: 'trip-1'),
      api: api,
    );
    await tester.pumpAndSettle();

    expect(
      find.byKey(const ValueKey('step-Confirmed-current')),
      findsOneWidget,
    );
    expect(find.text('Awaiting operator approval'), findsNothing);
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
    expect(find.text('Start planning'), findsOneWidget);
  });

  test('Approved is shown as Confirmed on the tourist timeline', () {
    expect(timelineStatus('Approved'), 'Confirmed');
    expect(timelineStatus('Planning'), 'Planning');
  });
}
