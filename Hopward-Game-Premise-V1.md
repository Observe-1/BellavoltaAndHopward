# Hopward — Game Premise V1

**A human pogo rider finding a beautiful line through living towns.**

Prepared 30 September 2026. Working title only. Companion shortlist: `Ten-New-Human-Trick-Concepts-Ranked.md`; foundational evidence: `Alto-Adventure-Research.md`. Existing alternatives remain in `Rooflight-and-Bellavolta-Game-Premises-V1.md`.

## 1. Brief and decision

The player controls a visible human performing tricks. The game travels through cities, towns and villages that feel inhabited. Movement should be impressive, controls basic and atmosphere welcoming. No skateboarding or humanless protagonist.

The latest correction governs the graphics: **large simple colour fields, very little surface detail, spare silhouettes and quiet depth**. The previous richly painted mock-ups were over-detailed. This premise follows the simplified regenerated Rooflight/Bellavolta direction, without reproducing Alto’s particular characters, course designs, landmarks, music or branding.

Hopward ranked first of ten new ideas at 8.70/10 using the same atmosphere/movement/expression/feasibility/living-world weights as the earlier round. That is design judgement, not evidence that it will outperform the existing concepts. This document specifies a game to prototype; no running build is claimed.

## 2. Player fantasy and distinctive hook

You are a young adult traveller on an original pogo stick, moving through welcoming places for the pleasure of movement. A square gives way to a canal path. One stronger rebound reaches a planted terrace. An aerial pose finishes just before the tip meets the next surface. A boat passes behind you while you continue towards the next neighbourhood.

**Hook: choose a flowing line of rebounds through a town.** Ordinary movement has a gentle repeating cadence; stronger rebounds change your available route. Tricks deliberately spend airtime and must return to a supported landing. The reward is a body and spring moving convincingly together, not merely a score popup.

There is no chase, combat, compulsory delivery or collapsing world. Background people have their own activity. The playable route is an authored public recreation path, separated from decorative traffic and pedestrians. It can exaggerate distances, but should still explain how a town connects.

In the first ten seconds the player should understand: the character moves forward and rebounds automatically; a tap timed near landing creates a higher next rebound; an airborne flick commits a trick that must finish before contact.

## 3. Core loop and choices

1. Follow a forgiving stretch and feel the base cadence.
2. Read the next surface, gap and optional higher path.
3. Choose a normal rebound or prepare a higher one near contact.
4. Use available airtime for a trick or keep the landing simple.
5. Return feet, hand and stick to a supported contact configuration.
6. Continue along the chosen line, then take a scenic breath.
7. Reach a neighbourhood landmark; replay for another view or a better line.

Important decisions: rebound height, commitment timing, which reachable route to take, whether a trick fits the remaining air, and whether to protect a sequence rather than risk a late flourish. More points are not the only reason to take an upper path: it offers a new view and a different local moment.

## 4. Platform and controls

Native iOS, initially targeting iOS 18 or later; landscape first. SpriteKit is the proposed active scene, with a proportionate SwiftUI shell. Confirm actual APIs and deployment support during implementation. No Xcode project is created here.

Use one finger on a broad playfield region, excluding pause and system gesture edges. The default scheme has three simple gestures:

| Gesture | Action | Commitment |
| --- | --- | --- |
| Short tap near a landing | Prepare one higher rebound at that contact | Changes the next arc and possible route |
| Upward flick while airborne | One no-footer | Feet leave the pegs, then return before landing |
| Downward flick while airborne | One peg grab | One hand reaches the foot-support area, then returns before landing |

Teach high rebound and no-footer first. Peg grab enters a later tutorial outing. Three gestures are still a design cost: if testing shows confusion, ship the slice with tap/no-footer and defer peg grab. Do not silently convert the requirement for basic controls into a gesture encyclopaedia.

### Recognition and ownership

Each touch resolves to exactly one action on release. An upward/downward displacement crossing the flick threshold consumes that touch and suppresses the tap. A cancelled or interrupted touch causes no action. Long holds have no hidden charge or balance meaning; repeated taps cannot stack rebound energy.

A high-rebound tap is accepted during the final approximately 0.30 seconds before predicted landing, or during the first approximately 0.08 seconds of a short compression phase. It applies to that immediate contact only. Prediction must agree with real geometry. A tap earlier in the flight does not store a surprise action for much later. An accepted input immediately changes the character’s preparation pose and gives one restrained acknowledgement.

