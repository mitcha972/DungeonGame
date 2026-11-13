> Historical snapshot. Superseded by [current project context](../../PROJECT_CONTEXT.md) and the subsystem references. Original requirements, file names, counts, and test limits below describe the earlier task, not current instructions. Original bytes are preserved in the organization restore ZIP.

# Project Audit Follow-up

## Scope

This document verifies and deepens the findings from the original audit without changing gameplay code, scenes, prefabs, packages, or project settings. It is limited to source-code and scene/prefab inspection and uses the existing Unity project state as evidence.

## 1. Verified findings from the prior audit

### 1.1 Inventory initialization and item addition are a real defect

Status: Definitely a problem.

Evidence:
- [Inventory.cs](<../../../Scripts/Inventory/Inventory.cs>) defines `public List<Item> items = new List<Item>(20);` and then check logic in `Add()` and `Remove()`. The list has capacity 20 but a count of 0 when created. This means the loop in `Add()` never finds a valid free slot unless the list is populated elsewhere.
- The relevant methods are `Add()`, `Remove()`, `Swap()`, and `AddToSlot()` in [Inventory.cs](<../../../Scripts/Inventory/Inventory.cs>), lines 18-77.

Affected scripts:
- [Inventory.cs](<../../../Scripts/Inventory/Inventory.cs>)
- [EquipmentManager.cs](<../../../Scripts/Inventory/Equipment/EquipmentManager.cs>) uses `inventory.Swap(oldItem, SlotIndex)` and `inventory.Remove(SlotIndex)` in `Unequip()`.
- [Equipment.cs](<../../../Scripts/Inventory/Items/Equipment.cs>) calls `equipmentManager.Equip(this, slotIndex)` during item equip flow.

Reachability:
- This is reachable during gameplay as soon as an item is attempted to be added to inventory or equipment is unequipped and swapped back into inventory.

What would happen if changed:
- If the list were initialized with a fixed number of null entries, inventory slots would behave as intended and UI slot binding would remain consistent.
- The safest correction is to initialize the list to a fixed slot count and use explicit slot indices rather than relying on `Count` to imply slot presence.

Safe correction:
- Treat inventory as a fixed-size collection with null placeholders. A simple `for`-loop or `new List<Item>(Enumerable.Repeat<Item>(null, 20))` style pattern is safer than relying on a list with capacity only.

### 1.2 Equipment callback ordering is not safe

Status: Definitely a problem.

Evidence:
- `PlayerStats.Awake()` subscribes to `equipmentManager.onEquipmentChanged += OnEquipmentChanged` in [PlayerStats.cs](<../../../Scripts/Player/PlayerStats.cs>), lines 29-42.
- In [EquipmentManager.cs](<../../../Scripts/Inventory/Equipment/EquipmentManager.cs>), the callback is fired inside `Unequip()` before `currentEquipment[slotIndex] = newItem` is set in the overloaded `Equip()` method. This is visible around lines 84, 118, and 161.
- The method may call `onEquipmentChanged.Invoke(newItem, oldItem)` while the new equipment state has not been fully applied, which can produce stale stat/UI state.

Affected scripts:
- [PlayerStats.cs](<../../../Scripts/Player/PlayerStats.cs>)
- [CharacterStats.cs](<../../../Scripts/Combat/Stats/CharacterStats.cs>)
- [EquipmentManager.cs](<../../../Scripts/Inventory/Equipment/EquipmentManager.cs>)

Reachability:
- Yes. Equipping and swapping gear is a standard gameplay flow, and it is reachable immediately after the first item pickup or class setup.

What would happen if changed:
- Reordering callback timing to happen after the dictionary update would make UI/stat updates represent the actual new state.

Safe correction:
- Update `currentEquipment` first, then fire the callback, ensuring `OnEquipmentChanged` sees the new state and old state in a consistent order.

### 1.3 Multiple player controller scripts are active in overlapping layers

Status: Confirmed architectural duplication; exact runtime usage uncertain without a live editor session.

