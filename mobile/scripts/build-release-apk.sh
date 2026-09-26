#!/usr/bin/env bash
# Builds the release APK pointed at the deployed API.
#   ./scripts/build-release-apk.sh https://tripcraft-api.onrender.com
# Output: build/app/outputs/flutter-apk/app-release.apk (+ its size and SHA-256 for the release notes).
set -euo pipefail

API_URL="${1:-${API_URL:-}}"
if [[ -z "$API_URL" ]]; then
  echo "Usage: $0 <deployed API URL, e.g. https://tripcraft-api.onrender.com>" >&2
  exit 1
fi
if [[ "$API_URL" != https://* ]]; then
  echo "The release APK only talks HTTPS (except 10.0.2.2 for the emulator). Got: $API_URL" >&2
  exit 1
fi
API_URL="${API_URL%/}"

cd "$(dirname "$0")/.."

echo "Checking $API_URL/health (wakes the Render free service if it is asleep)…"
curl -fsS --max-time 90 "$API_URL/health" || echo "  warning: the API did not answer; building anyway"
echo

flutter pub get
flutter build apk --release --dart-define=API_URL="$API_URL"

APK=build/app/outputs/flutter-apk/app-release.apk
echo
echo "APK:    $APK"
echo "Size:   $(du -h "$APK" | cut -f1)"
echo "SHA256: $(shasum -a 256 "$APK" | cut -d' ' -f1)"
echo "API:    $API_URL"
