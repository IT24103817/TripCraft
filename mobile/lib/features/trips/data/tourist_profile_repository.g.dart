// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'tourist_profile_repository.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(touristProfileRepository)
final touristProfileRepositoryProvider = TouristProfileRepositoryProvider._();

final class TouristProfileRepositoryProvider
    extends
        $FunctionalProvider<
          TouristProfileRepository,
          TouristProfileRepository,
          TouristProfileRepository
        >
    with $Provider<TouristProfileRepository> {
  TouristProfileRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'touristProfileRepositoryProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$touristProfileRepositoryHash();

  @$internal
  @override
  $ProviderElement<TouristProfileRepository> $createElement(
    $ProviderPointer pointer,
  ) => $ProviderElement(pointer);

  @override
  TouristProfileRepository create(Ref ref) {
    return touristProfileRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(TouristProfileRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<TouristProfileRepository>(value),
    );
  }
}

String _$touristProfileRepositoryHash() =>
    r'25f75ba11db6bb35759e8ad44785801585f3a16f';
