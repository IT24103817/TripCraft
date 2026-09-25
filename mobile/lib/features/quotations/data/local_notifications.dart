import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

part 'local_notifications.g.dart';

/// Shows a phone notification. An interface so tests can check what would have been shown.
abstract class StatusNotifier {
  Future<void> show(int id, String title, String body);
}

/// flutter_local_notifications on Android. Initialised on first use; asks for POST_NOTIFICATIONS on Android 13+.
class LocalStatusNotifier implements StatusNotifier {
  final _plugin = FlutterLocalNotificationsPlugin();
  bool _ready = false;

  static const _details = NotificationDetails(
    android: AndroidNotificationDetails(
      'trip_status',
      'Trip status updates',
      channelDescription: 'Tells you when your trip request changes status',
      importance: Importance.high,
      priority: Priority.high,
    ),
  );

  Future<void> _init() async {
    if (_ready) return;
    await _plugin.initialize(
      settings: const InitializationSettings(
        android: AndroidInitializationSettings('@mipmap/ic_launcher'),
      ),
    );
    await _plugin
        .resolvePlatformSpecificImplementation<
          AndroidFlutterLocalNotificationsPlugin
        >()
        ?.requestNotificationsPermission();
    _ready = true;
  }

  @override
  Future<void> show(int id, String title, String body) async {
    await _init();
    await _plugin.show(
      id: id,
      title: title,
      body: body,
      notificationDetails: _details,
    );
  }
}

@Riverpod(keepAlive: true)
StatusNotifier statusAlerts(Ref ref) => LocalStatusNotifier();
