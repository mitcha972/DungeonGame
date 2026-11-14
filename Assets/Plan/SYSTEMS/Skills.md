# Independent RPG skills

The 2026-11-13 [animation assessment](Animation.md) distinguishes this XP/book system from playable abilities: SkillTree_Test is a UI demo with no Animator or PlayerCombat; production ability input and complete spell effects remain unimplemented. Direct casting presentation was separately tested and its premature recovery timer repaired.

Implemented 2026-11-13. This reference owns the ten-skill system; [Progression](Progression.md) owns the retained character level/unlock tree. These serve different purposes: character kill/quest XP still calls PlayerStats.AddXP; independent skill XP never awards legacy unlock points.

## Ownership and API

PlayerSkillProgress lives beside PlayerStats/Inventory on PlayerHolder's Player object. Shared immutable SkillDefinition assets (Assets/Skills) contain the ten stable enum IDs/names/colors; SharedSkillXpCurve configures thresholds. Runtime XP arrays belong exclusively to each player. Magic, Combat, Blocking, Archery, Crafting, Mining, Smithing, Cooking, Fishing and Wisdom start at level 1 / 0 XP and cap at level 30.

`AwardXP(RpgSkill, int, Vector3? worldPosition = null)` validates configuration/ID/positive amount, saturates total XP at int.MaxValue, commits state, emits one OnXpGained event with actual credited XP, then OnSkillLevelUp for every crossed level. Reentrant awards/restores during notification are rejected; observers are isolated so one throwing observer cannot roll back committed XP or stop other observers. GetXP, GetLevel, XPToNextLevel and SkillName expose queries. At cap XP can continue increasing, but level stays 30. No singleton or mutable definition progress.

## Curve

For level L, n=L-1: total threshold = `100*n + 25*n*(n-1)`. The next-level cost starts at 100 and increases by 50. Both costs are configurable on the shared curve, clamped 1..1,000,000 at runtime to keep all 30 thresholds distinct and int-safe. Level is derived centrally from total XP; it is not independently saved.

| Level | Total XP | Level | Total XP | Level | Total XP |
| --- | ---: | --- | ---: | --- | ---: |
| 1 | 0 | 11 | 3,250 | 21 | 11,500 |
| 2 | 100 | 12 | 3,850 | 22 | 12,600 |
| 3 | 250 | 13 | 4,500 | 23 | 13,750 |
| 4 | 450 | 14 | 5,200 | 24 | 14,950 |
| 5 | 700 | 15 | 5,950 | 25 | 16,200 |
| 6 | 1,000 | 16 | 6,750 | 26 | 17,500 |
| 7 | 1,350 | 17 | 7,600 | 27 | 18,850 |
| 8 | 1,750 | 18 | 8,500 | 28 | 20,250 |
| 9 | 2,200 | 19 | 9,450 | 29 | 21,700 |
| 10 | 2,700 | 20 | 10,450 | 30 | 23,200 |

## Gameplay integrations

- CopperDeposit opts into 25 Mining XP per committed collection through ResourceNodeDefinition.awardsSkillXP/xpSkill/skillXP. PlayerGathering calls the central API only after ResourceNode.Commit succeeds; start, cancellation, missing requirements and inventory failure award nothing. Other nodes default to no skill XP. Normal inventory notifications continue updating quest possession facts.
- AttackHitBox and Projectile measure actual health loss around existing CharacterCombat.TakeDamage. AwardDamage accepts enabled EnemyStats with a different tag and a living player source; melee grants Combat, projectiles Archery, at configurable damageXpMultiplier (default 1). Existing contact deduplication and dead-target rejection remain authoritative. Armor/block reduction and overkill reduce XP to actual removed health. Misses and non-character/destructible contacts give no skill XP. Legacy enemy kill XP remains unchanged.
- CharacterCombat awards configurable successfulBlockXp (10) to the player only on a damage-reducing blocked impact while actually blocking, from a live hostile EnemyStats, with positive health loss and a surviving defender. Merely holding block gives none. Existing chip-damage semantics remain; shield durability is outside this scope.
- No full magic/crafting/smithing/cooking/fishing/wisdom actions were introduced. Existing ability projectiles currently follow the same Archery path; a future authored damage-kind contract should distinguish spells before magic combat ships.

## Skillbooks and inventory

Assets/Items/Skillbooks contains Skillbook_Magic/Combat/Blocking/Archery/Crafting/Mining/Smithing/Cooking/Fishing/Wisdom.asset, named `Skillbook: <skill>`. Skillbook extends existing Item.Use; every use awards constant 100 XP. Stackable=false / MaxStackSize=1; the existing bag already stores exactly one item per slot, so no separate stack model was added. Books are trade-restricted and use one consistent existing temporary icon. Existing Resource category is used without changing serialized enum values.

Use validates owner, exact source slot, living player, configured skills and ability to credit all 100 XP. It clears the exact slot before XP observers run, restores it if awarding fails, then notifies inventory after successful XP commit. Stale/repeated activation cannot reuse the consumed entry. At int saturation, book use fails and retains the book. Books remain usable at level 30 while 100 XP fits. Shared ScriptableObjects contain no consumption/player locks.

## HUD and testing

SkillFeedbackHUD is a stretched child of the existing UICanvas, referencing the player's progress. It creates a fixed pool of eight non-raycasting TMP popups once per lifetime; bursts replace the oldest popup. Available world positions project through the camera; book rewards use a HUD fallback. Popups rise 60 canvas units and smoothly fade over 1.8 unscaled seconds. One nonmodal gold banner pulses and fades after 3 seconds, displaying the final reached level for multi-level awards; OnSkillLevelUp still emits every crossed level. Existing UI_Click audio provides a temporary quiet celebration cue. No input, cursor, timescale or modal ownership is added.

