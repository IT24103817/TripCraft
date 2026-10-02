import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/user_facing_exception.dart';
import 'package:tripcraft_mobile/features/quotations/data/quotation_models.dart';
import 'package:tripcraft_mobile/features/quotations/data/quotations_repository.dart';
import 'package:tripcraft_mobile/features/quotations/presentation/quotation_screen.dart';
import 'package:tripcraft_mobile/features/quotations/presentation/quote_notes.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';

import '../helpers.dart';
import 'quotation_fakes.dart';

Future<FakeQuotationsRepository> pumpQuotation(
  WidgetTester tester,
  String tripStatus, {
  Quotation? served,
  int version = 1,
}) async {
  final repository = FakeQuotationsRepository(
    tripStatus,
    served: served,
    version: version,
  );
  await pumpScreen(
    tester,
    const QuotationScreen(tripId: 'trip-1'),
    api: MockApiClient(),
    overrides: [quotationsRepositoryProvider.overrideWithValue(repository)],
  );
  await tester.pumpAndSettle();
  return repository;
}

Future<void> scrollTo(WidgetTester tester, String text) =>
    tester.scrollUntilVisible(
      find.text(text),
      200,
      scrollable: find.byType(Scrollable).first,
    );

ButtonStyleButton buttonWithText(WidgetTester tester, String text) =>
    tester.widget<ButtonStyleButton>(
      find.ancestor(
        of: find.text(text),
        matching: find.bySubtype<ButtonStyleButton>(),
      ),
    );

