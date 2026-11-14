# Dungeon Game — project context

Authoritative current overview, audited against source/assets on **2026-11-12** (America/New_York). Read [architecture](ARCHITECTURE.md), [roadmap](DEVELOPMENT_ROADMAP.md), the relevant system and actual code before changing anything. [Maintenance](MAINTENANCE.md) records consolidation and validation. Historical reports are evidence, not active requirements.

## Direction and environment

Single-player dungeon-crawler RPG inspired by **OSRS and Barony**. Preserve the existing CharacterController, shared character combat, ScriptableObject content, interactables, inventory/equipment and uGUI architecture. Installed packages or service-hook names do not imply implemented multiplayer, quests, banking, saving, dual wielding or economy.

Unity **6000.3.28f1**. Active renderer: **Built-in**; URP 17.3.0 is installed but not active. Input System 1.20.1, Cinemachine 3.1.7, AI Navigation 2.0.16, uGUI 2.0.0 and Pipeline 1.0.0 are declared. Both input backends remain enabled. See [dependencies](SYSTEMS/Dependencies.md).

## Current implementation

| System | Actual status and authority |
| --- | --- |
| [Player](SYSTEMS/Player.md) | Implemented: StarterAssets ThirdPersonController + PlayerInput/StarterAssetsInputs; PlayerController handles actions and PlayerControlState owns gameplay/UI eligibility. Camera wiring varies by scene. |
| [Dialogue](SYSTEMS/Dialogue.md) | Implemented branching data, reusable TMP/uGUI panel, E interaction, keyboard/mouse responses, lifecycle and shop handoff. Merchant has historical passing Play Mode evidence. |
| [Inventory](SYSTEMS/Inventory.md) | Implemented slot initialization, pickups, equipment, chest transfer and health consumables. No runtime item-instance/stack/save contract. |
| [Shops](SYSTEMS/Shops.md) | Implemented finite per-merchant stock, configurable restocking/restrictions/prices, gold transactions and 3x3 buy/sell UI; session-only economy. |
| [Combat/NPCs](SYSTEMS/Combat.md) | Single-click light attacks, visible damage volumes, mobile player combat, shield-value active block reduction, shared melee/projectile execution, stats and enemy FSM implemented. Shield durability and some exposed collision-mask fields remain incomplete. |
| [Animation/models](SYSTEMS/Animation.md) | Live assessment of combat, gathering and skill-tree fixtures; obsolete clip events, equipment grip-layer routing and casting recovery repaired. Monster rig/controller gaps and animation folder identities are recorded in the subsystem reference. |
| [World](SYSTEMS/World.md) | Connected maze and transactional room/NavMesh regeneration implemented; graph connectivity does not establish physical doorway/exit reachability. Historical tests include partial exit paths. |
| [Progression](SYSTEMS/Progression.md) / [RPG skills](SYSTEMS/Skills.md) | Existing character XP/unlock tree retained; ten independent skills (levels 1–30), 100-XP skillbooks, success-based mining/melee/ranged/block XP and pooled TMP feedback implemented. Legacy ability effects remain prototypes. |
| [UI](SYSTEMS/UI.md) | Canvas/uGUI, TMP and some legacy Text, menus, inventory/HUD; PlayerHolder north-up minimap and M expanded map ([details](SYSTEMS/Minimap.md)). No runtime UI Toolkit migration. |
| [Quests](SYSTEMS/Quests.md)/bank/services | **Quest slice implemented:** staged definitions/runtime, dialogue conditions/actions, item/kill/location/NPC objectives, journal and completion rewards; Supplies for the Road connects real resource gathering to staged deliveries. PlayerHolder and Enemy_Main extended; NPC_Main inherits Enemy_Main. Quest JSON only; gameplay disk saves and bank remain unimplemented. |
| [Resource nodes](SYSTEMS/ResourceNodes.md) | Timed E gathering, action leases, atomic inventory rewards, charges/respawn and three starter prefabs implemented; test scene copy and validation are documented in the subsystem reference. Session-only world state. |
| Gameplay save/load | **Not implemented**. Static GameController fields are session state; SlimUI PlayerPrefs store preferences. |

## Important paths

- Custom source: `Assets/Scripts/{Core,Player,NPC,Dialogue,Inventory,Shops,Combat,World,Progression,UI,Tests}`; [full script index](SCRIPT_INDEX.md).
- Editor validation: `Assets/Editor/Validation/`. Ephemeral tooling: `AgentScripts/Dialogue/` and `AgentScripts/Organization/`, intentionally outside Assets.
- Player: `Assets/Prefabs/Player/PlayerHolder.prefab`. PlayerContent/backup variants also remain; inspect their actual wiring.
- Input: `Assets/GamePlay/Character/InputSystem/mehh.inputactions`. Adapted Starter Assets remain under `Assets/GamePlay/Character/`.
- Dialogue data: `Assets/Dialogue/`; panel: `Assets/Prefabs/Dialogue/DialoguePanel.prefab`; merchant: `Assets/Prefabs/WorldInteract/DialogueMerchant.prefab` (Shop variant).
- Production flow: `Assets/Scenes/Game/Start.unity` (menu) → `Loading.unity` → `Main.unity` (dungeon).
- Tests: `Assets/Scenes/Test_Scenes/{DialogueMerchant_Test,Combat_Test,DungeonGeneration_Test,SkillTree_Test}.unity`. Presence is not proof of a passing test.
- Gathering-delivery fixture: `Assets/Scenes/Test_Scenes/GatheringQuest_Test.unity`; Quartermaster Nessa and both adventure catalog entries are described in [quests](SYSTEMS/Quests.md).
- All active AI plans, architecture, status and handoffs belong in **Assets/Plan/**.
- Prefab folders were organized on 2026-11-13; [maintenance](MAINTENANCE.md#prefab-organization--2026-11-13) owns the folder guide, exclusions, legacy distinctions and validation. Gameplay prefab identities/references were preserved.

## Immediate handoff

Follow [the roadmap](DEVELOPMENT_ROADMAP.md). Prioritize measured production-flow and physical exit/NavMesh verification over a new architecture. Preserve current economy/shield/skill limitations. [Maintenance validation](MAINTENANCE.md) distinguishes checks actually executed from historical evidence and remaining gaps. Do not reapply old inventory/input/progression fixes already present in code.
