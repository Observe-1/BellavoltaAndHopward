# Bellavolta — implemented slice

An original human-and-moped journey through Porto Chiaro. Three replayable 90-second variants use six road sections with crests, dips, two low arches and one to three elevated promenade options. Journey records arrival and upper-route discovery locally; Flow Run exposes line score and retries on a stumble.

Hold after 0.12 seconds to wheelie; release to settle. The longer the hold, the farther back the balance moves. Flick up within the last 0.9 metres of a highlighted road lip for an assisted pop. A natural lip still launches without input. A fresh aerial upward flick commits a 0.65-second tabletop. A touch begun on the ground cannot become an aerial trick. Separate hold and pop/tabletop buttons are available in settings. Keyboard: Space hold, Up pop/tabletop, Escape pause.

Wheelies bank their award only after settling. Tabletop awards require a completed landing. Optional-path pops score only on the raised surface. A repeated move gives diminishing points. Line multiplier ×1 plus one per distinct move, capped at ×4. Over-balancing a wheelie, lifting under a low arch or a slightly unfinished tabletop is a wobble: the front wheel drops, the unbanked line clears and the ride continues; the hold must be released before another wheelie. Missing the road or a badly late tabletop is a real miss. Journey hides score; recovery returns to a verified ground checkpoint. Flow retries the outing.

## Visual and animation direction

Apricot sky #EAB991; cream sun/helmet #F4DDB1; lilac hills #A7A4B8; distant town #B8A0A1; harbour/moped #76A2AD; plaster #D19B83; course/mechanics #493F54; jacket #C77D66. Evening uses a complete quieter preset. Sparse windows, a ferry, laundry and local silhouettes give scale; no texture noise, brand badges or copied assets.

The original angular moped has two stable tyre anchors 0.94 world units apart, 0.225-unit radius wheels, a narrow shield, circular lamp and supported foot position. The human has fixed helmet dimensions and two-bone limbs with shared handle/foot anchors. Suspension compression, rear-wheel pivot, lean and a modest tabletop fold drive the rig. Tabletop is a limited stylized depth cue, not a backflip.

## Asset manifest

All assets are original source-generated vectors or synthesis, authored in this repository. No third-party production assets. Transparent individual shapes share an opaque scene; rendered at display resolution, no pixel atlas or independent sprite frames. World units are the authoritative size; approximately 47 px/unit in a 720 px-tall frame.

| Family | Source | Scale / anchors | Animation / collision |
| --- | --- | --- | --- |
| Human and moped | Assets/Game/MopedRig.cs | ~1.6 units high, rear tyre tip datum; handlebars and foot support fixed | Render-frame rig, 120 Hz interpolated pose; motor contact defines collision |
| Six road sections + raised branches | Bellavolta.CreateCourse | 450-unit outing; same smooth curves for drawing/support | Swept contact, explicit arch clearance |
| Hills, town, harbour, bridge, plants | Landscape.cs | Broad parallax planes | Decorative only |
| Ferry, laundry, distant person | Landscape.cs | Anchored to background plane | Slow clock; frozen by reduced motion |
| Menu/HUD/journal | StudioUI.cs | 960×540 reference, safe-area anchors | Touch targets ≥62 reference units (~45 pt on a landscape iPhone); accessible controls 82 units |
| Music, putter, tick, landing cue | Soundscape.cs | 24 kHz mono | Original synthesis; independently adjustable music/effects |

## Scope boundary

This is one implemented region, not the proposed multi-town full game. No vehicle upgrades, traffic simulation, online leaderboards or purchases. The journal tracks outing arrivals and upper paths. Mobile release work and device acceptance gates are in RELEASE.md; do not infer those from desktop verification.

## Verification

`Tests~/ModelChecks.cs` (14 deterministic checks): neutral completion of all outings, wheelie credit only after settling, over-balance wobble and fresh-touch rule, assisted pop to the promenade, tabletop scoring only on landing, grounded-touch ownership, ×4 cap and diminishing repeats. Unity editor smoke: `-executeMethod Fosters.Studio.Editor.StudioBuild.Smoke`.
