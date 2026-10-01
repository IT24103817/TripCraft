// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'assignment_providers.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning
/// The guide and vehicle booked for a trip.

@ProviderFor(tripAssignment)
final tripAssignmentProvider = TripAssignmentFamily._();

/// The guide and vehicle booked for a trip.

final class TripAssignmentProvider
    extends
        $FunctionalProvider<
          AsyncValue<TripAssignment>,
          TripAssignment,
          FutureOr<TripAssignment>
        >
    with $FutureModifier<TripAssignment>, $FutureProvider<TripAssignment> {
  /// The guide and vehicle booked for a trip.
  TripAssignmentProvider._({
    required TripAssignmentFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'tripAssignmentProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$tripAssignmentHash();

  @override
  String toString() {
    return r'tripAssignmentProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<TripAssignment> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<TripAssignment> create(Ref ref) {
    final argument = this.argument as String;
    return tripAssignment(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is TripAssignmentProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$tripAssignmentHash() => r'47c99ee3dc460daf802e62afe9691eaa4cbef26a';

/// The guide and vehicle booked for a trip.

final class TripAssignmentFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<TripAssignment>, String> {
  TripAssignmentFamily._()
    : super(
        retry: null,
        name: r'tripAssignmentProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  /// The guide and vehicle booked for a trip.

  TripAssignmentProvider call(String tripId) =>
      TripAssignmentProvider._(argument: tripId, from: this);

  @override
  String toString() => r'tripAssignmentProvider';
}

/// The tourist's rating of the guide; null until they have rated.

@ProviderFor(guideRating)
final guideRatingProvider = GuideRatingFamily._();

/// The tourist's rating of the guide; null until they have rated.

final class GuideRatingProvider
    extends
        $FunctionalProvider<
          AsyncValue<GuideRating?>,
          GuideRating?,
          FutureOr<GuideRating?>
        >
    with $FutureModifier<GuideRating?>, $FutureProvider<GuideRating?> {
  /// The tourist's rating of the guide; null until they have rated.
  GuideRatingProvider._({
    required GuideRatingFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'guideRatingProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$guideRatingHash();

  @override
  String toString() {
    return r'guideRatingProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<GuideRating?> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<GuideRating?> create(Ref ref) {
    final argument = this.argument as String;
    return guideRating(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is GuideRatingProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$guideRatingHash() => r'2e594b92f66f25ae9f9cd1846e0a11edbb451962';

/// The tourist's rating of the guide; null until they have rated.

final class GuideRatingFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<GuideRating?>, String> {
  GuideRatingFamily._()
    : super(
        retry: null,
        name: r'guideRatingProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  /// The tourist's rating of the guide; null until they have rated.

  GuideRatingProvider call(String tripId) =>
      GuideRatingProvider._(argument: tripId, from: this);

  @override
  String toString() => r'guideRatingProvider';
}
