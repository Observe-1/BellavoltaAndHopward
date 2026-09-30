# Bellavolta — endless coastal ride

One endless road along the Porto Chiaro coast, generated fresh every run. The moped cruises on its own and speeds up slowly but surely with distance (6.5 u/s at the start, about 10.8 by 3 km, 13+ later). The run ends on the first crash. Score, distance and lemons are recorded; best distance and best score are kept.

## Controls (one finger)

| Input | Result |
| --- | --- |
| Press on the road or a rope | Hop, instantly (plus any lift from a ramp lip) |
| Keep holding in the air | Backflip rotation (after 0.13 s, up to 500°/s); let go to stop |
| Quick tap in the air | Trick: tabletop, then no-hander on the next; must finish (0.5 s) before landing |
| Land within about -32° to +55° of the road | Ride away. Within 8° after a trick: Perfect landing. Nose-high: wheelie |
| Land further off, mid-trick, into a crate or the harbour | Crash: the run ends |

Accessible mode (Settings → Separate controls) splits this into a Hop/flip button (hold) and a Trick button.

## The road

Built from "moments" separated by short breathers, picked at random and scaled by distance:

- **Kicker and gap**: a steepening stone ramp, open water, a downhill landing. Early gaps clear without a hop; later ones need a hop off the lip.
- **Launch ramp**: a big kicker onto a long downhill: pure air time for flips.
- **Gap**: a broken span to hop.
- **Lemon crates**: hop over; tall stacks appear more often further on. Spacing always leaves room to land between them.
- **Drop**: a ledge down to a lower road; drops get taller with distance.
- **Bunting**: a small kicker, then a festival line strung over the road. Land on the rope to grind.
- **Promenade**: kick up onto a raised arcaded walkway (hop needed), ride it, then drop off the far end for big air. The lower road carries on beneath, sometimes with a crate.
- **Rollers**: smooth humps that pop you at speed.

Lemons sit along the natural lines (jump arcs, rope, promenade) to show where to go.

## Scoring

Backflip 150, double 400, triple 800; tabletop or no-hander 100; grind 40 plus 80 per second; perfect landing 50; wheelie 60; big air 40. Different moves chained within 2.5 s multiply the line (×1 plus one per distinct move, up to ×5); the line banks when you ride quietly. Each clean trick adds a short speed boost that grows with the chain. A crash loses the unbanked line.

## Movement values

Gravity 20, hop 7.6 u/s (0.76 s of air on the flat, not enough for a flip; ramps and drops give 0.95–1.4 s). Camera widens from 3.8 to 4.7 half-height and the rider moves from 32% to 25% across as speed rises. Motor runs at 120 Hz with interpolated rendering.

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

One endless region (Porto Chiaro). No purchases, accounts, leaderboards or online services. Not yet tuned on a device: hop timing, flip rate, landing windows and the speed curve are starting values for the first playtest.

## Verification

`Tests~/ModelChecks.cs` (19 deterministic checks): seeded generation is repeatable; speed ramps; hazards keep appearing past 6 km; a road-reading bot survives 4 km on 8 random coastlines; doing nothing crashes early; flip-sized air occurs often; a tap on the ground only hops; a timed hold lands a clean backflip; over- and under-rotation crash; air taps land tricks and late tricks crash; crates crash unless hopped; bunting grinds; combo multiplier.
