// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'pdf_sharer.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(pdfSharer)
final pdfSharerProvider = PdfSharerProvider._();

final class PdfSharerProvider
    extends $FunctionalProvider<PdfSharer, PdfSharer, PdfSharer>
    with $Provider<PdfSharer> {
  PdfSharerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'pdfSharerProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$pdfSharerHash();

  @$internal
  @override
  $ProviderElement<PdfSharer> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  PdfSharer create(Ref ref) {
    return pdfSharer(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(PdfSharer value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<PdfSharer>(value),
    );
  }
}

String _$pdfSharerHash() => r'c2e0bf7bd88fc9055fe3b5355d9a00570ec6c14b';
