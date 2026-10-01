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
  testWidgets('the timeline shows the eight steps of the lifecycle in order', (
    tester,
  ) async {
    await pumpProgress(tester, 'QuotationSent');

    const labels = [
      'Submitted',
      'Planning',
      'Pending review',
      'Quotation sent',
      'Accepted',
      'Confirmed',
      'In progress',
      'Completed',
    ];
    for (final label in labels) {
      expect(find.text(label), findsOneWidget);
    }
    // Each label sits below the one before it.
    final tops = [for (final l in labels) tester.getTopLeft(find.text(l)).dy];
    expect(tops, orderedEquals([...tops]..sort()));
    expect(
      find.byKey(const ValueKey('step-PendingReview-done')),
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
    expect(find.text('Pending review'), findsNothing);
  });
}
