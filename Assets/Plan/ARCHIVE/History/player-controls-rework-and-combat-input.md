> Historical snapshot. Superseded by [current project context](../../PROJECT_CONTEXT.md) and the subsystem references. Original requirements, file names, counts, and test limits below describe the earlier task, not current instructions. Original bytes are preserved in the organization restore ZIP.

# Project Overview
- Game Title: Dungeon Game.
- High-Level Concept: Existing dungeon-crawler RPG with character movement, equipment, combat and interaction. This task fixes the player's control pipeline in Test, not the broader game.
- Players: Existing local player; no multiplayer changes.
- Inspiration / Reference Games: No specific reference requested; predictable first-/third-person action controls.
- Tone / Art Direction: Preserve existing art and presentation; no visual redesign.
- Target Platform: Windows Standalone64.
- Screen Orientation / Resolution: Preserve existing landscape game-view/build configuration; no resolution changes.
- Render Pipeline: Built-in, Unity 6000.3.28f1.

## Scope and approved direction
User selected reworking the existing locomotion controller and smaller single-weapon combat, with holding right mouse used for shield blocking. Retain CharacterController, StarterAssetsInputs, PlayerInput, combat execution and existing UI. No dual-wield system, new combat art, dungeon-generation work, enemy spawning changes, or map/skill/character-menu implementations.
No plan file was present in Assets/Plans during inspection; this is the initial task plan. Implementation requires review approval. No game assets were changed during this planning pass.

# Game Mechanics
## Core Gameplay Loop
Move and look, sprint/jump across existing geometry, fight using the current weapon, interact with existing objects, manage inventory, and pause/resume. Controls should produce immediate, bounded responses without sliding, camera feedback or accidental actions after closing UI.

## Controls and Input Methods
| Input | Result |
|---|---|
| WASD | Camera-relative movement, normalized diagonals |
| Mouse movement | Look, with one sensitivity application and no artificial mouse smoothing |
| Left Shift held | Sprint; release returns to walking promptly |
| Space pressed | Single jump with a small landing buffer/coyote allowance, never auto-repeat from holding |
| LMB tap | Current weapon's light attack |
| LMB hold | One current-weapon power/heavy attack when supported |
| RMB tap | Current weapon's light attack when no shield is equipped; with a shield equipped, RMB is dedicated to blocking |
| RMB hold | Shield block while equipped and gameplay input is allowed; release drops block |
| E | Interact in gameplay, or close the active interaction interface; never close and reopen in the same press |
| Tab | Toggle existing inventory |
| Escape | Open pause and freeze gameplay, including when inventory/interaction is open; Escape again or existing Resume resumes |
| V | Toggle first-/third-person during gameplay only |
| M / K / C | No menu hotkeys |

The mouse scheme deliberately follows the user's smaller single-weapon/shield clarification instead of independent hand routing. With a shield equipped, dedicating RMB to block avoids adding a hold delay to defensive response. For LMB tap/hold disambiguation, short taps dispatch on release and holds dispatch once at an adjustable threshold (initially 0.25 seconds), with no extra light hit. Power attack support requires valid existing heavy animation/execution; no fabricated animations. Without that support, LMB performs light attack on press, without a hold delay. Simultaneous/conflicting combat input cannot bypass readiness, busy, death or cooldown checks.

# UI
Reuse Canvas/uGUI inventory, interaction and settings/pause panels, fonts and layout. No UI redesign or replacement menus.

State/layout sketch (existing screens):
```
Gameplay:      existing world + HUD       cursor locked/hidden
Inventory:     existing inventory panel  cursor free/visible; gameplay input blocked
Interaction:   existing chest/shop panel cursor free/visible; E closes
Pause:         existing pause panel      cursor free/visible; timeScale = 0
```
One state owner reconciles cursor, gameplay eligibility and UI transitions. Pause and menu navigation must remain responsive at timeScale zero. Clear transient look, jump, attack, toggle and block state on UI entry, focus loss, disabling and death. Resume must not trigger an attack from the UI click or held mouse button. Close/reconcile underlying panels before returning to gameplay so cursor state cannot conflict. Preserve existing public button callback methods used by scene/prefab events.

