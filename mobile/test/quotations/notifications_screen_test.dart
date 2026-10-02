import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:tripcraft_mobile/features/quotations/application/notification_target.dart';
import 'package:tripcraft_mobile/features/quotations/application/notifications_poller.dart';
import 'package:tripcraft_mobile/features/quotations/data/notification_models.dart';
import 'package:tripcraft_mobile/features/quotations/presentation/notifications_screen.dart';

import '../helpers.dart';

/// A poller whose list is fixed and that only records mark-read calls (no timer, no API).
class FakePoller extends NotificationsPoller {
  FakePoller(this.initial);

  final NotificationList initial;
  final markedRead = <String>[];
  var markedAllRead = false;

  @override
  NotificationList build() => initial;

  @override
  Future<void> checkNow() async {}

  @override
  Future<void> markRead(String id) async => markedRead.add(id);

  @override
  Future<void> markAllRead() async {
    markedAllRead = true;
    state = NotificationList(
      unreadCount: 0,
      items: [for (final n in state.items) n.copyWith(isRead: true)],
    );
  }
}

AppNotification notification(String id, {bool isRead = false}) =>
    AppNotification(
      id: id,
      type: 'TripConfirmed',
      title: 'Trip confirmed $id',
      body: 'Your vouchers are ready.',
      isRead: isRead,
      createdAt: '2026-10-01T09:30:00Z',
    );

Future<FakePoller> pumpAlerts(
  WidgetTester tester,
  List<AppNotification> items,
) async {
  final poller = FakePoller(
    NotificationList(
      unreadCount: items.where((n) => !n.isRead).length,
      items: items,
    ),
  );
  await pumpScreen(
    tester,
    const NotificationsScreen(),
    api: MockApiClient(),
    overrides: [notificationsPollerProvider.overrideWith(() => poller)],
  );
  await tester.pumpAndSettle();
  return poller;
}

void main() {
  testWidgets('no notifications shows the empty state', (tester) async {
    await pumpAlerts(tester, const []);

    expect(find.text('No notifications yet'), findsOneWidget);
    expect(find.text('Mark all read'), findsNothing);
  });

  testWidgets('lists notifications and marks the unread ones', (tester) async {
    await pumpAlerts(tester, [
      notification('n1'),
      notification('n2', isRead: true),
    ]);

    expect(find.text('Trip confirmed n1'), findsOneWidget);
    expect(find.text('Trip confirmed n2'), findsOneWidget);
    expect(find.byKey(const ValueKey('unread-n1')), findsOneWidget);
    expect(find.byKey(const ValueKey('unread-n2')), findsNothing);
  });

  testWidgets('tapping an unread notification marks it read', (tester) async {
    final poller = await pumpAlerts(tester, [notification('n1')]);

    await tester.tap(find.text('Trip confirmed n1'));
    await tester.pumpAndSettle();

    expect(poller.markedRead, ['n1']);
  });

  testWidgets('Mark all read clears every unread dot', (tester) async {
    final poller = await pumpAlerts(tester, [
      notification('n1'),
      notification('n2'),
    ]);

    await tester.tap(find.text('Mark all read'));
    await tester.pumpAndSettle();

    expect(poller.markedAllRead, isTrue);
    expect(find.byKey(const ValueKey('unread-n1')), findsNothing);
    expect(find.text('Mark all read'), findsNothing);
  });

  group('tapping a notification', () {
    AppNotification about(String type, {String? tripId = 'trip-1'}) =>
        AppNotification(
          id: 'n-$type',
          type: type,
          title: type,
          body: 'body',
          tripRequestId: tripId,
          isRead: false,
          createdAt: '2026-10-01T09:30:00Z',
        );

    test('QuotationSent and QuotationUpdated open the trip for a tourist', () {
      expect(
        notificationRoute(about('QuotationSent'), isTourist: true),
        '/trips/trip-1',
      );
      expect(
        notificationRoute(about('QuotationUpdated'), isTourist: true),
        '/trips/trip-1',
      );
    });

    test('the other lifecycle types open the trip too', () {
      for (final type in [
        'ClientAccepted',
        'ClientDeclined',
        'NeedsOperator',
        'TripConfirmed',
        'TripCancelled',
      ]) {
        expect(
          notificationRoute(about(type), isTourist: true),
          '/trips/trip-1',
          reason: type,
        );
      }
    });

    test('nothing to open for a guide or without a trip', () {
      expect(
        notificationRoute(about('TripAssigned'), isTourist: false),
        isNull,
      );
      expect(
        notificationRoute(
          about('QuotationUpdated', tripId: null),
          isTourist: true,
        ),
        isNull,
      );
    });
  });
}
