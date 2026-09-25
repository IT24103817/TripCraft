// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'trips_repository.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(tripsRepository)
final tripsRepositoryProvider = TripsRepositoryProvider._();

final class TripsRepositoryProvider
    extends
        $FunctionalProvider<TripsRepository, TripsRepository, TripsRepository>
    with $Provider<TripsRepository> {
  TripsRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'tripsRepositoryProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$tripsRepositoryHash();

  @$internal
  @override
  $ProviderElement<TripsRepository> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  TripsRepository create(Ref ref) {
    return tripsRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(TripsRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<TripsRepository>(value),
    );
  }
}

String _$tripsRepositoryHash() => r'1bbf0e61fdf96b5e3dae47a69656493fe8b496fb';
