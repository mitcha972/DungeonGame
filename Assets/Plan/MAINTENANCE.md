# Documentation and organization maintenance

## Gameplay audit — 2026-11-13

Completed the three requested checkpoints against Unity 6000.3.28f1. Initial state: stopped, clean editing scene Combat_Test; its pre-existing disk change (one `m_IsActive` override) was retained. Baseline compilation had no errors. Unity CLI 1.0.0 was verified current; installed Pipeline `commands/scripts.md`, `commands/build-and-compilation.md`, `commands/editor-lifecycle-and-observability.md` and CLI `playmode-verification-loop.md` were consulted. Slow reloads/terminal timeouts required the Unity connector fallback; interrupted calls were not counted as passes.

### Confirmed findings and fixes

| Priority | Reproduced problem | Implemented correction |
| --- | --- | --- |
| High | GameController treated Main as the menu after scene renaming. Selected Ranger/name became the prefab Warrior/empty name; dungeon defaults overwrote floor/difficulty. Loading carried a controller without a player. | Start/Menu reset menu state; Loading skips player setup; Main applies the selected player. Menu/loading diagnostics use current names. [Before](Validation/audit-production-before.json), [final flow](Validation/audit-production.json). |
| High | The **active** Main generator overrides its end room to DEnd: no exit, always-closed inner wall sections and two corridor sections under opposite entrances. A disabled legacy generator's DBig reference was not the active configuration. | DEnd reuses DBig's ExitObject. Ten inner wall sections follow the matching closed-wall toggle; two corridor sections follow the correct entrance. Existing room/model identities retained. [Baseline](Validation/audit-world-before.json), [final tests](Validation/audit-world.json). |
| High | Runtime baking reported Dungeon_Big_col unreadable: Editor baking worked, but player builds could not read its source mesh. | Enabled ModelImporter Read/Write on `Assets/Models/Building/Tiles/Dungeon_Big_col.fbx`; source FBX/GUID retained. The production rerun emitted no errors. No standalone executable was built. |
| Medium | Regeneration retained replaced native NavMeshData and accepted floors without a reachable exit. | DungeonGenerator owns only data it builds, releases replaced/failed/destroyed data, retains old data for rollback, rejects missing/inactive/unreachable exits and uses matching agent-type/area query filters. |

Gameplay files changed: GameController.cs, LoadingScript.cs, MenuController.cs, DungeonGenerator.cs, DEnd.prefab and Dungeon_Big_col.fbx.meta. Unity upgraded saved prefab/importer serialization. No source model, vendor code, scene, public callback, enum or assembly boundary changed. `AgentScripts/Skills/VerifySkills.cs` sets controlled enemy health/armor **after** Start applies equipment, correcting a fixture assumption without changing damage/XP rules.

### Checks actually executed on the final implementation

| Check | Result / evidence |
| --- | --- |
| Production loop | **20 passed:** Start → Loading → Main name/class/health/equipment, floor/difficulty, advancing frames, Tab/M/J/Escape ownership, focus, lethal damage/death reload and paused return to menu. Captured transition errors: **0**. [Report](Validation/audit-production.json). Menu cursor was observed free/visible. |
| Native world stress | **74 passed:** **20 seeded 4×4 floors**, 320 room-center paths, 20 complete exit paths, four end entrances, native-data count bounds, invalid configuration, missing/unreachable exit rollback, three disable cancellations, duplicate requests and initial failure termination. Repeated after the import fix. Failure injections intentionally log errors. [Report](Validation/audit-world.json). |
| Controlled capsule traversal | Four seeded routes reached exit interaction range through static colliders. Seeds 0/1/6 needed no upward probe; seed 4 needed one bounded probe at **Stairs_Low (1)**. Enemy colliders were disabled; this is not timed input jumping or normal gravity/locomotion certification. [Report](Validation/audit-traversal.json). |
| Existing Editor regressions | Passed scene guards, character XP boundaries/events/overflow/cap, combat intervals and **300 seeded logical graphs**. [Output](ARCHIVE/Evidence/Current/GAMEPLAY_MODERNIZATION_VALIDATION.txt). |
| Combat | **40 passed:** actual input, unarmed/Wood Sword/Iron Sword/Bow, native melee/projectile contacts, one attack per hold, volumes, movement and shields. [Output](ARCHIVE/Evidence/SimpleCombatValidation.txt). |
| Skills / input-physics | **115 + 21 passed** in Gameplay_Test. [Skills](Validation/skills-playmode.json), [additional checks](Validation/skills-inputs.json). |
| Resources / quests | **62** resource, **56** original quest and **59** gathering-quest assertions passed. [Resources](Validation/resource-nodes-playmode.json), [quest](Validation/quest-playmode.json), [gathering quest](Validation/gathering-quest-playmode.json). |
| Merchants | Keyboard, native UI mouse, buy/sell UI, lifecycle and stock/gold transaction failure checks passed. [Reports](Validation/audit-merchants.json). |
| Status / potion world item | **33 + 11 passed:** timers/modifiers/HUD/enemy death and drop/pickup/exact-slot guards. [Reports](Validation/audit-status-effects.json). |

