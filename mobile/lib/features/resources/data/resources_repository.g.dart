// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'resources_repository.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(resourcesRepository)
final resourcesRepositoryProvider = ResourcesRepositoryProvider._();

final class ResourcesRepositoryProvider
    extends
        $FunctionalProvider<
          ResourcesRepository,
          ResourcesRepository,
          ResourcesRepository
        >
    with $Provider<ResourcesRepository> {
  ResourcesRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'resourcesRepositoryProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$resourcesRepositoryHash();

  @$internal
  @override
  $ProviderElement<ResourcesRepository> $createElement(
    $ProviderPointer pointer,
  ) => $ProviderElement(pointer);

  @override
  ResourcesRepository create(Ref ref) {
    return resourcesRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(ResourcesRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<ResourcesRepository>(value),
    );
  }
}

String _$resourcesRepositoryHash() =>
    r'c8f9b1ecd63d919651e999468fe20c5fae30a7de';

@ProviderFor(mySchedule)
final myScheduleProvider = MyScheduleProvider._();

final class MyScheduleProvider
    extends
        $FunctionalProvider<
          AsyncValue<GuideSchedule>,
          GuideSchedule,
          FutureOr<GuideSchedule>
        >
    with $FutureModifier<GuideSchedule>, $FutureProvider<GuideSchedule> {
  MyScheduleProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'myScheduleProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$myScheduleHash();

  @$internal
  @override
  $FutureProviderElement<GuideSchedule> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<GuideSchedule> create(Ref ref) {
    return mySchedule(ref);
  }
}

String _$myScheduleHash() => r'4c58d49facd36cd9d76606b2fdd511ebd31504bc';
