#!/bin/bash
# Build Bellavolta (Unity) for iOS and upload it to TestFlight. macOS only.
#
#   bash Tools/testflight/testflight.sh            # builds the checkout this script lives in
#
# Steps: Unity export -> app icons + Info.plist -> (pod install if a Podfile appears) -> xcodebuild
# archive -> export with destination=upload. Signing is automatic via an App Store Connect API key;
# nothing secret lives in the repo. Settings come from ~/.bellavolta-ci.env (see bellavolta-ci.env.example).
# Sends an ntfy push on success/failure when NTFY_TOPIC is set. Exits non-zero on failure.
#
# Namespaced BELLAVOLTA_* / BellavoltaCI so it shares the Mac with the Shovey and Bus Switch pipelines
# without reading their settings.
set -euo pipefail

# launchd doesn't set a locale; CocoaPods and some Xcode tools need UTF-8.
export LANG=en_US.UTF-8 LC_ALL=en_US.UTF-8

ENV_FILE="${BELLAVOLTA_CI_ENV:-$HOME/.bellavolta-ci.env}"
[ -f "$ENV_FILE" ] && source "$ENV_FILE"

REPO="$(cd "$(dirname "$0")/../.." && pwd)"
PROJECT="$REPO"   # the Unity project is the repo root
UNITY_VERSION="$(sed -n 's/^m_EditorVersion: *//p' "$PROJECT/ProjectSettings/ProjectVersion.txt" | tr -d '[:space:]')"
UNITY="${BELLAVOLTA_UNITY:-/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity}"
CI_ROOT="${BELLAVOLTA_CI_ROOT:-$HOME/BellavoltaCI}"
ICONS="$REPO/Tools/testflight/AppIcon.appiconset"
PRODUCT="$(sed -n 's/.*"slug" *: *"\([^"]*\)".*/\1/p' "$PROJECT/Assets/Resources/Product.json")"

# Unique and always increasing: UTC yyMMddHHmm.
BUILD_NUMBER="${BELLAVOLTA_BUILD_NUMBER:-$(date -u +%y%m%d%H%M)}"
SHA="$(git -C "$REPO" rev-parse --short HEAD)"
SUBJECT="$(git -C "$REPO" log -1 --format=%s | cut -c1-80)"
OUT="$CI_ROOT/builds/$BUILD_NUMBER"
mkdir -p "$OUT"
STEP="setup"

notify() { # title, message, priority
  [ -n "${NTFY_TOPIC:-}" ] || return 0
  curl -fsS -m 15 -H "Title: $1" -H "Priority: ${3:-default}" -d "$2" "https://ntfy.sh/$NTFY_TOPIC" >/dev/null || true
}

on_error() {
  local log="$OUT/$STEP.log" tail=""
  [ -f "$log" ] && tail="$(grep -E 'error|Error|FAILED|failed' "$log" | grep -v 'warning' | tail -n 8 || true)"
  [ -z "$tail" ] && [ -f "$log" ] && tail="$(tail -n 5 "$log")"
  echo "FAILED at $STEP. Logs: $OUT" >&2
  notify "Bellavolta build $BUILD_NUMBER FAILED ($STEP)" "$SHA $SUBJECT
$tail
Logs: $OUT" high
}
trap on_error ERR

# The Unity export rewrites ProjectSettings (build number, signing). Put back any tracked file it
# changed so a manual run from a working checkout leaves it as it was; your own edits are left alone.
PRE_DIRTY="$(git -C "$REPO" diff --name-only)"
restore_project() {
  local f
  git -C "$REPO" diff --name-only | while IFS= read -r f; do
    grep -qxF "$f" <<<"$PRE_DIRTY" || git -C "$REPO" checkout --quiet -- "$f"
  done
}

# Only the logs are kept: the Xcode export, DerivedData and archive are several GB per build and the
# dSYMs go to App Store Connect with the upload. KEEP_BUILD_OUTPUT=1 keeps everything for debugging.
cleanup() {
  restore_project
  [ "${KEEP_BUILD_OUTPUT:-0}" = 1 ] && return 0
  rm -rf "$OUT"/xcode "$OUT"/xcode_BackUpThisFolder_ButDontShipItWithYourGame \
    "$OUT/DerivedData" "$OUT/Bellavolta.xcarchive" "$OUT/export" "$OUT/ExportOptions.plist"
}
trap cleanup EXIT

ls -1dt "$CI_ROOT"/builds/*/ 2>/dev/null | tail -n +31 | xargs rm -rf

STEP="config"
for v in ASC_KEY_ID ASC_ISSUER_ID ASC_KEY_PATH BELLAVOLTA_TEAM_ID BELLAVOLTA_NON_EXEMPT_ENCRYPTION; do
  [ -n "${!v:-}" ] || { echo "Set $v in $ENV_FILE" | tee -a "$OUT/$STEP.log" >&2; false; }
done
case "$BELLAVOLTA_NON_EXEMPT_ENCRYPTION" in
  no) ENCRYPTION=false ;; yes) ENCRYPTION=true ;;
  *) echo "BELLAVOLTA_NON_EXEMPT_ENCRYPTION must be yes or no" | tee -a "$OUT/$STEP.log" >&2; false ;;
esac
[ "$PRODUCT" = "bellavolta" ] ||
  { echo "Assets/Resources/Product.json selects '$PRODUCT', not bellavolta" | tee -a "$OUT/$STEP.log" >&2; false; }
[ -f "$ASC_KEY_PATH" ] || { echo "API key file not found: $ASC_KEY_PATH" | tee -a "$OUT/$STEP.log" >&2; false; }
[ -x "$UNITY" ] || { echo "Unity not found: $UNITY (install $UNITY_VERSION through Unity Hub)" | tee -a "$OUT/$STEP.log" >&2; false; }
[ -d "${UNITY%/Unity.app/*}/PlaybackEngines/iOSSupport" ] ||
  { echo "iOS Build Support missing for $UNITY (Unity Hub > Installs > Add modules)" | tee -a "$OUT/$STEP.log" >&2; false; }
[ -f "$ICONS/Contents.json" ] || { echo "App icon catalog missing: $ICONS" | tee -a "$OUT/$STEP.log" >&2; false; }

AUTH=(-allowProvisioningUpdates
      -authenticationKeyPath "$ASC_KEY_PATH"
      -authenticationKeyID "$ASC_KEY_ID"
      -authenticationKeyIssuerID "$ASC_ISSUER_ID")

echo "== Bellavolta $BUILD_NUMBER ($SHA: $SUBJECT) -> $OUT"

STEP="unity"
echo "-- Unity export"
STUDIO_BUILD_NUMBER="$BUILD_NUMBER" STUDIO_TEAM_ID="$BELLAVOLTA_TEAM_ID" STUDIO_BUNDLE_ID="${BELLAVOLTA_BUNDLE_ID:-}" \
  "$UNITY" -batchmode -buildTarget iOS -projectPath "$PROJECT" \
  -executeMethod Fosters.Studio.Editor.TestFlightBuild.IOS -studioBuildPath "$OUT/xcode" \
  -quit -logFile "$OUT/$STEP.log"

STEP="metadata"
echo "-- app icons + Info.plist"
{
  # The Unity project sets no icons; use the code-rendered catalog (Tools/testflight/make-icon.py).
  ICON_TARGET="$OUT/xcode/Unity-iPhone/Images.xcassets/AppIcon.appiconset"
  rm -rf "$ICON_TARGET" && mkdir -p "$ICON_TARGET" && cp "$ICONS"/* "$ICON_TARGET/"
  # Answering export compliance here stops each build waiting on the question in App Store Connect.
  /usr/libexec/PlistBuddy -c "Delete :ITSAppUsesNonExemptEncryption" "$OUT/xcode/Info.plist" 2>/dev/null || true
  /usr/libexec/PlistBuddy -c "Add :ITSAppUsesNonExemptEncryption bool $ENCRYPTION" "$OUT/xcode/Info.plist"
  sed "s#__TEAM_ID__#$BELLAVOLTA_TEAM_ID#" "$REPO/Tools/testflight/ExportOptions.plist" > "$OUT/ExportOptions.plist"
} > "$OUT/$STEP.log" 2>&1

# Bellavolta has no CocoaPods dependencies today; this only runs if a future plugin adds a Podfile.
STEP="pods"
if [ -f "$OUT/xcode/Podfile" ] && [ ! -d "$OUT/xcode/Unity-iPhone.xcworkspace" ]; then
  echo "-- pod install"
  command -v pod >/dev/null || { echo "CocoaPods missing: brew install cocoapods" > "$OUT/$STEP.log"; false; }
  (cd "$OUT/xcode" && pod install --repo-update) > "$OUT/$STEP.log" 2>&1
fi
XCODE_SOURCE=(-project "$OUT/xcode/Unity-iPhone.xcodeproj")
[ -d "$OUT/xcode/Unity-iPhone.xcworkspace" ] && XCODE_SOURCE=(-workspace "$OUT/xcode/Unity-iPhone.xcworkspace")

STEP="archive"
echo "-- xcodebuild archive"
xcodebuild "${XCODE_SOURCE[@]}" -scheme Unity-iPhone -configuration Release \
  -destination 'generic/platform=iOS' -archivePath "$OUT/Bellavolta.xcarchive" \
  -derivedDataPath "$OUT/DerivedData" "${AUTH[@]}" \
  archive > "$OUT/$STEP.log" 2>&1
# App Store Connect rejects a static archive embedded as a framework (ITMS-90208, seen with UnityRuntime).
# TestFlightBuild.cs removes that embed; this stops the upload if any static framework is still embedded.
for fw in "$OUT"/Bellavolta.xcarchive/Products/Applications/*.app/Frameworks/*.framework; do
  [ -e "$fw" ] || continue
  bin="$fw/$(/usr/libexec/PlistBuddy -c 'Print :CFBundleExecutable' "$fw/Info.plist")"
  if file "$bin" | grep -q 'ar archive'; then
    echo "Static archive embedded as a framework (App Store Connect rejects it, ITMS-90208): $fw" >> "$OUT/$STEP.log"
    false
  fi
done

STEP="upload"
echo "-- export + upload to App Store Connect"
xcodebuild -exportArchive -archivePath "$OUT/Bellavolta.xcarchive" \
  -exportOptionsPlist "$OUT/ExportOptions.plist" -exportPath "$OUT/export" \
  "${AUTH[@]}" > "$OUT/$STEP.log" 2>&1

trap - ERR
echo "== Uploaded build $BUILD_NUMBER. It appears in TestFlight after Apple finishes processing (~5-15 min)."
notify "Bellavolta build $BUILD_NUMBER uploaded" "$SHA $SUBJECT
Processing in App Store Connect, then it's in TestFlight."
