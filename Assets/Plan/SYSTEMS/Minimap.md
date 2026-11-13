# Minimap and expanded map

Implemented 2026-11-13 on PlayerHolder, inherited by Quest_Test. Reuses the existing MapCamera, RawImage and HUD Canvas; no second rendering camera. MapController remains at Assets/Scripts/World/Map/MapController.cs with its original GUID and floorText field.

## Behavior and integration
- LateUpdate follows the assigned player in world space at +45 Y, with absolute rotation (90,0,0): world +Z is north. No rotation smoothing/position lag that could displace the centered player.
- Centered TMP heading marker rotates with player yaw. Fixed TMP N bubble sits at the northern rim of a circular uGUI Mask, with a muted gold border and floor label.
- M toggles the expanded map; M/Escape closes it. PlayerControlState owns action handling, cursor and gameplay gating. The world is not paused. Existing dialogue/shop/journal priorities remain; opening the map requires gameplay eligibility.
- Expanded view uses 82% of the Canvas short side and a 100-world-unit orthographic half-range; compact view is 220 Canvas units with half-range 30. This is a larger local overview, not automatic fitting of the entire procedural dungeon, pan/zoom, fog of war or discovered-room mapping.
- Camera culls Default, Wall, Ground, Object and Map layers; excludes Character, Enemy, Item, UI and Attack. World meshes must use appropriate layers. No floor isolation/roof removal is implemented.
- Each runtime controller creates one owned 512x512 depth-16 RenderTexture, released on destruction. Existing shared Display 1 asset is retained as prefab preview wiring. No per-frame searches or allocations; simple transforms and floor-change label refresh. Main camera is untouched.
- Missing/destroyed target safely stops follow. Scene-local ownership naturally recreates output on scene reload; no global player search. Alternate PlayerContent prefab has not been migrated.

## Original defects and repairs
MapController was only a one-shot floor-label placeholder. MapCamera inherited player yaw; CharacterMapIcon was inactive; camera mask omitted Wall and included Character. The existing HUD had no circular masking or north marker. Repairs add explicit follow/world rotation, UI marker and owned render output while retaining existing assets/references.

## Changed files and tooling
MapController.cs, PlayerControlState.cs, PlayerHolder.prefab, and mehh.inputactions (Map/M action). New UI objects live in the existing prefab; two generated geometric UI sprites, MinimapCircle.png and MinimapRing.png under Assets/GamePlay/Character (256px, uncompressed, no mipmaps). AgentScripts/Minimap holds inspection/build/runtime validation tools and exact pre-task backups of the touched prefab/scripts, including pre-existing uncommitted work. Do not import recovery source into Assets.

## Validation
Unity 6000.3.28f1 Pipeline compiled the changed runtime scripts and builder. Quest_Test Play Mode checks passed: assigned controller/output, 512 texture, world-position following after displacement, yaw independence, marker rotation, Map action existence, expanded gameplay gating without pause, range/size expansion and restoration. Validation logs use [Minimap validation]. Screenshots inspected; capture aspect ratio requires matching the Game view because capture resizes its output.

Known existing scene errors: Enemy Animator Punch - Right events EnableMovement/DisableMovement/EnableDamage/DisableDamage have no receiver. These prevent claiming a clean whole-scene console. Production flow, procedural floor changes, actual dialogue/shop overlap, scene transitions, profiler measurements and alternate-prefab coverage remain unverified. Actual keyboard M open/toggle and Escape close passed through the Input System. Screenshots were inspected at fixed 1280x720 and 800x600; original Game view selection is restored afterward. Quest_Test minimap-only prefab presentation overrides were reconciled (background/floor text); other scene content was preserved.
Final polish: replaced the low-resolution built-in mask with smooth geometric sprites and a separate gold ring. Runtime assertions were rerun and passed after polish. Validation log lines were read from Editor.log. Play Mode stopped; auto-tick disabled and original Game view selection restored. Two Minimap QA fixed-resolution entries remain available in the Game view selector. Pipeline references consulted: commands/scripts.md and commands/capture.md from installed package Documentation~. CLI update check was attempted but CDN network access failed; installed CLI used successfully.
