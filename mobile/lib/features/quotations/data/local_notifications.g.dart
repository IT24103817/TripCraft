// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'local_notifications.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(statusAlerts)
final statusAlertsProvider = StatusAlertsProvider._();

final class StatusAlertsProvider
    extends $FunctionalProvider<StatusNotifier, StatusNotifier, StatusNotifier>
    with $Provider<StatusNotifier> {
  StatusAlertsProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'statusAlertsProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$statusAlertsHash();

  @$internal
  @override
  $ProviderElement<StatusNotifier> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  StatusNotifier create(Ref ref) {
    return statusAlerts(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(StatusNotifier value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<StatusNotifier>(value),
    );
  }
}

String _$statusAlertsHash() => r'e8fb4a03635f8bd04306cd753a4db70f5eae6578';
