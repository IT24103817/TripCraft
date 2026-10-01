import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/api/api_providers.dart';
import 'notification_models.dart';

part 'notifications_repository.g.dart';

/// The signed-in user's in-app notifications (tourists and guides alike).
class NotificationsRepository {
  NotificationsRepository(this._api);

  final ApiClient _api;

  /// GET /api/notifications/mine — the newest 50 and the unread count.
  Future<NotificationList> mine() async => NotificationList.fromJson(
    await _api.get('/api/notifications/mine') as Map<String, dynamic>,
  );

  /// POST /api/notifications/{id}/read (204).
  Future<void> markRead(String id) => _api.post('/api/notifications/$id/read');

  /// POST /api/notifications/read-all (204).
  Future<void> markAllRead() => _api.post('/api/notifications/read-all');
}

@riverpod
NotificationsRepository notificationsRepository(Ref ref) =>
    NotificationsRepository(ref.watch(apiClientProvider));