Evidence:
- [PlayerController.cs](<../../../Scripts/Player/PlayerController.cs>) handles mouse input, attack, focus, and movement state. It references `ThirdPersonController` and `PlayerCombat` directly.
- [ThirdPersonController.cs](<../../../GamePlay/Character/Scripts/ThirdPersonController.cs>) is a modern Starter Assets controller and is the main movement controller for a CharacterController-based player.
- [FPC.cs](<../../../Scripts/Player/Legacy/FPC.cs>) is an older first-person design script and is not a simple empty utility.

Affected scripts:
- [PlayerController.cs](<../../../Scripts/Player/PlayerController.cs>)
- [ThirdPersonController.cs](<../../../GamePlay/Character/Scripts/ThirdPersonController.cs>)
- [FPC.cs](<../../../Scripts/Player/Legacy/FPC.cs>)
- [UICanvasController.cs](<../../../Scripts/UI/UICanvasController.cs>)

Reachability:
- The project contains more than one player control path, so this is reachable in any scene that uses the wrong prefab or serialized references.
- The scene inspection evidence shows a PlayerFollowCamera object in DungeonTest.unity (`../Assets/Scenes/DungeonTest.unity`; historical target no longer exists) and the player prefab in [PlayerHolder.prefab](<../../../Prefabs/Player/PlayerHolder.prefab>), which confirms the project has multiple player variants and camera setups.

What would happen if changed:
- A single authoritative movement controller would reduce state conflicts, duplicate input handling, and camera/rotation bugs.

Safe correction:
- Do not delete scripts blindly. Instead, choose one active player control flow for the runtime prefab and disable or isolate legacy scripts until the scene/prefab is intentionally migrated.

### 1.4 Incomplete controller methods are real, but not necessarily fatal until used

Status: Definitely incomplete in the current code; runtime reachability depends on the active scene and prefab.

Evidence:
- [PlayerController.cs](<../../../Scripts/Player/PlayerController.cs>) contains empty or placeholder logic in `MouseLocked()`, `MouseUnlocked()`, `UpdateText()`, and `RemoveText()`. The exact places are visible in the file around the `MouseMode` logic and text-handling sections.
- The `RightClick()` and `LeftClick()` logic also mixes input flags, heavy attack timing, and focus logic in a way that needs a single consistent controller flow.

Affected scripts:
- [PlayerController.cs](<../../../Scripts/Player/PlayerController.cs>)
- [UICanvasController.cs](<../../../Scripts/UI/UICanvasController.cs>)
- [FPC.cs](<../../../Scripts/Player/Legacy/FPC.cs>)

Reachability:
- Reachable when the active player controller receives mouse input, but some code paths are currently placeholder-only and will not work as intended in their current form.

Safe correction:
- Preserve the intended input behavior but centralize it behind one movement/combat input controller; leave the placeholder methods as explicit stubs until the active controller is finalized.

### 1.5 Combat and damage calculations are functionally connected but not fully consistent

Status: Partially working; not a simple crash, but the design is brittle and has logic drift.

Evidence:
- Damage flow is processed via `CharacterStats.TakeDamage()` in [CharacterStats.cs](<../../../Scripts/Combat/Stats/CharacterStats.cs>), lines 54-110.
- Impact and target handling is handled in [AttackHitBox.cs](<../../../Scripts/Combat/AttackHitBox.cs>), lines 26-100, and [Projectile.cs](<../../../Scripts/Combat/Projectile.cs>), lines 65-162.
- `CharacterCombat.Attack()` sets `attackIsReady = false` and waits on `AttackCooldown()` in [CharacterCombat.cs](<../../../Scripts/Combat/CharacterCombat.cs>), lines 47-92.
- Damage reduction is applied with `damageTaken -= armor` and then clamped; this is a basic formula, not a robust damage model.

Affected scripts:
- [CharacterCombat.cs](<../../../Scripts/Combat/CharacterCombat.cs>)
- [PlayerCombat.cs](<../../../Scripts/Combat/PlayerCombat.cs>)
- [EnemyCombat.cs](<../../../Scripts/Combat/EnemyCombat.cs>)
- [AttackHitBox.cs](<../../../Scripts/Combat/AttackHitBox.cs>)
- [Projectile.cs](<../../../Scripts/Combat/Projectile.cs>)
- [CharacterStats.cs](<../../../Scripts/Combat/Stats/CharacterStats.cs>)