These windows are initial tuning proposals. The teaching scene uses generous surfaces and an obvious descending pose. If the timing window still requires hidden knowledge, widen it or revise the scheme. No tiny target has to be tapped; position within the broad input area does not steer the character.

Airborne tricks require a clear airborne state and enough time to start the pose (initially about 0.18 seconds). Acceptance is not a guarantee of enough time to finish. Start with flick travel around 28–36 points within 0.22 seconds, tuned on devices. Permit at most one trick per flight. A second flick cannot cancel a committed move into guaranteed safety or produce a double jump. A late commitment can fail; the reason must be visible in the unfinished pose.

Offer an accessible button scheme with Rebound and one large Trick control; select no-footer or peg grab in the pause settings. The first slice can use only no-footer in this alternative. Controls must not require simultaneous fingers or device tilt.

## 5. Movement and honest physics boundaries

Use stable forward travel, a bounded arcade spring response and contact-aware human animation. Ordinary rebounding is automatic; the player’s high-rebound timing and trick choice supply agency. The controller is not a freely tipping rigid-body puzzle. Random tumbles would conflict with both simplicity and tone.

Keep a real contact relationship: the stick tip meets the active surface, the spring compresses, the rider bends, and the rebound releases that compression. During ordinary travel, the rider’s hands hold the handle and feet meet the pegs. During a trick, deliberately changed contacts are restored before landing. Never stretch the stick through a terrace to manufacture support.

Proposed initial base airtime: about 0.70–0.85 seconds; high rebound: about 1.00–1.20 seconds; compression/release: about 0.12–0.16 seconds. Cruise can begin around three to four standing-character heights per second. These values need a single consistent gravity/impulse model and course validation; they are not measurements or physical claims.

Do not use animation to teleport the collider. A separate visual rig follows the controller and its contact states. Course surfaces are explicit collision curves. The contact solver tests the stick tip, permitted body clearances and support normal. Small valid joins preserve motion. A failed route cannot be repaired by pulling the player invisibly towards a platform.

High rebounds need early readable consequences. Their longer flight changes where the next contact occurs. Authored landing zones must distinguish the normal and high arcs without requiring a perfect pixel. A broad lower route remains available during introductory outings. The active silhouette—not an arrow hovering over every gap—does most of the explanation.

Camera follows horizontal progress with bounded look-ahead. Vertical follow responds slowly to sustained course elevation, **not to every bounce**. A repeated camera bob would ruin the intended softness. Reduced-motion mode holds the camera even steadier and removes decorative parallax; it preserves the gameplay arc.

## 6. Tricks, scoring and failure

**No-footer:** both hands retain the handle while the feet leave the pegs briefly. The pose opens clearly, then closes to a supported landing. Start with a complete animation around 0.45–0.55 seconds. No foot drift or knee geometry that passes through the stick.

**Peg grab:** one hand reaches towards the foot-support area while the other holds the handle. It returns before contact. Start around 0.50–0.60 seconds. The pose must differ unmistakably from a no-footer at phone scale.

**Precision rebound:** a deliberate normal/high choice reaches an authored narrow-but-forgiving terrace and releases cleanly into the next arc. This is a contact skill, not an automatic bonus on every bounce.

These durations are prototype starting points. Do not add full body/stick flips in the slice; they would increase animation, clearance and originality costs before the core is proven.

Only score-focused play exposes points. Starting base awards: no-footer 80, peg grab 100, eligible precision rebound 60. Award aerial tricks only after a clean supported landing. Standard automatic rebounds earn no trick points.

A line continues when an eligible special movement occurs within roughly 2.5 seconds. Safe ordinary travel banks it when the window expires. Sum the base awards, then apply a multiplier once: ×1 initially, plus one for each further distinct eligible movement, capped at ×3. Repeating one move gives diminishing value. A stumble clears only unbanked awards. An unfinished aerial trick can cause a stumble or a larger miss depending on the visible contact.

Journey recovery returns to the latest safe landmark after a brief non-graphic interruption, preserving discovered routes. Flow Run ends after a major miss and offers immediate retry. Failure must reset pending touches, spring state, camera and background triggers consistently. No injury animation, lives timer or paid retry.

## 7. Journey, progression and the living world

Journey is the main mode: authored outings of roughly 60–150 seconds, each ending at a recognisable place. A quiet canal landing, a garden gate or a square with a fountain is an arrival. Flow Run can later assemble compatible course chunks inside one region for scored replay. A relaxed recovery mode is a later option, not a requirement to weaken the main game.

