import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/features/resources/presentation/guide_home_screen.dart';

import '../helpers.dart';

Map<String, dynamic> scheduleWith(String status) => {
  'guideId': 'g1',
  'guideName': 'Nimal Perera',
  'trips': [
    {
      'tripRequestId': 'trip-1',
      'objective': 'Kandy and Ella',
      'startDate': '2026-10-10',
      'endDate': '2026-10-11',
      'pax': 4,
      'status': status,
      'days': <Object>[],
    },
  ],
};

void main() {
  late MockApiClient api;

  Future<void> pumpSchedule(WidgetTester tester, String status) async {
    api = MockApiClient();
    when(() => api.get('/api/guides/me/schedule'))
        .thenAnswer((_) async => scheduleWith(status));
    // Before the trip starts: a Confirmed trip is the guide's "Next trip".
    await pumpScreen(
      tester,
      const GuideHomeScreen(),
      api: api,
      overrides: [fixedClock(DateTime(2026, 10, 2, 9))],
    );
    await tester.pumpAndSettle();
  }

  Future<void> requestWithReason(WidgetTester tester, String reason) async {
    await tester.tap(find.text('Request a change'));
    await tester.pumpAndSettle();
    await tester.enterText(
      find.widgetWithText(TextFormField, 'Reason'),
      reason,
    );
    await tester.tap(find.text('Send request'));
    await tester.pumpAndSettle();
  }

  testWidgets('a guide asks for a replacement with a required reason', (
    tester,
  ) async {
    await pumpSchedule(tester, 'Confirmed');
    when(
      () => api.post(
        '/api/trip-requests/trip-1/guide-change-requests',
        body: any(named: 'body'),
      ),
    ).thenAnswer((_) async => <String, dynamic>{});

    // An empty reason is not sent.
    await tester.tap(find.text('Request a change'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Send request'));
    await tester.pumpAndSettle();
    expect(find.text('Please give a reason.'), findsOneWidget);
    await tester.tap(find.text('Back'));
    await tester.pumpAndSettle();

    await requestWithReason(tester, 'I am unwell that week');

    verify(
      () => api.post(
        '/api/trip-requests/trip-1/guide-change-requests',
        body: {'reason': 'I am unwell that week'},
      ),
    ).called(1);
    expect(
      find.text('Request sent. The operator will choose a replacement guide.'),
      findsOneWidget,
    );
  });

  testWidgets('a 409 shows the API message', (tester) async {
    await pumpSchedule(tester, 'Confirmed');
    when(
      () => api.post(
        '/api/trip-requests/trip-1/guide-change-requests',
        body: any(named: 'body'),
      ),
    ).thenThrow(
      const UserFacingException(
        'A replacement request for this trip is already open.',
        statusCode: 409,
      ),
    );

    await requestWithReason(tester, 'Family wedding');

    expect(
      find.text('A replacement request for this trip is already open.'),
      findsOneWidget,
    );
  });

  testWidgets('only a Confirmed trip offers Request a change', (tester) async {
    await pumpSchedule(tester, 'InProgress');

    expect(find.text('Request a change'), findsNothing);
  });
}