Reachability:
- Attack flow is reachable in gameplay. The system is active and designed to hit both melee and projectile attacks.

What would happen if changed:
- A unified damage pipeline (owner, target, mitigation, status, critical chance, hit validation) would make melee and projectile damage consistent.

Safe correction:
- Keep the existing flow but centralize the final damage resolution in one method rather than repeating reduction logic in multiple attack classes.

### 1.6 Enemy AI and state management are complex and partially redundant

Status: Partially working and higher-risk than the project wants to admit.

Evidence:
- Enemy state transitions occur in `ChangeState()` and the coroutines `Idle()`, `Patrol()`, `InCombat()`, `Attacking()`, `ResetAttackPosition()`, and `SearchWalkPoint()` in [EnemyAi.cs](<../../../Scripts/NPC/Enemies/EnemyAi.cs>), lines 92-321 and 420-787.
- Enemy target detection and line-of-sight checks are implemented in `TargetSearch()`, `IsTargetInSight()`, and `GetTargetRangeState()` in [EnemyAi.cs](<../../../Scripts/NPC/Enemies/EnemyAi.cs>), lines 757-787.
- Multiple coroutine-driven state switches can create overlapping logic or inconsistent target decisions.

Affected scripts:
- [EnemyAi.cs](<../../../Scripts/NPC/Enemies/EnemyAi.cs>)
- [EnemyCombat.cs](<../../../Scripts/Combat/EnemyCombat.cs>)
- [Enemy.cs](<../../../Scripts/NPC/Enemies/Enemy.cs>)
- [EnemyStats.cs](<../../../Scripts/NPC/Enemies/EnemyStats.cs>)

Reachability:
- Reachable in combat encounters. The state machine is active and event-driven.

Safe correction:
- Keep the existing state model but reduce destructive coroutine restart patterns (`StopAllCoroutines` plus new state starts) and centralize target-search and aim decision logic.

### 1.7 Procedural dungeon generation is workable but not guaranteed to be robust

Status: Partially working.

Evidence:
- The generator is implemented in [DungeonGenerator.cs](<../../../Scripts/World/Dungeon/DungeonGenerator.cs>), with the key stages `MazeGenerator()`, `GenerateDungeon()`, `FindStartRoom()`, `FindEndRoom()`, and `CheckNeighbors()` around lines 54-316.
- It creates cells, validates a minimum number of cells, and regenerates if fewer than half the cells are visited (`if (cellsMade < (size.x * size.y)/2)`).
- However, the algorithm is based on a maze-like graph and room choice is selected by `ProbabilityOfSpawning()`, which is a heuristic rather than a guarantee of a traversable, balanced dungeon.

Affected scripts:
- [DungeonGenerator.cs](<../../../Scripts/World/Dungeon/DungeonGenerator.cs>)
- [DungeonExit.cs](<../../../Scripts/World/Dungeon/DungeonExit.cs>)
- [MapController.cs](<../../../Scripts/World/Map/MapController.cs>)

Reachability:
- Fully reachable during the dungeon-generation scenes. It is not merely theoretical.

Safe correction:
- Keep the current algorithm but add a post-generation validation pass that verifies connectivity and required room placement before allowing the dungeon to be exposed to a player.

### 1.8 CodeMonkey is a real dependency, not just an aesthetic choice

Status: Definitely a dependency; essential for some UI/utility systems but not the core gameplay loop.

Evidence:
- [LevelSystemAnimated.cs](<../../../Scripts/Progression/Leveling/LevelSystemAnimated.cs>) uses `using CodeMonkey.Utils;` and `FunctionUpdater.Create(() => Update());`.
- Several UI and tooltip scripts reference CodeMonkey utilities: [Tooltip.cs](<../../../Scripts/UI/Tooltips/Tooltip.cs>), [Tooltip_ItemStats.cs](<../../../Scripts/UI/Tooltips/Tooltip_ItemStats.cs>), [UI_SkillTree.cs](<../../../Scripts/Progression/Skills/UI_SkillTree.cs>), [ExperienceBar.cs](<../../../Scripts/Progression/Leveling/ExperienceBar.cs>), and [Assets.cs](<../../../Plugins/CodeMonkey/Assets.cs>).