The travel journal records arrivals, alternate lines and a few illustrated local moments. Discoveries open connected outings; essential movement is available through teaching rather than grinding. No upgrade increases the rebound so far that old routes become unfair for a new player. A premium release or generous opening followed by a permanent unlock could fit the tone; pricing and purchases are outside the prototype.

### Full-game places, not first-build promises

| Destination | Simple visual identity | Play and independent life |
| --- | --- | --- |
| **Mallow Quay** — canal town | Honey blocks, slate-blue water, reed clusters and one arched bridge | Quayside low route and optional garden terraces; one barge wake, a window opening, a gardener |
| **Cressa** — hillside villages | Muted coral roofs, pale sky, rounded olive-grey slopes | Stepped recreation paths and stone garden walls; laundry, a distant cart, a market canopy |
| **Glassmere** — lakeside city | Broad cool lake band, spare pale towers and planted public terraces | Promenade routes and broad bridge approaches; a distant ferry and evening window lights |
| **Fernbank** — wooded town | Sage tree masses, cream buildings, low hill silhouettes | Raised boardwalks and garden paths; a train behind the trees, a gate opening, drifting seeds |
| **Sunwell** — inland villages | Warm ochre blocks, lilac distance and a few simple cypress shapes | Low aqueduct recreation lines and planted squares; fountain motion, shutters, café preparation |

The prototype contains Mallow Quay only. Destination changes occur through the journal, not a new geography appearing during a leap. Each district shares coherent materials and terrain. No hazardous rooftop shortcut through someone’s living room just because it makes a gap convenient.

Maintain three scenery tiers: quiet continuous ambience, sparse local actions and occasional landmarks. Continuous examples are a slow cloud shift or a thin water accent. Local actions include an opening shutter or a gardener pausing. A landmark might be a barge entering a bridge opening during a forgiving stretch.

At most one prominent background event should ask for attention at once. Use cooldowns and spatially plausible anchors. A tiny figure can perform one meaningful two-pose action; they do not need a detailed face or independent animation on every limb. A living minimalist world is built from timing, not thousands of decorative objects.

## 8. Exact art direction

The art must resemble a playable **minimal 2D game**, not a finely painted travel poster. Simplification is a production requirement, not a final filter.

### Colour roles: Mallow Quay, late afternoon

| Role | Proposed colour | Use |
| --- | --- | --- |
| Sky | `#E9D8B4` pale honey | Largest uninterrupted area |
| Sun and tiny highlights | `#F4EBD4` cream | One simple disk and sparse accents |
| Far hills | `#B8BDC2` quiet grey-lilac | Broad low silhouettes |
| Far settlement | `#A4A9B6` subdued lavender-grey | Grouped blocks and one landmark |
| Water | `#7FA8A7` muted teal | Flat horizontal depth band |
| Midground town | `#C7A78E` dusty warm stone | Simple building groups |
| Course, near shapes and human | `#394D61` deep muted ink | Strongest readable silhouettes |
| Character accent | `#BE7867` muted coral | One small shirt patch |

These eight roles are an art budget, not a ban on necessary antialiasing or lighting interpolation. Avoid introducing independent colours for every building. Distant groups mostly share a value. Night and daylight have their own complete role presets; do not simply dim the whole frame.

Sky occupies roughly 40–50% of the composition. It may have a very gentle gradient, but no elaborate clouds. Hills are two or three uninterrupted silhouette planes. Architecture uses rectangles, roof wedges and one recognisable bridge shape. Near buildings may carry one door or window grouping; distant buildings carry almost none. Foliage is a small set of clusters, not individually drawn leaves.

No stone texture, tile patterns, paper grain, painted material noise, gloss, realistic shadows, per-object outlines or blurred foreground. A small flat shadow under a contact may be useful; it must not become a realistic lighting simulation. Course edges stay crisp. Decorative surfaces use less contrast and cannot masquerade as the active route.

### Protagonist and pogo identity

One young adult, ordinary human proportions, a small simple helmet, short coral top, ink trousers and pale shoes. A compact head/hair shape is enough. No facial detail, scarf, flowing coat or oversized cartoon eyes. At projected standing height around 8–9% of the screen, the human should be readable through pose.

The original pogo has a narrow ink shaft, short horizontal handle, two clear foot supports and a small pale contact tip. It is neither a scooter nor a stick extending from the rider’s ankle. Approve a side-view reference sheet with hand, peg and tip anchors before animation. Equipment and rider scale stay fixed across every move.

