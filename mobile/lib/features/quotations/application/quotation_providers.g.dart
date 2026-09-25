// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'quotation_providers.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(quotationView)
final quotationViewProvider = QuotationViewFamily._();

final class QuotationViewProvider
    extends
        $FunctionalProvider<
          AsyncValue<QuotationView>,
          QuotationView,
          FutureOr<QuotationView>
        >
    with $FutureModifier<QuotationView>, $FutureProvider<QuotationView> {
  QuotationViewProvider._({
    required QuotationViewFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'quotationViewProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$quotationViewHash();

  @override
  String toString() {
    return r'quotationViewProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<QuotationView> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<QuotationView> create(Ref ref) {
    final argument = this.argument as String;
    return quotationView(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is QuotationViewProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$quotationViewHash() => r'b05cd2979b7b17185dbb52e1cd0ea7caec15adb7';

final class QuotationViewFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<QuotationView>, String> {
  QuotationViewFamily._()
    : super(
        retry: null,
        name: r'quotationViewProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  QuotationViewProvider call(String tripId) =>
      QuotationViewProvider._(argument: tripId, from: this);

  @override
  String toString() => r'quotationViewProvider';
}