Affected scripts:
- [Assets.cs](<../../../Plugins/CodeMonkey/Assets.cs>)
- [LevelSystemAnimated.cs](<../../../Scripts/Progression/Leveling/LevelSystemAnimated.cs>)
- [Tooltip.cs](<../../../Scripts/UI/Tooltips/Tooltip.cs>)
- [UI_SkillTree.cs](<../../../Scripts/Progression/Skills/UI_SkillTree.cs>)

Reachability:
- Yes. These scripts are active in the UI and progression-oriented scene flow.

Safe correction:
- Keep CodeMonkey as a utility layer for the time being, but isolate its use so gameplay code does not become tightly coupled to the third-party package.

### 1.9 Save/load functionality is not implemented

Status: Not implemented.

Evidence:
- A targeted search for `PlayerPrefs`, `Save()`, `Load()`, `SaveSystem`, `Serialize`, and `Deserialize` in the project scripts returned no persistent save system. The absence is itself meaningful in this project context.
- The only persistent state is static values in [GameController.cs](<../../../Scripts/Core/GameController.cs>) and scene transitions, not serializable player progression.

Affected scripts:
- [GameController.cs](<../../../Scripts/Core/GameController.cs>)
- [MapController.cs](<../../../Scripts/World/Map/MapController.cs>)
- [PlayerStats.cs](<../../../Scripts/Player/PlayerStats.cs>)

Reachability:
- Not reached in an actual save/load flow because no implementation exists.

Safe correction:
- The project should explicitly decide whether save files are required at all before adding a save system. If they are needed, create a single save contract that serializes player stats, inventory, and dungeon state rather than scattering save logic across scripts.

## 2. Player system map

### Actual player prefab and scene hierarchy

From source and scene evidence:
- The project includes a player hierarchy under [Assets/Prefabs/Player](<../../../Prefabs/Player>), especially [PlayerHolder.prefab](<../../../Prefabs/Player/PlayerHolder.prefab>) and [PlayerContent.prefab](<../../../Prefabs/Player/PlayerContent.prefab>).
- DungeonTest.unity (`../Assets/Scenes/DungeonTest.unity`; historical target no longer exists) contains a `PlayerFollowCamera` object around line 28433, indicating that the main scene is built around a third-person camera-follow setup.
- The player prefab includes multiple child objects such as `PlayerAim` and `PlayerFollowCamera`, which indicates the gameplay prefab is designed for a third-person aim/camera setup.

### Active movement and control flow

Controlled by multiple scripts:
- [ThirdPersonController.cs](<../../../GamePlay/Character/Scripts/ThirdPersonController.cs>): modern player movement, camera yaw and movement calculations, `Update()` and `LateUpdate()` call movement/camera rotation. This is the strongest control loop and matches the Starter Assets setup.
- [PlayerController.cs](<../../../Scripts/Player/PlayerController.cs>): custom controller for mouse input, focus, attack events, and combat triggers. It references `PlayerCombat`, `PlayerStats`, and `ThirdPersonController` and is a secondary override/integration layer.
- [FPC.cs](<../../../Scripts/Player/Legacy/FPC.cs>): legacy/mixed first-person controller code; it is not the active modern character controller but it still exists in the project.

Execution flow:
1. A scene loads with a `PlayerFollowCamera` and player root GameObject.
2. `ThirdPersonController.Update()` calls movement and camera updates on the same frame.
3. `PlayerController.Update()` calls `MouseInput()` and `GetInput()`, then triggers `playerCombat.Ability()` on `T` and attack input on mouse input.
4. The player uses `PlayerCombat` and `CharacterCombat` for attack startup and cooldown.
5. `PlayerStats` tracks health, equipment, level, and damage flash.
6. `EquipmentManager` governs equipment state and stat modifiers.
7. System integration is real but not cleanly centralized.

