# Installing the TripCraft Android app (APK)

The app is not on Google Play; it is installed ("sideloaded") from the APK attached to the GitHub Release.

## Build the APK (maintainers)

```bash
cd mobile
./scripts/build-release-apk.sh https://<your-api>.onrender.com
# -> build/app/outputs/flutter-apk/app-release.apk, with its size and SHA-256
```

The API URL is compiled into the app (`--dart-define=API_URL`). Rebuild if the API URL changes.
The APK is signed with the debug key (no keystore is committed); Android shows it as from an unknown source.

## Publish it as GitHub Release v1.0

```bash
git tag -a v1.0 -m "TripCraft v1.0 — SE3090 Assignment 1 submission"
git push origin v1.0
gh release create v1.0 mobile/build/app/outputs/flutter-apk/app-release.apk \
  --title "TripCraft v1.0" \
  --notes "Android app for Tourists and Guides. API: https://<your-api>.onrender.com. SHA-256: <from the build script>. Install steps: docs/APK-INSTALL.md"
```

Or in the browser: **Releases → Draft a new release → tag `v1.0` → attach `app-release.apk` → Publish**.

## Install on a phone (Android 7.0 or newer)

1. On the phone, open the release page and download **app-release.apk**.
2. Tap the downloaded file. Android asks to allow installs from this source (Chrome / Files):
   **Settings → Apps → Special app access → Install unknown apps → Chrome → Allow**.
3. Tap **Install**. If Play Protect warns "unknown developer", choose **Install anyway** (debug-signed build).
4. Open **TripCraft**. Before the first sign-in, open `https://<your-api>.onrender.com/health` once in the
   phone's browser: the free Render service may take ~50 s to wake up.
5. Allow **Camera** (passport photo, voucher QR), **Location** (guide check-in) and **Notifications** when asked.

Sign in with a seeded account (password `Passw0rd!`): `tourist1@tripcraft.test` or `guide1@tripcraft.test`,
or tap **Create an account** to register a new tourist.

## Install with a cable (developers)

```bash
adb install -r mobile/build/app/outputs/flutter-apk/app-release.apk
```

## Screenshots for the report

`docs/evidence/screenshots/apk-install-*.png` — download, install prompt, first launch, login.

## Troubleshooting

| Problem | Fix |
|---------|-----|
| "App not installed" | Uninstall an older TripCraft build signed with a different key, then install again |
| "Cannot reach TripCraft" on login | Wake the API (step 4); check the phone has internet; check the APK was built with the right `API_URL` |
| Camera/location never asked | Settings → Apps → TripCraft → Permissions → allow them |
