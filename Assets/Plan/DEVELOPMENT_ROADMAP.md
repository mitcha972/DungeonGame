# Development roadmap

One active backlog. Distinguish **implemented**, **partially implemented**, **planned** and **unverified**. Source existence is not production certification. Detailed behavior belongs in subsystem references.

## Established foundation — implemented

- CharacterController locomotion with bounded acceleration/braking, jump buffering/coyote time, substeps and first/third-person support.
- PlayerControlState, project-owned input actions and shared combat/interaction integration.
- Fixed inventory slots; guarded pickups/chest transfers; equipment state-before-event ordering; class health and local progression updater.
- Shared combat, initialized ordinary projectiles, contact deduplication and skipped-window sampling.
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
- **Quest system — first slice implemented:** see [quests](SYSTEMS/Quests.md). Finish production NPC/spawn placement, first-person combat coverage and coordinated gameplay persistence. Extend PlayerHolder/Enemy_Main and permanent NPC_Main; avoid duplicate temporary prefabs. Bank/services remain future scopes.
- **Skills:** replace log-only effects with designed abilities/stat changes and validate that each bootstrap uses the intended PlayerStats state.
- **Persistence:** choose a versioned contract for character, items/equipment, XP/skills, quests and session/floor seed policy. PlayerPrefs preferences are not gameplay saves.
- **Replayable generation:** define session/floor seed semantics before exposing a seed; preserve Random state in tests.
- **Shield/layers:** decide durability/interception and target/world-mask semantics before completing currently unused fields.

## Targeted maintenance

Retain FSM/generator/shared combat unless evidence justifies replacement. Profile before pooling/scheduling work. Vendor removal requires reference migration: CodeMonkey Button_UI still supplies serialized presentation; legacy NavMeshComponents includes editor/test/example assemblies and duplicate menu warnings. Review positional camera shake, shared-renderer materials, tooltip extremes and edit-mode conversion separately.

Remove completed work from pending lists. Historical approval gates, licensing failures and unavailable-tool statements belong to their original tasks; inspect current instructions and tooling instead.
