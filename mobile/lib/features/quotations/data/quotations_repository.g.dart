// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'quotations_repository.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(quotationsRepository)
final quotationsRepositoryProvider = QuotationsRepositoryProvider._();

final class QuotationsRepositoryProvider
    extends
        $FunctionalProvider<
          QuotationsRepository,
          QuotationsRepository,
          QuotationsRepository
        >
    with $Provider<QuotationsRepository> {
  QuotationsRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'quotationsRepositoryProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$quotationsRepositoryHash();

  @$internal
  @override
  $ProviderElement<QuotationsRepository> $createElement(
    $ProviderPointer pointer,
  ) => $ProviderElement(pointer);

  @override
  QuotationsRepository create(Ref ref) {
    return quotationsRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(QuotationsRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<QuotationsRepository>(value),
    );
  }
}

String _$quotationsRepositoryHash() =>
    r'28a33882042c03eca6ae870e3c76d3b078666daa';
