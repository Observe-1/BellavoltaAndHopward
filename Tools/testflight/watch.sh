#!/bin/bash
# Poll the TestFlight branch and build any new commit. Run every minute by launchd
# (com.fostersdigital.bellavolta.testflight-watch.plist). macOS only.
#
# Uses its own clone at $BELLAVOLTA_CI_ROOT/repo so it never touches a checkout you or Claude are editing.
# A commit is built once. If it fails, push a fix (a new commit) and that gets built.
#
# Shares the Mac with the Shovey (~/ShoveyCI) and Bus Switch (~/BusSwitchCI) watchers. Everything here
# is Bellavolta specific, and a build waits while either of their build locks is held so two
# Unity + Xcode builds don't compete for the machine.
set -uo pipefail

ENV_FILE="${BELLAVOLTA_CI_ENV:-$HOME/.bellavolta-ci.env}"
[ -f "$ENV_FILE" ] && source "$ENV_FILE"
CI_ROOT="${BELLAVOLTA_CI_ROOT:-$HOME/BellavoltaCI}"
BRANCH="${BELLAVOLTA_BRANCH:-TestFlight}"
REPO_URL="${BELLAVOLTA_REPO_URL:-https://github.com/Observe-1/BellavoltaAndHopward.git}"
GH_REPO="${BELLAVOLTA_GH_REPO:-Observe-1/BellavoltaAndHopward}"
WAIT_FOR_LOCKS="${BELLAVOLTA_WAIT_FOR_LOCKS-$HOME/ShoveyCI/lock $HOME/BusSwitchCI/lock}"
CLONE="$CI_ROOT/repo"
LOCK="$CI_ROOT/lock"
LAST="$CI_ROOT/last-built"
mkdir -p "$CI_ROOT"

# Another pipeline is building: try again next minute. Its locks also expire after 3 hours.
for other in $WAIT_FOR_LOCKS; do
  [ -d "$other" ] && [ -z "$(find "$other" -maxdepth 0 -mmin +180 2>/dev/null)" ] && exit 0
done

# One build at a time. A lock older than 3 hours is left over from a crash or reboot.
if ! mkdir "$LOCK" 2>/dev/null; then
  if [ -n "$(find "$LOCK" -maxdepth 0 -mmin +180 2>/dev/null)" ]; then rm -rf "$LOCK"; mkdir "$LOCK"; else exit 0; fi
fi
trap 'rm -rf "$LOCK"' EXIT

[ -d "$CLONE/.git" ] || git clone --quiet "$REPO_URL" "$CLONE" || exit 1
cd "$CLONE"
git fetch --quiet origin "$BRANCH" 2>/dev/null || exit 0   # branch not pushed yet, or offline
SHA="$(git rev-parse "origin/$BRANCH")"
[ "$SHA" = "$(cat "$LAST" 2>/dev/null)" ] && exit 0

status() { # state, description. Optional: needs the GitHub CLI signed in with access to the repo.
  command -v gh >/dev/null || return 0
  gh api -X POST "repos/$GH_REPO/statuses/$SHA" -f state="$1" -f context=testflight \
    -f description="$2" >/dev/null 2>&1 || true
}

echo "$(date '+%F %T') building $SHA"
echo "$SHA" > "$LAST"
git checkout --quiet -f -B "$BRANCH" "origin/$BRANCH"
git clean -fdq   # untracked files only; ignored Unity Library/ cache survives
status pending "Building for TestFlight"
if bash Tools/testflight/testflight.sh; then
  status success "Uploaded to TestFlight"
else
  status failure "Build failed on the Mac mini (see ~/BellavoltaCI/builds)"
fi
