// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'auth_notifier.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning
/// Who is signed in. AsyncLoading while the saved session is read at start-up;
/// data(null) = signed out; data(user) = signed in. The router listens to this.

@ProviderFor(AuthNotifier)
final authNotifierProvider = AuthNotifierProvider._();

/// Who is signed in. AsyncLoading while the saved session is read at start-up;
/// data(null) = signed out; data(user) = signed in. The router listens to this.
final class AuthNotifierProvider
    extends $AsyncNotifierProvider<AuthNotifier, AppUser?> {
  /// Who is signed in. AsyncLoading while the saved session is read at start-up;
  /// data(null) = signed out; data(user) = signed in. The router listens to this.
  AuthNotifierProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'authNotifierProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$authNotifierHash();

  @$internal
  @override
  AuthNotifier create() => AuthNotifier();
}

String _$authNotifierHash() => r'f4d450941ef125977e1846cafc82bc073f3d2402';

/// Who is signed in. AsyncLoading while the saved session is read at start-up;
/// data(null) = signed out; data(user) = signed in. The router listens to this.

abstract class _$AuthNotifier extends $AsyncNotifier<AppUser?> {
  FutureOr<AppUser?> build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<AsyncValue<AppUser?>, AppUser?>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<AppUser?>, AppUser?>,
              AsyncValue<AppUser?>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