The visual signature is **the upright human-and-spring shape changing into an open-legged aerial pose over a quiet architectural line**. That signature should survive a cropped screenshot without a title attached.

### Depth and camera

Side-view collision plane, with flat layered scenery. Initial parallax factors relative to course motion could be 0.02 sky, 0.08 hills, 0.18 far town, 0.40 local background and 1.0 active course. These are proposals. Keep the rider near 30–33% of screen width, with the next support visible ahead. Peripheral foreground silhouettes cannot cover the tip, gap or landing.

## 9. Animation, audio and tactile feedback

Animation families: idle supported pose, ordinary compression, high-rebound preparation, release, rising, apex, falling, supported landing, no-footer opening/hold/return, peg-grab reach/return, stumble and recovery. Use a consistent small rig or coherent sprite strip. Never generate every frame independently and hope the peg positions match.

The rider compresses at knees/hips while the stick telescopes within its designed mechanism. Release transfers visible energy into the arc. Do not squash the head or stretch the limbs to sell the spring. A no-footer retains both hand contacts. A peg grab retains one. The return pose restores support before contact.

The rig follows movement states; it cannot rescue an impossible collision. Inspect the animation at actual phone scale and normal speed. A beautiful still is not acceptance of an animation family.

Commission original music: sparse warm keys or plucked tones, sustained air and generous phrase space. The score can carry gentle forward momentum without matching every bounce. It should not turn into circus music, a comedy cue or a copy of an Alto melody. Region variants share an original motif while changing ambience.

The pogo contact is a soft tactile tick with a brief rounded spring release, not a repeated cartoon boing. Several restrained variants prevent obvious repetition. Surface changes can modestly alter the contact. Leave room for water and a distant local event. Avoid fanfares at every trick and constant vocal effort sounds.

An accepted high rebound gets one small haptic acknowledgement if enabled; ordinary travel does not buzz continuously. A meaningful clean trick landing may have a short different cue. Provide separate effects/music and haptic controls. On-device checks must include ten minutes of repeated contact audio through speakers and headphones; this premise claims no such test has happened.

## 10. HUD, screens and accessibility

Flow Run HUD: distance upper left, pause upper right, short landed-trick feedback near the upper centre. Example strings: `1,240 m` and `NO-FOOTER +80`. Feedback credits a completed trick, clears quickly and never covers the hands, tip or next landing. Journey can omit points and show only essential progress/arrival information.

No permanent jump gauge, complicated energy meter, mission list or collectible counter over the active route. Accepted high-rebound input is communicated by an immediate pose/small cue. The player should read the descending body and approaching surface rather than monitor a dashboard.

Representative screen requirements:

| Screen/state | Direction |
| --- | --- |
| Start/continue | A simple resting rider beside the next path; clear continue action |
| Journal/outing selection | Small original route illustration with arrivals and discovered branches |
| Teaching | One short demonstration at a forgiving stretch, then immediate play |
| Active run | Protected playfield and minimal safe-area HUD |
| Pause/settings | Resume/restart, control scheme, sound, haptics and motion settings |
| Arrival | A brief scenic rest and journal mark; continue or replay |
| Stumble/retry | Clear cause and immediate recovery/retry; no long results ceremony |

Use at least a 44-point pause target, even if the icon is smaller. Preserve safe areas across device proportions. Support high-contrast course edges, reduced decorative movement, optional buttons and separate audio controls. No essential cue depends only on colour, sound or haptics. Describe accessibility assistance honestly if score comparison is later added.

## 11. Asset manifest and native implementation direction

Begin with one approved human/equipment seed and one Mallow Quay kit. The following is a first-slice manifest, not a completed asset inventory.

| Family | First slice | Required consistency |
| --- | --- | --- |
| Rider/pogo | One identity, the movement families in section 9 | Shared scale; handle, hands, pegs, feet and tip anchors |
| Course | Six authored surface sections plus two validated optional-branch joins | Visual edge and collision curve agree; shared endpoint datum |
| Scenery | Two hill strips, three town groups, one bridge, three foliage shapes | Eight-role palette; scale and value assigned by layer |
| Life | One barge, two simple people actions, one shutter/laundry action | Plausible anchors and cooldowns; no active-course collisions |
| Atmosphere | Day and late-afternoon sky/role presets; one sparse water accent | No grain, fog pile or texture drift |
| UI | Pause, journal/arrival marks and restrained labels | One original lightweight type/icon language; accessible bounds |
| Audio | One original cue, contact variants, water/local-event ambience | Loop and loudness notes; no copied recordings |

