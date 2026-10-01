import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/trips/presentation/my_trips_screen.dart';
import 'package:tripcraft_mobile/shared/widgets/status_chip.dart';

import '../helpers.dart';

void main() {
  testWidgets('renders the tourist\'s trips from the API', (tester) async {
    usePhoneSize(tester, phoneSizes.currentValue!);
    final api = MockApiClient();
    when(() => api.get('/api/trip-requests', query: any(named: 'query')))
        .thenAnswer(
          (_) async => pagedJson([
            tripJson(
              id: 't1',
              objective: 'Kandy and Ella by train',
              status: 'PendingReview',
            ),
            tripJson(
              id: 't2',
              objective: 'Galle beaches weekend',
              status: 'Submitted',
            ),
          ]),
        );

    await pumpScreen(tester, const MyTripsScreen(), api: api);
    await tester.pumpAndSettle();

    expect(find.text('Kandy and Ella by train'), findsOneWidget);
    expect(find.text('Galle beaches weekend'), findsOneWidget);
    expect(
      find.byWidgetPredicate(
        (w) => w is StatusChip && w.status == 'PendingReview',
      ),
      findsOneWidget,
    );
    expect(
      find.text('10 Oct 2026 – 14 Oct 2026 · 4 travellers'),
      findsNWidgets(2),
    );
  }, variant: phoneSizes);

  testWidgets('shows the empty state with a call to action', (tester) async {
    final api = MockApiClient();
    when(() => api.get('/api/trip-requests', query: any(named: 'query')))
        .thenAnswer((_) async => pagedJson([]));

    await pumpScreen(tester, const MyTripsScreen(), api: api);
    await tester.pumpAndSettle();

    expect(find.text('No trip requests yet'), findsOneWidget);
    expect(find.text('Plan a trip'), findsOneWidget);
  });

  testWidgets('shows an error with Retry when the API fails', (tester) async {
    final api = MockApiClient();
    when(() => api.get('/api/trip-requests', query: any(named: 'query')))
        .thenThrow(Exception('boom'));

    await pumpScreen(tester, const MyTripsScreen(), api: api);
    await tester.pumpAndSettle();

    expect(find.text('Something went wrong'), findsOneWidget);
    expect(find.text('Retry'), findsOneWidget);
  });
}
