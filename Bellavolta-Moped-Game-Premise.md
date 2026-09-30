# Bellavolta — Italian Moped Game Premise

**A human rider performing flowing tricks through beautiful Italian coastal towns and hill villages.**

Standalone edition, 30 September 2026. Working title only; release-name clearance has not been performed. This brings the moped concept out of the combined Rooflight/Bellavolta document and incorporates the latest request for much simpler colours and detail.

This is a concept and prototype specification, not a built or tested game. Companion research: `Alto-Adventure-Research.md`. The existing regenerated moped gameplay mock-up is the visual direction.

## Fantasy and identity

You are a young adult Italian rider travelling between fictional coastal neighbourhoods, hill villages and small towns. The pleasure is taking a graceful line: lift the front wheel along a clear promenade, settle it before an archway, pop from a little rise and return smoothly to the road as the harbour opens behind you.

The protagonist is always recognisably human. They lean, stand briefly on the foot supports, absorb landings and look towards the route. The moped does not perform independently while the rider stays rigid.

The world is contemporary but gently timeless: warm plaster, shutters, planted balconies, hillside olive groves, corner cafés and small harbours. Local life is observed through specific actions and geography rather than caricature. No compulsory delivery job, countdown, police chase or traffic combat. People and moving cars occupy separated background streets.

**Hook: make an elegant riding line through a town that carries on around you.** Bellavolta’s character comes from the coordinated rider/machine motion, the road’s changing profile, and the way a town opens into countryside. It is not simply Alto with different equipment.

## Controls and tricks

The moped cruises automatically. Its modest speed lets the player read what is ahead. One finger provides two input families: a hold and an upward flick. Do not require device tilt, manual throttle, gears or multi-finger combinations.

| Situation and input | Action | Decision |
| --- | --- | --- |
| Hold on the road | Rider shifts weight back into a wheelie after a brief recognition delay | How long to balance and when to return to two wheels |
| Release | Front wheel settles with damped motion | Prepare for an arch, dip or landing |
| Upward flick near a readable road lip | Suppress the hold action and commit a short assisted pop | Choose the raised scenic line or stay low |
| A fresh upward flick in sufficient airtime | Commit one modest tabletop flourish | Spend airtime on style and return the wheels for landing |

Every fresh touch records whether it began grounded or airborne. A grounded touch is never retrospectively reassigned to an aerial trick. A short flick recognised during the initial hold window cancels wheelie preparation. A non-flick tap has no separate secret trick. Teach each context in a forgiving sequence. Offer separate wheelie and hop/trick buttons as an accessible alternative.

Start wheelie recognition around 0.12 seconds and tune on devices. Hold gradually raises pitch towards a manageable balance band. Remaining held too long pushes beyond that band and risks a stumble. Release actively lowers the front wheel, so a long wheelie is a timing decision rather than an indefinitely sustained animation. The initial balance band, maximum angle, damping and recovery time are prototype variables, not solved physics.

A road-lip pop is an arcade exaggeration of rider weight and suspension. It needs a visible rise and cannot happen anywhere in empty space. An ordinary ramp still launches naturally without the flick; the pop provides the extra height needed for a clearly shown optional path. There is no mid-air double jump.

The tabletop briefly leans the moped sideways while the rider stays attached and then returns it upright. It needs a coordinated rig and a limited simulated depth rotation even though the collision course remains side-view. Keep the flourish modest, about 0.65 seconds as an initial target, and allow it only once per jump. Do not disguise a generic full backflip as believable moped movement.

First vocabulary: **wheelie, lip pop and tabletop**. Small rear-wheel-only landings could become a later extension, not a fourth first-build system. Variation should come from when and where these actions are useful, not twenty swipe gestures.

## Movement model and challenge

Use a stable arcade chassis with front/rear contact probes, bounded suspension compression and explicit rider poses. Two wheels follow the actual road profile. Pitch, rider weight and road curvature explain the movement. Avoid an unconstrained rigid-body simulation that requires constant correction and creates random failures.

