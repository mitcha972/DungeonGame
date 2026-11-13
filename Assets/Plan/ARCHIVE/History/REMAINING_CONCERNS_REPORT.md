> Historical snapshot. Superseded by [current project context](../../PROJECT_CONTEXT.md) and the subsystem references. Original requirements, file names, counts, and test limits below describe the earlier task, not current instructions. Original bytes are preserved in the organization restore ZIP.

# Remaining-concerns follow-up — 2026-11-10

This continues the initial cleanup under the same restrictions. All source inspection and changes were confined to Assets/Scripts. Seventeen existing C# scripts were modified and one runtime helper was added. No folder moves, renames, scene/prefab edits, package changes, or metadata edits were made. Unity was not launched.

## Resolved or narrowed concerns

### Leveling update lifetime

LevelSystemAnimated no longer imports or creates CodeMonkey FunctionUpdater callbacks. A small local LevelSystemAnimatedUpdater component advances the model instead. PlayerStats and the fallback Player explicitly bind it to their own GameObject and dispose the model on destruction. Disposing detaches model events and removes the helper; helper destruction also disposes its model. Disposal is idempotent.

The existing one-argument constructor remains available and creates a scene-scoped helper object. No external updater cancellation API is assumed. The additional helper is runtime-created and needs no Inspector assignment.

### Dungeon failure and cancellation recovery

Generation now keeps the previous room hierarchy, board, spawn position, player control settings, and NavMeshData until the replacement is ready. The old rooms are temporarily inactive while the replacement navigation data is built.

On failure or cancellation, the candidate rooms are deactivated/removed, the previous room hierarchy and navigation data are restored, and player position/control settings are restored. A failed regeneration can retain the old floor's ready state so a later retry remains possible. First-generation failure still reports an error; no replacement scene or fallback floor is invented.

The public TT coroutine remains available. A wrapper catches exceptions from the generation iterator and clears IsGenerating in a finally block. OnDisable explicitly disposes an active generation iterator as well as stopping coroutines, so recovery does not depend solely on Unity invoking iterator cleanup.

DungeonExit now requests AdvanceFloor on the generator. Floor-number advancement is rolled back if generation fails or is interrupted. The counter is advanced before successful enemy setup, preserving floor-based equipment selection.

Navigation rollback uses NavMeshSurface.navMeshData/AddData. Package implementation and version could not be inspected under the directory restriction; import/compilation and actual NavMesh restoration require Editor verification.

### Equipment reset versus gameplay unequip

The original public UnequipAll remains a reset operation for existing callers such as menu class switching. The U shortcut uses a separate UnequipAllToInventory path. It returns items only when inventory accepts them and leaves items equipped if inventory is full. It also respects the available player-control and animation busy states; enemy equipment managers without inventory do nothing.

PlayerEquipmentManager clears the appropriate equipment UI when an item is unequipped. EquipmentSlotUI validates its displayed item against the actual equipped item before adding it to inventory, avoiding transfers from stale slot displays. The existing Unequip callback delegates to the actual unequip action.

Blend-shape writes now guard absent region arrays/meshes and invalid indices.

### Physics query saturation

Projectile sweeps, melee overlap queries, enemy candidate scans, and enemy line-of-sight queries keep their existing reusable buffers in ordinary cases. A full buffer triggers a complete allocating query for that exceptional case, avoiding silently omitted targets or nearer walls.

Projectile impacts still resolve nearest hits first. Existing query masks and friendly-fire rules are preserved; this pass does not guess how uninspected layer assignments should work.

### Camera shake

Rotation shake now applies a relative offset in LateUpdate and removes a recognized previous shake pose before applying the next offset. Ending or disabling a shake restores the tracked baseline when the camera has not been overwritten by another controller. A late execution order lets ordinary camera controllers establish their pose first.

This fixes accumulation when the shaker owns the pose and accommodates controllers that overwrite their camera pose absolutely. It cannot establish compatibility with every external incremental camera controller without inspecting or running that controller. Positional influence and RestPositionOffset remain unapplied, preserving the previous visual behavior until the intended camera hierarchy and amplitudes are verified.

### Item tooltip placement

Tooltip_ItemStats now converts the pointer into its parent plane and clamps its background's actual corners in canvas space. It accounts for canvas bounds, scale, pivots, and nested transforms instead of calculating unused bounds adjustments. Overlay canvases use a null event camera; other canvases use the assigned UI camera or canvas camera.

References are cached and missing text/background references are handled defensively. Empty-item hover hides the old tooltip. Layout is rebuilt when an item is shown before its bounds are measured. Actual canvas/camera assignments and visual placement still need Editor verification.

### Mesh conversion

MeshFromSkinnedMesh reuses existing MeshFilter/MeshRenderer components rather than always adding duplicates. It shares the source mesh/material references rather than requesting a mesh instance. It preserves the original bind-mesh conversion behavior; it does not bake a pose or redesign the helper.

During play, source/helper destruction is deferred with Destroy and the old renderer is disabled. DestroyImmediate remains confined to the existing edit-mode conversion path. Conversion preserves the source when a valid target component cannot be obtained.

### Material instances with clear ownership

