import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:mocktail/mocktail.dart';
import 'package:tripcraft_mobile/core/api/api_providers.dart';
import 'package:tripcraft_mobile/core/auth/auth_notifier.dart';
import 'package:tripcraft_mobile/core/storage/session_storage.dart';
import 'package:tripcraft_mobile/features/quotations/application/notifications_poller.dart';
import 'package:tripcraft_mobile/features/quotations/data/local_notifications.dart';
import 'package:tripcraft_mobile/features/quotations/data/notification_models.dart';

import '../helpers.dart';

/// Records every notification instead of showing it on the phone.
class FakeStatusNotifier implements StatusNotifier {
  final shown = <(String title, String body)>[];

  @override
  Future<void> show(int id, String title, String body) async =>
      shown.add((title, body));
}

Map<String, dynamic> notificationJson(
  String id, {
  bool isRead = false,
  String title = 'Your quotation is ready',
}) => {
  'id': id,
  'type': 'QuotationSent',
  'title': title,
  'body': 'Kandy and Ella: review and accept it in the app.',
  'tripRequestId': 'trip-1',
  'isRead': isRead,
  'createdAt': '2026-10-01T09:30:00Z',
};

void main() {
  late MockApiClient api;
  late FakeStatusNotifier phone;
  late InMemorySessionStorage storage;
  late ProviderContainer container;

  /// What GET /api/notifications/mine returns on the next poll.
  void serverHas(List<Map<String, dynamic>> items) {
    when(() => api.get('/api/notifications/mine')).thenAnswer(
      (_) async => {
        'unreadCount': items.where((n) => n['isRead'] == false).length,
        'items': items,
      },
    );
  }

  ProviderContainer newContainer() {
    final c = ProviderContainer(
      overrides: [
        apiClientProvider.overrideWithValue(api),
        sessionStorageProvider.overrideWithValue(storage),
        statusAlertsProvider.overrideWithValue(phone),
      ],
    );
    addTearDown(c.dispose);
    return c;
  }

  setUp(() async {
    api = MockApiClient();
    phone = FakeStatusNotifier();
    // A signed-in tourist: the poller stops when nobody is signed in.
    storage = InMemorySessionStorage()
      ..token = 'jwt-123'
      ..user = {
        'id': 'u1',
        'email': 'tourist@example.com',
        'fullName': 'Test Tourist',
        'role': 'Tourist',
        'isActive': true,
      };
    container = newContainer();
    await container.read(authNotifierProvider.future);
  });

  test('only unread items not shown before are picked', () {
    final items = [
      AppNotification.fromJson(notificationJson('n1')),
      AppNotification.fromJson(notificationJson('n2', isRead: true)),
      AppNotification.fromJson(notificationJson('n3')),
    ];

    expect(notificationsToShow(items, {}).map((n) => n.id), ['n1', 'n3']);
    expect(notificationsToShow(items, {'n1'}).map((n) => n.id), ['n3']);
  });

  test(
    'each new unread notification is shown on the phone exactly once',
    () async {
      final poller = container.read(notificationsPollerProvider.notifier);
      serverHas([notificationJson('n1')]);

      await poller.checkNow();
      await poller.checkNow(); // same list again: nothing new

      expect(phone.shown, [
        (
          'Your quotation is ready',
          'Kandy and Ella: review and accept it in the app.',
        ),
      ]);
      expect(container.read(notificationsPollerProvider).unreadCount, 1);

      serverHas([
        notificationJson('n2', title: 'Trip confirmed'),
        notificationJson('n1'),
      ]);
      await poller.checkNow();

      expect(phone.shown.map((s) => s.$1), [
        'Your quotation is ready',
        'Trip confirmed',
      ]);
    },
  );

  test('only trips with a notification that is new since the last poll are reloaded', () {
    AppNotification item(String id, String? tripId) => AppNotification.fromJson(
      {...notificationJson(id), 'tripRequestId': tripId},
    );
    final before = NotificationList(
      unreadCount: 1,
      items: [item('n1', 'trip-1')],
    );
    final after = NotificationList(
      unreadCount: 4,
      items: [
        item('n3', 'trip-2'), // "Trip confirmed"
        item('n4', null), // not about a trip
        item('n2', 'trip-2'),
        item('n1', 'trip-1'), // seen in the last poll
      ],
    );

    expect(tripsWithNewNotifications(before, after), {'trip-2'});
    expect(tripsWithNewNotifications(after, after), isEmpty);
  });

  test('read notifications are never shown', () async {
    serverHas([notificationJson('n1', isRead: true)]);

    await container.read(notificationsPollerProvider.notifier).checkNow();

    expect(phone.shown, isEmpty);
    expect(container.read(notificationsPollerProvider).items, hasLength(1));
  });

  test('shown ids survive an app restart (they are kept in storage)', () async {
    serverHas([notificationJson('n1')]);
    await container.read(notificationsPollerProvider.notifier).checkNow();
    expect(storage.shownNotificationIds, {'n1'});

    // A new app run: new providers, same storage.
    final restarted = newContainer();
    await restarted.read(authNotifierProvider.future);
    await restarted.read(notificationsPollerProvider.notifier).checkNow();

    expect(phone.shown, hasLength(1));
  });

  test(
    'mark read and mark all read call the API and update the list',
    () async {
      serverHas([notificationJson('n1'), notificationJson('n2')]);
      when(() => api.post(any())).thenAnswer((_) async => null);
      final poller = container.read(notificationsPollerProvider.notifier);
      await poller.checkNow();

      await poller.markRead('n1');
      expect(container.read(notificationsPollerProvider).unreadCount, 1);
      verify(() => api.post('/api/notifications/n1/read')).called(1);

      await poller.markAllRead();
      final list = container.read(notificationsPollerProvider);
      expect(list.unreadCount, 0);
      expect(list.items.every((n) => n.isRead), isTrue);
      verify(() => api.post('/api/notifications/read-all')).called(1);
    },
  );

  test('a network error is ignored until the next poll', () async {
    when(() => api.get('/api/notifications/mine'))
        .thenThrow(Exception('offline'));

    await container.read(notificationsPollerProvider.notifier).checkNow();

    expect(phone.shown, isEmpty);
    expect(container.read(notificationsPollerProvider).items, isEmpty);
  });
}
