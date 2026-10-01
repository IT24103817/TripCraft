// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'template_providers.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning
/// The mood packages for the home screen.

@ProviderFor(tripTemplates)
final tripTemplatesProvider = TripTemplatesProvider._();

/// The mood packages for the home screen.

final class TripTemplatesProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<TripTemplate>>,
          List<TripTemplate>,
          FutureOr<List<TripTemplate>>
        >
    with
        $FutureModifier<List<TripTemplate>>,
        $FutureProvider<List<TripTemplate>> {
  /// The mood packages for the home screen.
  TripTemplatesProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'tripTemplatesProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$tripTemplatesHash();

  @$internal
  @override
  $FutureProviderElement<List<TripTemplate>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<TripTemplate>> create(Ref ref) {
    return tripTemplates(ref);
  }
}

String _$tripTemplatesHash() => r'd3522d6083de70d257dd1a72aaa1d82c59bf75e7';

/// One package with its itinerary, for the package screen.

@ProviderFor(tripTemplate)
final tripTemplateProvider = TripTemplateFamily._();

/// One package with its itinerary, for the package screen.

final class TripTemplateProvider
    extends
        $FunctionalProvider<
          AsyncValue<TripTemplate>,
          TripTemplate,
          FutureOr<TripTemplate>
        >
    with $FutureModifier<TripTemplate>, $FutureProvider<TripTemplate> {
  /// One package with its itinerary, for the package screen.
  TripTemplateProvider._({
    required TripTemplateFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'tripTemplateProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$tripTemplateHash();

  @override
  String toString() {
    return r'tripTemplateProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<TripTemplate> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<TripTemplate> create(Ref ref) {
    final argument = this.argument as String;
    return tripTemplate(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is TripTemplateProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$tripTemplateHash() => r'8846ae32ee244225ee665a4d4b4d6cb8ac1ba7f7';

/// One package with its itinerary, for the package screen.

final class TripTemplateFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<TripTemplate>, String> {
  TripTemplateFamily._()
    : super(
        retry: null,
        name: r'tripTemplateProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  /// One package with its itinerary, for the package screen.

  TripTemplateProvider call(String templateId) =>
      TripTemplateProvider._(argument: templateId, from: this);

  @override
  String toString() => r'tripTemplateProvider';
}
