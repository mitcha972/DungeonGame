# World and dungeon generation

Prefab organization (2026-11-13): authored rooms now live in `Assets/Prefabs/Dungeons/Rooms`, generator in Dungeons/Generation, modular architecture/lamps in Environment/LowPolyDungeon and destructibles in Props/Destructibles/{Interactive,Debris,Unconfigured,Legacy/ImportedDuplicates}. Folder labels describe inspected components, not new functionality. See [organization record](../MAINTENANCE.md#prefab-organization--2026-11-13).

**Implemented graph/room/NavMesh prototype with native exit-path evidence and partial capsule traversal coverage.** Sources are under World/Dungeon, Interactions, Destructibles, Grabbing, Map and Utilities. ShopScript is under Shops and chest code under Inventory/Chests.

DungeonGenerator creates a randomized depth-first spanning graph with reciprocal passages and a breadth-first farthest exit, validates grids (2â€“16 per axis), and assembles room prefabs. The recursive-retry generator at the old plugin path is superseded. Unity Random is global; no exposed replay seed exists.

Generation retains the previous hierarchy, graph, spawn, movement flags and NavMeshData until replacement succeeds. Failure/cancellation disposes the iterator and restores prior state; AdvanceFloor rolls the floor counter back. IsGenerating/IsReady/LastFailure/Generated expose status. First generation has no old valid floor to restore; recovery UX remains a design decision.

Custom navigation uses Unity.AI.Navigation.NavMeshSurface; imported RoomBehaviour/WallCollision remain dependencies. Replacement data must provide an active end-room DungeonExit, a surface-agent spawn sample and a complete path to an exit sample strictly within the player's interaction range. Enemy samples use their own agent type/area mask. Invalid results roll back before player/enemy placement. This establishes navigation connectivity, not normal walking/jump clearance for every collider.

DungeonGenerator owns the native NavMeshData it builds. Successful replacement releases former owned data; failure releases failed data after restoring the previous reference; destruction removes/releases owned data. A preassigned scene/asset NavMeshData is retained rather than destroyed. Cancellation restores matching navigation, floor/graph and player state.

The active Main generator overrides its end room to **DEnd**. The separate disabled generator's DBig reference is legacy configuration. DEnd now reuses the existing DBig ExitObject. Its inner wall panels follow the appropriate closed-wall group, and opposite-parented corridor sections follow their actual entrance. RoomBehaviour still selects the same four passage flags; no new generator/interaction owner was added. Runtime collision-mesh readability is recorded in [dependencies](Dependencies.md).

Interactable is the shared focus contract dispatched by PlayerController E. DoorScript and DungeonExit specialize it. ObjectGrabbable/PlayerPickUpDrop are a separate physics-grab prototype, not inventory pickup. HealthSystem/IDamageable/destructibles handle world damage. MeshFromSkinnedMesh modifies components in edit mode; organization does not execute it.

The Editor validator covers logical graphs. The [2026-11-13 audit](../MAINTENANCE.md#gameplay-audit--2026-11-13) adds native seeded paths, rollback/cancellation/data-lifetime checks and controlled CharacterController traces. Earlier PathPartial evidence remains in history; it is superseded for repaired DEnd cases, not proof that arbitrary new rooms are traversable.

AgentScripts/Audit/WorldAudit.Run tests Main while preserving Unity Random state. TraversalAudit.Run disables enemy colliders and drives native CharacterController.Move along paths. One route needed bounded upward clearance at Stairs_Low (1); NavMesh height is not treated as the exact capsule foot height. This is not timed input/gravity/jump or combat traversal certification. New rooms must preserve corridor floors, passage flags, exit colliders and agent compatibility; remaining locomotion and initial-failure UX work lives in [the roadmap](../DEVELOPMENT_ROADMAP.md).
