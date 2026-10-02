import 'package:freezed_annotation/freezed_annotation.dart';

part 'notification_models.freezed.dart';
part 'notification_models.g.dart';

/// One in-app notification (NotificationDto). [type] is one of QuotationSent, QuotationUpdated, ClientAccepted,
/// ClientDeclined, NeedsOperator, TripConfirmed, TripAssigned, TripCancelled, GuideChangeRequested, GuideReplaced
/// or GuideChanged (docs/API-V11.md); the app treats every type the same way and never fails on a new one.
@freezed
abstract class AppNotification with _$AppNotification {
  const factory AppNotification({
    required String id,
    required String type,
    required String title,
    required String body,
    String? tripRequestId,
    required bool isRead,
    required String createdAt,
  }) = _AppNotification;

  factory AppNotification.fromJson(Map<String, dynamic> json) =>
      _$AppNotificationFromJson(json);
}

/// GET /api/notifications/mine (NotificationListDto): the newest 50 and how many are unread in total.
@freezed
abstract class NotificationList with _$NotificationList {
  const factory NotificationList({
    required int unreadCount,
    required List<AppNotification> items,
  }) = _NotificationList;

  factory NotificationList.fromJson(Map<String, dynamic> json) =>
      _$NotificationListFromJson(json);
}
