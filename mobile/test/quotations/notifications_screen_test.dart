import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/quotations/application/status_watcher.dart';
import 'package:tripcraft_mobile/features/quotations/data/quotation_models.dart';
import 'package:tripcraft_mobile/features/quotations/presentation/notifications_screen.dart';

import '../helpers.dart';

/// A watcher whose history is fixed, so the screen can be checked without polling.
class FakeStatusWatcher extends StatusWatcher {
  FakeStatusWatcher(this.history);

  final List<StatusChange> history;

  @override
  List<StatusChange> build() => history;
}

Future<void> pumpAlerts(WidgetTester tester, List<StatusChange> history) async {
  await pumpScreen(
    tester,
    const NotificationsScreen(),
    api: MockApiClient(),
    overrides: [
      statusWatcherProvider.overrideWith(() => FakeStatusWatcher(history)),
    ],
  );
  await tester.pumpAndSettle();
}

void main() {
  testWidgets('no status changes shows the empty state', (tester) async {
    await pumpAlerts(tester, const []);

    expect(find.text('No updates yet'), findsOneWidget);
  });

  testWidgets('a status change is listed with its old and new status', (
    tester,
  ) async {
    await pumpAlerts(tester, [
      StatusChange(
        tripId: 't1',
        objective: 'Kandy and Ella',
        from: 'Planning',
        to: 'PendingApproval',
        at: DateTime(2026, 10, 1, 9, 30),
      ),
    ]);

    expect(find.text('No updates yet'), findsNothing);
    expect(find.text('Kandy and Ella'), findsOneWidget);
    expect(find.textContaining('Planning → Pending approval'), findsOneWidget);
  });
}
