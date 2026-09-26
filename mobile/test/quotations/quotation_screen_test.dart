import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/features/quotations/application/status_watcher.dart';
import 'package:tripcraft_mobile/features/quotations/data/quotation_models.dart';
import 'package:tripcraft_mobile/features/quotations/presentation/quotation_screen.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';

import '../helpers.dart';

void main() {
  final quotation = Quotation.fromJson({
    'lines': [
      {
        'line_type': 'guide',
        'description': 'Guide',
        'qty': 5,
        'unit_lkr': 6000,
        'amount_lkr': 30000,
      },
      {
        'line_type': 'room',
        'description': 'Standard double',
        'qty': 8,
        'unit_lkr': 12000,
        'amount_lkr': 96000,
      },
    ],
    'subtotal_lkr': 162800,
    'margin_pct': 15,
    'margin_lkr': 24420,
    'total_lkr': 187220,
    'fx_rate': 300,
    'fx_as_of': '2026-10-01T00:00:00Z',
    'fx_stale': false,
    'total_usd': 624.07,
  });

  testWidgets(
    'formats lines, subtotal, margin and total in LKR and USD with the FX rate',
    (tester) async {
      usePhoneSize(tester, phoneSizes.currentValue!);
      await tester.pumpWidget(
        MaterialApp(
          theme: buildAppTheme(),
          home: Scaffold(
            body: QuotationBody(
              quotation: quotation,
              workflowStatus: 'PendingApproval',
            ),
          ),
        ),
      );

      expect(find.text('LKR 30,000.00'), findsOneWidget);
      expect(find.text('USD 100.00'), findsOneWidget);
      expect(find.text('5 × LKR 6,000.00'), findsOneWidget);
      expect(find.text('LKR 162,800.00'), findsOneWidget);
      expect(find.text('Service margin (15%)'), findsOneWidget);
      expect(find.text('LKR 187,220.00'), findsOneWidget);
      expect(find.text('USD 624.07'), findsOneWidget);
      expect(
        find.textContaining('1 USD = 300.00 LKR · as of 1 Oct 2026'),
        findsOneWidget,
      );
      expect(
        tester
            .widget<FilledButton>(
              find.widgetWithText(FilledButton, 'Accept quotation'),
            )
            .onPressed,
        isNull,
      );
    },
    variant: phoneSizes,
  );

  test('status diff reports only real changes', () {
    final now = DateTime(2026, 10, 1);
    final changes = diffStatuses(
      {'t1': 'Planning', 't2': 'Submitted'},
      const [
        TripStatusItem(id: 't1', objective: 'Kandy', status: 'PendingApproval'),
        TripStatusItem(id: 't2', objective: 'Galle', status: 'Submitted'),
        TripStatusItem(id: 't3', objective: 'New trip', status: 'Submitted'),
      ],
      now,
    );

    expect(changes, hasLength(1));
    expect(changes.single.tripId, 't1');
    expect(changes.single.from, 'Planning');
    expect(changes.single.to, 'PendingApproval');
  });

  Map<String, dynamic> stored(String status, {String? acceptedAt}) => {
    'id': 'q1',
    'status': status,
    'acceptedAt': acceptedAt,
    'lines': [
      {
        'lineType': 'guide',
        'description': 'Guide Nimal Perera, 5 days',
        'qty': 5,
        'unitLkr': 6000,
        'amountLkr': 30000,
      },
    ],
    'subtotalLkr': 30000,
    'marginPct': 15,
    'marginLkr': 4500,
    'totalLkr': 34500,
    'fxRate': 300,
    'fxAsOf': '2026-10-01T00:00:00Z',
    'fxStale': false,
    'totalUsd': 115,
  };

  MockApiClient apiWith(Map<String, dynamic> quotation) {
    final api = MockApiClient();
    when(() => api.get('/api/trip-requests/trip-1/workflow')).thenAnswer(
      (_) async => {
        'id': 'wf-1',
        'status': 'Completed',
        'finalOutcome': {
          'proposal': {'quotationId': 'q1'},
        },
      },
    );
    when(() => api.get('/api/quotations/q1'))
        .thenAnswer((_) async => quotation);
    return api;
  }

  testWidgets('an approved quotation can be accepted from the phone', (
    tester,
  ) async {
    final api = apiWith(stored('Approved'));
    when(() => api.post('/api/quotations/q1/accept')).thenAnswer(
      (_) async => stored('Approved', acceptedAt: '2026-10-02T09:00:00Z'),
    );

    await pumpScreen(tester, const QuotationScreen(tripId: 'trip-1'), api: api);
    await tester.pumpAndSettle();
    expect(find.text('Guide Nimal Perera, 5 days'), findsOneWidget);
    expect(
      find.text('Your operator approved this price. Accept it to confirm.'),
      findsOneWidget,
    );

    await tester.scrollUntilVisible(
      find.text('Accept quotation'),
      200,
      scrollable: find.byType(Scrollable).first,
    );
    await tester.tap(find.text('Accept quotation'));
    await tester.pumpAndSettle();

    verify(() => api.post('/api/quotations/q1/accept')).called(1);
    expect(find.text('Quotation accepted.'), findsOneWidget);
  });

  testWidgets('a pending or already accepted quotation cannot be accepted', (
    tester,
  ) async {
    await pumpScreen(
      tester,
      const QuotationScreen(tripId: 'trip-1'),
      api: apiWith(stored('Approved', acceptedAt: '2026-10-02T09:00:00Z')),
    );
    await tester.pumpAndSettle();

    expect(find.textContaining('You accepted this price on'), findsOneWidget);
    final button = tester.widget<FilledButton>(
      find.widgetWithText(FilledButton, 'Accept quotation'),
    );
    expect(button.onPressed, isNull);
  });
}