Partial runs are not passes: production needed a longer timeout for measured scene-load latency; gathering quest began before an initial focus-loss event settled, then passed unchanged; the skills fixture needed the Start-order correction. The first capsule probe incorrectly required NavMesh's approximate vertical height at every waypoint; horizontal arrival removed that false stop. The stair walking limitation remains explicit.

NUnit discovery contained retained vendor NavMesh assemblies, not a custom game suite; vendor tests were not run. No standalone build, hardware/gamepad test, broad resolution/FPS/slopes survey, durable save test or monster-rig certification ran. Console history retains intentional failure injections and pre-fix diagnostics; it is not a clean historical buffer. Final state/reference review is recorded below. Remaining priorities live only in [the roadmap](DEVELOPMENT_ROADMAP.md).

Audit helpers are indexed in [SCRIPT_INDEX](SCRIPT_INDEX.md). Run tests only in owned Play Mode fixtures; do not rerun one-time room migrations merely to test current content. Technical restore snapshots remain under ignored Temp/AuditRestore, outside Assets.

### Final review and handoff

[Final asset/state review](Validation/audit-final-review.json) passed: all **267** original DEnd object/component IDs remain (281 after exit addition), room/mesh GUIDs retained, zero missing scripts in DEnd/PlayerHolder/Enemy_Main, readable collision meshes and unchanged enabled build-scene order. The pre-existing Combat_Test diff matches its original SHA-256 exactly. Editor returned to clean stopped Combat_Test; the previous Play Mode start-scene override is restored and temporary auto-tick is disabled. Final `unity recompile` returned up_to_date, failed=false, no errors.

The [post-fix console window](Validation/audit-console.json) contains four expected failure-injection errors and **zero unexpected errors**; historical Console counts include earlier diagnostics. Windows standalone build support is available, but no build/run was executed. [Current documentation links](Validation/audit-links.json) were checked without rewriting unrelated legacy document encodings. The roadmap prioritizes packaged-player validation, real stair/jump controls, initial-generation recovery UX and remaining prefab/monster coverage; persistence/content design limits remain under their existing system owners.

## Prefab organization — 2026-11-13

Implemented folder-only cleanup using AssetDatabase.MoveAsset: **251 of 472 prefabs** relocated through 86 asset/folder operations. All prefab GUIDs and metadata retained. No gameplay code, scene content, public callbacks or plugin assets changed. [Exact inventory and move reasons](ARCHIVE/Organization/prefab-organization.json) cover every prefab, including exclusions; [byte validation](ARCHIVE/Organization/prefab-filesystem-validation.json) and [Editor validation](ARCHIVE/Organization/prefab-editor-validation.json) record executed checks.

| Folder under Assets/Prefabs | Role |
| --- | --- |
| Core | GAMECONTROLLER session bootstrap |
| Dungeons/Rooms, Generation, TestGeneration | Authored room assemblies, generator and retained generation fixtures |
| Environment/LowPolyDungeon | Cohesive imported modular architecture/lighting collection; Stage and SkyBounds sit under Environment |
| Models/Equipment | Armor, clothing, weapons and consumable display meshes recovered from Assets/Meshes; these are distinct from ItemInteract pickups |
| Models/Weapons/LowPoly, Models/Characters | Visual weapon collection and Metalon/Giant/Spider models, without gameplay wrappers |
| Items | Existing pickup armor/weapons/consumables retained; generic grabbable Item moved to Templates |
| UI/Inventory, HUD, Feedback, Menus, Progression, Tooltips | Canvas/slot templates, health displays, HitDamage overlay, menus, skill tree and tooltips |
| Props/Destructibles/Interactive, Debris, Unconfigured | Objects with DestructibleObject, broken outputs with BrokenObject, and source props without custom destructible behavior; replaces ambiguous Done naming |
| Props/Destructibles/Legacy/ImportedDuplicates | Box_02, Jug_02 and Rock_01: exact byte copies of plugin art prefabs, preserved with their distinct GUIDs and existing references |
| Props/RocksAndCrystals, Valuables, Potions, Decorations | Clearer groups for formerly loose decorative props; existing Clutter/Furniture retained |
| Enemies/Legacy, Player/Legacy/Backups, Player/Variants | Explicit old enemy test prefabs, existing player backups and the older PlayerContent stranded in a script folder |

