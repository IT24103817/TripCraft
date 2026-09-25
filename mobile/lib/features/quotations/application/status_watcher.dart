import 'dart:async';

import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/auth/auth_notifier.dart';
import '../../../core/config.dart';
import '../../../shared/utils/statuses.dart';
import '../data/local_notifications.dart';
import '../data/quotation_models.dart';
import '../data/quotations_repository.dart';

part 'status_watcher.g.dart';

/// Status changes between two polls. Pure, so it is unit-tested directly.
/// Trips seen for the first time are not reported (they did not "change").
List<StatusChange> diffStatuses(
  Map<String, String> previous,
  List<TripStatusItem> current,
  DateTime now,
) => [
  for (final trip in current)
    if (previous[trip.id] case final before? when before != trip.status)
      StatusChange(
        tripId: trip.id,
        objective: trip.objective,
        from: before,
        to: trip.status,
        at: now,
      ),
];

/// Polls the tourist's trips every 30 s, fires a phone notification for every status change and
/// keeps the history (newest first) for the Alerts screen. The first poll only records the baseline.
@Riverpod(keepAlive: true)
class StatusWatcher extends _$StatusWatcher {
  Timer? _timer;
  Map<String, String>? _lastStatuses;

  @override
  List<StatusChange> build() {
    ref.onDispose(() => _timer?.cancel());
    // Stop polling as soon as the tourist signs out (or the session expires).
    ref.listen(authNotifierProvider, (_, next) {
      if (next.value == null) stop();
    });
    return const [];
  }

  void start() {
    if (_timer != null) return;
    unawaited(checkNow());
    _timer = Timer.periodic(AppConfig.statusPollInterval, (_) => checkNow());
  }

  void stop() {
    _timer?.cancel();
    _timer = null;
    _lastStatuses = null;
  }

  /// One poll. Network errors are ignored: the next poll simply tries again.
  Future<void> checkNow() async {
    final List<TripStatusItem> trips;
    try {
      trips = await ref.read(quotationsRepositoryProvider).tripStatuses();
    } catch (_) {
      return;
    }
    if (!ref.mounted) return;

    final previous = _lastStatuses;
    _lastStatuses = {for (final t in trips) t.id: t.status};
    if (previous == null) return;

    final changes = diffStatuses(previous, trips, DateTime.now());
    if (changes.isEmpty) return;
    state = [...changes.reversed, ...state];
    final notifier = ref.read(statusAlertsProvider);
    for (final change in changes) {
      await notifier.show(
        change.tripId.hashCode & 0x7fffffff,
        'Trip ${statusLabel(change.to).toLowerCase()}',
        change.objective,
      );
    }
  }
}
