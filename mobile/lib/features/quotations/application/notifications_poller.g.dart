// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'notifications_poller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning
/// Polls GET /api/notifications/mine every 30 s while the app is in the foreground (the shell starts and stops it)
/// and shows a phone notification for every new unread item, once: the ids already shown are kept in storage.
/// The state is the latest list, used by the Alerts screen and the unread badge.

@ProviderFor(NotificationsPoller)
final notificationsPollerProvider = NotificationsPollerProvider._();

/// Polls GET /api/notifications/mine every 30 s while the app is in the foreground (the shell starts and stops it)
/// and shows a phone notification for every new unread item, once: the ids already shown are kept in storage.
/// The state is the latest list, used by the Alerts screen and the unread badge.
final class NotificationsPollerProvider
    extends $NotifierProvider<NotificationsPoller, NotificationList> {
  /// Polls GET /api/notifications/mine every 30 s while the app is in the foreground (the shell starts and stops it)
  /// and shows a phone notification for every new unread item, once: the ids already shown are kept in storage.
  /// The state is the latest list, used by the Alerts screen and the unread badge.
  NotificationsPollerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'notificationsPollerProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$notificationsPollerHash();

  @$internal
  @override
  NotificationsPoller create() => NotificationsPoller();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(NotificationList value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<NotificationList>(value),
    );
  }
}

String _$notificationsPollerHash() =>
    r'13a26d28602d2dcc574f2c462c9e68c1cc9ffcc0';

/// Polls GET /api/notifications/mine every 30 s while the app is in the foreground (the shell starts and stops it)
/// and shows a phone notification for every new unread item, once: the ids already shown are kept in storage.
/// The state is the latest list, used by the Alerts screen and the unread badge.

abstract class _$NotificationsPoller extends $Notifier<NotificationList> {
  NotificationList build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<NotificationList, NotificationList>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<NotificationList, NotificationList>,
              NotificationList,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
