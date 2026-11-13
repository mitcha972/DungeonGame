# Modular resource nodes

Implemented starter slice, 2026-11-13. This file is the authoritative resource-system reference. Production placement, dedicated chopping/tool presentation and coordinated world saving remain future work.

## Ownership and configuration

`Assets/Scripts/Resource/ResourceNode.cs` extends the existing Interactable (retaining the original empty script's GUID). ResourceNodeDefinition holds immutable shared configuration: display/action names, output Item and quantity, collection duration, animator state identifier, respawn duration, maximum charges, optional non-consumed bag item, extensible ResourceNodeRequirement assets and collection audio. Each placement owns its reservation, charges and scaled game-time deadline. The original resource slice added no crafting, recipe, cumulative quest-objective or disk-save system. Copper skill XP is now integrated through [Skills](Skills.md).

PlayerGathering on the existing PlayerController object owns one action and checks range against PlayerController.InteractionDistance. PlayerControlState owns the exclusive action lease and continues to evaluate existing UI, pause, focus and death eligibility separately. Gathering blocks locomotion, camera input, combat and other interactions, retains the locked cursor and never changes timeScale. Escape cancels without opening pause. Other action keys are ignored until completion. External UI/focus/pause/death, lost bag requirements, health loss, out-of-range movement, disabled player/gatherer or disabled/destroyed node interrupt with no reward or consumed charge. Gravity/world simulation continue.

## Transaction and respawn

E still routes through PlayerControlState -> PlayerController camera-center raycast -> Interactable.Interact. ResourceNode delegates to the same player's PlayerGathering. Start validates the configuration, requirements, existing combat/animation state and inventory capacity, reserves the instance, acquires control and begins presentation. Only timer completion calls Inventory.TryExchange with the entire configured reward. Capacity/requirements are checked again; failed transactions preserve the node. Node state commits before Inventory.NotifyExchange and success observers, preventing reentrant rewards. The bag remains one item per slot (no new stack model); quantity N requires N free slots.

Only gathering players poll per frame. Depleted enabled nodes sleep in WaitForSeconds, then restore their available visuals and charges. Disabled nodes catch up against Time.time on reenable; disabled intervals count when the game runs. Paused time does not count. Closing menus does not reset deadlines. Visual objects must be children: never put the root node/timer or its interaction collider in the toggled visual references.

CaptureState/RestoreState expose charges and remaining scaled seconds for a future coordinated world-save owner. There is no stable placed-node ID, disk persistence or offline time progression; scene/death reload starts fresh. Save ownership, IDs and floor lifecycle remain future integration requirements.

## Animation and feedback

CharacterAnimator owns BeginGathering/EndGathering presentation on the existing controller. Dedicated layer-0 states use the project's humanoid Gathering/MiningLoop clips; higher combat layers temporarily have zero weight and are restored afterward. Missing gathering states safely skip animation while timed collection still works. Completion returns to the previous base state; interruptions must preserve hit/death presentation. A dedicated chopping/tool animation and visible first-person tools remain art work.

PlayerController reuses its serialized TMP interaction prompt for available/depleted, progress percentage, success and failure messages. GatheringProgressHUD adds a narrow gold progress bar to the existing screen Canvas; it owns no input. Text feedback lasts three unscaled seconds. No extra Canvas, input asset or second E poller is needed.

## Quest and future extension points

Inventory.NotifyExchange automatically updates existing QuestController PossessItem objectives. PlayerGathering.ResourceCollected and ResourceNode.ResourceCollected emit the successful player/node/item/quantity context, while node UnityEvents support authored collection/respawn effects. The current quest model has no cumulative resource-collected objective; no second quest counter is introduced. Future skill XP/tool equipment checks can subscribe to the event or derive ResourceNodeRequirement. RequiredItem currently checks the bag and is not consumed; bottle transformation is not implemented.

## Authoring and verification tools

AgentScripts/Resources/BuildResourceNodes.cs authors definitions, prefabs, animation states and HUD through AssetDatabase. It requires stopped Play Mode and the active Gameplay_Test scene. It preserves the tree prefab GUID; its previous contents were an unconfigured cube placeholder. It extends PlayerHolder and the current test player. The test area is placed in the live Gameplay_Test with Undo; a separate ResourceNodes_Test copy is saved so unrelated dirty edits are not committed to Gameplay_Test. Do not rerun the builder merely to test: it resets starter definitions and rebuilds node visuals.

ResourceTestSession.Prepare/Restore temporarily select the isolated Play Mode start scene without closing/saving the user's editing scene. VerifyResourceNodes.Run is a retained script-run assertion suite using simulated keyboard input, the real player/inventory/Animator and shortened cloned respawn fixtures. Reports belong in Assets/Plan/Validation/resource-nodes-playmode.json.

## Starter assets and setup

| Prefab under Assets/Prefabs/WorldInteract/ | Definition under Assets/ResourceNodes/ | Output | Collection / respawn |
| --- | --- | --- | --- |
| ResourceNode_HealingPotionSource.prefab | HealingPotionSource.asset | 1 existing Health Potion | 2 / 60 seconds |
| ResourceNode_Tree.prefab | WoodcuttingTree.asset | 1 new Logs Item | 3 / 45 seconds |
| ResourceNode_CopperDeposit.prefab | CopperDeposit.asset | 1 new CopperOre Item | 4 / 90 seconds |

All default to one charge. The spring reuses Elixir_Of_Health presentation over a simple stone basin; the tree reuses the existing BanyanTree model and has a stump; ore reuses Stone_1 with copper-colored veins. Shared definition assets configure the timings; placed instances keep independent state. ItemType.Resource is appended without changing prior enum values. Logs/Copper ore have distinct small pixel inventory icons and no crafting/use effects.

For a new type: create a Resource Node Definition, assign an existing Item and complete reward quantity, choose duration/charges/respawn and a valid Animator state (or omit animation), then add ResourceNode and a non-trigger interaction collider to the prefab root. Keep the root within the player's configured interaction range. Assign available/depleted child visuals and optional events/audio/requirements. Add PlayerGathering on the same object as PlayerController/PlayerControlState/Inventory; PlayerHolder is already configured. Optional GatheringProgressHUD references that gatherer and a child Image under its existing Canvas. The existing TMP prompt provides feedback even without the bar.

Existing Gathering.anim is copied to GatheringCollectLoop.anim with looping enabled; MiningLoop.anim is reused directly for mining and as the chopping placeholder. Three states are added to the existing Player Animator movement layer; no new Animator controller is introduced. Dedicated chopping/axe and pickaxe/filling hand visuals remain art work; equipped weapons are not converted into tools. Source animation clips remain unchanged.

## Validation status

- Final live-Editor **62 assertions passed**, recorded in [resource-nodes-playmode.json](../Validation/resource-nodes-playmode.json). Uses real PlayerHolder wiring, synthetic E/Escape, camera raycast, Inventory and humanoid Animator. All three authored collection durations ran. Includes delayed rewards, capacity preflight and full-bag race, duplicate/reentrant attempts, charges, independent depletion, respawn/recollection, visual restoration, Escape/range/requirements/disable/damage cancellation, external journal ownership, incoming base-layer animation preservation, snapshots and elapsed-time catch-up while disabled. Respawn/recollection tests use cloned definitions with short timers; full 45/60/90-second real-duration respawns were not separately waited out.
- **Nine asset/reference checks passed**, recorded in [resource-node-assets.json](../Validation/resource-node-assets.json): no missing scripts in the three resource prefabs or PlayerHolder, valid definitions/colliders/visuals, item icons, correctly wired player progress HUD, and three saved test-scene instances. Final script compilation reported completed, no errors.
- Existing VerifyMerchantDialogue.Run, VerifyShopUI.Run and VerifyDialogueLifecycle.Run passed: synthetic dialogue input and control gating, direct dialogue/shop handoff, buy/sell, finite stock retention and lifecycle/service cleanup. [Regression record](../Validation/resource-regressions.json). These ran before the final narrow incoming-animation guard; the complete resource suite ran again afterward.
- First-person visual capture confirmed readable gold gathering text, percentage, cancellation hint and progress bar: AgentScripts/Resources/resource-gathering-first-person.png. This is a prototype test-room view, not an art-complete environment or dedicated first-person tool animation.
- Initial CLI status/inspection calls lost connection during reload and some timed out while Unity rebuilt package assemblies/cache. The HUD initially imported without joining the compilation source list; AssetDatabase rename/reimport to GatheringProgressHUD repaired registration. Builder fake-null handling and test-harness modal/layer assumptions were corrected before final passing runs. Failed/interrupted attempts are not counted as passing tests.

Unverified: actual lethal-death/scene-unload cancellation, manual hardware/controller input, every camera/prefab variant, full production dungeon placement/balance and disk-save restoration. Lifecycle cancellation is implemented but those specific end-to-end cases were not executed. The current inventory is not a stacking model; output quantities occupy individual slots. Do not claim resource XP/skills, consuming bottle recipes or durable world persistence.

Gathering quest integration (2026-11-13): existing Inventory notifications now have an authored end-to-end delivery example, Supplies for the Road. Two logs, one copper ore and a spring potion are gathered and turned in using the existing quest/dialogue/journal. No extra polling, resource-specific objective counter or quest dependency was added to ResourceNode. [Quests](Quests.md#gathering-adventure-integration-2026-11-13) owns the content, four-node fixture, validation results and remaining persistence/production requirements.

Independent skill integration (2026-11-13): ResourceNodeDefinition now optionally configures awardsSkillXP/xpSkill/skillXP. CopperDeposit opts into 25 Mining XP; PlayerGathering grants it only after successful Commit. Other starter nodes do not award skill XP. [Skills](Skills.md) owns XP calculations/feedback and validation.