### Player health, stats, combat, inventory, equipment

- Health and stat state are handled in [PlayerStats.cs](<../../../Scripts/Player/PlayerStats.cs>), which subscribes to equipment changes and updates UI.
- Player class and combat data flow are influenced by [Class.cs](<../../../Scripts/Player/Data/Class.cs>) and [Equipment.cs](<../../../Scripts/Inventory/Items/Equipment.cs>).
- Inventory is in [Inventory.cs](<../../../Scripts/Inventory/Inventory.cs>).
- Equipment is managed by [EquipmentManager.cs](<../../../Scripts/Inventory/Equipment/EquipmentManager.cs>).
- Player-specific combat is in [PlayerCombat.cs](<../../../Scripts/Combat/PlayerCombat.cs>).

### Redundant or experimental scripts

These scripts are not necessarily invalid, but they are indicators of experimental or overlapping design:
- [PlayerController.cs](<../../../Scripts/Player/PlayerController.cs>)
- [FPC.cs](<../../../Scripts/Player/Legacy/FPC.cs>)
- [Controller.cs](<../../../Scripts/Core/Legacy/Controller.cs>)
- GameController1.cs (`../Assets/Scripts/GameController1.cs`; historical target no longer exists)

The safest interpretation is: the project is not yet unified on a single authoritative player controller. The risk is not deletion; it is selecting the live one and isolating the rest.

## 3. Combat architecture map

### Attack initiation

- Player attack input is handled in [PlayerController.cs](<../../../Scripts/Player/PlayerController.cs>): `MouseInput()` calls `RightClick()` and `LeftClick()`, and `LeftClick()` calls `playerCombat.Attack(false)` or `Attack(true)` for heavy attacks.
- Non-player combat uses the same `CharacterCombat.Attack()` flow from [CharacterCombat.cs](<../../../Scripts/Combat/CharacterCombat.cs>): `Attack(bool isHeavyAttack)` runs `OnAttack`, sets `attackIsReady = false`, and starts `AttackCooldown()`.

### Attack processing and target detection

- Attack hit detection is in [AttackHitBox.cs](<../../../Scripts/Combat/AttackHitBox.cs>), `OnTriggerEnter()` and `CharacterCollision()`.
- Projectile hit detection is in [Projectile.cs](<../../../Scripts/Combat/Projectile.cs>), `OnTriggerEnter()`, `CharacterCollision()`, and `DealDamage()`.
- The hit box checks `CharacterStats` and avoids repeated hits using `collidedCharacters` lists.

### Damage calculation

- Final damage reduction logic is split between the hit box and `CharacterStats.TakeDamage()`.
- [CharacterStats.cs](<../../../Scripts/Combat/Stats/CharacterStats.cs>) subtracts armor and clamps damage to at least 1.
- `AttackHitBox.DealDamage()` sends `myStats.damage - damageReduction` to `target.TakeDamage(...)` while `Projectile` does the same.

### Weapon stats and armor effects

- Item equipment modifies `armor`, `damage`, and `range` in [CharacterStats.cs](<../../../Scripts/Combat/Stats/CharacterStats.cs>) via `OnEquipmentChanged()`.
- Equipment definitions live in [Equipment.cs](<../../../Scripts/Inventory/Items/Equipment.cs>), and weapon behavior is partly represented by [WeaponClass.cs](<../../../Scripts/Combat/Data/WeaponClass.cs>).

### Health reduction and death

- Health reduction occurs in `CharacterStats.TakeDamage()` and `PlayerStats.TakeDamage()`.
- Death triggers are in `CharacterStats.Die()` and `PlayerStats.Die()`, where `PlayerManager.instance.KillPlayer()` is invoked.

### Loot drops

- [Loot.cs](<../../../Scripts/Inventory/Items/Loot.cs>) and item pickup scripts indicate loot is part of the design, but the exact drop trigger flow is not unified into a single explicit loot controller. The project uses a data-driven item model, but the actual drop execution is spread across item and world-interact scripts.

### Enemy attacks and player attacks

