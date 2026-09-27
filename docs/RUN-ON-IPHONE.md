# Run the TripCraft app on your iPhone

The Flutter app (`mobile/`) has an iOS target (`mobile/ios/`, bundle id `lk.tripcraft.app`, iOS 15.0 or later).
You can put it on your own iPhone with a free Apple ID; no paid developer account is needed.

## What is already set up in the repo

- `mobile/ios/Runner/Info.plist` has the permission texts iOS shows the first time a feature is used:
  - `NSCameraUsageDescription`: passport photo and voucher QR scanning.
  - `NSPhotoLibraryUsageDescription`: choosing the passport photo from the library.
  - `NSLocationWhenInUseUsageDescription`: guide check-in at a stop.
  - `NSLocalNetworkUsageDescription` and `NSAppTransportSecurity > NSAllowsLocalNetworking`: talking to a TripCraft API on
    your laptop over the LAN during development.
- Local notifications have iOS (Darwin) settings (`mobile/lib/features/quotations/data/local_notifications.dart`).
- Every plugin supports iOS. The highest minimum is iOS 13.0 (flutter_secure_storage, image_picker, url_launcher,
  flutter_local_notifications); the project targets 15.0.

| Package | iOS support |
|---------|-------------|
| dio, go_router, flutter_riverpod, intl, latlong2, flutter_map | pure Dart, all platforms |
| flutter_secure_storage | Keychain, iOS 13+ |
| image_picker | iOS 13+ (camera and photo library) |
| geolocator | iOS 11+ (when-in-use location) |
| mobile_scanner | iOS 12+ (camera) |
| flutter_local_notifications | iOS 13+ |
| url_launcher | iOS 13+ |
| permission_handler | iOS 12+ (not called by the app code) |

## One-time setup on the Mac

1. **Install Xcode** from the Mac App Store (about 15 GB). Then, in Terminal:

   ```bash
   sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer
   sudo xcodebuild -runFirstLaunch
   brew install cocoapods
   flutter doctor          # the Xcode line should now be ✓
   ```

2. **Check that it compiles** (no signing needed):

   ```bash
   cd mobile
   flutter pub get
   flutter build ios --no-codesign
   ```

3. **Open the workspace once** to set signing:

   ```bash
   open ios/Runner.xcworkspace
   ```

   - In Xcode, go to **Settings → Accounts** and add your Apple ID.
   - Select the **Runner** project, then the **Runner** target, then **Signing & Capabilities**.
   - Tick **Automatically manage signing** and set **Team** to *Your Name (Personal Team)*.
   - A free team needs a bundle id that nobody else has used. If Xcode says `lk.tripcraft.app` is taken, change it to
     something unique, such as `lk.tripcraft.app.<yourname>`.

## One-time setup on the iPhone

1. Connect the iPhone with a cable and unlock it. When it asks **Trust This Computer?**, tap **Trust** and enter your
   passcode.
2. Turn on **Developer Mode**: go to **Settings → Privacy & Security → Developer Mode**, switch it on and restart. After
   the restart, confirm **Turn On**. The option only appears once the phone has been connected to Xcode.
3. The first time the app is installed, go to **Settings → General → VPN & Device Management**, open your Apple ID under
   *Developer App* and tap **Trust**.

## Run it

List the devices and copy the iPhone's name or id:

```bash
cd mobile
flutter devices
```

**Against the deployed API** (easiest; works anywhere):

```bash
flutter run -d "<my iPhone>" --dart-define=API_URL=https://<deployed-api-host>
```

**Against the API on your laptop** (phone and laptop on the same Wi-Fi):

1. Find the laptop's LAN IP: `ipconfig getifaddr en0` (for example `192.168.1.23`).
2. Start the API so it listens on every interface, not only localhost:

   ```bash
   cd backend/src/TripCraft.Api
   ASPNETCORE_URLS=http://0.0.0.0:5080 dotnet run --no-launch-profile   # plus your usual env vars
   ```

   If macOS asks whether *TripCraft.Api* may accept incoming connections, click **Allow**.
3. Run the app with the LAN address:

   ```bash
   flutter run -d "<my iPhone>" --dart-define=API_URL=http://192.168.1.23:5080
   ```

4. The first request makes iOS ask for **Local Network** access. Tap **Allow**.

For a build that keeps running after you unplug the phone, use `flutter run --release ...`. Debug builds need the
laptop attached to start.

## Things to know

- **Free-account builds expire after 7 days.** After that the app will not open; run `flutter run` again to reinstall.
  A free team can also have only a few apps installed at once.
- The API must listen on `0.0.0.0` (not `localhost`) for the phone to reach it over the LAN, and the laptop firewall
  must allow port 5080.
- `http://` is allowed only for local-network addresses (`NSAllowsLocalNetworking`). A deployed API must use
  `https://`.
- CORS does not apply to the app (it is not a browser), so `ALLOWED_ORIGINS` does not need the phone's address.
- Push notifications are not used. Trip status alerts are local notifications raised while the app polls, so no APNs
  set-up is needed.
