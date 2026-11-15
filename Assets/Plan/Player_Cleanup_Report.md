# Player cleanup report

Implemented 2026-11-15 for **PlayerHolder and its nested Player_Main**, as clarified by the user. [Architecture](Player_Architecture.md) owns current responsibilities; [technical debt](Character_Technical_Debt.md) owns ranked follow-ups. All task documentation uses Assets/Plan despite the pasted task's older Assets/Plans spelling.

## Measured reduction

| Measure | Before | After |
| --- | ---: | ---: |
| Distinct custom script types attached anywhere in playable PlayerHolder, including UI/children | 49 | 41 |
| Custom script components directly on the main character | 18 | 13 |
| C# files under Assets/Scripts | 147 | 135 |
| Gameplay subscribers to PlayerInput.onActionTriggered | 3 | 1 |
| Scripts independently writing player facing | 2 | 1 |

**Twelve source scripts removed:** eight consolidated into existing/shared owners and four dead/obsolete implementations deleted. Three ownership components rewritten: PlayerControlState, PlayerController and ThirdPersonController. Two empty animation subclasses now use the direct shared CharacterAnimator, and common equipment binding/healing is implemented in the existing CharacterStats. No runtime script, interface hierarchy or replacement manager was added. Shared base classes are dependencies, not extra attached-script types; moving ThirdPersonController does not count as removing an additional script.

Deleted sources/GUIDs: [machine-readable removal manifest](Validation/player-cleanup-deleted.json). The eleven first-stage deletions and the later HotKeySlot removal each ran AssetDatabase dependency checks across all prefabs/scenes before deletion. The FPC script had no serialized consumers. Legacy PlayerMotor/PlayerManager/CameraController/BasicRigidBodyPush still have older asset consumers and were retained outside the main-character architecture.

## Discovery and dependency decisions

Inspected the complete original PlayerHolder hierarchy (702 transforms), all attached MonoBehaviours including nested UI/vendor components, Player_Main after the user's extraction, older PlayerContent/StarterAssets variants, humanoid Enemy_Main/NPC_Main/Quartermaster assets, creature variants, input actions, player/NPC controllers, clip callbacks, scene overrides and external consumers. Editor inspection recorded all component types/object references and AssetDatabase dependencies. Final [asset/hierarchy evidence](Validation/player-cleanup-assets.json) includes every component on the rebuilt player and relevant humanoid prefabs.

The original ThirdPersonController moved the capsule and PlayerController routed actions; they were not two simultaneous capsule movers. They did independently rotate the character, while PlayerCameraController and StarterAssetsInputs added forwarding/state. The cleanup resolves that ownership conflict without creating a giant movement/combat/inventory component.

Inventory below is measured against the original production holder. Repeated inventory/shop/slot/presentation instances are grouped by script; source/asset locations remain in SCRIPT_INDEX and the hierarchy evidence.