- Enemy attack processing is mirrored by [EnemyCombat.cs](<../../../Scripts/Combat/EnemyCombat.cs>), which overrides projectile creation and attack paths.
- `EnemyAi` drives target acquisition and attack timing, with a broad coroutine-based decision tree.

### Architectural assessment

The project can support a common underlying damage framework, but it is not currently centralized. The right direction is:
- one source of truth for final damage resolution,
- one common hit validation pipeline,
- one standard way to apply armor, resistances, criticals, and status effects,
- one explicit item-data contract for weapon stats.

This would support future expansions for weapon classes, armour defense, enemy resistances, critical hits, and status effects without changing the core combat model.

## 4. Inventory, items, and equipment lifecycle

The intended lifecycle is:

Item definition -> Item instance -> Inventory -> Equipment -> Stat change -> Visual mesh attach

### Actual flow

- Item definition: [Item.cs](<../../../Scripts/Inventory/Items/Item.cs>)
- Equipment: [Equipment.cs](<../../../Scripts/Inventory/Items/Equipment.cs>)
- Inventory: [Inventory.cs](<../../../Scripts/Inventory/Inventory.cs>)
- Equipment manager: [EquipmentManager.cs](<../../../Scripts/Inventory/Equipment/EquipmentManager.cs>)
- Player stat application: [CharacterStats.cs](<../../../Scripts/Combat/Stats/CharacterStats.cs>) and [PlayerStats.cs](<../../../Scripts/Player/PlayerStats.cs>)

### Inventory and equipment issues

- The list initialization bug is real and changes the whole item lifecycle from the first add operation.
- The equipment callback ordering issue is real and affects UI/stat updates during equip and unequip.
- Potential item dropping is implemented in `EquipmentManager.DropItem()`, but it is not fully integrated with a central loot orchestration system.

### Data-driven design recommendation

The future design should remain simple and reusable:
- `ItemDefinition` (ScriptableObject) for data only
- `ItemInstance` for runtime references, count, and unique state
- `InventorySlot` as a slot index with item reference and count
- `EquipmentSlot` with explicit type mapping
- `EquipmentManager` as a stat recalculation step only
- `Visual equipment attachment` handled by one mesh-attach routine, not repeated in multiple scripts

This aligns with the project’s stated design goal of creating new weapons, armour, and items mostly through data and prefabs rather than duplicate scripts.

## 5. Procedural dungeon generation analysis

### Algorithm summary

The generator uses a grid-based maze algorithm and then selects room objects based on rules from [DungeonGenerator.cs](<../../../Scripts/World/Dungeon/DungeonGenerator.cs>):

- `MazeGenerator()` builds a visited cell list and traverses neighbors using `CheckNeighbors()`.
- `FindStartRoom()` and `FindEndRoom()` mark room locations.
- `GenerateDungeon()` instantiates room prefabs based on the cell state and room rules.
- A retry path exists when cells made is too low: `if (cellsMade < (size.x * size.y)/2)`.

### What this means

This is a constructive maze-like generator, not a perfect dungeon validation system. It does not provide a guaranteed, mathematically validated path network before use.

### Risks

Possible issues:
- isolated rooms if the maze generation does not produce full connectivity,
- visually valid but not traversable rooms when path selection is mis-specified,
- room overlap or duplicate instantiation if rules produce conflicting positions,
- missing/end-room logic on a malformed map,
- infinite or repeated regenerate attempts if generation state is unlucky or size configuration is pathological.

### Safe improvement without rewriting the generator

- Add a post-generation connectivity pass.
- Validate start-to-end traversal using BFS/DFS on the generated room graph.
- Reject and regenerate if required objective room placement is invalid.
- Log generation failures with a clear seed and room count for debugging.

## 6. Enemy system analysis

### Configuration and behavior

- Enemy data is defined in [Enemy.cs](<../../../Scripts/NPC/Enemies/Enemy.cs>) and stats in [EnemyStats.cs](<../../../Scripts/NPC/Enemies/EnemyStats.cs>).
- Behavior is centralized in [EnemyAi.cs](<../../../Scripts/NPC/Enemies/EnemyAi.cs>), with idle, patrol, combat, attack, reset-position, and death states.
- Combat logic is in [EnemyCombat.cs](<../../../Scripts/Combat/EnemyCombat.cs>).