void main() {
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
              tripStatus: 'ClientAccepted',
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
      // Once the trip has moved on from QuotationSent, the tourist cannot decide again.
      await scrollTo(tester, 'Decline');
      expect(buttonWithText(tester, 'Accept quotation').onPressed, isNull);
      expect(buttonWithText(tester, 'Decline').onPressed, isNull);
    },
    variant: phoneSizes,
  );

  testWidgets('at QuotationSent the tourist accepts the quotation', (
    tester,
  ) async {
    final repository = await pumpQuotation(tester, 'QuotationSent');
    expect(find.text('Guide'), findsOneWidget);

    await scrollTo(tester, 'Accept quotation');
    expect(
      find.text(
        'Accept to go ahead, or decline and tell the operator what to change.',
      ),
      findsOneWidget,
    );
    await tester.tap(find.text('Accept quotation'));
    await tester.pumpAndSettle();

    expect(repository.acceptedId, 'q1');
    expect(
      find.text('Quotation accepted. The operator will now confirm your trip.'),
      findsOneWidget,
    );
    // Reloaded: the trip is ClientAccepted, so both buttons are now disabled.
    expect(buttonWithText(tester, 'Accept quotation').onPressed, isNull);
  });

  testWidgets('declining asks for a required reason and sends it', (
    tester,
  ) async {
    final repository = await pumpQuotation(tester, 'QuotationSent');
    await scrollTo(tester, 'Decline');

    await tester.tap(find.text('Decline'));
    await tester.pumpAndSettle();
    expect(find.text('Decline this quotation?'), findsOneWidget);

    // Empty reason: the dialog stays open and nothing is sent.
    await tester.tap(find.widgetWithText(FilledButton, 'Decline'));
    await tester.pumpAndSettle();
    expect(find.text('Please give a reason.'), findsOneWidget);
    expect(repository.declined, isNull);

    await tester.enterText(
      find.widgetWithText(TextFormField, 'Reason'),
      '  Too expensive, please drop the safari  ',
    );
    await tester.tap(find.widgetWithText(FilledButton, 'Decline'));
    await tester.pumpAndSettle();

    expect(repository.declined, (
      'q1',
      'Too expensive, please drop the safari',
    ));
    expect(
      find.text('Quotation declined. The operator will prepare a new version.'),
      findsOneWidget,
    );
  });

  testWidgets('a quote within budget shows no budget note and no version', (
    tester,
  ) async {
    await pumpQuotation(tester, 'QuotationSent');

    expect(find.byKey(const ValueKey('best-price-note')), findsNothing);
    expect(find.textContaining('Best price we can offer'), findsNothing);
    expect(find.textContaining('Updated quote'), findsNothing);
  });

  testWidgets('the best available price shows the budget note', (tester) async {
    await pumpQuotation(tester, 'QuotationSent', served: overBudgetQuotation);
    await scrollTo(
      tester,
      'Best price we can offer — USD 120 above your budget',
    );

    expect(
      find.text('Best price we can offer — USD 120 above your budget'),
      findsOneWidget,
    );
  });

  testWidgets('a resent quote says "Updated quote (version 2)"', (
    tester,
  ) async {
    await pumpQuotation(tester, 'QuotationSent', version: 2);

    expect(find.text('Updated quote (version 2)'), findsOneWidget);
  });

  test('the budget note and version label follow the flag and version', () {
    expect(bestPriceNote(quotation), isNull);
    // The flag decides, not the note: a stray note on an in-budget quote is not shown.
    expect(
      bestPriceNote(quotation.copyWith(budgetNote: 'Best price we can offer')),
      isNull,
    );
    expect(
      bestPriceNote(overBudgetQuotation),
      'Best price we can offer — USD 120 above your budget',
    );
    expect(
      bestPriceNote(
        quotation.copyWith(bestAvailablePrice: true, overBudgetUsd: 85.5),
      ),
      'Best price we can offer — USD 85.50 above your budget',
    );
    expect(updatedQuoteLabel(null), isNull);
    expect(updatedQuoteLabel(1), isNull);
    expect(updatedQuoteLabel(3), 'Updated quote (version 3)');
  });

  testWidgets('a 409 from accept shows the API message', (tester) async {
    final repository = await pumpQuotation(tester, 'QuotationSent');
    repository.error = const UserFacingException(
      'The quotation can only be accepted while it is sent to you.',
      statusCode: 409,
    );
    await scrollTo(tester, 'Accept quotation');

    await tester.tap(find.text('Accept quotation'));
    await tester.pumpAndSettle();

    expect(
      find.text('The quotation can only be accepted while it is sent to you.'),
      findsOneWidget,
    );
  });

  test(
    'the repository reads the trip status, workflow and stored quotation',
    () async {
      final api = MockApiClient();
      when(() => api.get('/api/trip-requests/trip-1')).thenAnswer(
        (_) async => tripJson(id: 'trip-1', status: 'QuotationSent'),
      );
      when(() => api.get('/api/trip-requests/trip-1/workflow')).thenAnswer(
        (_) async => {
          'id': 'wf-1',
          'status': 'Approved',
          'finalOutcome': {
            'proposal': {'quotationId': 'q1'},
          },
        },
      );
      when(() => api.get('/api/quotations/q1')).thenAnswer(
        (_) async => {
          'id': 'q1',
          'version': 2,
          'status': 'Approved',
          'acceptedAt': null,
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
          'bestAvailablePrice': true,
          'overBudgetUsd': 120,
          'budgetNote': 'Best price we can offer — USD 120 above your budget',
        },
      );
      when(
        () => api.post('/api/quotations/q1/decline', body: any(named: 'body')),
      ).thenAnswer(
        (_) async => {
          'quotationId': 'q1',
          'tripRequestId': 'trip-1',
          'workflowId': 'wf-1',
          'decision': 'Declined',
          'tripStatus': 'ClientDeclined',
          'workflowStatus': 'Approved',
          'holdsCreated': 0,
        },
      );
      final repository = QuotationsRepository(api);

      final view = await repository.quotationFor('trip-1');
      final decision = await repository.decline('q1', ' Too long ');

      expect(view.tripStatus, 'QuotationSent');
      expect(view.quotationId, 'q1');
      expect(
        view.quotation?.lines.single.description,
        'Guide Nimal Perera, 5 days',
      );
      expect(decision.tripStatus, 'ClientDeclined');
      expect(view.version, 2);
      expect(view.quotation?.bestAvailablePrice, isTrue);
      expect(view.quotation?.overBudgetUsd, 120);
      expect(
        view.quotation?.budgetNote,
        'Best price we can offer — USD 120 above your budget',
      );
      verify(
        () => api.post(
          '/api/quotations/q1/decline',
          body: {'reason': 'Too long'},
        ),
      ).called(1);
    },
  );
}