| Original script | Original attachment | Decision / implemented result |
| --- | --- | --- |
| CameraShaker | `FirstPersonCamera` | KEEP — existing cohesive gameplay/presentation owner |
| ShakeDemo | `Player/GameManager` | KEEP — existing cohesive gameplay/presentation owner |
| AttackHitBox | `Player/Armature/Hips/Spine/Chest/Shoulder.R/Upper Arm.R/Lower Arm.R/Hand.R/Hand Hold.R/AttackHitBox` | KEEP — existing cohesive gameplay/presentation owner |
| BlockHitBox | `Player/Armature/Hips/Spine/Chest/Shoulder.L/Upper Arm.L/Lower Arm.L/Hand.L/Block` | KEEP — existing cohesive gameplay/presentation owner |
| PlayerCombat | `Player` | KEEP — existing cohesive gameplay/presentation owner |
| StatusEffectController | `Player` | KEEP — existing cohesive gameplay/presentation owner |
| DialogueController | `Player` | KEEP — existing cohesive gameplay/presentation owner |
| DialoguePanel | `UICanvas/DialoguePanel` | KEEP — existing cohesive gameplay/presentation owner |
| ChestInventorySlot | `12 UI/hierarchy instances; e.g. UICanvas/InventoryUIs/UISlots/LOG/ChestInventory/ItemsParent/InventorySlot` | KEEP — existing cohesive gameplay/presentation owner |
| ChestInventoryUI | `UICanvas/InventoryUIs/UISlots/LOG/ChestInventory` | KEEP — existing cohesive gameplay/presentation owner |
| PlayerEquipmentManager | `Player` | KEEP — existing cohesive gameplay/presentation owner |
| Inventory | `Player` | KEEP — existing cohesive gameplay/presentation owner |
| EquipmentSlotUI | `5 UI/hierarchy instances; e.g. UICanvas/InventoryUIs/UISlots/PlayerUI/ItemsParent/Slots/HelmetSlot` | KEEP — existing cohesive gameplay/presentation owner |
| HotKeySlot | `6 UI/hierarchy instances; e.g. UICanvas/HotKeysUI/HotKeys/ItemsParent/HotKeySlot1` | MERGE → HotKeysUI slot data / PlayerController request; deleted |
| InventorySlot | `20 UI/hierarchy instances; e.g. UICanvas/InventoryUIs/UISlots/LOG/Inventory/ItemsParent/InventorySlot1` | KEEP — existing cohesive gameplay/presentation owner |
| InventoryUI | `UICanvas/InventoryUIs/UISlots/LOG/Inventory` | KEEP — existing cohesive gameplay/presentation owner |
| PlayerAim | `PlayerFollowCamera/PlayerAim` | DELETE — unused transform follower / unused combat aim reference |
| PlayerAnimator | `Player` | SHARE → direct CharacterAnimator; deleted |
| PlayerCameraController | `Player` | MERGE → ThirdPersonController view modes; deleted |
| PlayerContent | `(holder root)` | KEEP — existing cohesive gameplay/presentation owner |
| PlayerControlState | `Player` | REWRITE — central input/eligibility boundary, preserving modal/action leases |
| PlayerController | `Player` | REWRITE — domain action dispatcher, interaction/focus/prompt; no transform writes |
| PlayerDamageFlash | `UICanvas/HitDamage` | KEEP — existing cohesive gameplay/presentation owner |
| PlayerStats | `Player` | SIMPLIFY — common equipment binding/healing moved to CharacterStats |
| ExperienceBar | `UICanvas/StatsUI/UI_LevelSystem/ExperienceBar` | KEEP — existing cohesive gameplay/presentation owner |
| Player | `Player` | MERGE → existing PlayerStats; demo model in UI_SkillTree; deleted |
| PlayerSkillProgress | `Player` | KEEP — existing cohesive gameplay/presentation owner |
| PlayerSword | `Player` | DELETE — obsolete E-only log prototype |
| SkillFeedbackHUD | `UICanvas/SkillFeedbackHUD` | KEEP — existing cohesive gameplay/presentation owner |
| Testing | `UICanvas/InventoryUIs/UI_SkillTree` | MERGE → UI_SkillTree binding; deleted |
| UI_SkillTree | `UICanvas/InventoryUIs/UI_SkillTree` | SIMPLIFY — direct PlayerStats binding; self-contained UI-only demo fallback |
| QuestController | `Player` | KEEP — existing cohesive gameplay/presentation owner |
| QuestJournal | `WorldSpaceCanvas/QuestJournal` | KEEP — existing cohesive gameplay/presentation owner |
| GatheringProgressHUD | `UICanvas/GatheringHUD` | KEEP — existing cohesive gameplay/presentation owner |
| PlayerGathering | `Player` | KEEP — existing cohesive gameplay/presentation owner |
| ShopInventorySlot | `10 UI/hierarchy instances; e.g. UICanvas/ShopUI/ItemsParent/content/InventorySlot` | KEEP — existing cohesive gameplay/presentation owner |
| ShopInventoryUI | `UICanvas/ShopUI` | KEEP — existing cohesive gameplay/presentation owner |
| StarterAssetsInputs | `Player` | MERGE → PlayerControlState intentions; deleted |
| ThirdPersonController | `Player` | REWRITE — sole locomotion/facing/view owner; GUID-preserving move to Player |
| StatusEffectHUD | `UICanvas/StatusEffects` | KEEP — existing cohesive gameplay/presentation owner |
| DragDrop | `20 UI/hierarchy instances; e.g. UICanvas/InventoryUIs/UISlots/LOG/Inventory/ItemsParent/InventorySlot1/Icon` | KEEP — existing cohesive gameplay/presentation owner |
| HotKeysUI | `UICanvas/HotKeysUI` | SIMPLIFY — keep icon UI; own quick-slot configuration without hidden-object key polling |
| SettingsMenu | `UICanvas/SettingUI` | KEEP — existing cohesive gameplay/presentation owner |
| Tooltip | `UICanvas/InventoryUIs/ToolTip/Tooltip` | KEEP — existing cohesive gameplay/presentation owner |
| Tooltip_ItemStats | `UICanvas/InventoryUIs/ToolTip/Tooltip_ItemStats` | KEEP — existing cohesive gameplay/presentation owner |
| Tooltip_Warning | `UICanvas/InventoryUIs/ToolTip/Tooltip_Warning` | KEEP — existing cohesive gameplay/presentation owner |
| UICanvasController | `UICanvas` | KEEP — existing cohesive gameplay/presentation owner |
| PlayerPickUpDrop | `Player` | MERGE → PlayerController Q action / grab configuration; deleted |
| MapController | `UICanvas/MapDisplay` | KEEP — existing cohesive gameplay/presentation owner |

