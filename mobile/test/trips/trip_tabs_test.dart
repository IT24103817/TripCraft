import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:qr_flutter/qr_flutter.dart';

import '../helpers.dart';
import 'trip_fakes.dart';
import 'vouchers_test.dart' show tripPayload, nightOnePayload, voucherJson;

/// The Itinerary and Vouchers tabs, and the cards a confirmed or completed trip adds to the Overview.
void main() {
  late MockApiClient api;
  setUp(() => api = MockApiClient());

  /// A saved itinerary (GET /api/trip-requests/{id}/itinerary) with positions and weather in the notes.
  final savedItinerary = {
    'days': [
      {
        'dayNumber': 1,
        'city': 'Kandy',
        'notes': 'By road; weather: Light rain, 24°C',
        'stops': [
          {
            'attractionId': 'a1',
            'attractionName': 'Temple of the Tooth',
            'latitude': 7.2936,
            'longitude': 80.6413,
          },
          {
            'attractionId': 'a2',
            'attractionName': 'Peradeniya Gardens',
            'latitude': 7.2687,
            'longitude': 80.5966,
          },
        ],
      },
      {
        'dayNumber': 2,
        'city': 'Ella',
        'notes': 'By train',
        'stops': [
          {
            'attractionId': 'a3',
            'attractionName': 'Nine Arches Bridge',
            'latitude': 6.8768,
            'longitude': 81.0608,
          },
        ],
      },
    ],
  };

  testWidgets(
    'Itinerary: each day with its weather icon, and a map with the route',
    (tester) async {
      stubTrip(api, 'Confirmed', itinerary: savedItinerary);
      await pumpTripDetail(tester, api);
      await openTab(tester, 'Itinerary');

      expect(find.text('Day 1 — Kandy'), findsOneWidget);
      expect(find.text('• Temple of the Tooth'), findsOneWidget);
      expect(find.text('Light rain, 24°C'), findsOneWidget);
      expect(find.byKey(const ValueKey('weather-1-rain')), findsOneWidget);
      // Day 2's notes say nothing about the weather: no icon.
      expect(find.text('Day 2 — Ella'), findsOneWidget);
      expect(
        find.byWidgetPredicate(
          (w) =>
              w.key is ValueKey<String> &&
              (w.key! as ValueKey<String>).value.startsWith('weather-2-'),
        ),
        findsNothing,
      );

      await scrollTo(tester, find.byType(MarkerLayer));
      final markers = tester.widget<MarkerLayer>(find.byType(MarkerLayer));
      expect(markers.markers, hasLength(3));
      final route = tester.widget<PolylineLayer>(find.byType(PolylineLayer));
      expect(
        route.polylines.single.points.map((p) => (p.latitude, p.longitude)),
        [(7.2936, 80.6413), (7.2687, 80.5966), (6.8768, 81.0608)],
      );
      // The positions came with the itinerary, so no attraction was fetched.
      verifyNever(() => api.get(any(that: startsWith('/api/attractions/'))));
    },
  );

  testWidgets(
    'Itinerary from the agents\' proposal loads the stop positions for the map',
    (tester) async {
      stubTrip(
        api,
        'Planning',
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
                  'weather': 'Sunny, 29°C',
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
      for (final (id, lat, lng) in [
        ('a1', 7.2936, 80.6413),
        ('a2', 7.2687, 80.5966),
      ]) {
        when(() => api.get('/api/attractions/$id')).thenAnswer(
          (_) async => {
            'id': id,
            'name': id,
            'city': 'Kandy',
            'latitude': lat,
            'longitude': lng,
          },
        );
      }
      await pumpTripDetail(tester, api);
      await openTab(tester, 'Itinerary');

      expect(find.byKey(const ValueKey('weather-1-sun')), findsOneWidget);
      await scrollTo(tester, find.byType(MarkerLayer));
      final layer = tester.widget<MarkerLayer>(find.byType(MarkerLayer));
      expect(layer.markers, hasLength(2));
    },
  );

  testWidgets('Vouchers: QR codes with the trip voucher first', (tester) async {
    stubTrip(
      api,
      'Confirmed',
      vouchers: [
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
    await pumpTripDetail(tester, api);
    await openTab(tester, 'Vouchers');

    expect(find.byKey(const ValueKey('qr-$tripPayload')), findsOneWidget);
    expect(find.byType(QrImageView), findsWidgets);
    expect(find.text('Trip voucher'), findsOneWidget);
    await scrollTo(tester, find.text('Kandy Hills'));
    expect(
      tester.getTopLeft(find.text('Trip voucher')).dy,
      lessThan(tester.getTopLeft(find.text('Kandy Hills')).dy),
    );
  });

  testWidgets('Vouchers before confirmation: a hint, and no API call', (
    tester,
  ) async {
    stubTrip(api, 'QuotationSent');
    await pumpTripDetail(tester, api);
    await openTab(tester, 'Vouchers');

    expect(find.text('No vouchers yet'), findsOneWidget);
    verifyNever(() => api.get('/api/trips/trip-1/vouchers'));
  });

  testWidgets('a confirmed trip shows its guide and vehicle', (tester) async {
    stubTrip(api, 'Confirmed');
    await pumpTripDetail(tester, api);
    await scrollTo(tester, find.text('Your guide and vehicle'));
    await scrollTo(tester, find.text('CAB-1234'));

    expect(find.text('Nimal Perera'), findsOneWidget);
    expect(find.text('+94 77 123 4567'), findsOneWidget);
    expect(find.text('Van · 9 seats'), findsOneWidget);
    expect(find.byTooltip('Call your guide'), findsOneWidget);
  });

  testWidgets(
    'a completed trip asks to rate the guide, then shows the rating',
    (tester) async {
      stubTrip(api, 'Completed');
      when(
        () => api.post(
          '/api/trip-requests/trip-1/guide-rating',
          body: any(named: 'body'),
        ),
      ).thenAnswer(
        (_) async => {
          'tripRequestId': 'trip-1',
          'guideId': 'g1',
          'guideName': 'Nimal Perera',
          'stars': 4,
          'comment': 'Very kind',
          'ratedAt': '2026-10-15T10:00:00Z',
        },
      );
      await pumpTripDetail(tester, api);
      await scrollTo(tester, find.text('Send rating'));

      // No stars yet: nothing to send.
      final send = tester.widget<FilledButton>(
        find.widgetWithText(FilledButton, 'Send rating'),
      );
      expect(send.onPressed, isNull);

      await tester.ensureVisible(find.byTooltip('4 stars'));
      await tester.tap(find.byTooltip('4 stars'));
      await tester.enterText(
        find.widgetWithText(TextFormField, 'Comment (optional)'),
        'Very kind',
      );
      // After rating, the API returns the rating for the card to show.
      when(() => api.get('/api/trip-requests/trip-1/guide-rating')).thenAnswer(
        (_) async => {
          'tripRequestId': 'trip-1',
          'guideId': 'g1',
          'guideName': 'Nimal Perera',
          'stars': 4,
          'comment': 'Very kind',
          'ratedAt': '2026-10-15T10:00:00Z',
        },
      );
      await tester.tap(find.text('Send rating'));
      await tester.pumpAndSettle();

      verify(
        () => api.post(
          '/api/trip-requests/trip-1/guide-rating',
          body: {'stars': 4, 'comment': 'Very kind'},
        ),
      ).called(1);
      await scrollTo(tester, find.textContaining('You rated Nimal Perera'));
      expect(
        find.text('You rated Nimal Perera 4 out of 5. Thank you!'),
        findsOneWidget,
      );
      expect(find.text('Send rating'), findsNothing);
    },
  );
}
