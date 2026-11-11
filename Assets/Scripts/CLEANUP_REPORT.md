# Unity C# cleanup report — initial pass

Scope: `Assets/Scripts/` only. Reviewed all 106 C# files. Modified 54 C# files and added this report. No scripts or folders were moved or renamed. No scene, prefab, material, animation, package, project configuration, or metadata file was edited. Unity was not launched and gameplay was not tested.

## General cleanup

- Removed empty Update methods and large, inactive implementations from legacy component shells. Kept the component classes and filenames for existing Unity references.
- Removed unused private controller/player-sword/enemy-stat caches where their only use was assignment.
- Simplified chest/shop inventory capacity checks and rejected null items; retained both component types and their serialized fields.
- Replaced the Stat.GetValue closure with a straightforward loop.
- Cached the projectile collider array once during owner collision exclusion rather than allocating it once per owner collider.
- Removed a redundant enemy projectile velocity assignment that overrode Projectile.StartUp's validated speed.
- Replaced string-based coroutine calls with typed calls in loading and broken-object cleanup.
- Removed redundant slot-array rescans when closing chest/shop panels; include inactive slots during initial discovery.
- InventoryUI's existing UpdateUI coroutine now refreshes slots from inventory data rather than iterating without updating anything. Existing private UI callback names are retained.
- HotKeysUI uses the inventory's common change notification and immediate icon refresh instead of starting duplicate delayed coroutines. Its public UpdateUI coroutine remains available.

## Bugs fixed through source inspection

### Player, presentation, and interaction

- PlayerStats now owns one health-bar animation coroutine. Repeated damage/healing replaces the previous interpolation; disabling the component stops that animation.
- Health text, main slider, and trailing slider can update independently. Health normalization guards against a zero maximum.
- PlayerStats guards optional equipment/UI references and null class equipment arrays. Death restart is scheduled only once.
- PlayerController rejects interaction when gameplay input is blocked or the target component is disabled, clears focus when disabled, and does not enable movement for a dead player.
- CharacterAnimator guards invalid animator/override-controller setup before use.
- ItemInteract validates pickup data and prevents repeated pickup before deferred destruction completes.
- Inventory drops remove items only after TryDropItem succeeds. Unsupported items and invalid drop prefabs retain the inventory entry; the original public DropItem callback remains available.
- Equipment and slot use guard missing owners, inventory, animators, and icons. Non-positive healing consumables are not consumed.
- PlayerDamageFlash starts hidden and guards missing images and invalid intensity. Camera aim safely handles a missing main camera.
- HealthChange detaches its health event, destroys its separately spawned HUD on owner destruction, handles missing camera/mesh/prefab references, and restarts the flash timeout on repeated hits.
- Tooltip static entry points handle an absent instance; destroyed instances are cleared.
- Loading progress handles an absent slider without interrupting loading.
- Legacy FPC records CharacterController.Move collision flags and checks Below as a bitmask.

### Leveling and skills

- Reassigning LevelSystemAnimated's model detaches the previous model's handlers and resets pending interpolation.
- LevelWindow and UI_SkillTree detach old model handlers when rebound and detach on destruction.
- The fallback Player component detaches its level-display events on destruction.
- Already unlocked skills and SkillType.None no longer spend skill points.
- Simplified prerequisite checks while preserving the existing alternative-path rule: either prerequisite can unlock Whirlwind.
- Animated leveling stops at the maximum level even when the model retains excess XP.
- Level threshold lookup rejects negative indices instead of indexing the array.

### Combat, camera, destructibles, and loot