### How the AI works

- `TargetSearch()` identifies the player if in sight range.
- `IsTargetInSight()` checks the line-of-sight ray.
- `ChangeState()` switches among combat states.
- `ResetAttackPosition()` tries to find a valid position around the player.
- `Attack()` invokes `combat.Attack(false)` and then resets `canAttack` after `timeBetweenAttacks`.

### Extensibility

This is a good base for a shared enemy framework because it already uses:
- a common `CharacterStats` base,
- a common `CharacterCombat` base,
- a common AI decision layer,
- different stats and prefabs for different enemy types.

The main issue is not the concept but the state/AI coupling. It is too coroutine-heavy to be cleanly extensible without discipline.

## 7. Plugin and CodeMonkey dependency map

| Dependency | Used by | Functionality | Replacement difficulty | Notes |
| --- | --- | --- | --- | --- |
| CodeMonkey Utilities | [LevelSystemAnimated.cs](<../../../Scripts/Progression/Leveling/LevelSystemAnimated.cs>), [Tooltip.cs](<../../../Scripts/UI/Tooltips/Tooltip.cs>), [UI_SkillTree.cs](<../../../Scripts/Progression/Skills/UI_SkillTree.cs>) | UI helpers, timers, updater loops, bars, tooltips | Medium | Runtime dependency; probably replaceable but not urgent |
| NavMeshComponents | [EnemyAi.cs](<../../../Scripts/NPC/Enemies/EnemyAi.cs>) and AI navigation flow | Pathfinding and movement | Medium | Standard runtime dependency |
| Cinemachine | Scene camera behavior, `PlayerFollowCamera` | Third-person camera follow | Low to medium | Standard runtime dependency |
| URP / lighting pipeline | Project-wide rendering | Rendering and post-process | Low | Core package, not custom gameplay |
| ProBuilder | editor-level scene construction | editor tooling | Editor-only | Not runtime gameplay |
| Standard Assets | Old movement scripts and path examples | Legacy input/movement examples | Medium | Mostly legacy or reference code |
| SlimUI Modern Menu | Menu scenes and UI | Menus | Medium | UI system, not core gameplay |

### Ownership recommendation

The project can realistically own its own gameplay logic, but the simplest path is to keep the third-party tooling at the utility layer while clearly separating it from the game systems.

## 8. Project-wide implementation status

| System | Status | Rationale |
| --- | --- | --- |
| Character movement | PARTIALLY WORKING | There is a modern controller and older/experimental controllers present |
| Camera follow | WORKING | Scene contains PlayerFollowCamera and Cinemachine setup |
| Player combat | PARTIALLY WORKING | Attack flow exists and is reachable, but it is not a unified final damage pipeline |
| Enemy AI | PARTIALLY WORKING | State machine exists and is active, but is coroutine-rich and complex |
| Inventory | BROKEN | List initialization bug is established |
| Equipment | PARTIALLY WORKING | Basic equipment flow exists but callback ordering is unsafe |
| Dungeon generation | PARTIALLY WORKING | Functional prototype, but no guaranteed validation pass |
| Leveling / skill system | PARTIALLY WORKING | CodeMonkey dependencies and UI system exist |
| Save/load | NOT IMPLEMENTED | No serialization or save contract found |
| Loot | PARTIALLY WORKING | Item and world-interact logic exists, but not centralized |
| CodeMonkey dependency | WORKING | It is clearly used in active systems |
| Legacy player controller layer | UNUSED / REDUNDANT | Several examples exist but not all are in the live path |

## 9. Unity editor investigation requests

Unity MCP is not available in this environment, so this is a ready-to-copy Unity AI inspection request.

### Unity AI task request

Inspect the main gameplay scene and player prefab, then return the following:

