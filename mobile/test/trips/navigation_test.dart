import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/trips/presentation/my_trips_screen.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_detail_screen.dart';

import '../helpers.dart';

/// PLAN.md section 11, Flutter: "navigation to itinerary" — tapping a trip opens its itinerary.
void main() {
  testWidgets(
    'tapping a trip in the list opens its detail with the itinerary',
    (tester) async {
      final api = MockApiClient();
      when(() => api.get('/api/trip-requests', query: any(named: 'query')))
          .thenAnswer(
            (_) async => pagedJson([
              tripJson(
                id: 'trip-1',
                objective: 'Kandy and Ella by train',
                status: 'Confirmed',
              ),
            ]),
          );
      when(() => api.get('/api/trip-requests/trip-1'))
          .thenAnswer((_) async => tripJson(id: 'trip-1', status: 'Confirmed'));
      when(() => api.get('/api/trip-requests/trip-1/workflow')).thenThrow(
        const UserFacingException('We could not find that.', statusCode: 404),
      );
      when(() => api.get('/api/trip-requests/trip-1/itinerary')).thenAnswer(
        (_) async => {
          'days': [
            {
              'dayNumber': 1,
              'city': 'Kandy',
              'stops': [
                {
                  'attractionId': '',
                  'attractionName': 'Temple of the Sacred Tooth Relic',
                },
              ],
            },
          ],
        },
      );

      final router = GoRouter(
        initialLocation: '/trips',
        routes: [
          GoRoute(path: '/trips', builder: (_, _) => const MyTripsScreen()),
          GoRoute(
            path: '/trips/:id',
            builder: (_, s) =>
                TripDetailScreen(tripId: s.pathParameters['id']!),
          ),
        ],
      );
      await tester.pumpWidget(
        ProviderScope(
          retry: (_, _) => null,
          overrides: [
            apiClientProvider.overrideWithValue(api),
            sessionStorageProvider.overrideWithValue(InMemorySessionStorage()),
          ],
          child: MaterialApp.router(routerConfig: router),
        ),
      );
      await tester.pumpAndSettle();

      await tester.tap(find.text('Kandy and Ella by train'));
      await tester.pumpAndSettle();

      expect(router.state.matchedLocation, '/trips/trip-1');
      expect(find.text('Trip request'), findsOneWidget);
      await tester.scrollUntilVisible(
        find.text('Day 1 — Kandy'),
        200,
        scrollable: find.byType(Scrollable).first,
      );
      expect(find.text('• Temple of the Sacred Tooth Relic'), findsOneWidget);
    },
  );
}
