# Development roadmap

One active backlog. Distinguish **implemented**, **partially implemented**, **planned** and **unverified**. Source existence is not production certification. Detailed behavior belongs in subsystem references.

## Established foundation — implemented

- CharacterController locomotion with bounded acceleration/braking, jump buffering/coyote time, substeps and first/third-person support.
- PlayerControlState, project-owned input actions and shared combat/interaction integration.
- Fixed inventory slots; guarded pickups/chest transfers; equipment state-before-event ordering; class health and local progression updater.
- Shared combat, initialized ordinary projectiles, contact deduplication and skipped-window sampling. Single-click player light attacks, visible damage volumes, mobile combat and shield-value active reduction verified across unarmed/swords/bow in Combat_Test; see [combat](SYSTEMS/Combat.md).
- Positive/overflow-safe XP awards and level-cap behavior.
- Enemy FSM and transactional maze/room/NavMesh regeneration.
- Branching merchant dialogue, keyboard/mouse panel, safe close and existing-shop handoff.
- Assets/Plan authority, root AGENTS entry point and subsystem-based custom script organization.

These describe retained implementations, not every scene passing. [Maintenance](MAINTENANCE.md) and archived evidence identify executed checks.

## Next milestone — dependable primary loop

1. **Production-flow integration:** verify Start → Loading → Main after scene renaming: class/name/equipment propagation, startup wiring, death reload and return-to-menu. Paths/build identities are reconciled; full gameplay flow needs a run.
2. **Physical dungeon reachability:** measure player-to-exit paths across generated floors, doorway widths, agent types/area masks and interaction range. Historical foundation logs contain PathPartial exit paths; graph tests cannot close this item.
3. **Failure/cancellation:** exercise initial failure, failed regeneration, disable/cancel and NavMesh/floor rollback. A user-facing recovery experience remains a design decision.
4. **Cross-prefab controls:** verify production PlayerContent camera/input/modal wiring as well as the merchant fixture; preserve held-input/focus/death behavior.

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

Retain FSM/generator/shared combat unless evidence justifies replacement. Profile before pooling/scheduling work. Vendor removal requires reference migration: CodeMonkey Button_UI still supplies serialized presentation; legacy NavMeshComponents includes editor/test/example assemblies and duplicate menu warnings. Review positional camera shake, shared-renderer materials, tooltip extremes and edit-mode conversion separately.

Remove completed work from pending lists. Historical approval gates, licensing failures and unavailable-tool statements belong to their original tasks; inspect current instructions and tooling instead.
