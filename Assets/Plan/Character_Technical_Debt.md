# Character technical debt

Current scope: 2026-11-15 player cleanup. [Report](Player_Cleanup_Report.md) records actual validation; [architecture](Player_Architecture.md) defines the implemented boundaries. Historical evidence is not certification of this implementation.

## P0 — critical

No new unresolved critical control conflict or missing script on PlayerHolder/Player_Main is known after the asset checks. Runtime checks and their limits are recorded in the report; this does not certify every older scene or monster.

## P1 — high

| Problem / affected assets | Why it matters / recommended next work | Status |
| --- | --- | --- |
| Dungeon stair clearance: DungeonGenerator, Stairs_Low room content | Earlier traversal needed upward clearance. Jump is intentionally removed now: audit normal walking/stepOffset/slope/ceiling clearance and author traversable stairs. Complete NavMesh paths are not proof that the player capsule can walk them. See [World](SYSTEMS/World.md). | **Unfinished; production floor traversal not certified in this task.** |
| Monster rig/controller gaps: Giant, Spider, Metalon | Shared CharacterAnimation does not repair incompatible rigs, missing motions or duplicate Animators. Keep one rig-appropriate animator per creature and author native clips before expanding content. See [Animation](SYSTEMS/Animation.md). | Existing issue; humanoid Enemy_Main is the regression target. |
| Historical missing enemy-prefab sources in Gameplay_Test, GatheringQuest_Test and ResourceNodes_Test | Existing unpacked/override remnants report missing prefab GUID 84da4102c56346e4399c946fe8377828. Do not claim every old test scene is clean based on the main player audit. Repair fixture provenance separately. | Observed during migration; not a Player_Main script deletion. |

## P2 — medium

| Problem / affected code | Why it matters / recommended next work | Status |
| --- | --- | --- |
| Combat sampling/recovery remains prototype work: CharacterCombat, AttackHitBox, CharacterAnimation | Equipment selection and motion playback are now separated with captured attack execution values; busy/recovery timing remains a shared contract and melee samples current animated bounds, not historical swept poses. Replace these deliberately during the separate combat rework, preserving one damage authority. | Deferred; no new combat system introduced. |
| BlockHitBox durability/projectile bookkeeping and exposed collision-mask semantics | Some fields do not implement the intended durability/interception/filtering policy. Decide rules before implementing them; preserve current frontal active reduction/minimum damage in the interim. | Deferred to combat rework. |
| Legacy ability effects/unlock tree: Abilities, CharacterCombat.Ability, UI_SkillTree | Full authored ability input/effects and outgoing spell/skill classification are incomplete. Keep the demo model distinct from the implemented independent RPG skill XP system. | Deferred; obsolete E-only PlayerSword logger removed. |
| Optional Q grabbing configuration: PlayerController | Existing main-character grab anchor/mask was incomplete. Input and null-safe dispatch are consolidated, but broad grab-target placement/physics is not certified. Configure a hold point and target mask if this mechanic remains part of the design. | Partially implemented inherited feature; no speculative grab system added. |
| Character damage arithmetic under extreme integer values: CharacterStats | Existing float scaling/int conversion is not a fully overflow-safe arbitrary-damage contract. Normal gameplay damage is covered; define saturation before accepting unbounded/modded damage. | Observed during review; outside normal combat balance. |
| Production hardware, stairs/slopes/ceilings/frame rates, first-person equipment art, standalone builds | Synthetic Editor checks cover specific flows, not all input devices/rendered content or packaged-player behavior. Follow the roadmap for manual/controller/platform coverage. | Unverified beyond the report's explicit checks. |
| Session-owned inventory/quests/skills/status state | Death reload recreates these systems; no coordinated gameplay disk-save contract exists. | Existing planned persistence work; see roadmap. |

## P3 — low

| Problem / affected assets | Recommended next work | Status |
| --- | --- | --- |
| Retained PlayerMotor/PlayerManager/CameraController and BasicRigidBodyPush in older variants/menu previews | Replace legacy demo/preview content deliberately before deleting referenced scripts. None is attached to Player_Main. Preserve vendor code and serialized callbacks. | Intentionally retained outside production scope. |
| ThirdPersonController retains its historical StarterAssets namespace/class | It now contains project-owned walking/view logic in Scripts/Player. A cosmetic rename would require class/serialization migration and is unnecessary for sole authority. | Intentionally retained GUID/API identity. |

## Fixed in this cleanup

- Competing player facing and scattered gameplay key polling: one motor and one Input System intent source now own those responsibilities.
- Redundant player/NPC Animator wrappers and inert event receiver: migrated to shared CharacterAnimation and removed after asset-dependency checks.
- Duplicate player progression facade/skill-tree bootstrap, obsolete E logger, input bridge and view-mode wrapper: consolidated or removed.
- Shared stats equipment subscriptions/healing no longer need duplicate player/NPC implementations; shared stats no longer directly reference EnemyAi.
- A pre-existing missing camera script on StarterAssets_PlayerContent blocked saving. Removed that missing component, retained a recovery snapshot outside Assets, and migrated its valid shared-animation reference.
- Quick-slot forwarding components were tied to hidden UI. Their slot data now lives in HotKeysUI and the active player routes shortcuts independently of HUD visibility.
- Cleared missing optional skill materials/shop placeholder sprites and invalid tutorial/SlimUI callback entries in PlayerHolder; removed dormant UnityEvent input-forwarding arrays from Player_Main and repaired the inventory remove-button callback. Valid callbacks and native pointer routes remain. This is reference repair, not certification of the imported settings template's optional audio/key-rebinding features.

Equipment/animation follow-up fixes: animator-owned weapon lookup and placeholder overrides removed; gear state exposed read-only; in-flight attacks capture execution settings; main-hand changes cancel old motions; unequip restores the damage volume; overlapping body coverage and reentrant/full-bag equipment transactions repaired. Generic creature definitions explicitly retain their pre-existing punch fallback pending native content work. Validation status belongs to the current Inventory/Animation references.
