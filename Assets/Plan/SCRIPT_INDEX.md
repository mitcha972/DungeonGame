# Custom script index

## Gameplay audit tooling — 2026-11-13

No runtime script was added/moved. External files under `AgentScripts/Audit/`:

- `AuditSession.cs`: preserve/restore the Play Mode start-scene override; inspect selection/runtime state.
- `WorldAudit.cs`: seeded production paths, failure/cancellation/rollback and native NavMesh data lifetime.
- `TraversalAudit.cs`: controlled CharacterController static-collision routes, with walking versus bounded upward clearance reported separately.
- `ProductionAudit.cs`: real menu/loading/dungeon, modal/focus, death and menu-return checks.
- `FinalReview.cs`: asset identities/local IDs, missing scripts, source-scene preservation and final Editor state.
- `InspectWorldAssets.cs`, `EndRoomProbe.cs`: source-room inspection and isolated Play Mode geometry diagnostics.
- `RepairEndRoom.cs`, `NorthWallRepair.cs`: one-time AssetDatabase prefab repairs for the audited baseline; do not rerun as tests.

Tests require owned Play Mode fixtures; migration helpers require stopped Play Mode. Technical backups stay outside Assets. `AgentScripts/Skills/VerifySkills.cs` also corrects enemy test initialization order. [Maintenance](MAINTENANCE.md#gameplay-audit--2026-11-13) owns executed results and integration limits.

Current 2026-11-13 locations. Classes/namespaces/GUIDs retained. [Architecture](ARCHITECTURE.md) owns relationships; this index owns file paths/responsibilities. Imported vendor/package sources are excluded; adapted Camera code and legacy shells are identified.

## Imported custom/adapted source (118 files)

| File | Responsibility |
| --- | --- |
| [Assets/Editor/Validation/CombatCheckpointRunner.cs](<../Editor/Validation/CombatCheckpointRunner.cs>) | Editor combat checkpoint runner; mutates live fixture. |
| [Assets/Editor/Validation/GameplayModernizationValidation.cs](<../Editor/Validation/GameplayModernizationValidation.cs>) | Editor scene/XP/interval/300-graph checks. |
| [Assets/Scripts/Camera/CameraShakeInstance.cs](<../Scripts/Camera/CameraShakeInstance.cs>) | Adapted EZCameraShake state/fade simulation. |
| [Assets/Scripts/Camera/CameraShaker.cs](<../Scripts/Camera/CameraShaker.cs>) | Adapted EZCameraShake LateUpdate rotation offset. |
| [Assets/Scripts/Camera/CameraShaker1.cs](<../Scripts/Camera/CameraShaker1.cs>) | Retained compatibility shake component. |
| [Assets/Scripts/Camera/CameraUtilities.cs](<../Scripts/Camera/CameraUtilities.cs>) | Adapted EZCameraShake math helpers. |
| [Assets/Scripts/Camera/ShakeDemo.cs](<../Scripts/Camera/ShakeDemo.cs>) | Game-facing shake entry point/demo adapter. |
| [Assets/Scripts/Combat/Animation/CharacterAnimationEventReceiver.cs](<../Scripts/Combat/Animation/CharacterAnimationEventReceiver.cs>) | Retained animation event receiver component. |
| [Assets/Scripts/Combat/Animation/CharacterAnimator.cs](<../Scripts/Combat/Animation/CharacterAnimator.cs>) | Weapon clips and equip/busy/hit/death presentation. |
| [Assets/Scripts/Combat/Animation/ResetAnimatorBool.cs](<../Scripts/Combat/Animation/ResetAnimatorBool.cs>) | Animator state behavior resetting a bool. |
| [Assets/Scripts/Combat/AttackHitBox.cs](<../Scripts/Combat/AttackHitBox.cs>) | Melee overlap sampling and per-attack deduplication. |
| [Assets/Scripts/Combat/BlockHitBox.cs](<../Scripts/Combat/BlockHitBox.cs>) | Shield volume and incomplete projectile/durability bookkeeping. |
| [Assets/Scripts/Combat/CharacterCombat.cs](<../Scripts/Combat/CharacterCombat.cs>) | Attack timeline, cancellation, damage/block/cooldown owner. |
| [Assets/Scripts/Combat/Data/Abilities.cs](<../Scripts/Combat/Data/Abilities.cs>) | ScriptableObject ability configuration; some fields unused. |
| [Assets/Scripts/Combat/Data/WeaponClass.cs](<../Scripts/Combat/Data/WeaponClass.cs>) | ScriptableObject weapon clips/timing/projectile data. |
| [Assets/Scripts/Combat/EnemyCombat.cs](<../Scripts/Combat/EnemyCombat.cs>) | Enemy attack gating and projectile/ability adapter. |
| [Assets/Scripts/Combat/PlayerCombat.cs](<../Scripts/Combat/PlayerCombat.cs>) | Player block eligibility and projectile/ability adapter. |
| [Assets/Scripts/Combat/Projectile.cs](<../Scripts/Combat/Projectile.cs>) | Swept nearest-hit projectile collision and range. |
| [Assets/Scripts/Combat/Stats/CharacterStats.cs](<../Scripts/Combat/Stats/CharacterStats.cs>) | Shared armor/health/modifiers and death state. |
| [Assets/Scripts/Combat/Stats/Stat.cs](<../Scripts/Combat/Stats/Stat.cs>) | Generic integer modifier collection. |
| [Assets/Scripts/Core/GameController.cs](<../Scripts/Core/GameController.cs>) | Session name/class/floor/difficulty and player setup. |
| [Assets/Scripts/Core/Legacy/Controller.cs](<../Scripts/Core/Legacy/Controller.cs>) | Empty compatibility component; serialized identity retained. |
| [Assets/Scripts/Core/Scenes/LoadingScript.cs](<../Scripts/Core/Scenes/LoadingScript.cs>) | Validated asynchronous build-scene loading. |
| [Assets/Scripts/Dialogue/DialogueAction.cs](<../Scripts/Dialogue/DialogueAction.cs>) | Extensible ScriptableObject response action contract. |
| [Assets/Scripts/Dialogue/DialogueController.cs](<../Scripts/Dialogue/DialogueController.cs>) | Per-player conversation lifecycle and handoff. |
| [Assets/Scripts/Dialogue/DialogueConversation.cs](<../Scripts/Dialogue/DialogueConversation.cs>) | ScriptableObject graph/node/response data. |
| [Assets/Scripts/Dialogue/DialogueInteractable.cs](<../Scripts/Dialogue/DialogueInteractable.cs>) | Ordinary NPC adapter to Interactable. |
| [Assets/Scripts/Dialogue/DialogueNpc.cs](<../Scripts/Dialogue/DialogueNpc.cs>) | Speaker/data/shop/service-request context. |
| [Assets/Scripts/Dialogue/DialoguePanel.cs](<../Scripts/Dialogue/DialoguePanel.cs>) | TMP/uGUI response list and numeric shortcuts. |
| [Assets/Scripts/Dialogue/ServiceDialogueAction.cs](<../Scripts/Dialogue/ServiceDialogueAction.cs>) | Shop/end actions and bank/quest/service hooks. |
| [Assets/Scripts/Inventory/Chests/ChestInventory.cs](<../Scripts/Inventory/Chests/ChestInventory.cs>) | Capacity-limited chest item list. |
| [Assets/Scripts/Inventory/Chests/ChestInventorySlot.cs](<../Scripts/Inventory/Chests/ChestInventorySlot.cs>) | Failure-aware chest-to-player transfer. |
| [Assets/Scripts/Inventory/Chests/ChestInventoryUI.cs](<../Scripts/Inventory/Chests/ChestInventoryUI.cs>) | Chest slot binding and close behavior. |
| [Assets/Scripts/Inventory/Chests/ChestScript.cs](<../Scripts/Inventory/Chests/ChestScript.cs>) | Chest loot generation and Interactable opening. |
| [Assets/Scripts/Inventory/Equipment/EnemyEquipmentManager.cs](<../Scripts/Inventory/Equipment/EnemyEquipmentManager.cs>) | Probabilistic enemy default equipment. |
| [Assets/Scripts/Inventory/Equipment/EquipmentManager.cs](<../Scripts/Inventory/Equipment/EquipmentManager.cs>) | Equipment state/meshes/events/reset/unequip/drop. |
| [Assets/Scripts/Inventory/Equipment/EquipmentManager1.cs](<../Scripts/Inventory/Equipment/EquipmentManager1.cs>) | Empty compatibility component. |
| [Assets/Scripts/Inventory/Equipment/EquipmentManagerUI.cs](<../Scripts/Inventory/Equipment/EquipmentManagerUI.cs>) | Retained EquipmentManager specialization. |
| [Assets/Scripts/Inventory/Equipment/PlayerEquipmentManager.cs](<../Scripts/Inventory/Equipment/PlayerEquipmentManager.cs>) | Player equipment UI and equip guards. |
| [Assets/Scripts/Inventory/Inventory.cs](<../Scripts/Inventory/Inventory.cs>) | Fixed slot storage, transfers and notifications. |
| [Assets/Scripts/Inventory/Items/Consumable.cs](<../Scripts/Inventory/Items/Consumable.cs>) | Validated health consumable effect and slot removal. |
| [Assets/Scripts/Inventory/Items/Equipment.cs](<../Scripts/Inventory/Items/Equipment.cs>) | Equipment slot/mesh/stat data and equip adapter. |
| [Assets/Scripts/Inventory/Items/Item.cs](<../Scripts/Inventory/Items/Item.cs>) | ScriptableObject base item data/use contract. |
| [Assets/Scripts/Inventory/Items/ItemDrop.cs](<../Scripts/Inventory/Items/ItemDrop.cs>) | World drop configuration/helper component. |
| [Assets/Scripts/Inventory/Items/Loot.cs](<../Scripts/Inventory/Items/Loot.cs>) | Serializable pickup/chance configuration. |
| [Assets/Scripts/Inventory/Items/Potion.cs](<../Scripts/Inventory/Items/Potion.cs>) | Retained compatibility component. |
| [Assets/Scripts/Inventory/UI/EquipmentSlotUI.cs](<../Scripts/Inventory/UI/EquipmentSlotUI.cs>) | Equipment display and guarded inventory transfer. |
| [Assets/Scripts/Inventory/UI/HotKeySlot.cs](<../Scripts/Inventory/UI/HotKeySlot.cs>) | Inventory slot hotkey adapter. |
| [Assets/Scripts/Inventory/UI/InventorySlot.cs](<../Scripts/Inventory/UI/InventorySlot.cs>) | Native pointer inventory-item use adapter. |
| [Assets/Scripts/Inventory/UI/InventoryUI.cs](<../Scripts/Inventory/UI/InventoryUI.cs>) | Discover/render player inventory slots. |
| [Assets/Scripts/Inventory/UI/UISlot.cs](<../Scripts/Inventory/UI/UISlot.cs>) | Shared item display/use/drop/drag behavior. |
| [Assets/Scripts/NPC/Enemies/Enemy.cs](<../Scripts/NPC/Enemies/Enemy.cs>) | ScriptableObject enemy tuning/equipment/loot data. |
| [Assets/Scripts/NPC/Enemies/EnemyAi.cs](<../Scripts/NPC/Enemies/EnemyAi.cs>) | Update-driven perception/navigation/combat FSM. |
| [Assets/Scripts/NPC/Enemies/EnemyAnimator.cs](<../Scripts/NPC/Enemies/EnemyAnimator.cs>) | CharacterAnimator specialization for enemy. |
| [Assets/Scripts/NPC/Enemies/EnemyInteract.cs](<../Scripts/NPC/Enemies/EnemyInteract.cs>) | Enemy focus/Outline presentation. |
| [Assets/Scripts/NPC/Enemies/EnemyStats.cs](<../Scripts/NPC/Enemies/EnemyStats.cs>) | Enemy stats, damage/death, XP and loot. |
| [Assets/Scripts/NPC/Legacy/EnemyAiTutorial.cs](<../Scripts/NPC/Legacy/EnemyAiTutorial.cs>) | Retained tutorial AI implementation. |
| [Assets/Scripts/NPC/Legacy/EnemyManager.cs](<../Scripts/NPC/Legacy/EnemyManager.cs>) | Retained enemy-instance placeholder. |
| [Assets/Scripts/Player/Data/Class.cs](<../Scripts/Player/Data/Class.cs>) | ScriptableObject starting health/equipment data. |
| [Assets/Scripts/Player/Legacy/CameraController.cs](<../Scripts/Player/Legacy/CameraController.cs>) | Legacy orbit/zoom camera controller. |
| [Assets/Scripts/Player/Legacy/FPC.cs](<../Scripts/Player/Legacy/FPC.cs>) | Legacy first-person controller with Standard Assets input. |
| [Assets/Scripts/Player/Legacy/PlayerManager.cs](<../Scripts/Player/Legacy/PlayerManager.cs>) | Legacy singleton build-index reload. |
| [Assets/Scripts/Player/Legacy/PlayerMotor.cs](<../Scripts/Player/Legacy/PlayerMotor.cs>) | Legacy NavMeshAgent movement/follow controller. |
| [Assets/Scripts/Player/PlayerAim.cs](<../Scripts/Player/PlayerAim.cs>) | Orient player aim from main camera. |
| [Assets/Scripts/Player/PlayerAnimator.cs](<../Scripts/Player/PlayerAnimator.cs>) | CharacterAnimator specialization for player. |
| [Assets/Scripts/Player/PlayerCameraController.cs](<../Scripts/Player/PlayerCameraController.cs>) | Camera mode selection and player renderer visibility. |
| [Assets/Scripts/Player/PlayerContent.cs](<../Scripts/Player/PlayerContent.cs>) | Apply player identity/class and class changes. |
| [Assets/Scripts/Player/PlayerControlState.cs](<../Scripts/Player/PlayerControlState.cs>) | Central gameplay eligibility, cursor and modal input. |
| [Assets/Scripts/Player/PlayerController.cs](<../Scripts/Player/PlayerController.cs>) | Combat input, focus and Interactable raycast dispatch. |
| [Assets/Scripts/Player/PlayerDamageFlash.cs](<../Scripts/Player/PlayerDamageFlash.cs>) | Player screen damage flash presentation. |
| [Assets/Scripts/Player/PlayerStats.cs](<../Scripts/Player/PlayerStats.cs>) | Health/class gear, XP/skills and death restart. |
| [Assets/Scripts/Progression/Leveling/ExperienceBar.cs](<../Scripts/Progression/Leveling/ExperienceBar.cs>) | XP bar display and retained demo setup. |
| [Assets/Scripts/Progression/Leveling/LevelSystem.cs](<../Scripts/Progression/Leveling/LevelSystem.cs>) | Bounded XP thresholds/levels/events model. |
| [Assets/Scripts/Progression/Leveling/LevelSystemAnimated.cs](<../Scripts/Progression/Leveling/LevelSystemAnimated.cs>) | Displayed XP interpolation and owned subscriptions. |
| [Assets/Scripts/Progression/Leveling/LevelSystemAnimatedUpdater.cs](<../Scripts/Progression/Leveling/LevelSystemAnimatedUpdater.cs>) | Local runtime interpolation/lifecycle driver. |
| [Assets/Scripts/Progression/Leveling/LevelWindow.cs](<../Scripts/Progression/Leveling/LevelWindow.cs>) | Level/XP display and example callbacks. |
| [Assets/Scripts/Progression/Skills/MaterialTintColor.cs](<../Scripts/Progression/Skills/MaterialTintColor.cs>) | Owned material tint/fade for skills presentation. |
| [Assets/Scripts/Progression/Skills/Player.cs](<../Scripts/Progression/Skills/Player.cs>) | PlayerStats progression delegation; standalone demo fallback. |
| [Assets/Scripts/Progression/Skills/PlayerSkills.cs](<../Scripts/Progression/Skills/PlayerSkills.cs>) | Skill points/prerequisites/unlock model. |
| [Assets/Scripts/Progression/Skills/PlayerSword.cs](<../Scripts/Progression/Skills/PlayerSword.cs>) | Legacy E-polling log-only skill prototype. |
| [Assets/Scripts/Progression/Skills/Testing.cs](<../Scripts/Progression/Skills/Testing.cs>) | Skill-tree bootstrap using selected Player wrapper. |
| [Assets/Scripts/Progression/Skills/UI_SkillTree.cs](<../Scripts/Progression/Skills/UI_SkillTree.cs>) | Skill selection, visual links and pointer callbacks. |
| [Assets/Scripts/Shops/ShopInventory.cs](<../Scripts/Shops/ShopInventory.cs>) | Merchant stock list and compatibility singleton. |
| [Assets/Scripts/Shops/ShopInventorySlot.cs](<../Scripts/Shops/ShopInventorySlot.cs>) | Price/icon and prototype free item grant. |
| [Assets/Scripts/Shops/ShopInventoryUI.cs](<../Scripts/Shops/ShopInventoryUI.cs>) | Existing shop row binding and close synchronization. |
| [Assets/Scripts/Shops/ShopScript.cs](<../Scripts/Shops/ShopScript.cs>) | Stock generation, optional dialogue and shop opening. |
| [Assets/Scripts/Tests/Runtime/DialogueWorldMarker.cs](<../Scripts/Tests/Runtime/DialogueWorldMarker.cs>) | Runtime test-only scaled-time witness. |
| [Assets/Scripts/UI/Feedback/DamageFlash.cs](<../Scripts/UI/Feedback/DamageFlash.cs>) | Renderer damage-color feedback; ownership needs review. |
| [Assets/Scripts/UI/HUD/HealthChange.cs](<../Scripts/UI/HUD/HealthChange.cs>) | World health HUD, renderer flash and owner cleanup. |
| [Assets/Scripts/UI/HUD/HealthTextDisplay.cs](<../Scripts/UI/HUD/HealthTextDisplay.cs>) | Floating health/damage text setup. |
| [Assets/Scripts/UI/HUD/ScreenDamage.cs](<../Scripts/UI/HUD/ScreenDamage.cs>) | HUD damage image feedback. |
| [Assets/Scripts/UI/Inventory/DragDrop.cs](<../Scripts/UI/Inventory/DragDrop.cs>) | Inventory drag event forwarding. |
| [Assets/Scripts/UI/Inventory/Drop.cs](<../Scripts/UI/Inventory/Drop.cs>) | Retained UI drop callback. |
| [Assets/Scripts/UI/Inventory/HotKeysUI.cs](<../Scripts/UI/Inventory/HotKeysUI.cs>) | Inventory hotkey icon/event refresh. |
| [Assets/Scripts/UI/Menus/MenuController.cs](<../Scripts/UI/Menus/MenuController.cs>) | Character selection and Start→Loading transition. |
| [Assets/Scripts/UI/Menus/SettingsMenu.cs](<../Scripts/UI/Menus/SettingsMenu.cs>) | Pause/resume tabs and return to Start menu. |
| [Assets/Scripts/UI/Tooltips/ItemInfo.cs](<../Scripts/UI/Tooltips/ItemInfo.cs>) | Retained item/tooltip demo component. |
| [Assets/Scripts/UI/Tooltips/Tooltip.cs](<../Scripts/UI/Tooltips/Tooltip.cs>) | Generic pointer-bound tooltip API/extensions. |
| [Assets/Scripts/UI/Tooltips/Tooltip_ItemStats.cs](<../Scripts/UI/Tooltips/Tooltip_ItemStats.cs>) | Item stats tooltip and clamped canvas placement. |
| [Assets/Scripts/UI/Tooltips/Tooltip_Warning.cs](<../Scripts/UI/Tooltips/Tooltip_Warning.cs>) | Warning tooltip presentation. |
| [Assets/Scripts/UI/Tooltips/Window.cs](<../Scripts/UI/Tooltips/Window.cs>) | Native pointer tooltip-window example. |
| [Assets/Scripts/UI/Tooltips/Window_Items.cs](<../Scripts/UI/Tooltips/Window_Items.cs>) | Retained item-window example. |
| [Assets/Scripts/UI/UICanvasController.cs](<../Scripts/UI/UICanvasController.cs>) | Inventory/player UI toggles and tabs. |
| [Assets/Scripts/UI/UIPointerEvents.cs](<../Scripts/UI/UIPointerEvents.cs>) | Native pointer adapter with legacy hover compatibility. |
| [Assets/Scripts/World/Destructibles/BrokenObject.cs](<../Scripts/World/Destructibles/BrokenObject.cs>) | Debris fading and owned-material cleanup. |
| [Assets/Scripts/World/Destructibles/DestructibleConObject.cs](<../Scripts/World/Destructibles/DestructibleConObject.cs>) | Destructible specialization/effect callback. |
| [Assets/Scripts/World/Destructibles/DestructibleObject.cs](<../Scripts/World/Destructibles/DestructibleObject.cs>) | IDamageable health/effects and break behavior. |
| [Assets/Scripts/World/Destructibles/HealthSystem.cs](<../Scripts/World/Destructibles/HealthSystem.cs>) | Reusable clamped health/damage/heal/death events. |
| [Assets/Scripts/World/Destructibles/IDamageable.cs](<../Scripts/World/Destructibles/IDamageable.cs>) | World damage interface. |
| [Assets/Scripts/World/Dungeon/DungeonExit.cs](<../Scripts/World/Dungeon/DungeonExit.cs>) | Request generator floor advancement. |
| [Assets/Scripts/World/Dungeon/DungeonGenerator.cs](<../Scripts/World/Dungeon/DungeonGenerator.cs>) | Connected maze, room/NavMesh assembly and rollback. |
| [Assets/Scripts/World/Grabbing/ObjectGrabbable.cs](<../Scripts/World/Grabbing/ObjectGrabbable.cs>) | Physics grab-point movement and release. |
| [Assets/Scripts/World/Grabbing/PlayerPickUpDrop.cs](<../Scripts/World/Grabbing/PlayerPickUpDrop.cs>) | Separate raycast rigidbody-grab prototype. |
| [Assets/Scripts/World/Interactions/DoorScript.cs](<../Scripts/World/Interactions/DoorScript.cs>) | Interactable door opening/closing. |
| [Assets/Scripts/World/Interactions/Interactable.cs](<../Scripts/World/Interactions/Interactable.cs>) | Shared focus and Interact(owner) contract. |
| [Assets/Scripts/World/Interactions/ItemInteract.cs](<../Scripts/World/Interactions/ItemInteract.cs>) | Guarded world pickup/add-before-destroy. |
| [Assets/Scripts/World/Map/MapController.cs](<../Scripts/World/Map/MapController.cs>) | North-up minimap follow, owned render output, heading marker and expanded view; see SYSTEMS/Minimap.md. |
| [Assets/Scripts/World/Utilities/MeshFromSkinnedMesh.cs](<../Scripts/World/Utilities/MeshFromSkinnedMesh.cs>) | Edit-mode bind-mesh conversion; not auto-executed. |

## Retained integration locations

Not moved; do not treat these as deletion candidates.

| File | Responsibility |
| --- | --- |
| [Assets/GamePlay/Character/InputSystem/StarterAssetsInputs.cs](<../GamePlay/Character/InputSystem/StarterAssetsInputs.cs>) | PlayerInput callback bridge and input reset/gating. |
| [Assets/GamePlay/Character/LowPoly Characters/Characters/LowPoly/Editor/MaterialTilingOffset.cs](<../GamePlay/Character/LowPoly Characters/Characters/LowPoly/Editor/MaterialTilingOffset.cs>) | Imported BLINK editor helper, kept under Editor. |
| [Assets/Prefabs/Player/DemoPlayer/MouseCamLook.cs](<../Prefabs/Player/DemoPlayer/MouseCamLook.cs>) | Imported demo mouse-look. |
| [Assets/Prefabs/Player/DemoPlayer/PlayerMovement.cs](<../Prefabs/Player/DemoPlayer/PlayerMovement.cs>) | Imported demo movement. |
| [Assets/Prefabs/Player/DemoPlayer/ScreenshotMaker.cs](<../Prefabs/Player/DemoPlayer/ScreenshotMaker.cs>) | Imported demo screenshot helper. |

## Ephemeral Unity CLI tools

Outside Assets intentionally; run with script run, not runtime components.

| File | Responsibility |
| --- | --- |
| [AgentScripts/Dialogue/BuildMerchantDialogue.cs](<../../AgentScripts/Dialogue/BuildMerchantDialogue.cs>) | Regenerate dedicated merchant assets/scene; not a test command. |
| [AgentScripts/Dialogue/VerifyDialogueLifecycle.cs](<../../AgentScripts/Dialogue/VerifyDialogueLifecycle.cs>) | Combat cancellation, disable/reopen and service hooks. |
| [AgentScripts/Dialogue/VerifyDialogueMouse.cs](<../../AgentScripts/Dialogue/VerifyDialogueMouse.cs>) | Temporary paired mouse and actual uGUI pointer checks. |
| [AgentScripts/Dialogue/VerifyMerchantDialogue.cs](<../../AgentScripts/Dialogue/VerifyMerchantDialogue.cs>) | Input System keyboard/control/world/shop checks. |
| [AgentScripts/Organization/OrganizationPlaySmoke.cs](<../../AgentScripts/Organization/OrganizationPlaySmoke.cs>) | Organization/reference validation utility. |
| [AgentScripts/Organization/OrganizeProjectAssets.cs](<../../AgentScripts/Organization/OrganizeProjectAssets.cs>) | One-shot archived-manifest migration; do not rerun after completion. |
| [AgentScripts/Organization/VerifyOrganization.cs](<../../AgentScripts/Organization/VerifyOrganization.cs>) | Organization/reference validation utility. |
| [AgentScripts/Organization/OrganizePrefabs.cs](<../../AgentScripts/Organization/OrganizePrefabs.cs>) | Prefab dependency/state audit and completed one-shot AssetDatabase migration. Do not rerun Move on the completed manifest. |
| [AgentScripts/Organization/inspect_prefabs.py](<../../AgentScripts/Organization/inspect_prefabs.py>) | Filesystem prefab component/GUID/hash inventory. |
| [AgentScripts/Organization/plan_prefab_moves.py](<../../AgentScripts/Organization/plan_prefab_moves.py>) | Completed one-shot prefab manifest/snapshot generator; historical old paths, not a reusable migration. |
| [AgentScripts/Organization/verify_prefab_files.py](<../../AgentScripts/Organization/verify_prefab_files.py>) | Validate all prefab/meta bytes against manifest and reconcile live tool path literals. |

## Quest additions (2026-11-13)

| File | Responsibility |
| --- | --- |
| Assets/Scripts/Dialogue/DialogueCondition.cs | Shared dialogue condition contract and all-condition evaluator. |
| Assets/Scripts/Quests/QuestDefinition.cs | Immutable quest/stage/objective definitions and content validation. |
| Assets/Scripts/Quests/QuestService.cs | Character progress, checked transitions/rewards and versioned quest DTO. |
| Assets/Scripts/Quests/QuestController.cs | Player inventory/progression binding. |
| Assets/Scripts/Quests/QuestEnemyBridge.cs | Existing enemy death attribution adapter. |
| Assets/Scripts/Quests/QuestLocation.cs | Semantic location trigger adapter. |
| Assets/Scripts/Quests/QuestDialogueCondition.cs | Quest facts exposed to existing dialogue. |
| Assets/Scripts/Quests/QuestDialogueAction.cs | Accept, turn-in and authored NPC report actions. |
| Assets/Scripts/Quests/QuestJournal.cs | Paged uGUI journal presentation. |

Builder/assertion scripts remain outside Assets in AgentScripts/Quests. Existing source list above has 118 pre-quest entries; these nine additions bring the indexed custom source total to 127.

Shop addition (2026-11-13): Assets/Scripts/Shops/ShopDefinition.cs — ShopDefinition ScriptableObject and serializable ShopStockRule (stock, allowlists, prices, restock). Existing ShopInventory/ShopScript/ShopInventoryUI/ShopInventorySlot expanded; Inventory owns gold, Item exposes tradeRestricted. Ephemeral builders/checks: AgentScripts/Shops/.

## Resource nodes (2026-11-13)

| File | Responsibility |
| --- | --- |
| Assets/Scripts/Resource/ResourceNode.cs | Existing placeholder extended to Interactable; per-node reservation, charges, respawn, events and state snapshot. |
| Assets/Scripts/Resource/ResourceNodeDefinition.cs | Shared reward/timing/animation/requirement configuration. |
| Assets/Scripts/Resource/ResourceNodeRequirement.cs | Optional reusable tool/skill/quest check contract. |
| Assets/Scripts/Resource/PlayerGathering.cs | Timed player action, atomic inventory reward, cancellation and action-lease lifecycle. |
| Assets/Scripts/Resource/GatheringProgressHUD.cs | Existing-Canvas progress bar presentation. |

Retained authoring/verification tools: AgentScripts/Resources/{BuildResourceNodes,ResourceIcons,InspectResources,ResourceTestSession,VerifyResourceNodes,VerifyResourceAssets,PreviewResourceNodes}.cs, outside Assets. ResourceNode retains its original script GUID; no source files were moved across assembly boundaries.

Gathering quest follow-up (2026-11-13): no new runtime scripts. DialogueController/DialoguePanel add versioned node-bound response handling while preserving Choose(int) callbacks. Authoring/verification tools: AgentScripts/GatheringQuests/{BuildGatheringQuest,GatheringQuestSession,VerifyGatheringQuest,VerifyGatheringQuestAssets}.cs. Existing AgentScripts/Quests/BuildQuestSlice preserves additional catalog entries; VerifyQuestSlice selects the original quest by stable ID.

## Independent skill slice (2026-11-13 additions)

| File | Responsibility |
| --- | --- |
| Assets/Scripts/Progression/Skills/SkillDefinition.cs | Stable RpgSkill enum and immutable skill presentation definition. |
| Assets/Scripts/Progression/Skills/SkillXpCurve.cs | Shared configurable monotonic level 1–30 thresholds. |
| Assets/Scripts/Progression/Skills/PlayerSkillProgress.cs | Per-player XP, award/events/queries and versioned snapshot contract. |
| Assets/Scripts/Progression/Skills/SkillFeedbackHUD.cs | Existing-Canvas pooled TMP feedback, celebration and hidden debug display. |
| Assets/Scripts/Progression/Skills/SkillbookTestSupply.cs | Gameplay_Test-only atomic ten-book Interactable supply. |
| Assets/Scripts/Inventory/Items/Skillbook.cs | Existing Item.Use integration; exact 100-XP guarded consumption. |

AgentScripts/Skills contains AssetDatabase authoring, temporary Play Mode scene selection and retained skills/inputs/assets validation scripts. These technical tooling files remain outside Assets; [Skills](SYSTEMS/Skills.md) owns behavior and executed validation.

## Animation assessment tooling (2026-11-13)

No imported runtime script was added or moved. `AgentScripts/Animations/` retains out-of-Assets tooling: AnimationAudit (controller/clip/rig inventory and temporary Play Mode start scene), AnimationPlayTest (synthetic mouse smoke test), AnimationCoverage (weapon/grip/casting tests), EnemyAnimationTest (real accepted enemy punch), SkillAnimationProbe (skill-tree demo inspection), InputDiagnosis (temporary callback/device tracing), RepairAnimationEvents (obsolete clip-event migration), OrganizeAnimations (AssetDatabase moves and GUID/dependency/local-ID verification), and AnimationHandoff (user-approved saved-scene restoration). Python helpers plan moves, update authoring paths and maintain the assessment text. [Animation/models](SYSTEMS/Animation.md) owns findings and executed validation; do not rerun mutating migration helpers just to inspect or test.

Combat simplification validation (2026-11-13): `AgentScripts/Combat/SimpleCombatCheck.cs` is an out-of-Assets, nonblocking Play Mode regression runner (`CombatRegression.Main`) for Combat_Test. It temporarily spawns Enemy_Main and equips player weapons/shields, checks real input/timelines/native damage, then writes evidence; stop the test session afterward to discard runtime fixtures/state. No runtime scripts were added or renamed. See [combat](SYSTEMS/Combat.md).

## Status effects (2026-11-13)

- Assets/Scripts/Combat/StatusEffectDefinition.cs: shared configuration.
- Assets/Scripts/Combat/StatusEffectController.cs: active instances, timers, modifiers and application results.
- Assets/Scripts/Combat/StatusEffectTestSupply.cs: explicit Combat_Test potion fixture.
- Assets/Scripts/UI/HUD/StatusEffectHUD.cs: icons, TMP countdowns and hover tooltip.
