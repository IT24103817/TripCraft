// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'trip_templates_repository.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(tripTemplatesRepository)
final tripTemplatesRepositoryProvider = TripTemplatesRepositoryProvider._();

final class TripTemplatesRepositoryProvider
    extends
        $FunctionalProvider<
          TripTemplatesRepository,
          TripTemplatesRepository,
          TripTemplatesRepository
        >
    with $Provider<TripTemplatesRepository> {
  TripTemplatesRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'tripTemplatesRepositoryProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$tripTemplatesRepositoryHash();

  @$internal
  @override
  $ProviderElement<TripTemplatesRepository> $createElement(
    $ProviderPointer pointer,
  ) => $ProviderElement(pointer);

  @override
  TripTemplatesRepository create(Ref ref) {
    return tripTemplatesRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(TripTemplatesRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<TripTemplatesRepository>(value),
    );
  }
}

String _$tripTemplatesRepositoryHash() =>
    r'4a9bb5561c41a27d9d8025a68117676a6bd7b3db';
