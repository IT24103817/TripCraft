import 'dart:async';
import 'dart:math';

import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_providers.dart';
import '../../../core/auth/auth_notifier.dart';
import '../../../core/config.dart';
import '../data/local_notifications.dart';
import '../data/notification_models.dart';
import '../data/notifications_repository.dart';

part 'notifications_poller.g.dart';

/// The unread notifications that have not been shown on the phone yet. Pure, so it is unit-tested directly.
List<AppNotification> notificationsToShow(
  List<AppNotification> items,
  Set<String> alreadyShown,
) => [
  for (final item in items)
    if (!item.isRead && !alreadyShown.contains(item.id)) item,
];

/// The trips that got a notification since the previous poll (for example "Trip confirmed" or "Your quote was
/// updated"), so the app can reload those trips instead of showing a stale status. Pure, so it is unit-tested.
Set<String> tripsWithNewNotifications(
  NotificationList previous,
  NotificationList next,
) {
  final known = {for (final item in previous.items) item.id};
  return {
    for (final item in next.items)
      if (!known.contains(item.id) && item.tripRequestId != null)
        item.tripRequestId!,
  };
}

/// Polls GET /api/notifications/mine every 30 s while the app is in the foreground (the shell starts and stops it)
/// and shows a phone notification for every new unread item, once: the ids already shown are kept in storage.
/// The state is the latest list, used by the Alerts screen and the unread badge.
@Riverpod(keepAlive: true)
class NotificationsPoller extends _$NotificationsPoller {
  Timer? _timer;
  Set<String>? _shown;

  static const _empty = NotificationList(unreadCount: 0, items: []);

  /// How many shown ids to remember (the API returns the newest 50).
  static const _rememberShown = 200;

  @override
  NotificationList build() {
    ref.onDispose(() => _timer?.cancel());
    // Stop polling as soon as the user signs out (or the session expires), and forget their list.
    ref.listen(authNotifierProvider, (_, next) {
      if (next.value == null) {
        stop();
        state = _empty;
      }
    });
    return _empty;
  }

  void start() {
    if (_timer != null) return;
    unawaited(checkNow());
    _timer = Timer.periodic(
      AppConfig.notificationPollInterval,
      (_) => checkNow(),
    );
  }

  void stop() {
    _timer?.cancel();
    _timer = null;
  }

  /// One poll. Network errors are ignored: the next poll simply tries again.
  Future<void> checkNow() async {
    final NotificationList list;
    try {
      list = await ref.read(notificationsRepositoryProvider).mine();
    } catch (_) {
      return;
    }
    if (!ref.mounted) return;
    state = list;
    await _showNew(list.items);
  }

  Future<void> _showNew(List<AppNotification> items) async {
    final storage = ref.read(sessionStorageProvider);
    final shown = _shown ??= await storage.readShownNotificationIds();
    final fresh = notificationsToShow(items, shown);
    if (fresh.isEmpty) return;

    final phone = ref.read(statusAlertsProvider);
    for (final item in fresh) {
      shown.add(item.id);
      await phone.show(item.id.hashCode & 0x7fffffff, item.title, item.body);
    }
    // Keep only the newest ids (a Set keeps insertion order) so storage does not grow for ever.
    final ids = shown.toList();
    final keep = ids.skip(max(0, ids.length - _rememberShown)).toSet();
    _shown = keep;
    await storage.writeShownNotificationIds(keep);
  }

  /// POST /api/notifications/{id}/read, then marks it read in the list straight away.
  Future<void> markRead(String id) async {
    await ref.read(notificationsRepositoryProvider).markRead(id);
    final wasUnread = state.items.any((n) => n.id == id && !n.isRead);
    state = NotificationList(
      unreadCount: wasUnread ? state.unreadCount - 1 : state.unreadCount,
      items: [
        for (final n in state.items) n.id == id ? n.copyWith(isRead: true) : n,
      ],
    );
  }

  /// POST /api/notifications/read-all.
  Future<void> markAllRead() async {
    await ref.read(notificationsRepositoryProvider).markAllRead();
    state = NotificationList(
      unreadCount: 0,
      items: [for (final n in state.items) n.copyWith(isRead: true)],
    );
  }
}