Enemy_Main, PlayerHolder, active PlayerContent, WorldInteract, Dialogue and Projectiles retain their established paths. Similar names do not imply duplicates: pickup/display pairs, decorative versus interactive chests/props, projectile/model arrows and enemy/model GiantDummy variants have different responsibilities. No speculative deletion or obsolete classification was applied. Legacy means retained historical/test/duplicate content, not guaranteed unused. The old Starter Assets-derived PlayerContent filename now identifies its provenance; its original binary serialization and internal names were preserved after Unity attempted an automatic version upgrade during renaming.

Excluded: Assets/Plugins, Starter Assets camera/model prefabs retained with their owner, and imported particle-library prefabs under Assets/Particles. Two non-CFX-named ring effects share that library's components/hierarchy, so they remain with it. Game-owned prefabs stranded under ScriptableObjects and script folders were recovered. Only the emptied former destructible folder tree was removed. Live organization helper paths were reconciled; historical evidence retains original paths.

Validation actually executed: SHA-256 checks of all 472 prefab files and .meta files against the pre-move snapshot; 358 files within moved collections (including folder metadata and the tooltip image) verified against Git/LFS identities; Editor loading and before/after dependency-GUID/missing-script-count comparison for 434 non-plugin prefabs/scenes; active Combat_Test scene and its clean state preserved, Play Mode remained stopped. Eighteen audited prefabs already had missing scripts (17 particle prefabs and the older StarterAssets_PlayerContent variant); counts were unchanged and are recorded in the Editor report. No gameplay/Play Mode tests or builds ran because behavior was not changed. Initial read-only audit exceeded 60 seconds but completed; later checks used a longer timeout. CLI updated to 1.0.0 after sandbox network/discovery restrictions were resolved. Auto-tick returned to disabled.

Next integration: use the manifest's destination paths for future tooling; retain prefab GUID references. Missing scripts in the retained imported effects/player variant need a separate repair scope, not silent removal. Existing production-loop/physical-navigation verification remains on the roadmap.

Consolidation decision record, not a parallel feature roadmap. Updated 2026-11-12. [Context](PROJECT_CONTEXT.md), [roadmap](DEVELOPMENT_ROADMAP.md), [architecture](ARCHITECTURE.md), [index](SCRIPT_INDEX.md) and system references are authoritative current documentation.

## Audit and dispositions

No Assets/Plan or root/ancestor AGENTS.md existed at audit. Documents were split across Assets/Plans, Assets/Doc, docs, a root JSON proposal and checkpoint outputs. The latest user instruction authorized execution without another approval presentation. Findings were compared with current source, scene GUIDs, player/shop/dialogue assets, package/settings and actual saved test outputs. Generated caches/logs and vendor readmes/licenses/package docs are not game development plans; retain them with their owners.