BrokenObject explicitly creates and owns each debris fade material, caches it for fading, restores its prior material when appropriate, and destroys only its own instances. Repeated per-frame material getters were removed.

MaterialTintColor explicitly owns the material it creates for its default Body renderer. Replacement/disposal restores the renderer's original material when appropriate and destroys that owned instance. Materials passed through the existing public SetMaterial method remain caller-owned and are not destroyed.

### Shield bookkeeping

BlockHitBox identifies its owner, rejects its owner's projectiles and duplicate entries, prunes destroyed projectile references, and clears hit bookkeeping when disabled. Durability loss, regeneration rules, interception/destruction, and combat damage multipliers were not invented or rebalanced.

## Still unresolved under the same restrictions

- **Shield design:** durability/interception code remains incomplete. The existing combat damage-reduction path remains authoritative. Its intended durability and projectile-consumption rules need design confirmation.
- **Shop design:** ShopInventorySlot still grants an item without charging currency or depleting stock. No currency model or purchase policy is established by these scripts.
- **Collision masks:** solidCollisionMask and WeaponClass.collisionMask remain unapplied. Choosing whether they filter targets, world obstacles, or both requires actual layer assignments and intended behavior.
- **Camera hierarchy:** positional shake remains deferred; external incremental pose controllers may need coordinated shake-offset removal.
- **Other material users:** DamageFlash and HealthChange still use Renderer.material. Their ownership is less clear because multiple effects can share a renderer. A later review should coordinate their color overrides before destroying or replacing shared runtime material instances. They were inspected and deliberately left unchanged here.
- **Editor conversion effects:** ExecuteInEditMode conversion still changes components when run by Unity. The helper was not executed. Mesh pose expectations, Undo/prefab overrides, and real asset conversion must be reviewed in the Editor.
- **UI/data assignments:** missing-reference guards do not establish Inspector correctness. World-space tooltip camera setup and oversized tooltips require visual inspection.
- **External APIs and dependencies:** NavMeshSurface, RoomBehaviour, WallCollision, StarterAssets controllers/input, Outline, and other package implementations remain outside the authorized directory and were not inspected.
- **First-floor failure:** no valid generated floor exists to restore on initial failure. The code restores prior state and reports LastFailure; choosing a loading/error/menu experience would require scene knowledge and would be a design change.

## Compatibility

Existing component class names, namespaces, inheritance, serialized field names, enum ordering, filenames, and public member names were retained. The source adds methods/overloads and one runtime-only helper; no existing serialized field was renamed, so no FormerlySerializedAs attributes were needed.

LevelSystemAnimated's existing constructor still automatically updates. Existing Unity-facing callbacks such as UnequipAll, DropItem, TT, and EquipmentSlotUI.Unequip remain available. These are source-level compatibility checks, not proof that asset serialization or Inspector assignments are intact.

## Verification

- Traced source callers for leveling, equipment reset/unequip, materials, blocking, and collision masks.
- Captured the 19 inspected existing scripts and checked their hashes/content before editing.
- Reread all 20 involved files after editing: all matched the intended source. This includes the new helper and the two unchanged material-effect scripts.
- Checked balanced delimiters after excluding comments and strings.
- Compared existing public member names against the captured source; no removals were found.
- Reviewed generation success, validation failure, mid-generation failure, exception, disable/cancellation, and floor-counter paths at source level.
- Reviewed idempotent model disposal, owner/helper destruction, saturated query fallbacks, stale equipment slots, and full-inventory unequip behavior at source level.
- No Unity launch, scene run, gameplay testing, asset inspection, dependency installation, or project compilation was performed. Source checks do not substitute for C# compilation or runtime validation.

Editor checks still required: compile/import; ordinary and cancelled/failed floor transitions; retry after generation failure; NavMeshData restoration; XP updates and owner destruction; U with full/partially full inventory; menu class switching; equipment UI consistency; dense collision queries; tooltip canvas modes; shake completion/disable; debris/tint material cleanup; and mesh conversion in edit/play modes.

## Files changed this pass

- `Camera/CameraShaker.cs`
- `Dungeon/DungeonGenerator.cs`
- `GamePlay/AttackHitBox.cs`
- `GamePlay/Combat/BlockHitBox.cs`
- `GamePlay/Combat/Projectile.cs`
- `GamePlay/DungeonExit.cs`
- `GamePlay/Enemy/EnemyAi.cs`
- `GamePlay/Inventory/EquipmentSlotUI.cs`
- `GamePlay/Items/EquipmentManager.cs`
- `GamePlay/Items/PlayerEquipmentManager.cs`
- `GamePlay/Stats/PlayerStats.cs`
- `Systems/DestructibleObject_Scripts/BrokenObject.cs`
- `Systems/Helper/MeshFromSkinnedMesh.cs`
- `Systems/LevelingSystemScripts/LevelSystemAnimated.cs`
- `Systems/LevelingSystemScripts/LevelSystemAnimatedUpdater.cs` — new runtime helper
- `Systems/SkillTree/MaterialTintColor.cs`
- `Systems/SkillTree/Player.cs`
- `Systems/ToolTip/Tooltip_ItemStats.cs`

## Inspected and left unchanged

- `DamageFlash.cs`
- `UI/HealthChange.cs`