Additional dependencies: CharacterStats/CharacterCombat/CharacterAnimator, EquipmentManager, Item/Equipment/Consumable/WeaponClass/Abilities/Class data, Interactable/resource/shop/chest/dialogue/quest definitions and services, progression models, EnemyAi/EnemyStats/EnemyCombat/EnemyEquipmentManager, DungeonGenerator, GameController/menu/loading and existing uGUI controls. Execution remains in those owners. PlayerController → PlayerCombat/EquipmentManager/HotKeysUI/Interactable; motor → input intentions/CharacterStats; damage → CharacterStats/derived death handlers; NPC → EnemyAi + shared combat/stats/presentation. [Architecture](Player_Architecture.md) describes integration and extension contracts.

Vendor/package components (TMP, uGUI, EventSystem/InputSystemUIInputModule, Cinemachine, SlimUI and CodeMonkey presentation) were retained. CharacterController, kinematic/physics setup, Animator/avatar, audio, colliders, rig bones and equipment/damage volumes were preserved. Empty PlayerAim was removed after checking its only gameplay reference was the now-unused PlayerCombat.aim field. No third-party scripts were moved or deleted.

## Assets and behavior

- Preserved the user's Player_Main prefab extraction, PlayerHolder nesting and PlayerContent move to Variants. Main source identity and holder overrides remain linked; camera and UI dependencies resolve from the aggregate.
- Migrated camera mode/render hiding into the motor, grab data into actions, skill-tree binding into UI_SkillTree, quick-slot data into HotKeysUI and animation subclasses into CharacterAnimator. Serialized component identities were retained for the animation substitution; the movement GUID/class/namespace and public callback methods remain.
- Player_Main retains PlayerStats, CharacterAnimator, PlayerCombat, ThirdPersonController, PlayerController, PlayerEquipmentManager, Inventory, PlayerControlState, DialogueController, QuestController, PlayerGathering, PlayerSkillProgress and StatusEffectController.
- Input remains mehh + PlayerInput. Removed Jump action/binding; Q/U/1–6 are authored in that same map. Removed buffered/coyote jump logic/timers and player jump/heavy Animator states/parameter; retained gravity, free fall and landing. No authored player slide input/state existed to migrate. Shared NPC heavy clips/data remain intact. Inherited Attack(true) is normalized to light at the player boundary.
- Updated Enemy_Main and affected animation-wrapper consumers, including legacy creature sources and scene-added enemy components, without changing EnemyAi navigation/combat policy. Friendly NPCs inherit shared presentation with hostile AI/combat still disabled. Common CharacterStats equipment subscription/healing replaces duplicated code; EnemyStats owns AI updates.
- Updated direct scene consumers (including Start's loose preview, SkillTree_Test's UI-only model and enemy overrides in combat/gathering fixtures). Removed scripts are not left as missing components. Earlier missing prefab-source remnants in some test scenes remain separately recorded debt.
- Removed the pre-existing missing camera script on the binary StarterAssets_PlayerContent variant because Unity refused to save its valid animation migration otherwise. Its original binary snapshot is a recovery artifact outside Assets. This repairs a missing component; it does not certify that old variant's complete gameplay.
- Parent room/NPC assets that only inherited changes were restored from their task-local pre-edit snapshots after incidental Unity reserialization; they inherit the source migration without unrelated saved changes.
- Repaired invalid serialized callbacks/missing optional UI references in PlayerHolder and removed the dormant UnityEvent input-forwarding arrays in Player_Main. Existing native pointer routes remain authoritative; the inventory RemoveButton now calls the existing RemoveFromInventory method. [Repair manifest](Validation/player-cleanup-reference-repairs.json) records individual edits. This does not implement optional imported settings-template audio/key rebinding.