1. Identify the root player GameObject in the main gameplay scene and list its immediate children.
2. Confirm which active components are attached to the live player object.
3. Report the active movement script, camera script, animator, CharacterController, PlayerStats, PlayerCombat, EquipmentManager, and Inventory components used by the live player.
4. Confirm whether the scene references the same player prefab or a different player instance than the prefab asset.
5. Inspect any duplicate player controllers and determine which one is serialized in the scene.
6. Report whether the scene contains any `GameController`, `PlayerFollowCamera`, `DungeonGenerator`, or enemy spawn objects.
7. Check the prefab for `PlayerAim`, `Inventory`, `Canvas`, and any `GameController` references.
8. Record all Unity console warnings/errors from the project or active scene.
9. Return the exact object names and component types, not just script names.
10. State whether any missing script references or null references are present in the scene/prefab.

### Why this is needed

The current code inspection confirms the source-level uncertainty: there are multiple player-controller and prefab patterns, and only a live scene/prefab inspection can identify which path is actually active at runtime.

## 10. Build and migration validation

### .NET build result

Attempted command:
`dotnet build "Dungeon Game.sln" -nologo`

Observed result:
`dotnet : The term 'dotnet' is not recognized ...`

This tells us the local machine does not have the .NET SDK installed, so the .NET build check is not valid in this environment.

### Unity Editor validation

The project does have Unity installed, and a direct Unity batch-mode validation is available. Command used:

`& "C:\Program Files\Unity\Hub\Editor\6000.3.28f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\mitch\Documents\Unity\My_Projects\Dungeon Game"`

Observed result:
- Exit code: 0

This is important: the Unity project successfully completed a batch-mode compile check without editor changes, which indicates the project itself is not trivially broken in the Unity compile sense.

### Conclusion

- The failed `dotnet build` is an environment issue, not a project defect.
- The Unity batch-mode compile is a more appropriate validation path for this project.
- The project may still have gameplay logic issues even with a successful compile, which is why we separate compile validation from gameplay correctness.

## 11. Prioritized repair recommendations

### Priority 1: Fix inventory initialization and slot logic

Why first:
- This is a concrete defect that affects core item flow.
- It is simple to verify and low-risk to fix if done correctly.

Dependencies:
- Inventory UI slot mapping
- Equipment swap logic
- Item pickup scripts

Potential side effects:
- UI may need slot rebind if a new list pattern changes the slot order.

### Priority 2: Centralize equipment callback ordering

Why second:
- This prevents inconsistent stat or UI updates when equipping gear.

Dependencies:
- The current `OnEquipmentChanged` callback model
- Player and enemy stat recalculation

Potential side effects:
- UI may refresh after a new equip state, which is expected and desirable.

### Priority 3: Pick one player controller and isolate legacy ones

Why third:
- This reduces duplicated input and movement logic.

Dependencies:
- Scene/prefab inventory of the live player object
- Input handling and camera logic

Potential side effects:
- Some old scenes or prefabs may need rebinds if the wrong controller was doing work.

### Priority 4: Stabilize enemy AI state transitions

Why fourth:
- This is an active combat system and should be made consistent before adding new enemy behaviors.

Dependencies:
- Target detection
- Attack timing logic
- Pathing and combat cooldown

Potential side effects:
- AI may become more predictable; this is an acceptable improvement.

### Priority 5: Add procedural generation validation

Why fifth:
- Prevents badly connected dungeons from reaching the player.

Dependencies:
- Dungeon room counts and seed handling
- Room rules and graph validation

Potential side effects:
- Some generation attempts may require rerolls more often, which is expected.

### Priority 6: Define save/load strategy explicitly

Why sixth:
- This project does not currently have a save system, and adding one without a contract will create more inconsistency.

Dependencies:
- Player progression, inventory, and dungeon state

Potential side effects:
- Requires serialization design and a clear save-file schema.

## 12. Final assessment

This project is not an abandoned codebase; it is an active prototype with a sensible design direction and a partially implemented game loop. The biggest threats are not theoretical—they are concrete, source-backed issues in inventory logic, equipment callback ordering, scene/controller duplication, and a loosely validated enemy and dungeon flow.

The safest first repair is to address the inventory initialization and equipment callback ordering before adding more feature work. These are the highest-confidence defects and they affect the core item loop directly.

The project is developable, but it needs a disciplined consolidation pass before a larger content expansion is reliable.