Enable SkillFeedbackHUD.showDebug in the runtime Inspector to show all levels, total XP and remaining XP to next level; hidden by default, MAX at cap. Content colors are currently shared gold; SkillDefinition.color is reserved for future styling.

Gameplay_Test contains a labeled book supply near the player start. E grants all ten books atomically if ten bag slots are free, through existing Interactable/Inventory.TryExchange. Open inventory with Tab and click each book; supply can be reused after freeing slots. It is scene-only, never on the production player prefab. Existing copper nodes support mining tests. Existing scene content and disk changes were retained, and only the authorized Gameplay_Test scene was saved.

AgentScripts/Skills/BuildSkills.cs authors assets/prefab/fixture through AssetDatabase. Do not rerun just for tests: it saves the active Gameplay_Test and reconfigures authored content. SkillTestSession.Prepare/Restore temporarily selects that saved scene for Play Mode without closing the editing scene. VerifySkills.Run retains the script-run integration/assertion suite. Validation results are recorded below after actual execution; these are not NUnit test assemblies.

Audit fixture correction (2026-11-13): VerifySkills sets controlled enemy health/armor after Start applies equipment. Setting them only before Start incorrectly expected 20 XP from a subsequently armored target. The corrected main and additional input/physics suites passed; [maintenance](../MAINTENANCE.md#gameplay-audit--2026-11-13) owns the rerun record. Gameplay damage/XP behavior was unchanged.

## Persistence and future integration

No disk gameplay-save owner exists. CaptureState/RestoreState expose version-1 SkillProgressSnapshot with a cloned ten-entry int XP array; restore validates version/length/nonnegative XP atomically, derives levels and does not emit rewards. Definitions and curve must be configured first. A future coordinated character save must commit inventory consumption and skills together with legacy character XP, quests, equipment and world state. Current progress resets on scene/death reload. Adding/reordering skills requires explicit version migration; preserve existing enum IDs. Balance/production placement, additional player prefab variants and manual hardware/controller coverage remain future integration work.

## Validation

- Final main live-Editor run: **115 assertions passed**, [skills-playmode.json](../Validation/skills-playmode.json). Covers all ten initial states and books through synthetic Tab/native inventory pointer callbacks, exact/below thresholds through 30, multi-level events, negative/invalid/overflow/reentrant awards, snapshot isolation/validation, failed/stale/repeated book use, real timed copper collection and synthetic Escape/full-bag race, shared melee/projectile contact handlers, successful blocking, chip/overkill/dead-target behavior and bounded/nonblocking/fading TMP feedback.
- Additional input/physics run: **21 checks passed**, [skills-inputs.json](../Validation/skills-inputs.json). Synthetic E with camera-center raycast retrieves all ten books; full-bag supply transaction fails atomically; missing/disabled/dead-health owner use retains books; extreme curve configuration remains monotonic; all ten skills reach 30; native Physics.OverlapBox melee hit/miss and a moving Rigidbody/Projectile sphere sweep award the expected skills. These use isolated real Enemy_Main instances with AI disabled, not manual weapon aim/combat-feel certification.
- **21 asset/integration checks passed**, [skills-assets.json](../Validation/skills-assets.json): real PlayerHolder/Gameplay_Test have no missing scripts, configuration/HUD/font/audio/book references are valid, production prefab contains no supply, saved test XP is zero, and the existing full GameplayModernizationValidation completed (legacy XP bounds/events, combat windows and 300 seeded dungeon graph cases).
- Existing **62-assertion resource-node suite passed again** on the final runtime code: [resource-nodes-playmode.json](../Validation/resource-nodes-playmode.json). Includes actual E/Escape, all three authored collection durations, charge/respawn, inventory races, requirements, range/disable/damage/modal cancellation and reentrant inventory observers.
- Compilation completed with no errors. The skills main/additional runs produced no new console errors. A subsequent resource regression logged an unrelated UnityEditor.Connect.AsyncUtils InvalidOperationException; all resource assertions still passed. Historical missing enemy animation receiver/SlimUI errors predate this task and were not relabeled as new failures.
- In-game 1280x720 capture verified visible gold XP feedback, a pulsing level banner and optional all-skills debug display; AgentScripts/Skills/skills-feedback.png. The initial supply sign was oversized; it was reduced on the saved scene and verified again in-game. Debug remains hidden by default.
- Initial compilation-time inspection timed out during reload; compilation subsequently completed. The first main test stopped after 54 assertions because its synthetic keyboard needed explicit EnableDevice; corrected helper then passed 115. Additional native projectile attempts failed because the fixture added a Rigidbody before positioning it without synchronizing body.position; after correcting fixture initialization and allowing equipment Start before zeroing test armor, all 21 passed. No failed/interrupted attempt is counted as passing.

Not executed: manual hardware/gamepad testing, every resolution/camera/player prefab variant, production dungeon combat balance, dedicated spell XP attribution, coordinated disk saves or crash recovery. Native collision tests and input simulation provide concrete coverage but do not certify every gameplay combination.


Merchant regressions also passed on the final runtime code: VerifyMerchantDialogue (synthetic dialogue/input/control gating), VerifyShopUI (buy/sell/stock/ownership) and VerifyDialogueLifecycle (close/handoff/service lifecycle). [skills-regressions.json](../Validation/skills-regressions.json) records the observed results. No new console errors appeared during this merchant pass.

