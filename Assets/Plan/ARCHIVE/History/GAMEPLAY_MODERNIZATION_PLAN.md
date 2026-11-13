> Historical snapshot. Superseded by [current project context](../../PROJECT_CONTEXT.md) and the subsystem references. Original requirements, file names, counts, and test limits below describe the earlier task, not current instructions. Original bytes are preserved in the organization restore ZIP.

# Gameplay modernization plan — 2026-11-11

Status: documented before implementation. Incremental corrections only; no substantial multi-system redesign is proposed. Existing user edits are preserved. Baseline: Git HEAD 7891507 plus GameplayCheckpoints/Modernization_20261111 copies of touched files and preexisting-working-tree.patch. Copies, rather than a commit, avoid including unrelated work.

## Audit and architecture decisions
Unity 6000.3.28f1. Actual enabled scenes: Main, Loading, DungeonFloorTest, Test. Other scenes: DungeonTest, DungeonGeneration, SkillTree. Historical names in the supplied directive no longer match the files. Keep the enabled primary sequence and serialized GUIDs.

- Movement/input/camera: CharacterController + project-owned StarterAssets controller; PlayerControlState gates actions, cursor and menus; PlayerController owns combat tap/hold and interaction. Controller already has acceleration/braking, jump buffering/coyote time and collision substeps. Retain; inspect actual scene wiring and playtest before another redesign.
- Enemies: EnemyAi finite state machine with perception, memory, complete-path checks, repath throttling, stuck recovery and death. EnemyStats/Enemy define tuning; EnemyCombat gates attacks. Retain small FSM; behavior trees offer no demonstrated benefit here.
- Dungeon: randomized depth-first spanning maze, reciprocal passages, breadth-first farthest exit; transactional room/NavMesh regeneration with rollback. Retain over BSP/MST replacement. Missing explicit reproducible seed and physical exit reachability remain investigation items.
- Combat: CharacterCombat owns attack/cooldown/hit recovery; AttackHitBox deduplicates contacts; Projectile sweeps nearest contacts and handles saturated buffers. Keep ownership; fix attack windows skipped by slow frames.
- State/progression: static GameController session selection; LoadingScript transitions; generator owns floor advancement rollback; PlayerStats owns death restart; LevelSystem emits level/XP events. Menu references nonexistent DungeonFloorTemp; loader accepts default Main index; XP accepts negative/overflowing additions.
- Inventory/equipment/interaction: slot inventory and equipment callbacks, PlayerContent/class setup, chest/shop panels. Recent uncommitted UI/plugin work and prior cleanup reports already address several issues. Retain; full cross-scene serialized wiring still requires Editor verification.
- Existing validation: CombatCheckpointRunner (already modified by user); no project-owned Assets/Tests directory. Plugin NavMesh tests are not gameplay coverage. Prior csproj filenames alone do not prove tests exist.

## 1. Restore primary scene transition (highest priority)
Existing: MenuController.Play looks up removed DungeonFloorTemp; loader blindly uses static index (default zero).
Evidence: enabled build list contains DungeonFloorTest; no DungeonFloorTemp scene exists. Play always returns with an error.
Approach: resolve actual DungeonFloorTest path; verify Loading is enabled before committing selection; loader rejects out-of-range, current-scene and Main destinations, logging actionable errors rather than looping to menu. Retain existing static selection and async loader.
Precedent/benefit: explicit build-scene lookup and boundary validation; restores intended flow without new scene manager.
Risks/components: MenuController.cs and LoadingScript.cs only; direct Loading entry intentionally reports invalid destination. Preserve public callbacks and inspector fields.
Acceptance/tests: target paths resolve in enabled build list; invalid indices/self/menu rejected; valid dungeon accepted; run Main -> Loading -> DungeonFloorTest when Editor is available.
Rollback: restore the two baseline copies; no scene migration.

## 2. Reliable combat windows
Existing: coroutine samples normalized attack phase once per frame; melee checks overlaps only in FixedUpdate.
Evidence: phase can jump from before hitStart to after hitEnd without releasing a projectile or enabling melee; a short melee window can also contain no physics update.
Approach: detect overlap of elapsed phase interval with active window, including final duration boundary; explicitly sample melee overlaps while window is active using the existing query and deduplication logic. Keep damage values, authored timings, attack ownership, cooldown and animation callbacks.
Precedent/benefit: interval crossing rather than point sampling for discrete timed events; guarantees a sampling opportunity on skipped windows. Retaining current point sampling leaves a correctness defect; a new combat framework is unnecessary.
Risks/components: CharacterCombat.cs and AttackHitBox.cs; extra overlap query per active render frame, deduplicated per attack. This does not reconstruct historical moving weapon poses or guarantee swept melee collision.
Acceptance/tests: intervals before/inside/across/after window, zero-width window, final boundary; melee retains per-attack contact deduplication; interrupted/dead attacks stay disabled. Playtest weapon timing and slow frames when possible. No performance gain claimed.
Rollback: restore both baseline copies.

## 3. Bounded XP awards
Existing: int addition before multi-level loop; accepts negative XP.
Evidence: AddExperience(-1) creates negative normalized XP; int.MaxValue plus existing XP wraps negative.
Approach: ignore nonpositive awards; use a wide local accumulator through the existing event/threshold loop and discard surplus at cap. Keep thresholds, level numbering, event ordering and animation model.
Precedent/benefit: validate reward boundaries, use nonoverflowing intermediate arithmetic; retains small progression model over unnecessary service replacement.
Risks/components: LevelSystem.cs; negative XP is no longer an implicit penalty (no penalty caller identified); max-level surplus normalized to zero.
Acceptance/tests: zero/negative awards, exact threshold, multi-level award, int.MaxValue with existing XP, cap idempotency and events.
Rollback: restore baseline LevelSystem.cs.

## Validation and change control
Implement stages sequentially. Add an isolated Editor batch validation entry point for scene configuration, XP boundaries, combat intervals and maze graph invariants across seeds; preserve the existing combat runner. No packages or third-party changes. Do not edit generated caches. Editor availability and execution results will be documented honestly in the final report. Broad rewrites, scene rewiring, seed-system changes and design changes require a separate concrete plan and approval under the supplied directive.