- Camera shake advances its simulation once per frame rather than once per influence vector.
- StartShake assigns the active shake instead of returning an unattached local instance.
- Shake fades clamp their endpoints and avoid division by zero for immediate fade-out.
- Invalid player projectile prefabs are destroyed after their missing component is reported.
- Projectile range termination respects the existing rangeEnd flag; contact handling guards an empty contact list.
- BlockHitBox handles absent visual/collider components when toggled.
- HealthSystem rejects negative damage/healing, guarantees a positive maximum, clamps health when maximum decreases, and emits death once per life. Explicit Die sets health to zero; healing can restore a subsequent life as before.
- DestructibleObject detaches its event and tolerates absent optional broken/smoke effects. BrokenObject skips missing part renderers and clamps fade alpha.
- Chest/shop generation validates loot entries and uses the same exclusive percentage comparison as enemy loot: zero percent never drops.
- Chest/shop display loops respect slot capacity; chest interaction opens the inventory without toggling an already open inventory closed.
- Dungeon generation validates null room-rule entries before disabling the player.

## Architecture and folder organization

Existing responsibilities remain intact: CharacterCombat owns attack execution; CharacterAnimator owns presentation; EnemyAi owns enemy navigation/state; inventory and equipment remain separate.

The existing folders already group the principal systems. Physical reorganization was deferred because scene/prefab usage cannot be inspected under this task's restrictions. In particular, EnemyManager remains in GamePlay/Player and EnemyAnimator remains at the root. The legacy FPC, EnemyAiTutorial, CameraShaker1, EquipmentManager1, EquipmentManagerUI, and placeholder components remain available rather than being deleted.

Source map:

| Location | Responsibility |
| --- | --- |
| Root | Menu/game flow, loading, door interaction, shared presentation helpers and legacy components |
| Controllers | Player actions/control state/cameras, character animation, inventory canvas, legacy FPC |
| Camera | Camera shake simulation, influence helpers and presentation entry point |
| Dungeon | Dungeon layout, room spawning, NavMesh build and actor placement |
| GamePlay/Combat | Attack/block execution, weapon definitions and projectiles |
| GamePlay/Enemy | Enemy definitions, active AI, legacy tutorial AI and focus presentation |
| GamePlay/Inventory | Inventory storage, item/equipment slots, chest/shop panels |
| GamePlay/Items | Item definitions, consumables, pickups and equipment management |
| GamePlay/Stats | Base/player/enemy statistics and stat modifiers |
| GamePlay/Player | Player content, legacy motor/manager and animator specialization |
| GamePlay/WorldInteracts | Chest and shop interaction/loot setup |
| Systems/DestructibleObject_Scripts | Damageable objects, reusable health and debris cleanup |
| Systems/DragAndDrrop | Rigidbody grabbing/dropping |
| Systems/Helper | Skinned-mesh conversion helper |
| Systems/LevelingSystemScripts | XP model, animated model and XP displays |
| Systems/SkillTree | Skill points, prerequisites, skill UI and legacy player integration |
| Systems/ToolTip | Tooltip display and demonstration components |
| UI | Drag/drop, hotkeys, world health HUD and damage presentation |

## Follow-up status

The remaining concerns were revisited in REMAINING_CONCERNS_REPORT.md. That report supersedes the deferred-issue statuses below and records the second pass. Counts and verification details in this document describe the initial pass.

## Compatibility and remaining concerns

Component class names, inheritance, namespaces, filenames, enum ordering, serialized field names, and public member names were retained. No FormerlySerializedAs attributes were needed because serialized fields were not renamed. This is source-level evidence only; Inspector assignments and serialized asset compatibility have not been verified.

Items intentionally left unresolved:

