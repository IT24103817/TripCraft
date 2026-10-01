import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/auth/auth_notifier.dart';
import '../../../core/auth/profile_button.dart';
import '../../../core/router/routes.dart';
import '../../../shared/theme/app_theme.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/friendly_error.dart';
import '../../../shared/widgets/empty_state.dart';
import '../application/notifications_poller.dart';
import '../data/notification_models.dart';

/// The user's notifications from GET /api/notifications/mine (kept fresh by the 30-second poller, which also
/// shows phone notifications). Tap one to mark it read; tourists then open the trip it is about.
class NotificationsScreen extends ConsumerWidget {
  const NotificationsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final list = ref.watch(notificationsPollerProvider);
    final poller = ref.read(notificationsPollerProvider.notifier);
    final isTourist = ref.watch(authNotifierProvider).value?.role == 'Tourist';

    Future<void> run(Future<void> Function() action) async {
      final messenger = ScaffoldMessenger.of(context);
      try {
        await action();
      } catch (error) {
        messenger.showSnackBar(SnackBar(content: Text(friendlyMessage(error))));
      }
    }

    Future<void> open(AppNotification item) async {
      if (!item.isRead) await run(() => poller.markRead(item.id));
      final tripId = item.tripRequestId;
      if (isTourist && tripId != null && context.mounted) {
        context.push(Routes.trip(tripId));
      }
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('Alerts'),
        actions: [
          if (list.unreadCount > 0)
            TextButton(
              onPressed: () => run(poller.markAllRead),
              child: const Text('Mark all read'),
            ),
          const ProfileButton(),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: poller.checkNow,
        child: list.items.isEmpty
            ? ListView(
                children: const [
                  SizedBox(height: 48),
                  EmptyState(
                    icon: Icons.notifications_none,
                    title: 'No notifications yet',
                    message: 'We check every 30 seconds and tell you when something changes on your trips.',
                  ),
                ],
              )
            : ListView.separated(
                padding: const EdgeInsets.all(16),
                itemCount: list.items.length,
                separatorBuilder: (_, _) => const SizedBox(height: 8),
                itemBuilder: (_, i) => _NotificationTile(
                  item: list.items[i],
                  onTap: () => open(list.items[i]),
                ),
              ),
      ),
    );
  }
}

class _NotificationTile extends StatelessWidget {
  const _NotificationTile({required this.item, required this.onTap});

  final AppNotification item;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ListTile(
        leading: Icon(
          item.isRead ? Icons.notifications_none : Icons.notifications_active,
          color: item.isRead ? AppColors.muted : AppColors.brand,
        ),
        title: Text(
          item.title,
          style: TextStyle(
            fontWeight: item.isRead ? FontWeight.w400 : FontWeight.w700,
          ),
        ),
        subtitle: Text('${item.body}\n${formatDateTime(item.createdAt)}'),
        isThreeLine: true,
        trailing: item.isRead
            ? null
            : Semantics(
                label: 'Unread',
                child: Container(
                  key: ValueKey('unread-${item.id}'),
                  width: 10,
                  height: 10,
                  decoration: const BoxDecoration(
                    color: AppColors.brand,
                    shape: BoxShape.circle,
                  ),
                ),
              ),
        onTap: onTap,
      ),
    );
  }
}
