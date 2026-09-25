import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/auth/profile_button.dart';
import '../../../core/router/routes.dart';
import '../../../shared/utils/formatters.dart';
import '../../../shared/utils/statuses.dart';
import '../../../shared/widgets/empty_state.dart';
import '../../../shared/widgets/status_chip.dart';
import '../application/status_watcher.dart';

/// History of status changes found by the 30-second status watcher (which also fires phone notifications).
class NotificationsScreen extends ConsumerWidget {
  const NotificationsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final history = ref.watch(statusWatcherProvider);
    return Scaffold(
      appBar: AppBar(
        title: const Text('Alerts'),
        actions: const [ProfileButton()],
      ),
      body: RefreshIndicator(
        onRefresh: () => ref.read(statusWatcherProvider.notifier).checkNow(),
        child: history.isEmpty
            ? ListView(
                children: const [
                  SizedBox(height: 48),
                  EmptyState(
                    icon: Icons.notifications_none,
                    title: 'No updates yet',
                    message: 'We check your trips every 30 seconds and tell you when their status changes.',
                  ),
                ],
              )
            : ListView.separated(
                padding: const EdgeInsets.all(16),
                itemCount: history.length,
                separatorBuilder: (_, _) => const SizedBox(height: 8),
                itemBuilder: (_, i) {
                  final change = history[i];
                  return Card(
                    child: ListTile(
                      title: Text(
                        change.objective,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      subtitle: Text(
                        '${statusLabel(change.from)} → ${statusLabel(change.to)} · ${formatDateTime(change.at.toIso8601String())}',
                      ),
                      trailing: StatusChip(status: change.to),
                      onTap: () => context.push(Routes.trip(change.tripId)),
                    ),
                  );
                },
              ),
      ),
    );
  }
}