# Key Asset & Context
## Verified findings
- Both input backends are enabled (activeInputHandler = 2); use event-driven New Input System, not new legacy polling.
- Installed Input System 1.20.1 and Cinemachine 3.1.7. No package installation or upgrades required.
- Test's PlayerInput uses `Packages/com.unity.inputsystem/InputSystem/Runtime/Plugins/PlayerInput/DefaultInputActions.inputactions`. Its Player map has Move, Look and Fire only: Sprint, Jump and ToggleCamera are absent. Never modify this package asset.
- Project-owned `Assets/GamePlay/Character/InputSystem/mehh.inputactions` exists but is not the active player's asset.
- ThirdPersonController.Move interpolates scalar speed from collision velocity and carries the last direction during stopping. Jump resets its 0.5-second timeout in air and discards airborne presses. These require actual logic fixes, not just Inspector changes.
- CharacterController currently has height 1.8, radius 0.28, center Y 0.93, slope limit 45, step offset 0.25, skin width 0.02. Ground mask is 512 (Ground layer 9). Preserve reliable geometry collision; validate actual floor and slopes before changing the probe.
- Player Rigidbody is already kinematic with gravity disabled. Animator applyRootMotion is true: inspect root-motion callbacks before deciding locomotion authority.
- ThirdPersonController sensitivity in current Test is 10. Camera input currently risks duplicated/scaled sensitivity.
- Main view uses one CinemachineBrain on FirstPersonCamera. PlayerFollowCamera uses legacy-compatible CinemachineVirtualCamera/3rdPersonFollow in Cinemachine 3.1.7, plus baseline noise (amplitude 0.5) and an additional CinemachineCollider. Follow damping is currently 0.1/0.25/0.3 and distance 6. These differ from earlier claims.
- CameraRoot is under CameraHolder/Eyes/Head, exposing camera position to animated bone motion. PlayerCameraRoot's virtual camera component is disabled and lacks a body component. Activating its GameObject alone is insufficient.
- PlayerCameraController writes first-person body yaw in Update and sets cursor unconditionally; PlayerController separately changes rotate-on-move for focus. Resolve authority rather than adding another controller.
- PlayerCombat currently binds block to Z. CharacterCombat supports light/heavy Attack(bool), damage windows, cooldown and block routines. CharacterAnimator selects one MainHand weapon. OffHand already has equipment support, but this task does not add independent attacks.
- UICanvasController binds inventory to I and Tab; SettingsMenu Escape closes inventory instead of pausing. E currently passes through a gameplay-only gate, so it cannot close an interaction screen.

## Files to modify or add
- `Assets/Scenes/Test.unity`: scene-specific player, camera, action and UI wiring/Inspector overrides. Do not apply overrides to PlayerHolder prefab or change Main scene.
- `Assets/GamePlay/Character/InputSystem/mehh.inputactions`: reuse the project-owned asset, preserve useful existing action IDs, add required Gameplay/System/UI actions and remove obsolete keyboard shortcuts; assign to Test's PlayerInput.
- `Assets/GamePlay/Character/InputSystem/StarterAssetsInputs.cs`: reliable callback bridge and press-edge/reset handling.
- `Assets/GamePlay/Character/Scripts/ThirdPersonController.cs`: bounded horizontal velocity, jump/probe/collision handling and coordinated look/rotation.
- `Assets/Scripts/Controllers/PlayerCameraController.cs`: mode authority, eligible toggle handling, valid camera activation and heading preservation.
- `Assets/Scripts/Controllers/PlayerController.cs`: consume shared actions, single-weapon tap/hold and interaction dispatch; remove duplicate competing runtime actions for replaced controls.
- `Assets/Scripts/GamePlay/Combat/PlayerCombat.cs`: RMB shield-block eligibility and safe cancellation, removing duplicate Z input handling.
- `Assets/Scripts/Controllers/UICanvasController.cs`, `Assets/Scripts/SettingsMenu.cs`: shared UI input and pause/cursor transition policy.
- Add a small `Assets/Scripts/Controllers/PlayerControlState.cs` only if needed to centralize state without tightly coupling existing controllers.
- Conditional, narrowly scoped edits to existing chest/shop close scripts, CharacterAnimator/CharacterCombat and active hotkey handlers only where necessary for correct control mechanics. Preserve enemy behavior and combat API compatibility.
- Temporary automated test code or tests under the existing test convention; clean up temporary runners after verification.

