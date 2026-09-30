# Bellavolta TestFlight pipeline (Unity → Mac mini → TestFlight)

A commit pushed to **`TestFlight`** ends up in TestFlight with no manual steps. Same mechanics as the Shovey and Bus Switch pipelines, on the same Mac mini.

```
work on dev ──push──▶ GitHub Observe-1/BellavoltaAndHopward TestFlight
                          │  (polled every 60 s)
                          ▼
Mac mini: ~/BellavoltaCI/watch.sh → testflight.sh:
          Unity export → icons + Info.plist → xcodebuild archive → upload
                          │
                          ▼
          App Store Connect → TestFlight + ntfy push to your phone
```

## Shipping a build

```bash
git push origin dev && git push origin dev:TestFlight
```

- **Build number:** UTC `yyMMddHHmm`. **Version:** `PlayerSettings.bundleVersion` from `StudioBuild.Configure` (0.1.0).
- **Bundle ID:** `com.fostersdigital.bellavolta` (override with `BELLAVOLTA_BUNDLE_ID`). The App Store Connect app record must exist for it.
- **Timing:** first build of a fresh clone imports the project (longer); after that about 10 minutes, then 5–15 minutes of Apple processing.
- **Failed builds** aren't retried. Push a fix. Logs: `~/BellavoltaCI/builds/<build number>/` (`unity.log`, `metadata.log`, `archive.log`, `upload.log`). Last 30 kept, logs only.

## Sharing the Mac

| | Bellavolta | Shovey | Bus Switch |
|---|---|---|---|
| LaunchAgent | `com.fostersdigital.bellavolta.testflight-watch` | `com.shovey.testflight-watch` | `com.busswitch.testflight-watch` |
| Working folder | `~/BellavoltaCI/` | `~/ShoveyCI/` | `~/BusSwitchCI/` |
| Settings | `~/.bellavolta-ci.env` | `~/.shovey-ci.env` | `~/.bus-switch-ci.env` |
| Trigger branch | `TestFlight` | `unity-testflight` | `unity-testflight` |

A Bellavolta build waits while `~/ShoveyCI/lock` or `~/BusSwitchCI/lock` exists.

## Install (once)

`bash Tools/testflight/install-mac.sh` from any checkout. It creates `~/BellavoltaCI`, copies the App Store Connect key settings and ntfy topic from the Shovey or Bus Switch settings file into `~/.bellavolta-ci.env`, and loads the LaunchAgent. Xcode, Unity 6000.6.3f1 with iOS Build Support, and the API key are shared with the other pipelines. No CocoaPods needed today.

## Files

| File | Purpose |
|---|---|
| `testflight.sh` | Builds and uploads whatever is checked out. |
| `watch.sh` | Polls `TestFlight`, builds each new commit once in `~/BellavoltaCI/repo`. |
| `com.fostersdigital.bellavolta.testflight-watch.plist` | LaunchAgent, every 60 s. |
| `install-mac.sh` | Installs or updates the watcher. |
| `ExportOptions.plist` | App Store Connect upload, automatic signing; team ID filled in at build time. |
| `bellavolta-ci.env.example` | Template for `~/.bellavolta-ci.env`. |
| `make-icon.py` + `AppIcon.appiconset/` | Code-rendered app icon (Porto Chiaro palette). Re-run after changing the art. |
| `Assets/Editor/TestFlightBuild.cs` | Unity entry point: applies the product settings, sets build number/team/bundle from the environment, exports, unembeds UnityRuntime (ITMS-90208). |

## Troubleshooting

- Nothing happens: `~/BellavoltaCI/watch.log`, `launchctl list | grep bellavolta`, check for another pipeline's lock. Delete `~/BellavoltaCI/last-built` to rebuild the same commit.
- Upload rejected for the bundle ID: create the app in App Store Connect for `com.fostersdigital.bellavolta` on team 4B8Z6NM2D4.
- Unity licence errors in `unity.log`: sign in again in Unity Hub.
