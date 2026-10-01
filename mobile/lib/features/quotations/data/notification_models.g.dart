// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'notification_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

_AppNotification _$AppNotificationFromJson(Map<String, dynamic> json) =>
    _AppNotification(
      id: json['id'] as String,
      type: json['type'] as String,
      title: json['title'] as String,
      body: json['body'] as String,
      tripRequestId: json['tripRequestId'] as String?,
      isRead: json['isRead'] as bool,
      createdAt: json['createdAt'] as String,
    );

Map<String, dynamic> _$AppNotificationToJson(_AppNotification instance) =>
    <String, dynamic>{
      'id': instance.id,
      'type': instance.type,
      'title': instance.title,
      'body': instance.body,
      'tripRequestId': instance.tripRequestId,
      'isRead': instance.isRead,
      'createdAt': instance.createdAt,
    };

_NotificationList _$NotificationListFromJson(Map<String, dynamic> json) =>
    _NotificationList(
      unreadCount: (json['unreadCount'] as num).toInt(),
      items: (json['items'] as List<dynamic>)
          .map((e) => AppNotification.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$NotificationListToJson(_NotificationList instance) =>
    <String, dynamic>{
      'unreadCount': instance.unreadCount,
      'items': instance.items,
    };