Impact: direct AssetDatabase dependency inspection was performed for Test, PlayerHolder and mehh. Before implementation, enumerate reverse references of every changed shared script/input asset using dependency queries (AssetDatabase.GetDependencies fallback if a dedicated dependency tool is unavailable). Shared scripts can affect Main/prefabs/enemies even though scene configuration changes are limited to Test. Keep existing serialized fields/public APIs or migrate them explicitly; do not edit package source, compatibility guards or scripting defines.

# Implementation Steps
1. **Confirm targeted wiring and impact.** Inspect actual reference properties (not solely JSON instance IDs), active Animator parameter/state names, OnAnimatorMove handlers, player-owned attack/block hitboxes, shield identification and chest/shop close paths. Search active hotkey scripts for M/K/C and duplicate E/Tab/Escape handlers. Dependency: plan approval. No full project audit.
2. **Repair action ownership.** Configure/assign project-owned mehh with Move/Look/Sprint/Jump/ToggleCamera, Attack/Block, Interact, Inventory and Pause. Use performed/canceled/started callbacks as appropriate; one subscription per consumer, cleaned up on disable/destroy. Retain UI point/click/navigation so existing buttons and dragging work. System actions stay enabled while gameplay actions are blocked. Remove obsolete keyboard menu shortcuts and competing handlers for replaced controls. Dependency: Step 1.
3. **Centralize eligibility and UI transitions.** Update UICanvasController/SettingsMenu and optional small state helper. E closes active interaction first; Tab toggles inventory; Escape always enters pause, not merely inventory-close. Guard missing optional references. Clear pending gameplay inputs and drop blocks on state changes. Keep UI clicks from becoming attacks. Dependency: Step 2.
4. **Rework movement without replacing physics.** Use horizontal vector MoveTowards with separate acceleration and braking (starting targets: walk 4 m/s, sprint 7 m/s, acceleration 35 m/s², braking 50 m/s², reduced configurable air acceleration). Normalize diagonals. Direction changes affect movement independently of smooth visual facing. Resolve blocked motion without storing velocity into walls; issue coordinated CharacterController.Move and use CollisionFlags for floor/ceiling handling. Use bounded turn smoothing (starting 0.06–0.08 seconds). If animation root motion writes locomotion too, remove only that authority conflict while preserving essential combat presentation. Dependency: Steps 1–3.
5. **Make jumping reliable.** Derive a short ground probe from the capsule foot; exclude self and triggers, reject wall/steep-ground false positives. Keep gravity downward/clamped, cancel upward speed on ceilings, and avoid re-grounding while rising. Consume press-edge jumps, initial jump-ready state, ~0.1-second buffer and ~0.08-second coyote time; no double jump/held repeat. Start with height 1.2 and gravity -22 and tune against existing geometry. Preserve slope/step collision limits. Dependency: Step 4.
6. **Fix camera authority in both modes.** Keep one output camera/Brain and two exclusive mode cameras; enable valid virtual-camera components and configure an actual first-person position/orientation solution using installed APIs. Use stable player-root eye-height targets rather than animated head position for baseline view. Preserve yaw/pitch across toggles, disable unsolicited baseline noise, apply sensitivity once (initial mouse target ~0.1–0.2 degrees/pixel), and use frame-independent gamepad rates only if preserving existing gamepad support. Coordinate first-person body yaw with movement/look in a defined order and honor canRotate/busy/UI states; third-person faces travel. Use existing practical collision solution with correct masks/self exclusion, avoiding two collision solvers fighting. Validate lens/near-clip on the mode lens, not just the physical camera. Dependency: Steps 2–5.
7. **Implement smaller combat scheme.** LMB tap/light and supported hold/heavy; RMB shields block immediately while equipped and release cancels, otherwise tap/light. Use actual offhand shield eligibility rather than arbitrary items. Drop block on pause/UI/focus loss/death/unequip; block and attacks cannot execute simultaneously. Reuse Attack(bool), animation states, hit windows and cooldowns. Do not add dual-wield assets or alter enemy mechanics. Dependency: Steps 1–3 and 6.
8. **Verify and save Test.** Run focused compile/EditMode checks and real-device-event Play Mode tests; tune within the above starting ranges based on measurements. Save only relevant scene overrides. Provide a short honest report of changed behavior, tested controls and remaining blockers. Dependency: Steps 1–7.

