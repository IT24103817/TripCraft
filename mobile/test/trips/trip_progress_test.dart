import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/trips/presentation/trip_progress_card.dart';
import 'package:tripcraft_mobile/shared/theme/app_theme.dart';
import 'package:tripcraft_mobile/shared/utils/statuses.dart';

Future<void> pumpProgress(WidgetTester tester, String status) =>
    tester.pumpWidget(
      MaterialApp(
        theme: buildAppTheme(),
        home: Scaffold(
          body: SingleChildScrollView(child: TripProgressCard(status: status)),
        ),
      ),
    );

void main() {
  testWidgets('the timeline shows the seven steps of the lifecycle in order', (
    tester,
  ) async {
    await pumpProgress(tester, 'QuotationSent');

    const labels = [
      'Submitted',
      'Planning',
      'Quotation sent',
      'Accepted',
      'Confirmed',
      'In progress',
      'Completed',
    ];
    expect(tripTimelineSteps, hasLength(7));
    for (final label in labels) {
      expect(find.text(label), findsOneWidget);
    }
    // No operator review step any more.
    expect(find.text('Pending review'), findsNothing);
    // Each label sits below the one before it.
    final tops = [for (final l in labels) tester.getTopLeft(find.text(l)).dy];
    expect(tops, orderedEquals([...tops]..sort()));
    expect(find.byKey(const ValueKey('step-Planning-done')), findsOneWidget);
    expect(
      find.byKey(const ValueKey('step-QuotationSent-current')),
      findsOneWidget,
    );
    expect(
      find.byKey(const ValueKey('step-ClientAccepted-todo')),
      findsOneWidget,
    );
  });

  testWidgets('ClientDeclined stays on Quotation sent and says what happens', (
    tester,
  ) async {
    await pumpProgress(tester, 'ClientDeclined');

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
    expect(
      find.byKey(const ValueKey('step-ClientAccepted-todo')),
      findsOneWidget,
    );
  });

  testWidgets('NeedsOperator stays on Planning and has no Try again', (
    tester,
  ) async {
    await pumpProgress(tester, 'NeedsOperator');

    expect(
      find.text('Our team is looking at your trip and will send you a quote.'),
      findsOneWidget,
    );
    expect(find.byKey(const ValueKey('step-Planning-current')), findsOneWidget);
    expect(
      find.byKey(const ValueKey('step-QuotationSent-todo')),
      findsOneWidget,
    );
    expect(find.text('Try again'), findsNothing);
  });

  testWidgets('every status explains what happens next', (tester) async {
    final sentences = <String>{};
    for (final status in tripStatuses) {
      await pumpProgress(tester, status);
      final sentence = whatHappensNext(status);
      expect(find.text(sentence), findsOneWidget, reason: status);
      sentences.add(sentence);
    }
    // One distinct sentence per status, none of them the fallback.
    expect(sentences, hasLength(tripStatuses.length));
    expect(sentences, isNot(contains(whatHappensNext('Unknown'))));
  });

  testWidgets('a cancelled trip shows no timeline', (tester) async {
    await pumpProgress(tester, 'Cancelled');

    expect(
      find.text('This trip was cancelled. Nothing else will happen.'),
      findsOneWidget,
    );
    expect(find.text('Planning'), findsNothing);
  });

  test('side states sit on the step they came from', () {
    expect(timelineStatus('ClientDeclined'), 'QuotationSent');
    expect(timelineStatus('NeedsOperator'), 'Planning');
    expect(timelineStatus('QuotationSent'), 'QuotationSent');
  });
}
