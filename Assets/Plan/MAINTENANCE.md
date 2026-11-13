# Documentation and organization maintenance

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