A small number of authored road features create the challenge: gentle crests, shallow dips, clearly separated promenade branches and occasional low clearances. Wheelies earn style on clear stretches but should finish before a low arch or awkward crest. Raised branches have broad, visible landings. Do not fill the scene with invisible blockers or place pedestrians in the active route.

A recoverable mistake produces a brief wobble and loses the current unbanked line. A larger miss returns to a nearby terrace checkpoint in Journey; Flow Run can end the scored attempt. No injury spectacle. The point is to improve a line and enjoy the town.

Suggested score starting values: a completed wheelie segment 80, a clean optional-route pop 60 and a landed tabletop 120. Credit wheelies only after the front wheel settles cleanly; no infinitely accumulating score while held. Repeated wheelies alone receive diminishing rewards. A line continues when eligible actions occur within roughly 2.5 seconds. Ordinary safe riding banks it after that window. Add base awards, then apply a multiplier once: ×1 initially, plus one for each additional distinct eligible movement, capped at ×4. A stumble clears only the unbanked line. Values require playtesting.

## Places and living artwork

| Destination | Identity | Riding and background moments |
| --- | --- | --- |
| **Porto Chiaro** | Coastal town: terracotta, pale plaster, blue-green harbour | Broad promenade rises, planted retaining walls; a distant ferry, café shutters opening, a laundry line moving in the breeze |
| **Valle Oliva** | Hill village among olive groves and soft inland ridges | Winding road represented by coherent side-view sections, stone bridges and readable crests; a distant tractor, gardener and changing shadows |
| **Pietra Alta** | Small stone town with arches and planted squares | Lower streets and an optional outer-wall route; a fountain, market setup, bicycles on a separated lane |
| **Riva Sera** | Compact waterfront city at dusk | Canal-side road and bridge approaches; reflected windows, a local bus on another street and people closing shops |

Prototype only Porto Chiaro. Other destinations describe the vision. Keep the ride on roads, promenades and purpose-built recreation routes rather than implausible rooftop motorbike chains. Long travel between destinations occurs through a short route-map transition. Geography should explain what appears behind the road.

Use the same softness principles as the research, with a distinct art sheet: more curved road profiles, planted roadside mass, shutter shapes and coastal light. The moped has an original angular frame, narrow leg shield, small circular lamp and exposed dark mechanical details. Avoid tracing any real manufacturer’s bodywork, badge, lettering or distinctive model silhouette. A generic functional description is not a clearance opinion.

The rider wears a cream helmet, rust-coloured short jacket, teal trousers and pale shoes. They remain large enough to read the body movement. The vehicle’s muted teal colour contrasts with warm stone. Design the side pose, wheel contact anchors, seated/wheelie/jump/tabletop poses and helmet identity on one approved sheet before animation.

## Simplified art direction — latest approved direction

Use the visual restraint discussed in the Alto research: a small readable human/moped silhouette, generous empty sky, large flat colour regions and sparse architecture. Create original settings and shapes. The earlier richly painted image is superseded by the simplified moped mock-up.

Limit a scene to roughly six to eight main colour roles. Depth comes from overlap, scale and quieter distant values. No stonework, roof-tile patterns, individual leaves, detailed clothing, realistic clouds, photographic lighting, material grain or glossy 3D rendering. Near buildings need only a roof profile and a few shutter/door marks; distant settlements are grouped blocks.

| Role | Starting colour | Purpose |
| --- | --- | --- |
| Sky | `#EAB991` dusty apricot | Broad open upper half |
| Sun and small highlights | `#F4DDB1` pale cream | One simple disk and sparse accents |
| Far hills | `#A7A4B8` grey-lilac | Quiet atmospheric silhouette |
| Distant settlement | `#B8A0A1` muted rose-grey | Small grouped building shapes |
| Harbour | `#76A2AD` muted blue-teal | Flat broad water band |
| Near town | `#D19B83` dusty plaster | Simple local architectural mass |
| Course and dark rider parts | `#493F54` warm plum | Crisp active edge and readable contacts |
| Rider accent | `#C77D66` muted coral | One jacket patch; equipment uses existing teal |

