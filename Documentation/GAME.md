# Bellavolta — implemented slice

An original human-and-moped journey through Porto Chiaro. Three replayable 90-second variants use six road sections with crests, dips, two low arches and one to three elevated promenade options. Journey records arrival and upper-route discovery locally; Flow Run exposes line score and retries on a stumble.

Hold after 0.12 seconds to wheelie; release to settle. The longer the hold, the farther back the balance moves. Flick up within the last 0.9 metres of a highlighted road lip for an assisted pop. A natural lip still launches without input. A fresh aerial upward flick commits a 0.65-second tabletop. A touch begun on the ground cannot become an aerial trick. Separate hold and pop/tabletop buttons are available in settings. Keyboard: Space hold, Up pop/tabletop, Escape pause.

Wheelies bank their award only after settling. Tabletop awards require a completed landing. Optional-path pops score only on the raised surface. A repeated move gives diminishing points. Line multiplier ×1 plus one per distinct move, capped at ×4. Over-balancing a wheelie, lifting under a low arch or a slightly unfinished tabletop is a wobble: the front wheel drops, the unbanked line clears and the ride continues; the hold must be released before another wheelie. Missing the road or a badly late tabletop is a real miss. Journey hides score; recovery returns to a verified ground checkpoint. Flow retries the outing.

## Visual and animation direction

Matched to the approved late-afternoon Porto Chiaro reference: peach sky and low sun, lilac mountain ridges falling to the sea, slate coastal hills dotted with villages and cypresses, a hill town with bell tower and arcaded sea wall, a ferry with its wake, a lighthouse islet, near terracotta houses with green shutters, balconies and laundry, and the ride itself on a plum stone bridge with arches over the water and dark foreground planting with coral flowers. An evening preset re-colours every role and lights windows and lamps.

All art is original code-drawn vector geometry (`PortoArt.cs`, `PortoPalette.cs`, `MopedRig.cs`). No bitmaps, no third-party art. HUD type is Jost (SIL Open Font License, bundled with its licence in `Assets/Resources/Fonts`).

**Depth and level of detail.** Ten sorted layers: sky and sun, far ridges (parallax 0.03), coastal hills with villages (0.07), sea, ferry, hill town (0.2), near town (0.5), laundry, the road (1.0), rider, foreground planting (1.25). Detail follows depth: villages are grouped blocks; the hill town has windows and a domed tower under a light atmospheric haze; the near town carries shutters, rails, doors and laundry. Curve tessellation scales with on-screen size (`Ink.Segments`).

**Performance.** Static art is generated once per tile into its own mesh and only moved each frame; tiles are pooled and rebuilt only when they scroll into view, the course changes or the evening preset is toggled. Per frame the game rebuilds only the sky, sea streaks, ferry, laundry and rider. Typical frame: about 8,000 to 12,000 triangles. Reduced motion freezes background parallax and animation.

**Framing.** Camera half-height 3.8 world units; road about 70% down the screen; rider about a third across and ~20% of screen height, as in the reference. The camera follows sustained support (including the raised promenade) and lifts only when a big arc would leave the frame.

**Rider and moped.** Slim original step-through moped (rack, long seat, teal frame and fenders, round lamp, tall fork), wheel base 0.94 and wheel radius 0.225 to match contact physics. Rider: cream helmet with visor edge, long hair streaming back, coral jacket, dark teal trousers, pale shoes; hands and feet stay on the grips and foot plate. Tabletop is a limited depth rotation of the machine with the rider attached.

## Asset manifest

| Family | Source | Scale / anchors | Animation / collision |
| --- | --- | --- | --- |
| Human and moped | Assets/Game/MopedRig.cs | ~1.5 u tall; rear tyre contact datum; grips and foot plate fixed | 120 Hz motor pose, interpolated |
| Bridge road, parapet, lamps, gates, promenade, finish bunting | PortoArt.Road | World units; drawn from the same course curves used for contact | Gate apex matches the 1.65 u physics clearance |
| Ridges, hills, villages, lighthouse | PortoArt.Ridges / Hills | Depth scale 0.79 | Decorative, baked tiles |
| Hill town, sea wall, rocks, boats | PortoArt.HillTown | Depth scale 0.79, 12% haze | Decorative, baked tiles |
| Near town, laundry | PortoArt.NearTown / Laundry | Depth scale 0.79 | Laundry sways; frozen by reduced motion |
| Ferry, sea streaks, sun | PortoArt.Boats / Sea / Sky | Screen-relative | Slow drift |
| Foreground planting | PortoArt.Foreground | World units | Always below the riding line (checked) |
| Menus, HUD | StudioUI.cs, Jost | 960×540 reference, safe area; targets ≥62 units | — |
| Music, putter, cues | Soundscape.cs | 24 kHz mono synthesis | Separate music/effects levels |

## Scope boundary

This is one implemented region, not the proposed multi-town full game. No vehicle upgrades, traffic simulation, online leaderboards or purchases. The journal tracks outing arrivals and upper paths. Mobile release work and device acceptance gates are in RELEASE.md; do not infer those from desktop verification.

## Verification

`Tests~/ModelChecks.cs` (16 deterministic checks): neutral completion of all outings, wheelie credit only after settling, over-balance wobble and fresh-touch rule, assisted pop to the promenade, tabletop scoring only on landing, grounded-touch ownership, ×4 cap and diminishing repeats, deterministic scenery tiles and foreground kept below the riding line. Unity editor smoke: `-executeMethod Fosters.Studio.Editor.StudioBuild.Smoke`.
