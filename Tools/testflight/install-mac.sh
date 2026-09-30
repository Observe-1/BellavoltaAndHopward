#!/bin/bash
# One-time install of the Bellavolta TestFlight build watcher on the Mac mini. Safe to re-run
# (it also picks up changes to watch.sh or the LaunchAgent). Leaves the Shovey and Bus Switch watchers alone.
#
#   bash Tools/testflight/install-mac.sh
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"
LABEL="com.fostersdigital.bellavolta.testflight-watch"
CI_ROOT="$HOME/BellavoltaCI"
AGENT="$HOME/Library/LaunchAgents/$LABEL.plist"
ENV_FILE="$HOME/.bellavolta-ci.env"
mkdir -p "$CI_ROOT" "$HOME/Library/LaunchAgents"

missing=0
command -v xcodebuild >/dev/null || { echo "Missing: Xcode (App Store), then run: sudo xcodebuild -license accept"; missing=1; }
UNITY_VERSION="$(sed -n 's/^m_EditorVersion: *//p' "$HERE/../../ProjectSettings/ProjectVersion.txt" | tr -d '[:space:]')"
UNITY_BIN="${BELLAVOLTA_UNITY:-/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity}"
if [ ! -x "$UNITY_BIN" ]; then
  echo "Missing: Unity $UNITY_VERSION with iOS Build Support (install through Unity Hub and sign in there)"; missing=1
elif [ ! -d "${UNITY_BIN%/Unity.app/*}/PlaybackEngines/iOSSupport" ]; then
  echo "Missing: iOS Build Support for Unity $UNITY_VERSION (Unity Hub > Installs > Add modules)"; missing=1
fi
command -v gh >/dev/null || echo "Optional: GitHub CLI (brew install gh && gh auth login) for build status on commits"

if [ ! -f "$ENV_FILE" ]; then
  cp "$HERE/bellavolta-ci.env.example" "$ENV_FILE"
  chmod 600 "$ENV_FILE"
  # Reuse the App Store Connect key and ntfy topic the other games on this Mac already use.
  for other in "$HOME/.shovey-ci.env" "$HOME/.bus-switch-ci.env"; do
    [ -f "$other" ] || continue
    for v in ASC_KEY_ID ASC_ISSUER_ID ASC_KEY_PATH NTFY_TOPIC; do
      line="$(grep -E "^$v=" "$other" | tail -n 1 || true)"
      [ -n "$line" ] || continue
      python3 - "$ENV_FILE" "$v" "$line" <<'PY'
import sys, re
path, key, line = sys.argv[1:]
text = open(path).read()
text = re.sub(r"(?m)^" + re.escape(key) + r"=.*$", lambda m: line, text, count=1)
open(path, "w").write(text)
PY
    done
    echo "Created $ENV_FILE with the App Store Connect key and ntfy topic from $other."
    break
  done
  grep -q '^ASC_KEY_ID=XXXXXXXXXX' "$ENV_FILE" && { echo "Fill in the App Store Connect key in $ENV_FILE."; missing=1; }
fi

cp "$HERE/watch.sh" "$CI_ROOT/watch.sh"
sed "s#__HOME__#$HOME#g" "$HERE/$LABEL.plist" > "$AGENT"
launchctl bootout "gui/$(id -u)/$LABEL" 2>/dev/null || true
launchctl bootstrap "gui/$(id -u)" "$AGENT"
echo "Watcher $LABEL installed: checks Bellavolta's TestFlight branch every minute. Log: $CI_ROOT/watch.log"

[ "$missing" = 0 ] && echo "Ready. Push to TestFlight, or test by hand: bash $HERE/testflight.sh" || echo "Finish the items above, then re-run this script."