- LevelSystemAnimated creates a CodeMonkey FunctionUpdater callback. Its implementation and destruction API are outside this folder. Cancellation of that updater remains unverified; guessing an external API would risk compilation failures. Consumer subscriptions are cleaned up.
- Dungeon failures after disabling the player can leave movement disabled and partially generated rooms present. Exception/cancellation recovery also needs review. A safe recovery policy requires confirming the scene's loading and failure behavior.
- UnequipAll removes equipment without returning it to inventory and is bound to U in EquipmentManager. Menu class switching also relies on it. These competing uses need design confirmation before changing semantics.
- BlockHitBox durability and projectile interception contain incomplete logic. The main combat path separately resolves blocking. No durability or shield mechanic was invented.
- Projectile solidCollisionMask and WeaponClass collisionMask are not applied by the current collision queries; some nonalloc query buffers can saturate. Intended layer setup cannot be established from these scripts alone.
- CameraShaker computes position influence but only applies rotation, and adds rotation to the current transform each update. Correct transform ownership/restoration depends on camera hierarchy and update order.
- Tooltip_ItemStats computes bounds adjustments without applying them. Canvas render mode, scale, anchors, and pivots must be inspected before changing coordinate behavior.
- ShopInventorySlot currently grants an item without charging a price or depleting stock. Currency/purchase design is absent here; no purchase system was invented.
- Several scripts still require specific UI child hierarchies/components. Added defensive checks do not establish that all Inspector assignments are valid.
- Required types such as RoomBehaviour, WallCollision, StarterAssets controllers/input, Outline, CodeMonkey utilities, and Unity package APIs are outside the authorized source scope; their implementations and versions were not inspected.
- ExecuteInEditMode mesh conversion and material-instance ownership merit a later asset-aware review. They were not redesigned.

## Verification performed

- Enumerated and read all 106 C# files and reviewed their source-level relationships.
- Captured original source in memory and compared the saved files with the intended changes: all 106 matched, including the 52 unchanged scripts.
- Reread the final review corrections after writing them; saved content matched.
- Checked balanced braces, brackets, and parentheses after excluding strings/comments.
- Compared public member names and serialized/public field declarations against the original source. No removals were detected by these source checks.
- Reviewed retained callbacks and changed method calls against the available scripts.
- No C# compilation was performed: project/package configuration and external dependencies were outside the authorized directory. Delimiter and declaration checks are not compiler validation.
- All file reads and writes for the code audit were confined to Assets/Scripts. Execution tools ran outside the sandbox after sandbox execution hung; their source paths remained confined to the authorized directory.
- Unity was not launched. Scenes, prefabs, Inspector references, runtime behavior, and gameplay were not tested.

Unity Editor verification still needed: compile/import; character setup; rapid damage/healing; death/restart; equipment and hotkeys; repeated skill UI binding and max-level XP; item pickup; full chest transfer; chest/shop opening; ranged impacts/range limits; camera shake; destructible death; and successful/failed dungeon generation.

## Modified C# files

- `Camera/CameraShakeInstance.cs`
- `Camera/CameraShaker.cs`
- `Camera/CameraShaker1.cs`
- `Camera/ShakeDemo.cs`
- `Controller.cs`
- `Controllers/CharacterAnimationEventReceiver.cs`
- `Controllers/CharacterAnimator.cs`
- `Controllers/FPC.cs`
- `Controllers/PlayerController.cs`
- `Controllers/UICanvasController.cs`
- `Dungeon/DungeonGenerator.cs`
- `EnemyAnimator.cs`
- `GamePlay/Combat/BlockHitBox.cs`
- `GamePlay/Combat/EnemyCombat.cs`
- `GamePlay/Combat/PlayerCombat.cs`
- `GamePlay/Combat/Projectile.cs`
- `GamePlay/Inventory/ChestInventory.cs`
- `GamePlay/Inventory/ChestInventoryUI.cs`
- `GamePlay/Inventory/HotKeySlot.cs`
- `GamePlay/Inventory/InventoryUI.cs`
- `GamePlay/Inventory/ShopInventory.cs`
- `GamePlay/Inventory/ShopInventoryUI.cs`
- `GamePlay/Inventory/UISlot.cs`
- `GamePlay/Items/Consumable.cs`
- `GamePlay/Items/Equipment.cs`
- `GamePlay/Items/EquipmentManager.cs`
- `GamePlay/Items/EquipmentManager1.cs`
- `GamePlay/Items/EquipmentManagerUI.cs`
- `GamePlay/Items/ItemInteract.cs`
- `GamePlay/Items/PlayerEquipmentManager.cs`
- `GamePlay/Items/Potion.cs`
- `GamePlay/PlayerAim.cs`
- `GamePlay/PlayerDamageFlash.cs`
- `GamePlay/Player/PlayerAnimator.cs`
- `GamePlay/Stats/PlayerStats.cs`
- `GamePlay/Stats/Stat.cs`
- `GamePlay/WorldInteracts/ChestScript.cs`
- `GamePlay/WorldInteracts/ShopScript.cs`
- `LoadingScript.cs`
- `Systems/DestructibleObject_Scripts/BrokenObject.cs`
- `Systems/DestructibleObject_Scripts/DestructibleObject.cs`
- `Systems/DestructibleObject_Scripts/HealthSystem.cs`
- `Systems/LevelingSystemScripts/LevelSystem.cs`
- `Systems/LevelingSystemScripts/LevelSystemAnimated.cs`
- `Systems/LevelingSystemScripts/LevelWindow.cs`
- `Systems/SkillTree/Player.cs`
- `Systems/SkillTree/PlayerSkills.cs`
- `Systems/SkillTree/UI_SkillTree.cs`
- `Systems/ToolTip/Tooltip.cs`
- `Systems/ToolTip/Tooltip_ItemStats.cs`
- `Systems/ToolTip/Tooltip_Warning.cs`
- `Systems/ToolTip/Window_Items.cs`
- `UI/HealthChange.cs`
- `UI/HotKeysUI.cs`