Changed retained code: `Player/ThirdPersonController.cs`, `Player/PlayerController.cs`, `Player/PlayerControlState.cs`, `Player/PlayerStats.cs`, `Combat/Animation/CharacterAnimator.cs`, `Combat/PlayerCombat.cs`, `Combat/Stats/CharacterStats.cs`, `NPC/Enemies/EnemyStats.cs`, `Inventory/Equipment/EquipmentManager.cs`, `Inventory/Chests/ChestScript.cs`, `Progression/Skills/UI_SkillTree.cs`, `UI/Inventory/HotKeysUI.cs` and `Editor/Validation/CombatCheckpointRunner.cs`. The twelve deleted sources are in the removal manifest. Authored data changes are mehh.inputactions, Player Animator.controller, the player aggregate/nested/legacy assets, shared animation consumers and the scene overrides described above. No unrelated world/quest/shop algorithm changed.

## Executed validation

Unity CLI 1.0.0 (latest check succeeded), connected Unity 6000.3.28f1 Editor. Runtime tests used real PlayerHolder components in Combat_Test, synthetic keyboard/mouse events and advancing frames. A temporary isolated platform avoids the fixture's obstructed spawn; the NPC test builds/removes an isolated native NavMesh. Temporary input settings are cloned/restored. Runtime fixtures and changes are not saved into gameplay assets.

| Check | Result | Evidence |
| --- | --- | --- |
| Player runtime | 36 / 36 passed; errors 0 | [player-cleanup-runtime.json](Validation/player-cleanup-runtime.json) |
| NPC/combat/equipment integration | 15 / 15 passed; errors 0 | [player-cleanup-integration.json](Validation/player-cleanup-integration.json) |
| Dialogue/chest/shop | 9 / 9 passed; errors 0 | [player-cleanup-modals.json](Validation/player-cleanup-modals.json) |
| Player death/reload | 2 / 2 passed | [death evidence](Validation/player-cleanup-death.json) |
| Prefab/component/input/animation wiring | 971 assertions passed, including no missing scripts across five main/humanoid prefabs | [asset evidence](Validation/player-cleanup-assets.json) |
| Serialized references and UnityEvents | No broken object references or invalid persistent callbacks in PlayerHolder, Player_Main, Enemy_Main and NPC_Main | [reference evidence](Validation/player-cleanup-references.json) |
| Unity compilation | Completed; compilationFailed=false; no compiler errors | [compiler evidence](Validation/player-cleanup-compilation.json) |

