# ADR-002: Flutter state management

- **Status:** Accepted
- **Date:** 2026-09-26
- **Author:** Student A

## Context

The mobile app (`mobile/`) serves Tourists and Guides. It needs a global auth state that the router listens to,
API-backed screens with loading/error/empty states and pull-to-refresh, a workflow status that polls every 10 s
while Planning, a background status watcher, and tests that replace the API with a mock. Four days, one Flutter
owner per feature folder, and the code must be explainable line by line.

## Options considered

| Option | Pros | Cons |
|--------|------|------|
| **Provider** | Simple, taught widely, small API. | Depends on the widget tree and `BuildContext`; overriding a dependency in tests is clumsy. No built-in async state (loading/error). |
| **Bloc** | Strict events → states, very testable, popular in industry. | An event class, state class and bloc per screen: a lot of ceremony for a four-day build. |
| **Riverpod** | Providers are global and compile-safe; `AsyncValue` gives loading/error/data; `ProviderScope(overrides: …)` swaps the API client in tests; `family` and `autoDispose` fit per-trip data and polling. | Code generation (`riverpod_generator`) adds `build_runner` and generated files. Riverpod 3 changed some names (e.g. auto-retry by default), so we had to read the current docs. |

## Decision

**Riverpod 3** with `riverpod_annotation` code generation, plus `freezed`/`json_serializable` models.

## Consequences

- Every screen maps `AsyncValue` to the four states through one `AsyncView` widget.
- Tests override `apiClientProvider` with a mocktail `MockApiClient` and storage with `InMemorySessionStorage`,
  and run at 360×640 and 412×915.
- Riverpod 3 retries failing providers automatically; we turn that off in `ProviderScope(retry: …)` because
  every screen has its own Retry button.
- The generated provider name drops a `Notifier` suffix, so the auth provider is named explicitly
  (`@Riverpod(name: 'authNotifierProvider')`).
- Generated `*.g.dart` / `*.freezed.dart` files are committed, so CI does not need `build_runner`.

## Where this shows in the code

- `mobile/lib/main.dart` — `ProviderScope` with retry disabled
- `mobile/lib/core/auth/auth_notifier.dart` — `authNotifierProvider` (router refresh source)
- `mobile/lib/core/router/app_router.dart` — `GoRouter` provider listening to auth
- `mobile/lib/features/trips/application/trips_providers.dart` — `myTrips`, `tripDetail`, the 10 s `tripWorkflow` stream
- `mobile/lib/shared/widgets/async_view.dart` — the four states
- `mobile/test/helpers.dart` — provider overrides with `MockApiClient`