| Original document | Decision and preserved value |
| --- | --- |
| Assets/Plans/CLEANUP_REPORT.md | MERGE current fixes/compatibility into systems; ARCHIVE initial scope and test limits. |
| Assets/Plans/REMAINING_CONCERNS_REPORT.md | MERGE still-open shield/economy/layer/material/failure issues into roadmap/systems; ARCHIVE source-only history. |
| Assets/Plans/PROJECT_WIDE_PLUGIN_AUDIT.md | MERGE ownership/vendor boundaries into Dependencies/UI; ARCHIVE inventory/GUID evidence and migration reasoning. |
| Assets/Plans/player-controls-rework-and-combat-input.md | MERGE current controls into Player/Architecture; ARCHIVE original task constraints. Escape-always-pauses is superseded for dialogue/shop; missing-action/controller findings are not reapplied. |
| Assets/Doc/COMPLETE_CODEBASE_AUDIT.txt | ARCHIVE canonical plain-text early audit; retain original design/test questions without treating resolved bugs as pending. |
| docs/COMPLETE_CODEBASE_AUDIT.md | ARCHIVE distinct detailed Markdown audit; MERGE useful data-driven design and verification principles into architecture/roadmap. |
| docs/PROJECT_AUDIT_FOLLOWUP.md | ARCHIVE investigation history; current source supersedes its inventory/equipment/input/progression defect statuses. |
| docs/GAMEPLAY_MODERNIZATION_PLAN.md and REPORT.md | MERGE retained algorithms/ownership, path/XP/window corrections and unfinished checks; ARCHIVE original plan/report/licensing evidence. |
| docs/DIALOGUE_SYSTEM.md | UPDATE/MERGE into Dialogue/Shops with current scene/source/tool paths; ARCHIVE original implementation report. |
| SCRIPT_ORGANIZATION_AUDIT.json | ARCHIVE superseded 32-move proposal/baseline; current manifest and index replace it. |
| Checkpoint test/compile/JSON reports | ARCHIVE under Evidence, preserving passing/failing outputs and original bytes. Source/binary recovery artifacts remain outside Assets. |
| docs/COMPLETE_CODEBASE_AUDIT.txt and checkpoint copy | REMOVE two exact duplicates only after SHA-256 equality; retain canonical text, provenance and original-byte restore ZIP. |
| Vendor readmes/licenses/package documentation | KEEP at existing owner locations; not active game plans. |

Historical reports live under ARCHIVE/History. [Manifest](ARCHIVE/Organization/organization-manifest.json) lists every original document, destination and hash. No unique history was silently discarded. Empty Assets/Plans, Assets/Doc and root docs directories are retired after consolidation.

## Claims corrected against current source

Inventory.Awake initializes slots; ReplaceEquipment updates state before notifications; ordinary projectiles call StartUp; PlayerStats consumes class health; health consumables are consumed/clamped; movement respects PlayerControlState; EnemyAi is an Update FSM; Player delegates progression to co-located PlayerStats; a local updater replaced CodeMonkey FunctionUpdater. These old pending fixes are complete at source level.

Economy/quests/banking/save completeness, native exit traversal, full shields, actual skill effects and cross-prefab wiring remain separate unfinished work. Historical reports disagree on Editor/license availability because they describe different runs: one batch validation failed before execution, while later merchant tests used a working Editor/Pipeline. Check current tools rather than inheriting either restriction.

## Script and scene organization

105 imported scripts moved through AssetDatabase.MoveAsset with .meta files; 13 custom/adapted sources retained their locations. Editor sources stay under Editor/Validation and runtime sources stay in Assembly-CSharp. No class/API/namespace/enum/serialized-field/asmdef renames. Plugins, Starter Assets, Camera integration and imported demo helpers were not moved. Four ephemeral tools moved to AgentScripts/Dialogue outside runtime import.

Custom folders now cover Core, Player, NPC, Dialogue, Inventory, Shops, Combat, World, Progression, UI and Tests. No empty Quest or SaveSystem implementation folders were invented. See the [exact manifest](ARCHIVE/Organization/organization-manifest.json), [journal](ARCHIVE/Organization/asset-move-journal.txt) and [current index](SCRIPT_INDEX.md).

The user's preexisting scene moves retain identities: old Main Ã¢â€ â€™ Game/Start (menu), old Loading Ã¢â€ â€™ Game/Loading, old DungeonFloorTest Ã¢â€ â€™ Game/Main (dungeon), old Test Ã¢â€ â€™ Test_Scenes/Combat_Test, merchant Ã¢â€ â€™ Test_Scenes/DialogueMerchant_Test. Build settings were reconciled by GUID, preserving enabled order. MenuController, LoadingScript, SettingsMenu.Exit, validator and merchant builder now use those destination roles/paths. Runners write reports under Assets/Plan/ARCHIVE/Evidence/Current instead of recreating docs or root checkpoint reports. No gameplay algorithm changed.

## Validation

All following checks actually ran during this organization pass:

