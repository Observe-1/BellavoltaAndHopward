# Local release workflow

Open this folder in Unity 6000.6.3f1. On a product branch, the editor creates Assets/Scenes/Journey.unity and configures the product. Open that scene and press Play. Runtime bootstrap creates the scene's original vector world and menus.

Build from the **fostersdigital > Build** menu, or run Unity locally with `-batchmode -quit -projectPath <this folder> -executeMethod Fosters.Studio.Editor.StudioBuild.IOS` (Android / Desktop are analogous). There are no GitHub Actions; TestFlight builds run on the Mac mini (Tools/testflight).

Identifiers follow the verified OpenPrayer scheme `com.fostersdigital.openprayer`: `com.fostersdigital.bellavolta` and `com.fostersdigital.hopward`. Display names and local progress stores are separate. Foundation is not a release product.

## iOS

TestFlight builds are automatic: push to the `TestFlight` branch and the Mac mini builds, signs and uploads it. See `Tools/testflight/README.md`.

For a local export, build an unsigned Xcode project under Builds/iOS. Deployment minimum 18.0, landscape, IL2CPP. Automatic signing is off and team ID is blank. Transfer to a Mac with the currently required Xcode/SDK; the owner selects their team, provisioning and signing and performs the final archive/upload. No certificate or signing profile is committed.

## Android

Install Android Build Support plus Unity's matching SDK/NDK/OpenJDK through Unity Hub; it was absent at initial inspection. The local build creates an ARM64 IL2CPP App Bundle. Minimum API 26, installed target API chosen by Unity. Verify target API against Play Console at release time. Configure your release keystore before producing the upload bundle; secrets stay outside Git. The starter build uses Unity's default signing configuration, not a claimed production signing setup.

## Before store submission

Device performance, touch timing, interruptions, audio fatigue, small-screen legibility, screenshots, icon exports, store descriptions, privacy disclosures, title clearance and release signing remain release gates. Windows editor smoke evidence does not establish mobile build or store approval. Target 60 fps is a target, not a measured device result. Synthetic music is an original quiet study; musical polish and ten-minute listening checks remain to be reviewed.

The user requested minimal Unity testing after implementation confidence. Deterministic model checks precede one short editor smoke per product; no repeated exploratory playtesting is scheduled.