# Verification & Testing
- Compilation: no new errors, no missing action names/receivers, no package edits or additional locomotion controllers.
- Input wiring: assert Test PlayerInput references the project asset with Sprint/Jump/V present. Check each callback fires once; no I/M/K/C menu activation or duplicate mouse attacks.
- Tests must inject keyboard/mouse events through Input System, not merely assign StarterAssetsInputs fields. Record measured displacement, yaw, jump height, state and accepted attack/block events; inspecting an isFirstPerson bool alone does not prove camera switching.
- Both modes: WASD including diagonal/opposing input, 90/180-degree changes, release stopping distance, Shift press/release, stationary and sprinting Space. Targets: walk/sprint within 10% of configured unobstructed speed, stop within ~0.15 seconds, jump press consumed on next eligible update. Compare 30/60/120 FPS where practical.
- Jump: landing buffer/coyote boundaries, held Space, initial spawn, edge walk-off, ceiling collision, repeated jumps after landing, pause midair. No double-jumps, sinking or wall-grounding.
- Collision: existing floors, walls, corners, stairs/steps and steep slopes; no tunnelling or persistent stored drift after obstruction.
- Camera: repeated V toggles preserve heading; actual Brain live camera matches mode and exactly one mode controls it. Stationary mouse-free view has no unsolicited drift/noise. First-person strafing/backpedaling does not rotate the body toward movement. Near-wall TP camera avoids geometry and excludes player colliders; manual game-view framing check in each mode.
- Combat: short LMB and supported long LMB each produce exactly one accepted attack; no hold auto-repeat; respect cooldown/death/busy. RMB without shield taps light; equipped shield RMB blocks and release drops block. Verify existing damage reduction through shield block using existing combat test fixtures, not new enemy spawning. Unsupported heavy/shield behavior must be reported, not claimed successful.
- UI: E opens then closes existing chest/shop without reopening; Tab opens/closes inventory; Escape from gameplay or inventory pauses actual time-driven movement, gravity and combat. Resume restores proper mode/cursor. UI click must never attack; V cannot relock cursor while a menu is open.
- Focus/lifecycle: alt-tab/focus loss, disable/re-enable and scene exit cannot leave sprint/look/attack/block latched or timeScale stuck.
- Shared-code smoke check: preserve existing enemy Attack(bool) and Animator APIs; no unrelated systems changes.
- Report tested versus untested controls separately. Existing earlier tests only changed bridge fields and booleans, so they are not accepted as evidence for this task. Keep automated runs minimal; do not repeatedly expand tests or regenerate assets to chase cosmetic issues.