These are proposed role colours, not exact measurements from another game or the generated mock-up. Validate rider/course contrast in daylight and dusk. A helmet may use the pale highlight role rather than introducing a new colour.

Frame the rider around 30–33% across the landscape screen. Keep the road near the lower third, with substantial look-ahead. Camera follows forward progress and sustained terrain elevation, not every suspension movement. Foreground plants remain sparse and cannot obscure tyres, lips or landings.

## Screens, accessibility and production requirements

Required screens: start/continue, route journal, short teaching scene, active play, pause/settings, arrival and immediate recovery/retry. Protect the playfield. In Journey, keep points hidden; in Flow Run, use restrained distance and brief completed-trick feedback. Pause has a minimum 44-point touch target and safe-area allowance.

Offer separate sound/music controls, optional haptics, reduced decorative motion, high-contrast active surfaces and the alternative wheelie/hop buttons. Essential feedback must not depend on sound or colour alone. Cancelled touches, pause, interruption and retry clear pending input and reset movement consistently.

Approve one rider/moped reference sheet before making animation: shared rider proportions, wheel scale, wheel/handlebar/foot-support anchors, seated pose, wheelie, launch, tabletop, landing, wobble and recovery. Hands and feet must remain attached where the move requires them. Do not generate unrelated animation frames with drifting equipment.

First-slice assets: one rider/moped rig; six road chunks and one optional branch; a small coastal building kit; two far landscape layers; a ferry, a shutter action and a laundry action; two light presets; original audio and minimal UI. Record provenance, dimensions, rendered scale, anchors, animation families and collision roles in the asset manifest.

Before expanding, test hold/flick recognition, rear-wheel support, settling before crests, natural launch versus assisted pop, tabletop completion, route joins, pause/resume and actual phone-scale readability. Target stable 60 fps and verify with device profiling; no benchmark is claimed here. Listen to a long session to check whether the engine and frequent contacts remain pleasant.

## Sound and presentation

Give the engine a quiet, rounded putter rather than a constant aggressive buzz. Layer wind and wheel/road contact subtly; engine pitch follows motion without dominating the mix. Commission original warm keys, lightly plucked guitar and restrained brushed rhythm if musical experiments support the mood. These are proposals, not a requirement to imitate a stereotype of Italian music.

A crucial test is whether the engine remains pleasant after ten minutes. Allow effects volume to reduce it without losing all useful contact cues. Background cafés and water should occasionally emerge during a quiet road section.

Landscape camera, generous look-ahead and minimal HUD remain suitable, but the road has its own gently curved identity. At most one prominent background event competes for attention. In Journey, reaching a village square is an arrival, not a currency payout. A simple illustrated route journal records towns and discovered promenade branches.

## Neighbours and independent expression

Italian travel and motorbike tricks already have game precedents. Wheels of Aurelia is a narrative driving journey through 1970s Italy; Urban Trial Tricky mixes motorbike tricks, platforming and racing. Neither is evidence that this precise premise is unique, and neither supplies assets or an art style for copying. [C04, C05]

Bellavolta’s proposed identity is a contemporary, gentle moped journey with restrained tricks, one-finger line decisions and inhabited regional scenery. Avoid borrowing another product’s character, route geometry, score presentation, soundtrack or branding. Review the finished title and assets before release. No legal safety guarantee is made.

## Focused moped prototype

Build one approximately 90-second Porto Chiaro outing from six authored road sections, with one clearly optional raised branch, one rider/moped rig, the three tricks, day and late-afternoon lighting, and a small set of believable background events. Use SpriteKit for active gameplay and a proportionate SwiftUI shell for menus and settings. Start with an iOS 18 deployment direction; verify specific APIs when implementation begins. Include retry, pause, local discovery saving and alternative buttons. Defer multiple towns, extensive vehicle customisation and simulated traffic.

