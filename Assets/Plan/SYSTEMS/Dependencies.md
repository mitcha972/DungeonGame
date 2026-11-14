# Dependencies and retained integrations

Unity 6000.3.28f1 uses **Built-in rendering** despite URP 17.3.0 being installed. Input System 1.20.1, Cinemachine 3.1.7, AI Navigation 2.0.16, uGUI 2.0.0 and Pipeline 1.0.0 are declared. Both input backends remain enabled. Package presence is not active-use evidence; preserve licenses/attribution and inspect the renderer before authoring materials.

Runtime NavMesh import requirement (2026-11-13): `Assets/Models/Building/Tiles/Dungeon_Big_col.fbx` now has ModelImporter Read/Write enabled. Runtime PhysicsColliders baking reads its collision mesh; Editor-only access previously masked a player-build failure and logged unreadable-source diagnostics. FBX/GUID/material mappings remain; Unity updated importer serialization. Production Editor transitions reran without errors; packaged-player validation remains pending. Future runtime bake sources must permit required CPU reads. [Audit evidence](../MAINTENANCE.md#gameplay-audit--2026-11-13).

## Retained boundaries

- `Assets/GamePlay/Character/`: adapted Starter Assets movement/input/push source stays at existing paths.
- `Assets/Plugins/CodeMonkey/`: application pointer callbacks use UIPointerEvents, but Button_UI presentation is serialized in game assets. One intentional type-name bridge remains; no-imports alone does not justify deletion.
- `Assets/Plugins/DungeonGenerator/`: RoomBehaviour/WallCollision still integrate with runtime generation and room assets.
- `Assets/Plugins/QuickOutline/`: EnemyInteract uses Outline.
- `Assets/Plugins/SlimUI/`: menus/settings/preferences. PlayerPrefs are not gameplay saves.
- `Assets/Plugins/Standard Assets/`: legacy FPC/input dependencies remain.
- `Assets/Plugins/NavMeshComponents-master/`: legacy editor/examples/tests and six asmdefs remain distinct from current AI Navigation. Duplicate menu warnings predate organization; removal requires its own reference-aware cleanup.
- `Assets/Scripts/Camera/`: adapted EZCameraShake namespace and related helpers remain together at their original paths.
- DemoPlayer and BLINK MaterialTilingOffset helpers remain at original locations; provenance/usage do not justify opportunistic moves or fixes.

No vendor/Starter Assets/package source or asmdef was edited. [Index](../SCRIPT_INDEX.md) records retained integration locations; [maintenance](../MAINTENANCE.md) records byte/GUID checks/tool access. Historical license and disconnected-editor statements are run-specific, not permanent restrictions. A newer CLI release was detected; tooling/package upgrades were outside this organization pass.
