# mobile — TripCraft app for Tourists and Guides

Flutter 3 (Riverpod 3, go_router, dio, freezed) Android app, id **`lk.tripcraft.app`**. It talks only to the
ASP.NET Core API. Operations Managers and Admins who sign in are sent to a "use the web dashboard" screen.

## Setup

```bash
brew install --cask flutter android-commandlinetools && brew install openjdk@17
export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home
export ANDROID_HOME=/opt/homebrew/share/android-commandlinetools
sdkmanager "platform-tools" "platforms;android-36" "build-tools;36.0.0" "emulator" \
           "system-images;android-35;google_apis;arm64-v8a"
flutter config --android-sdk "$ANDROID_HOME" --jdk-dir "$JAVA_HOME"
flutter doctor                        # Android toolchain must be green (Xcode only for iOS: docs/RUN-ON-IPHONE.md)

cd mobile
flutter pub get
dart run build_runner build -d        # freezed / json_serializable / riverpod_generator code
```

## Scripts

| What | Command |
|------|---------|
| Create an emulator (once) | `avdmanager create avd -n tripcraft -k "system-images;android-35;google_apis;arm64-v8a" -d pixel_6` |
| Start it | `$ANDROID_HOME/emulator/emulator -avd tripcraft &` |
| Run on the emulator (local API) | `flutter run` — defaults to `http://10.0.2.2:5080` (the host's `localhost` from the emulator) |
| Run against the deployed API | `flutter run --dart-define=API_URL=https://<your-api>.onrender.com` |
| Build the release APK | `flutter build apk --release --dart-define=API_URL=https://<your-api>.onrender.com` → `build/app/outputs/flutter-apk/app-release.apk` |
| Install it on a phone | `adb install -r build/app/outputs/flutter-apk/app-release.apk` |
| Static analysis | `flutter analyze` (zero issues) |
| Tests | `flutter test` (no API or device needed) |
| Regenerate code after changing models/providers | `dart run build_runner build -d` |
| Regenerate the launcher icon | `dart run flutter_launcher_icons` (from `assets/icon.png`) |

`API_URL` is read at build time (`lib/core/config.dart`). Release builds only allow HTTPS, except plain
HTTP to `10.0.2.2`/`localhost` for local testing (`android/app/src/main/res/xml/network_security_config.xml`).

**Signing:** the release APK is signed with the **debug key** (no keystore is committed, per CLAUDE.md). That
is fine for the assignment's sideloaded APK but not for Google Play. To sign properly, create a keystore, add a
gitignored `android/key.properties` and a `signingConfigs` block in `android/app/build.gradle.kts`.

**minSdk is 24** (Android 7.0): the assignment asked for 23, but Flutter 3.47's engine requires 24.

## Test accounts (seeded by the API, password `Passw0rd!`)

| Role | Email | Lands on |
|------|-------|----------|
| Tourist | `tourist1@tripcraft.test` (or register a new one) | My trips |
| Guide | `guide1@tripcraft.test` | Schedule |

## Folder structure

```
lib/
  main.dart, app.dart       ProviderScope (no automatic retries) and MaterialApp.router
  core/
    config.dart             API_URL from --dart-define
    api/                    ApiClient (dio): JWT header, 401 -> logout, UserFacingException mapping
    auth/                   AuthRepository, authNotifierProvider, login/register screens, profile sheet
    router/                 routes, pure authRedirect rule, role shell (bottom nav), GoRouter
    storage/                SessionStorage over flutter_secure_storage
  shared/
    theme/  utils/          palette, status colours/labels, intl money/date formatting, validators
    widgets/                AppTextField, PrimaryButton, StatusChip, AsyncView, EmptyState, SectionCard,
                            StatusTimeline, MoneyText, PendingApiNotice
  features/
    trips/        (Tourist) new trip form (dates, pax, budget, chips, passport photo), my trips, trip detail
                  with timeline, itinerary, OpenStreetMap markers, 10 s workflow polling
    quotations/   (Tourist) quotation in LKR/USD, 30 s status watcher + local notifications, alerts history
    resources/    (Guide) schedule, GPS check-in (500 m rule), QR voucher scanner
test/
  core/ shared/ trips/ quotations/ resources/   mocked ApiClient (mocktail); layouts at 360x640 and 412x915
```

**Boundaries:** a feature imports `core` and `shared`, never another feature; `shared` imports nothing from
the app. `test/core/architecture_test.dart` fails the build if this is broken. Only `core/router` (the
composition root) knows every feature's screens; features navigate by path (`core/router/routes.dart`).

## API used

| Screen | Endpoint |
|--------|----------|
| Register / login | `POST /api/auth/register`, `POST /api/auth/login` |
| My trips, notifications | `GET /api/trip-requests` (a Tourist only ever gets their own trips) |
| New trip | `POST /api/trip-requests`, `POST /api/trip-requests/{id}/passport-photo` (multipart `file`), `POST /api/trip-requests/{id}/start-planning` |
| Trip detail | `GET /api/trip-requests/{id}`, `GET /api/trip-requests/{id}/workflow` (polled every 10 s while Planning), `GET /api/trip-requests/{id}/itinerary`, `GET /api/attractions/{id}` (map markers) |
| Quotation | the quotation inside the workflow's proposal (`finalOutcome.proposal.quotation`) |

Registration takes no nationality in the API, so the nationality entered at registration is kept on the phone
(secure storage) and pre-fills the trip form, where the API does take it.

## Students B and C's features

Guide schedule, GPS / QR check-in (`POST /api/check-ins`) and **Accept quotation** use the B and C endpoints,
which are merged on `main`. The whole PLAN.md section 6 workflow was run on the emulator on 27 Sep 2026
(`docs/evidence/final-run/`).

## iPhone

The app has an iOS target (`ios/`, bundle id `lk.tripcraft.app`, iOS 15.0+), with camera, photo-library,
location and local-network usage strings in `ios/Runner/Info.plist`. Setup with a free Apple ID:
[docs/RUN-ON-IPHONE.md](../docs/RUN-ON-IPHONE.md).