| Check | Result and evidence |
| --- | --- |
| Filesystem/source/meta preservation | PASS: 118 custom/adapted script identities, 105 moves, exact .meta bytes and unique GUIDs. All 880 protected source/serialized/dependency files preserved except the five declared scene/output-path source edits. [Filesystem report](ARCHIVE/Organization/filesystem-validation.json). |
| Unity compilation | PASS: recompile completed, failed=false, no compilation errors. |
| Editor asset references | PASS: all 118 MonoScript assets resolve; zero missing scripts across PlayerHolder, PlayerContent, base Shop, DialogueMerchant and UI_SkillTree; seven scenes resolve and enabled build order/roles match. [Editor report](ARCHIVE/Organization/editor-reference-validation.json). |
| Existing Editor regressions | PASS: scene destination guards, XP boundaries/overflow/events/cap, skipped combat-window cases, and **300 seeded maze graphs**. [Actual output](ARCHIVE/Evidence/Current/GAMEPLAY_MODERNIZATION_VALIDATION.txt). This does not test physical doorway reachability. |
| Merchant keyboard Play Mode smoke | PASS: real Input System E/raycast, input blocking, scaled-time world, branches, arrows/Enter, continuous shop handoff, close/reopen and Goodbye. [Current run](ARCHIVE/Evidence/Current/merchant-keyboard.json). |
| Merchant mouse Play Mode smoke | PASS: temporary paired Input System mouse through GraphicRaycaster/EventSystem/UI module, branch selection and Trade. [Current run](ARCHIVE/Evidence/Current/merchant-mouse.json). |
| Dialogue lifecycle smoke | PASS: cancel in-flight attack, panel/NPC disable/reopen, unavailable-shop fallback and three service request hooks. [Current run](ARCHIVE/Evidence/Current/merchant-lifecycle.json). |
| Live Console after Play Mode | PASS: consoleErrors=0, compilationFailed=false. [Console evidence](ARCHIVE/Evidence/Current/console-status.json). The Pipeline historical buffer still includes an older unrelated AI relay error; retained vendor NavMesh warnings predate this work. |
| Markdown links | PASS: root AGENTS and every current/archived Markdown file target checked; final exact count is in the filesystem report. Historical missing targets are explicitly non-link text. |

Available NUnit discovery returned the retained vendor NavMesh test assemblies, not a custom game test suite. Those unrelated vendor tests, manual hardware/platform/standalone builds, the entire Startâ†’Loadingâ†’Main gameplay flow, and physical/cancelled floor/NavMesh integration were not executed. Do not infer them from the passing logical/merchant tests. The original clean Start scene setup was restored, Play Mode stopped, and temporary auto-tick is disabled. Runtime test input settings/devices were restored by the existing harness finally blocks.

The historical checkpoint image moved from Assets/Doc to ARCHIVE/Evidence/Legacy with its .meta preserved. A remaining Assets/Scripts/GamePlay/Enemy directory contains only a legacy Enemy-Test prefab, deliberately retained as data; it contains no stranded custom C# source. Only verified-empty retired folders were removed.

Installed skill workflow: unity-cli/unity-pipeline; local Pipeline commands/scripts.md and editor-lifecycle-and-observability.md were read for matching APIs, along with the earlier playmode-verification-loop.md workflow. Restore ZIP and migration journal preserve original bytes and partial-operation history; no unrelated user edits were reset.

## Permanent maintenance

Root AGENTS.md is the entry point. Update affected existing systems after each task and context/roadmap/index/architecture only when relevant. Avoid new per-task reports and duplicate backlogs. Label partial/planned/unverified work honestly; retain decisions, known limits and handoff requirements. Archive superseded history rather than leaving contradictory active plans.


## Final authoritative structure

```text
Assets/Plan/
  PROJECT_CONTEXT.md
  DEVELOPMENT_ROADMAP.md
  ARCHITECTURE.md
  SCRIPT_INDEX.md
  MAINTENANCE.md
  SYSTEMS/
    Player.md  Dialogue.md  Inventory.md  Shops.md  Combat.md
    UI.md  World.md  Progression.md  Dependencies.md
  ARCHIVE/
    README.md
    History/
    Evidence/
    Organization/
```

Recommended next task: run the production loop and measure actual player-to-exit NavMesh reachability across generated floors. Resolve the historical partial paths before expanding content. The current backlog lives only in DEVELOPMENT_ROADMAP.md.

Final cross-check: all six edited source/tool files match exactly the declared literal path/output replacements against the restore ZIP; archived document metadata and the historical checkpoint image/meta are byte-preserved. The relocated merchant builder also passed an Editor compile-only check.
