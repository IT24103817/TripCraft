import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/auth/auth_notifier.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/quotations/application/status_watcher.dart';
import 'package:tripcraft_mobile/features/quotations/data/local_notifications.dart';

import '../helpers.dart';

/// Records every notification instead of showing it on the phone.
class FakeStatusNotifier implements StatusNotifier {
  final shown = <(String title, String body)>[];

  @override
  Future<void> show(int id, String title, String body) async =>
      shown.add((title, body));
}

void main() {
  late MockApiClient api;
  late FakeStatusNotifier alerts;
  late ProviderContainer container;

  /// The tourist's trips as the next poll will see them.
  void tripsAre(String status) {
    when(() => api.get('/api/trip-requests', query: any(named: 'query')))
        .thenAnswer(
          (_) async => pagedJson([
            tripJson(id: 't1', status: status, objective: 'Kandy and Ella'),
          ]),
        );
  }

  setUp(() async {
    api = MockApiClient();
    alerts = FakeStatusNotifier();
    container = ProviderContainer(
      overrides: [
        apiClientProvider.overrideWithValue(api),
        // A signed-in tourist: the watcher stops (and forgets its baseline) when nobody is signed in.
        sessionStorageProvider.overrideWithValue(
          InMemorySessionStorage()
            ..token = 'jwt-123'
            ..user = {
              'id': 'u1',
              'email': 'tourist@example.com',
              'fullName': 'Test Tourist',
              'role': 'Tourist',
              'isActive': true,
            },
        ),
        statusAlertsProvider.overrideWithValue(alerts),
      ],
    );
    addTearDown(container.dispose);
    await container.read(authNotifierProvider.future);
  });

  test(
    'a status change fires a notification titled with the new status',
    () async {
      final watcher = container.read(statusWatcherProvider.notifier);
      tripsAre('Planning');
      await watcher.checkNow(); // the first poll only records the baseline
      expect(alerts.shown, isEmpty);

      tripsAre('PendingApproval');
      await watcher.checkNow();

      expect(alerts.shown, [('Trip pending approval', 'Kandy and Ella')]);
      final history = container.read(statusWatcherProvider);
      expect(history.single.from, 'Planning');
      expect(history.single.to, 'PendingApproval');
    },
  );

  test('an unchanged status fires no notification', () async {
    final watcher = container.read(statusWatcherProvider.notifier);
    tripsAre('Planning');
    await watcher.checkNow();
    await watcher.checkNow();

    expect(alerts.shown, isEmpty);
    expect(container.read(statusWatcherProvider), isEmpty);
  });
}
