# Development roadmap

One active backlog. Distinguish **implemented**, **partially implemented**, **planned** and **unverified**. Source existence is not production certification. Detailed behavior belongs in subsystem references.

## Established foundation — implemented

- CharacterController walking/gravity with bounded acceleration/braking, substeps and first/third-person support. Jumping removed in the 2026-11-15 cleanup; see [player architecture](Player_Architecture.md).
- PlayerControlState as the sole gameplay input/eligibility boundary, one movement/facing/view owner, project-owned actions and shared character combat/animation/stats. [Cleanup report](Player_Cleanup_Report.md).
- Fixed inventory slots; guarded pickups/chest transfers; equipment state-before-event ordering; class health and local progression updater.
- Shared combat, initialized ordinary projectiles, contact deduplication and skipped-window sampling. Single-click player light attacks, visible damage volumes, mobile combat and shield-value active reduction verified across unarmed/swords/bow in Combat_Test; see [combat](SYSTEMS/Combat.md).
- Positive/overflow-safe XP awards and level-cap behavior.
- Enemy FSM and transactional maze/room/NavMesh regeneration.
- Branching merchant dialogue, keyboard/mouse panel, safe close and existing-shop handoff.
- Assets/Plan authority, root AGENTS entry point and subsystem-based custom script organization.

These describe retained implementations, not every scene passing. [Maintenance](MAINTENANCE.md) and archived evidence identify executed checks.

## Next milestone — dependable primary loop

1. **Packaged-player verification — next:** build/run the intended platform after the runtime collision-mesh Read/Write fix. Editor Start → Loading → Main, selection/equipment, controls, death reload and return-to-menu now pass; no standalone build is certified. [Audit results](MAINTENANCE.md#gameplay-audit--2026-11-13).
2. **Normal stair traversal without jumping — partial:** all room centers/exits have complete paths across 20 seeded floors and four end entrances. DEnd's missing exit, closed inner walls and misplaced corridors are fixed. Controlled CharacterController traces reached four exits, but seed 4 required upward clearance at Stairs_Low (1). Jump is now intentionally absent. Test real walking/gravity/step clearance, stairs/slopes/door feel and frame rates; NavMesh readiness is not a walking guarantee.
3. **Failure UX — planned:** initial failure, bad configuration, partial assembly, unreachable-exit/native-data rollback, three cancellations and duplicate requests now pass. Design a visible retry/return path for initial-generation failure; there is no previous floor to restore then.
4. **Remaining prefab/content coverage — unverified:** production PlayerHolder modal/focus/death controls and merchant fixtures pass. Alternate PlayerContent/legacy prefabs, hardware/controller input and monster-specific rigs remain separate coverage; prioritize the animation gaps below before expanding enemy content.

## Subsequent milestones — choose scope first

- **Inventory/economy — merchant slice implemented:** finite stock/restocking, allowlists, gold and atomic buy/sell with existing slot inventory. See [shops](SYSTEMS/Shops.md). Remaining: production balance/placement, player quantities/identity and coordinated persistence.
- **Quest system — first slice and resource-delivery integration implemented:** original adventure plus Supplies for the Road, existing journal/turn-ins and stale-response protection; see [quests](SYSTEMS/Quests.md). Finish production NPC/spawn placement, first-person combat coverage and coordinated gameplay persistence. Extend PlayerHolder/Enemy_Main and permanent NPC_Main; avoid duplicate temporary prefabs. Bank/services remain future scopes.
- **Resource gathering — starter slice implemented:** three configurable nodes with timed E collection, humanoid animations, action ownership, atomic inventory rewards, depletion and scaled respawn. See [resource nodes](SYSTEMS/ResourceNodes.md). Remaining: dedicated chopping/tool visuals, production placement and coordinated world persistence; copper Mining XP is now integrated; see [skills](SYSTEMS/Skills.md).
- **Skills — independent XP slice implemented:** ten skills, levels 1–30, shared curve, ten consumable books, mining/melee/ranged/block integrations, pooled feedback and test supply; see [Skills](SYSTEMS/Skills.md). Remaining: balance/production placement, alternate player prefab bootstrap coverage, authored skill effects and coordinated saves. Legacy unlock-tree effects remain prototype work.
- **Persistence:** choose a versioned contract for character, items/equipment, XP/skills, quests and session/floor seed policy. PlayerPrefs preferences are not gameplay saves.
- **Replayable generation:** define session/floor seed semantics before exposing a seed; preserve Random state in tests.
- **Shield/layers:** decide durability/interception and target/world-mask semantics before completing currently unused fields.

## Targeted maintenance

Animation assessment (2026-11-13): obsolete combat clip callbacks, grip-layer indexing and ability recovery timing repaired. Remaining: rig-appropriate Spider/Metalon controllers and native motion recovery, one authoritative Giant Animator, missing enemy block/weapon clips, dedicated gathering tools/chopping and designed ability input/effects. See [animation/models](SYSTEMS/Animation.md) for evidence, organization and actual test coverage.

Retain FSM/generator/shared combat unless evidence justifies replacement. Profile before pooling/scheduling work. Vendor removal requires reference migration: CodeMonkey Button_UI still supplies serialized presentation; legacy NavMeshComponents includes editor/test/example assemblies. Its duplicate menus and code warnings were repaired on 2026-11-14; remaining content/tooling diagnostics and next actions are tracked as W01–W11 in [maintenance](MAINTENANCE.md#remaining-diagnostic-tracking). Review positional camera shake, shared-renderer materials, tooltip extremes and edit-mode conversion separately.

Remove completed work from pending lists. Historical approval gates, licensing failures and unavailable-tool statements belong to their original tasks; inspect current instructions and tooling instead.

- **Status effects — starter slice implemented:** shared Poison/Slow/Bleeding, non-refreshing stable IDs, derived movement/damage modifiers, HUD and Poison Potion in Combat_Test. Remaining: production application sources/balance, alternate prefab coverage and coordinated persistence. See [status effects](SYSTEMS/StatusEffects.md).

Equipment/animation consolidation (2026-11-15): explicit item attack sets, authored motion states, atomic gear transactions and shared named equipment/animation owners implemented. Source compilation completed without errors; 95/95 gameplay and 19/19 asset checks passed, with scope and limits in the linked [Inventory](SYSTEMS/Inventory.md) and [Animation](SYSTEMS/Animation.md) references. Full combat policy redesign and native creature animation content remain subsequent work.

Scope decision (2026-11-15): equipment/animation cleanup is complete; broader build, lighting, shader and imported-navigation work is deferred at the user's request. The attempted standalone verification identified separate compiler/fixture blockers and was cancelled; this does not complete the packaged-player milestone above. See [recorded follow-ups](MAINTENANCE.md#broader-tests-deferred--2026-11-15).

- **Mouse-facing third-person camera — implemented (2026-11-15):** view-relative strafe/backpedal, shared driven camera anchor/startup binding repair and directional locomotion blending; 36 focused Editor checks passed. Manual hardware and visual polish remain unverified. [Player](SYSTEMS/Player.md#mouse-facing-third-person-camera--implemented-2026-11-15).
