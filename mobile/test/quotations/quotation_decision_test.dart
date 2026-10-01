import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/quotations/data/quotations_repository.dart';
import 'package:tripcraft_mobile/features/quotations/presentation/quotation_decision_card.dart';

import '../helpers.dart';
import 'quotation_fakes.dart';

/// The Accept / Decline panel on the trip's Overview at QuotationSent.
void main() {
  late FakeQuotationsRepository repository;
  late int decidedCalls;

  Future<void> pumpCard(WidgetTester tester) async {
    repository = FakeQuotationsRepository('QuotationSent');
    decidedCalls = 0;
    await pumpScreen(
      tester,
      Scaffold(
        body: SingleChildScrollView(
          child: QuotationDecisionCard(
            tripId: 'trip-1',
            onDecided: () => decidedCalls++,
          ),
        ),
      ),
      api: MockApiClient(),
      overrides: [quotationsRepositoryProvider.overrideWithValue(repository)],
    );
    await tester.pumpAndSettle();
  }

  testWidgets('shows the total and accepts the quotation', (tester) async {
    await pumpCard(tester);

    expect(find.text('LKR 187,220.00'), findsOneWidget);
    expect(find.text('USD 624.07'), findsOneWidget);

    await tester.tap(find.text('Accept quotation'));
    await tester.pumpAndSettle();

    expect(repository.acceptedId, 'q1');
    expect(decidedCalls, 1);
    expect(
      find.text('Quotation accepted. The operator will now confirm your trip.'),
      findsOneWidget,
    );
  });

  testWidgets('Decline asks for a required reason in a bottom sheet', (
    tester,
  ) async {
    await pumpCard(tester);

    await tester.tap(find.widgetWithText(OutlinedButton, 'Decline'));
    await tester.pumpAndSettle();
    expect(find.byType(BottomSheet), findsOneWidget);
    expect(find.text('Decline this quotation?'), findsOneWidget);

    // Empty reason: the sheet stays open and nothing is sent.
    await tester.tap(find.widgetWithText(FilledButton, 'Decline'));
    await tester.pumpAndSettle();
    expect(find.text('Please give a reason.'), findsOneWidget);
    expect(repository.declined, isNull);
    expect(decidedCalls, 0);

    await tester.enterText(
      find.widgetWithText(TextFormField, 'Reason'),
      'Please swap the safari for a beach day',
    );
    await tester.tap(find.widgetWithText(FilledButton, 'Decline'));
    await tester.pumpAndSettle();

    expect(find.byType(BottomSheet), findsNothing);
    expect(repository.declined, (
      'q1',
      'Please swap the safari for a beach day',
    ));
    expect(decidedCalls, 1);
  });

  testWidgets('backing out of the sheet declines nothing', (tester) async {
    await pumpCard(tester);

    await tester.tap(find.widgetWithText(OutlinedButton, 'Decline'));
    await tester.pumpAndSettle();
    await tester.tap(find.widgetWithText(OutlinedButton, 'Back'));
    await tester.pumpAndSettle();

    expect(repository.declined, isNull);
    expect(decidedCalls, 0);
  });
}
