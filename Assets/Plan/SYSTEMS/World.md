# World and dungeon generation

Prefab organization (2026-11-13): authored rooms now live in `Assets/Prefabs/Dungeons/Rooms`, generator in Dungeons/Generation, modular architecture/lamps in Environment/LowPolyDungeon and destructibles in Props/Destructibles/{Interactive,Debris,Unconfigured,Legacy/ImportedDuplicates}. Folder labels describe inspected components, not new functionality. See [organization record](../MAINTENANCE.md#prefab-organization--2026-11-13).

**Implemented graph/room/NavMesh prototype; physical traversal unverified.** Sources moved to World/Dungeon, Interactions, Destructibles, Grabbing, Map and Utilities. ShopScript is under Shops and chest code under Inventory/Chests.

DungeonGenerator creates a randomized depth-first spanning graph with reciprocal passages and a breadth-first farthest exit, validates grids (2â€“16 per axis), and assembles room prefabs. The recursive-retry generator at the old plugin path is superseded. Unity Random is global; no exposed replay seed exists.

Generation retains the previous hierarchy, graph, spawn, movement flags and NavMeshData until replacement succeeds. Failure/cancellation disposes the iterator and restores prior state; AdvanceFloor rolls the floor counter back. IsGenerating/IsReady/LastFailure/Generated expose status. First generation has no old valid floor to restore; recovery UX remains a design decision.

Custom navigation uses Unity.AI.Navigation.NavMeshSurface; imported RoomBehaviour/WallCollision remain dependencies. Graph connectivity does not establish doorway widths, room/agent placement, NavMesh type/areas or exit interaction reachability. Historical foundation logs include **PathPartial** exit paths: preserve that unresolved evidence.

Interactable is the shared focus contract dispatched by PlayerController E. DoorScript and DungeonExit specialize it. ObjectGrabbable/PlayerPickUpDrop are a separate physics-grab prototype, not inventory pickup. HealthSystem/IDamageable/destructibles handle world damage. MeshFromSkinnedMesh modifies components in edit mode; organization does not execute it.

The Editor validator covers 300 seeded graphs (connectivity, reciprocity, bounds, tree edges, unique/farthest exit). [Maintenance](../MAINTENANCE.md) distinguishes actual runs from prior compiled-but-unexecuted tests. Native NavMesh rollback, multi-floor physical reachability and failure UX remain next work.

Organization rerun: all 300 logical graph checks actually executed and passed. Physical NavMesh/exit checks remain unverified.