Prove the engine sound and hold/flick separation before investing in extra environments. Test a wheelie settling before a crest, natural ramp launch versus assisted pop, tabletop completion versus early landing, route joins, foreground readability and pause during each action. Device checks must include a long session with audio, not just a pretty screenshot.

## My assessment before treating it as the preferred direction

| Criterion | Score | Reason |
| --- | ---: | --- |
| Visual/emotional fit, 25% | 10 | Italian coastal and village geography strongly supports the requested warmth and scenery |
| Movement potential, 25% | 9 | A human/machine pair communicates weight, wheelies and jumps clearly |
| Distinctive expression, 20% | 7 | Strong theme, but travelling and stunt-riding games already exist |
| Feasibility, 20% | 8 | A constrained road controller and one rider rig are plausible; coordination still needs careful work |
| Living places, 10% | 9 | Roads naturally pass cafés, gardens, harbours and town squares |
| **Weighted total** | **8.65/10** | Comparative design judgement, not market validation |

Bellavolta narrowly exceeds Rooflight’s 8.50 in this judgement. It is my preferred next prototype **if the wheelie/pop/tabletop sequence proves fun and the engine remains gentle**. Rooflight remains a separate developed alternative; this score is comparative judgement, not a measured result.

The biggest risk is that wheelies become a single repetitive gesture over attractive backgrounds. Meaningful release timing, optional branches and mixed trick opportunities must prevent that. The second risk is that a petrol-powered vehicle undermines the requested softness. Original sound design and controlled speed are central, not optional polish. The third risk is an awkward compromise between cartoon stunts and grounded local atmosphere; exaggerated motion must still explain its contacts and landings.

Do not increase the trick catalogue to hide a weak first mechanic. First make one short journey something a player voluntarily repeats.

## Bellavolta screenshot brief

Create a full-screen landscape iPhone gameplay concept, ideally 19.5:9, with no phone bezel, status bar, title logo, watermark or visible touch buttons. This is a comparable mid-run screenshot target, not a marketing poster or actual app capture.

Porto Chiaro in soft late-afternoon light: warm terracotta and plaster houses, sage shutters, planted balconies, turquoise harbour, a small distant ferry, olive hills and a café beginning to open. Use only a few moving-life cues: a person at a shutter, a laundry line and a distant boat. Keep the active road visibly separate from background streets.

The rider sits at approximately 32% of screen width, performing a modest wheelie on a broad stone promenade. Rear tyre touches the road; front wheel is visibly raised; the rider leans back with hands on the handlebar and feet supported. Cream helmet, rust jacket, teal trousers, pale shoes. Muted teal original moped with narrow shield and simple angular frame. No real brand, copied production-model silhouette or scarf.

Road around the lower-middle of the image, a gentle crest ahead, generous sky and clear look-ahead. Minimal flat 2D illustration with crisp contact geometry, broad simple colour fields and soft distant silhouette layers. No paper grain, painted texture or detailed materials. Minimal deep blue-grey HUD: `1,240 m` upper left, pause icon upper right, brief `WHEELIE +80` upper centre. The score credits an earlier completed wheelie while a new one is underway.

## Reference context

- `Alto-Adventure-Research.md`: sourced analysis of visual softness, gameplay rhythm, music attribution, recreation methods and originality limits.
- `IOS-GAME-STUDIO.md` and the attached design, asset/QA and Swift reference collections: concept first, an original art sheet, protected playfield, shared asset anchors, native scene/shell boundaries and device verification.
- **[C04]** [Wheels of Aurelia official page](https://www.wheelsofaurelia.com/PC.html): existing Italian narrative driving precedent.
- **[C05]** [Urban Trial Tricky official page](https://urbantrialtricky.com/): existing motorbike trick/platforming precedent.

All destinations, tuning values, score values, palettes and mechanics are proposals. No game implementation, direct playtest, final title clearance or legal immunity is claimed. Existing games provide research context, not reusable characters, art, recordings or level designs.
