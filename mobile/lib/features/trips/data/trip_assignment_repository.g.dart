// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'trip_assignment_repository.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(tripAssignmentRepository)
final tripAssignmentRepositoryProvider = TripAssignmentRepositoryProvider._();

final class TripAssignmentRepositoryProvider
    extends
        $FunctionalProvider<
          TripAssignmentRepository,
          TripAssignmentRepository,
          TripAssignmentRepository
        >
    with $Provider<TripAssignmentRepository> {
  TripAssignmentRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'tripAssignmentRepositoryProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$tripAssignmentRepositoryHash();

  @$internal
  @override
  $ProviderElement<TripAssignmentRepository> $createElement(
    $ProviderPointer pointer,
  ) => $ProviderElement(pointer);

  @override
  TripAssignmentRepository create(Ref ref) {
    return tripAssignmentRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(TripAssignmentRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<TripAssignmentRepository>(value),
    );
  }
}

String _$tripAssignmentRepositoryHash() =>
    r'6813f384cc5efca8aaee004311d0d364b05d0f0f';
