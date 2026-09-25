// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'api_providers.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(sessionStorage)
final sessionStorageProvider = SessionStorageProvider._();

final class SessionStorageProvider
    extends $FunctionalProvider<SessionStorage, SessionStorage, SessionStorage>
    with $Provider<SessionStorage> {
  SessionStorageProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'sessionStorageProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$sessionStorageHash();

  @$internal
  @override
  $ProviderElement<SessionStorage> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  SessionStorage create(Ref ref) {
    return sessionStorage(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(SessionStorage value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<SessionStorage>(value),
    );
  }
}

String _$sessionStorageHash() => r'cdda297699f32e9b70a06b624aed9af4196dd3cc';

/// The app-wide API client. A 401 tells the auth notifier the session is over; the router then shows /login.

@ProviderFor(apiClient)
final apiClientProvider = ApiClientProvider._();

/// The app-wide API client. A 401 tells the auth notifier the session is over; the router then shows /login.

final class ApiClientProvider
    extends $FunctionalProvider<ApiClient, ApiClient, ApiClient>
    with $Provider<ApiClient> {
  /// The app-wide API client. A 401 tells the auth notifier the session is over; the router then shows /login.
  ApiClientProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'apiClientProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$apiClientHash();

  @$internal
  @override
  $ProviderElement<ApiClient> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  ApiClient create(Ref ref) {
    return apiClient(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(ApiClient value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<ApiClient>(value),
    );
  }
}

String _$apiClientHash() => r'e0df2440efbd72a34f469cb67d9387d10e270590';