Retain source files and record names, provenance/licence, dimensions, rendered scale, anchors, transparency, animation rate and collision role. A possible convention is `hopward_mallow_course_bridge_01`, with movement families named explicitly. Texture dimensions follow actual rendered points and export scale, not arbitrary giant canvases. In a later build, capture those concrete dimensions in the manifest before multiplying assets.

Flat scenery can be authored as simple original paths or clean small textures. Bundle compatible sprites in atlases. Choose the rendering method from profiler evidence and maintain the same visual output. Generated concept screenshots do not supply separated layers, a contact rig, collision curves or a usable sprite sheet.

For the native slice, separate input recognition, movement/contact state, animation, chunk validation, scoring, background events and scene presentation. Gameplay state changes drive the HUD. SwiftUI should not be rebuilt at the scene’s render frequency. Use a stable coordinate system and later world rebasing if endless travel requires it.

Target stable 60 fps, with profiling on actual devices. Prioritise contact and input before background events. Cull off-screen art and reuse nodes/effects. Avoid large transparent overlays and per-frame allocations. No performance result is claimed; these are implementation targets.

## 12. First playable slice and validation gates

Three authored outings in Mallow Quay, one rider, one pogo, two light presets and a small living-world set:

1. **Quayside Start, about 60 seconds:** base cadence, high rebound timing, forgiving contact and arrival at the bridge.
2. **Garden Reach, about 90 seconds:** optional high route and introduced no-footer; scenic lower route remains viable.
3. **Bridge Line, about 120 seconds:** precision contacts, optional peg-grab teaching and a sequence requiring a deliberate choice about trick airtime.

Durations and content are proposals. Implement the base spring/contact loop first. Add high-rebound timing, then no-footer, then one optional branch. Add the second aerial trick only if controls and phone-scale poses remain clear. Living scenery and the final palette enter after the movement is readable, not before.

Authored chunks record entrance position/speed, exit position/speed, support normals, full human/pogo clearance, eligible trick airtime and difficulty. Validate joins against normal and high arcs. A future generator selects compatible chunks; it does not randomise every gap independently.

Required prototype features: pause/resume, recovery/retry, local discoveries, audio/motion settings and accessible input. Defer multiple destinations, online leaderboards, purchases, vehicle upgrades and a large move catalogue.

Acceptance gates for a future build:

- Four of five first-time formative testers finish Quayside Start within two attempts without spoken help. This is a small usability check, not a statistical retention finding.
- They can explain when a tap changes the next rebound and why a late trick failed. If not, revise feedback/windows before expanding courses.
- At least one optional line demands a real choice, and repeating an outing can produce a materially different view or sequence.
- The stick tip meets the surface without tunnelling, feet/hands meet their supports, and accepted high inputs cannot produce a hidden future rebound.
- Touch cancellation, pause during compression, background/resume during a trick and retry restore consistent state.
- Camera does not bob at the bounce frequency. Contact sounds remain tolerable in a ten-minute session.
- Day and dusk screenshots preserve rider/course contrast. Decorative bridges and paths cannot be mistaken for active surfaces.
- Simulator and phone checks cover teaching, active play, interruption, pause, arrival, stumble, high-contrast/reduced-motion settings and both gesture/button controls.

Nothing has been built or playtested here. These gates define what would count as evidence in the next phase.

## 13. Originality and self-critique

Pogostuck establishes existing human pogo traversal; Pogo Stick Champion establishes that a relaxing pogo presentation also exists. The new game cannot rely on “pogo but calm” as its original contribution. Its proposed expression comes from an original traveller and equipment design, town-route grammar, bounded rebound preparation, short trick commitments, journal progression, living-place timing and independent music. [N01, N02]

Do not copy competitors’ physics tuning, routes, character poses, UI, scenery, names or recordings. No similarity percentage or changed palette proves legal safety. This document has not performed title, trademark, patent or final-asset clearance. The earlier research covers the general idea/expression distinction and platform copycat rules; this premise does not add a legal guarantee.

My main objections to my own selection:

- **Automatic bouncing could be decorative rather than gameplay.** High-route reach and deliberate trick timing must create decisions; ordinary rebounds alone cannot carry the product.
- **The cadence could be tiring.** Stable camera, restrained audio and scenic breaths are central. If a player wants the bouncing to stop after a minute, revisit the mechanic rather than adding more scenery.
- **A pogo stick could feel childish.** Mature restrained art and grounded human movement help, but cannot manufacture appeal. Test the fantasy with real players.
- **Three gestures could exceed the intended simplicity.** No-footer alone may be the better starting vocabulary. Peg grab must earn its complexity through a distinct readable risk.
- **The novelty score is uncertain.** The precedent check was targeted, not comprehensive. Stronger differentiation may be needed after broader competitive review.
- **Minimal graphics still require refinement.** Without texture to hide mistakes, contact, layer composition and colour relationships matter more.

Hopward is the best of this new shortlist for a prototype, not a commitment that it replaces Bellavolta or Rooflight. Reject it if the small slice does not produce voluntary replay.

## 14. Gameplay screenshot brief and generation prompt

Create one full-bleed landscape iPhone gameplay concept, roughly 19.5:9. No phone bezel, status bar, title/logo, watermark, collage, promotional heading or visible touch-button cluster. It must look like a game halfway through a run.

Mallow Quay, late afternoon. The upper half is largely empty pale honey sky with one small cream sun. Two broad grey-lilac hill silhouettes and a sparse distant town establish depth. A flat muted teal canal crosses the background. One tiny barge leaves a thin wake; one person by a shutter and a simple gardener shape suggest local life. One restrained arched bridge is enough. No detailed clouds, leaves, windows, stonework, roof tiles, grain or realistic rendering.

A crisp dark-ink public terrace path lies around 67% down. It has a modest gap and a clearly reachable broad landing to the right, around one-and-a-half standing-human heights from the take-off. The next active edge is distinct from the decorative canal bridge. Show substantial look-ahead and only sparse peripheral foliage masses.

At roughly 32% of screen width, a small recognisably human pogo rider is in the upper part of an arc performing a no-footer. Their projected standing height is around 8–9% of screen height. Both hands hold a short horizontal handle; the narrow vertical pogo shaft hangs below them, foot pegs visibly empty, both feet lifted outward, body upright enough to restore the landing pose. Small cream helmet, coral shirt patch, ink trousers, pale shoes. The equipment has no wheels, manufacturer mark or oversized mechanism. The stick tip is visibly airborne, not accidentally touching the scenery. Coherent human proportions and equipment anchors matter more than detail.

Minimal ink-coloured thin sans-serif HUD: `1,240 m` top left, small pause symbol top right, short `NO-FOOTER +80` upper centre. The transient feedback credits a preceding completed no-footer while another is underway. This is an art/composition target, not a captured running app.

**Prompt policy:** emphasise flat 2D graphic silhouettes, only six to eight main colour roles, sparse architectural forms, original human/equipment identity and atmospheric depth from value. Avoid describing elaborate materials and then expecting the generator to simplify them. Do not import an Alto screenshot as production art.

## 15. Sources and studio-reference application

- **[N01]** [Pogostuck official site](https://www.pogostuck.com/) and [developer Steam listing](https://store.steampowered.com/app/688130/Pogostuck_Rage_With_Your_Friends/). Existing human pogo traversal precedent; product description retrieved through search.
- **[N02]** [Pogo Stick Champion developer Steam listing](https://store.steampowered.com/app/3571800/Pogo_Stick_Champion/). Existing relaxing-aesthetic pogo platformer; product description retrieved through search. Direct opening failed, so no frame-by-frame competitor visual study is claimed.
- `Alto-Adventure-Research.md`: source-separated analysis of clarity, depth, motion, sound and originality. Its direct-audio/device limitations remain.
- `IOS-GAME-STUDIO.md`: concept and art sheet before implementation; native routing and service policy.
- `PROJECT-DESIGN-REFERENCES.md`: player decisions before HUD; protect the active route; deliberate touch, accessibility and state design.
- `PROJECT-ASSET-QA-REFERENCES.md`: approved identity, shared scale/anchors, coherent animation families and actual-size review.
- `PROJECT-SWIFT-REFERENCES.md`: SpriteKit active scene and proportionate shell; snapshot version examples do not set the deployment target.
- `MANUAL-SETUP.md`, `README.md`, `SOURCES.md`: reference/provenance boundaries; optional services are not prerequisites.

All values, art roles, worlds, mechanics, scores and prototype gates are original proposals unless explicitly attributed. No existing game assets or audio are authorised for reuse. This session produces documents and concept imagery, not a shipped game.