Runtime coverage includes walk/sprint, mouse look, both views, absent jump, inventory/pause/journal/map/focus gating, one light attack per press/hold/release, movement during attacks, shield cancellation/re-press, shared damage/healing, opening the skill tree, timed gathering/reward, native E interaction and Escape cancellation. NPC integration includes native navigation, accepted punch animation, cancellation, actual player sword/projectile damage, shared health/effects, friendly NPC compatibility, active quick-slot item use, player-only U and attributed enemy death. Modal checks cover dialogue attack cancellation/E/Tab suppression, Escape, chest opening/transfer/close and shop opening/close. Death checks confirm immediate denial and fresh living player after reload.

Initial attempts are not counted as passing: locomotion was initially probed against obstructed spawn geometry; mouse delta was reset by the test's extra InputSystem.Update; hidden UI panels had not initialized; quick-slot forwarding was attached to inactive HUD objects. The fixture was isolated, delta processing corrected, actual panel opening tested, skill binding repaired, and shortcut routing consolidated into the active action owner. A later held-shield run using the physical mouse failed; the final player suite explicitly pairs temporary virtual keyboard/mouse devices and restores the previous scheme/devices, avoiding interference from physical mouse state. Final suites above ran after those corrections and serialized-reference repairs. An old binary prefab initially rejected saving due to its existing missing script; the missing component was removed before successful save/dependency checks. One migration exceeded the command's 60-second response window but completed; completion/deletion was checked before further work. Pipeline was briefly unavailable during a compilation/domain reload. The final explicit compile command exceeded 120 seconds; that timeout is [preserved](Validation/player-cleanup-compilation-timeout.json). Subsequent same-Editor recompile_status confirmed completed, compilationFailed=false and zero errors; the final compiler evidence records that result.

## Limits and handoff

No standalone build, physical keyboard/controller session, exhaustive slopes/ceilings/frame rates, production floor/stair traversal, full monster rig repair, complete ability system, grabbing-content certification or coordinated save system is claimed. Jump removal makes walkable stair clearance a required next world check. No full combat redesign was performed. See [technical debt](Character_Technical_Debt.md) and roadmap.

One-time migration/validation tooling is under AgentScripts/PlayerCleanup outside Assets; migration tools that mention deleted classes are historical tooling, not runtime code or repeatable builders for the final architecture. Task-local snapshots are under Temp/PlayerCleanup/Baseline. Final ownership, current source paths and next-agent requirements are in the linked architecture/index/subsystem documents.

Final Editor state: **stopped, not compiling, compilationFailed=false**, Combat_Test open, null Play Mode start-scene override, automatic background ticking disabled. The scene has unsaved differences (including camera/UI serialized overrides); these were preserved instead of saving or discarding unrelated state. A save-as-copy recovery snapshot is at `Temp/PlayerCleanup/Combat_Test.unsaved-snapshot.unity`, outside Assets. [Handoff evidence](Validation/player-cleanup-handoff.json). PlayerHolder/Player_Main source assets and the documented migrations are saved; no clean-scene claim is made.


Equipment/animation follow-up (2026-11-15) supersedes this pass's former equipment/animation class names and default/replacement-clip design. Five scripts were renamed with GUIDs retained, two empty shells removed, and explicit item attack sets/direct authored states implemented. Current responsibilities, migration and 95 gameplay/19 asset checks are owned by [Inventory](SYSTEMS/Inventory.md#equipment-and-weapon-ownership--2026-11-15) and [Animation](SYSTEMS/Animation.md#explicit-weapon-motions--2026-11-15). The measurements and evidence above remain historical results of the preceding player pass.
