// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'status_watcher.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning
/// Polls the tourist's trips every 30 s, fires a phone notification for every status change and
/// keeps the history (newest first) for the Alerts screen. The first poll only records the baseline.

@ProviderFor(StatusWatcher)
final statusWatcherProvider = StatusWatcherProvider._();

/// Polls the tourist's trips every 30 s, fires a phone notification for every status change and
/// keeps the history (newest first) for the Alerts screen. The first poll only records the baseline.
final class StatusWatcherProvider
    extends $NotifierProvider<StatusWatcher, List<StatusChange>> {
  /// Polls the tourist's trips every 30 s, fires a phone notification for every status change and
  /// keeps the history (newest first) for the Alerts screen. The first poll only records the baseline.
  StatusWatcherProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'statusWatcherProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$statusWatcherHash();

  @$internal
  @override
  StatusWatcher create() => StatusWatcher();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(List<StatusChange> value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<List<StatusChange>>(value),
    );
  }
}

String _$statusWatcherHash() => r'aaf61a96fb0a436d0c827a86391bf2ebc2aff85b';

/// Polls the tourist's trips every 30 s, fires a phone notification for every status change and
/// keeps the history (newest first) for the Alerts screen. The first poll only records the baseline.

abstract class _$StatusWatcher extends $Notifier<List<StatusChange>> {
  List<StatusChange> build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<List<StatusChange>, List<StatusChange>>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<List<StatusChange>, List<StatusChange>>,
              List<StatusChange>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