## Reviewed C# files left unchanged

- `Camera/CameraUtilities.cs`
- `Controllers/CameraController.cs`
- `Controllers/PlayerCameraController.cs`
- `Controllers/PlayerControlState.cs`
- `DamageFlash.cs`
- `DoorScript.cs`
- `Drop.cs`
- `GameController.cs`
- `GamePlay/Abilities.cs`
- `GamePlay/AttackHitBox.cs`
- `GamePlay/Class.cs`
- `GamePlay/Combat/CharacterCombat.cs`
- `GamePlay/Combat/WeaponClass.cs`
- `GamePlay/DungeonExit.cs`
- `GamePlay/Enemy/Enemy.cs`
- `GamePlay/Enemy/EnemyAi.cs`
- `GamePlay/Enemy/EnemyAiTutorial.cs`
- `GamePlay/Enemy/EnemyInteract.cs`
- `GamePlay/Inventory/ChestInventorySlot.cs`
- `GamePlay/Inventory/EquipmentSlotUI.cs`
- `GamePlay/Inventory/Inventory.cs`
- `GamePlay/Inventory/InventorySlot.cs`
- `GamePlay/Inventory/ShopInventorySlot.cs`
- `GamePlay/Items/EnemyEquipmentManager.cs`
- `GamePlay/Items/Interactable.cs`
- `GamePlay/Items/Item.cs`
- `GamePlay/Items/ItemDrop.cs`
- `GamePlay/Loot.cs`
- `GamePlay/Player/EnemyManager.cs`
- `GamePlay/Player/PlayerContent.cs`
- `GamePlay/Player/PlayerManager.cs`
- `GamePlay/Player/PlayerMotor.cs`
- `GamePlay/Stats/CharacterStats.cs`
- `GamePlay/Stats/EnemyStats.cs`
- `HealthTextDisplay.cs`
- `MapController.cs`
- `MenuController.cs`
- `ResetAnimatorBool.cs`
- `SettingsMenu.cs`
- `Systems/DestructibleObject_Scripts/DestructibleConObject.cs`
- `Systems/DestructibleObject_Scripts/IDamageable.cs`
- `Systems/DragAndDrrop/ObjectGrabbable.cs`
- `Systems/DragAndDrrop/PlayerPickUpDrop.cs`
- `Systems/Helper/MeshFromSkinnedMesh.cs`
- `Systems/LevelingSystemScripts/ExperienceBar.cs`
- `Systems/SkillTree/MaterialTintColor.cs`
- `Systems/SkillTree/PlayerSword.cs`
- `Systems/SkillTree/Testing.cs`
- `Systems/ToolTip/ItemInfo.cs`
- `Systems/ToolTip/Window.cs`
- `UI/DragDrop.cs`
- `UI/ScreenDamage.cs`
