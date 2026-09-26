// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'trips_providers.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(myTrips)
final myTripsProvider = MyTripsFamily._();

final class MyTripsProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<TripRequest>>,
          List<TripRequest>,
          FutureOr<List<TripRequest>>
        >
    with
        $FutureModifier<List<TripRequest>>,
        $FutureProvider<List<TripRequest>> {
  MyTripsProvider._({
    required MyTripsFamily super.from,
    required ({String search, String status}) super.argument,
  }) : super(
         retry: null,
         name: r'myTripsProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$myTripsHash();

  @override
  String toString() {
    return r'myTripsProvider'
        ''
        '$argument';
  }

  @$internal
  @override
  $FutureProviderElement<List<TripRequest>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<TripRequest>> create(Ref ref) {
    final argument = this.argument as ({String search, String status});
    return myTrips(ref, search: argument.search, status: argument.status);
  }

  @override
  bool operator ==(Object other) {
    return other is MyTripsProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$myTripsHash() => r'a683591f224f27d8ba959c4eb1b572fef2017969';

final class MyTripsFamily extends $Family
    with
        $FunctionalFamilyOverride<
          FutureOr<List<TripRequest>>,
          ({String search, String status})
        > {
  MyTripsFamily._()
    : super(
        retry: null,
        name: r'myTripsProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  MyTripsProvider call({String search = '', String status = ''}) =>
      MyTripsProvider._(argument: (search: search, status: status), from: this);

  @override
  String toString() => r'myTripsProvider';
}

@ProviderFor(tripDetail)
final tripDetailProvider = TripDetailFamily._();

final class TripDetailProvider
    extends
        $FunctionalProvider<
          AsyncValue<TripRequest>,
          TripRequest,
          FutureOr<TripRequest>
        >
    with $FutureModifier<TripRequest>, $FutureProvider<TripRequest> {
  TripDetailProvider._({
    required TripDetailFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'tripDetailProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$tripDetailHash();

  @override
  String toString() {
    return r'tripDetailProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<TripRequest> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<TripRequest> create(Ref ref) {
    final argument = this.argument as String;
    return tripDetail(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is TripDetailProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$tripDetailHash() => r'3e576b602cdafc77ab64080a3f30506c9368e39e';

final class TripDetailFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<TripRequest>, String> {
  TripDetailFamily._()
    : super(
        retry: null,
        name: r'tripDetailProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  TripDetailProvider call(String tripId) =>
      TripDetailProvider._(argument: tripId, from: this);

  @override
  String toString() => r'tripDetailProvider';
}

/// The trip's workflow, re-fetched every 10 s while the agents are Planning; stops once it leaves Planning.
/// Leaving the screen disposes the provider, which cancels the loop.

@ProviderFor(tripWorkflow)
final tripWorkflowProvider = TripWorkflowFamily._();

/// The trip's workflow, re-fetched every 10 s while the agents are Planning; stops once it leaves Planning.
/// Leaving the screen disposes the provider, which cancels the loop.

final class TripWorkflowProvider
    extends
        $FunctionalProvider<
          AsyncValue<TripWorkflow?>,
          TripWorkflow?,
          Stream<TripWorkflow?>
        >
    with $FutureModifier<TripWorkflow?>, $StreamProvider<TripWorkflow?> {
  /// The trip's workflow, re-fetched every 10 s while the agents are Planning; stops once it leaves Planning.
  /// Leaving the screen disposes the provider, which cancels the loop.
  TripWorkflowProvider._({
    required TripWorkflowFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'tripWorkflowProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$tripWorkflowHash();

  @override
  String toString() {
    return r'tripWorkflowProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $StreamProviderElement<TripWorkflow?> $createElement(
    $ProviderPointer pointer,
  ) => $StreamProviderElement(pointer);

  @override
  Stream<TripWorkflow?> create(Ref ref) {
    final argument = this.argument as String;
    return tripWorkflow(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is TripWorkflowProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$tripWorkflowHash() => r'b761c9683dec71780935319ccafeb0fe8f98e604';

/// The trip's workflow, re-fetched every 10 s while the agents are Planning; stops once it leaves Planning.
/// Leaving the screen disposes the provider, which cancels the loop.

final class TripWorkflowFamily extends $Family
    with $FunctionalFamilyOverride<Stream<TripWorkflow?>, String> {
  TripWorkflowFamily._()
    : super(
        retry: null,
        name: r'tripWorkflowProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  /// The trip's workflow, re-fetched every 10 s while the agents are Planning; stops once it leaves Planning.
  /// Leaving the screen disposes the provider, which cancels the loop.

  TripWorkflowProvider call(String tripId) =>
      TripWorkflowProvider._(argument: tripId, from: this);

  @override
  String toString() => r'tripWorkflowProvider';
}

@ProviderFor(tripHistory)
final tripHistoryProvider = TripHistoryFamily._();

final class TripHistoryProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<TripHistoryEntry>>,
          List<TripHistoryEntry>,
          FutureOr<List<TripHistoryEntry>>
        >
    with
        $FutureModifier<List<TripHistoryEntry>>,
        $FutureProvider<List<TripHistoryEntry>> {
  TripHistoryProvider._({
    required TripHistoryFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'tripHistoryProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$tripHistoryHash();

  @override
  String toString() {
    return r'tripHistoryProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<List<TripHistoryEntry>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<TripHistoryEntry>> create(Ref ref) {
    final argument = this.argument as String;
    return tripHistory(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is TripHistoryProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$tripHistoryHash() => r'05defb0462e64f3e85c9993629c1a61deae040b2';

final class TripHistoryFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<List<TripHistoryEntry>>, String> {
  TripHistoryFamily._()
    : super(
        retry: null,
        name: r'tripHistoryProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  TripHistoryProvider call(String tripId) =>
      TripHistoryProvider._(argument: tripId, from: this);

  @override
  String toString() => r'tripHistoryProvider';
}

@ProviderFor(savedItinerary)
final savedItineraryProvider = SavedItineraryFamily._();

final class SavedItineraryProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<TripDay>>,
          List<TripDay>,
          FutureOr<List<TripDay>>
        >
    with $FutureModifier<List<TripDay>>, $FutureProvider<List<TripDay>> {
  SavedItineraryProvider._({
    required SavedItineraryFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'savedItineraryProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$savedItineraryHash();

  @override
  String toString() {
    return r'savedItineraryProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<List<TripDay>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<TripDay>> create(Ref ref) {
    final argument = this.argument as String;
    return savedItinerary(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is SavedItineraryProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$savedItineraryHash() => r'be0caa365164ea1abfba0f277bcb89f4abc4cb57';

final class SavedItineraryFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<List<TripDay>>, String> {
  SavedItineraryFamily._()
    : super(
        retry: null,
        name: r'savedItineraryProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  SavedItineraryProvider call(String tripId) =>
      SavedItineraryProvider._(argument: tripId, from: this);

  @override
  String toString() => r'savedItineraryProvider';
}

/// Coordinates of the stops for the map. [attractionIds] is a comma-separated list (a String keeps
/// the provider key stable; a List would be a new key on every build).

@ProviderFor(stopLocations)
final stopLocationsProvider = StopLocationsFamily._();

/// Coordinates of the stops for the map. [attractionIds] is a comma-separated list (a String keeps
/// the provider key stable; a List would be a new key on every build).

final class StopLocationsProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<Attraction>>,
          List<Attraction>,
          FutureOr<List<Attraction>>
        >
    with $FutureModifier<List<Attraction>>, $FutureProvider<List<Attraction>> {
  /// Coordinates of the stops for the map. [attractionIds] is a comma-separated list (a String keeps
  /// the provider key stable; a List would be a new key on every build).
  StopLocationsProvider._({
    required StopLocationsFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'stopLocationsProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$stopLocationsHash();

  @override
  String toString() {
    return r'stopLocationsProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<List<Attraction>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<Attraction>> create(Ref ref) {
    final argument = this.argument as String;
    return stopLocations(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is StopLocationsProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$stopLocationsHash() => r'06f8423cc0612b784f5a05c843132c8bed400471';

/// Coordinates of the stops for the map. [attractionIds] is a comma-separated list (a String keeps
/// the provider key stable; a List would be a new key on every build).

final class StopLocationsFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<List<Attraction>>, String> {
  StopLocationsFamily._()
    : super(
        retry: null,
        name: r'stopLocationsProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  /// Coordinates of the stops for the map. [attractionIds] is a comma-separated list (a String keeps
  /// the provider key stable; a List would be a new key on every build).

  StopLocationsProvider call(String attractionIds) =>
      StopLocationsProvider._(argument: attractionIds, from: this);

  @override
  String toString() => r'stopLocationsProvider';
}
