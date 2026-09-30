# Shared studio foundation

Unity 6000.6.3f1. Landscape, iOS and Android, company **fostersdigital**. No GitHub Actions, analytics, accounts, advertisements, purchases or external runtime services.

The user's Unity request supersedes the reference pack's proposed Swift/SpriteKit implementation. The two original premise documents remain design inputs, not executable instructions or proof of completed work. Source pack content is not copied into this product. All generated art, motion, courses and synthesis code here are original.

`shared-foundation` owns the reusable framework. `bellavolta` and `hopward` descend from it and each supply a concrete module under Assets/Game. Cherry-pick shared fixes to both descendants; avoid merging an entire product branch into the other. Never switch Git branches while Unity is running.

## Boundaries

- `GameModule`: product identity, authored outings, palette, motor and rig contract.
- `Motor`: product simulation at 120 Hz. Previous/current snapshots are interpolated at render time. No arbitrary rigid-body tipping.
- `Course`: explicit smooth support curves and swept descending contact. The renderer reads the exact same curves.
- `GestureInput`: touch ownership, thresholding, optional controls, cancellation. No gameplay input behind menus.
- `StudioApp`: state transitions, slow terrain camera, recovery, progression and score orchestration.
- `Ink` / `Landscape`: original vector mesh, parallax, world events. Reused buffers and one mesh/material; no external artwork dependency.
- `StudioUI`: safe-area Canvas, journal, recovery, pause, settings, accessible control option.
- `Soundscape`: original eight-note study, quiet tactile contact and optional engine synthesis; separate music/effects levels.
- `StudioBuild`: local builds only, product-specific identifiers and unsigned iOS export.

## Art and motion contract

Six to eight palette roles, crisp active edges, quiet distant values, empty sky. Architecture uses broad forms and sparse marks. A restrained sun, water and inhabited-town actions establish place. Human limbs are constructed from shared contact anchors with two-bone geometry. Head dimensions never squash; compression belongs in knees and equipment. Camera elevation follows the road, never the bounce or suspension.

World scale: roughly 1.6 units per rider/equipment silhouette; camera 15.2 units tall; rider near 32% across landscape. Shapes are resolution independent, alpha blended and MSAA eligible. Rig redraw matches interpolated simulation snapshots. No generated frame drift, texture atlas, copied music or manufacturer branding.

## Extension skeleton

Create a product module, a Motor subclass, a static rig drawing method and authored Course data. A RuntimeInitializeOnLoadMethod creates StudioApp and calls Initialize(new ProductModule()). Set Resources/Product.json. Keep gestures, support physics and scoring rules inside that product's implementation. Supply palette presets and lessons through the contract. The foundation intentionally has no playable product selected.
