> Historical snapshot. Superseded by [current project context](../../PROJECT_CONTEXT.md) and the subsystem references. Original requirements, file names, counts, and test limits below describe the earlier task, not current instructions. Original bytes are preserved in the organization restore ZIP.

# Modular dialogue

## Merchant test

Open `Assets/Scenes/DialogueMerchantTest.unity` and enter Play Mode. Look at Rowan in front of the player and press **E**. Choose a response with the mouse, Up/Down + Enter, or 1-4. Escape leaves dialogue. Trade opens the existing ShopUI; E or Escape closes the shop. Neither dialogue nor trading pauses world simulation. The rotating cube is a scaled-time simulation witness.

The merchant is a **variant of `Assets/Prefabs/WorldInteract/Shop.prefab`**. Its `ShopScript.dialogue` points to its DialogueNpc, which references MerchantConversation. Three pickup prefabs (iron sword, wooden shield, potion) have 100% item-drop entries. ShopScript.Start reads their root ItemInteract.item and adds the items to its local ShopInventory. The scene instance explicitly references PlayerHolder's existing ShopUI. The original Shop prefab and other shops' stock configurations are unchanged.

ShopUI was inside PlayerHolder's hidden InventoryUIs container. The same ShopUI object now sits directly under its existing UICanvas, preserving its components, item slots, events, and object references. Opening a shop initializes inactive slots and shows only populated slots. Purchase behavior is unchanged, including its existing zero prices, missing currency deduction, and unlimited stock.

## Architecture

- **DialogueConversation** is a ScriptableObject graph: entry node ID, NPC text, responses, next-node IDs, and optional action assets. No runtime progress is written into shared assets.
- **DialogueNpc** stores speaker name, conversation, optional ShopScript, and service-request events. Regular NPCs use DialogueInteractable : Interactable. Merchants use the existing ShopScript, which delegates to its optional DialogueNpc; shops without dialogue keep direct trading.
- **DialogueController** is on the player beside PlayerControlState. It owns the current conversation and references an inactive DialoguePanel under the existing Canvas. PlayerHolder is wired already.
- **DialoguePanel.prefab** uses uGUI and TextMeshPro with a reusable response template. Standard EventSystem navigation handles arrows/Enter/gamepad; mouse clicks use the existing GraphicRaycaster and InputSystemUIInputModule. Number keys are optional shortcuts. Escape is handled by PlayerControlState.
- **PlayerControlState** includes dialogue in its gameplay eligibility check. Existing movement, look, camera switching, and combat input already respect that check. Opening dialogue also cancels an in-progress attack. Inventory/E/pause-menu opening are blocked during dialogue; Escape closes dialogue without pausing. Disabling the panel/NPC/controller or player death releases the dialogue state.
- **OpenShop** activates ShopUI before closing dialogue. PlayerControlState remains locked throughout the transfer. ShopUI.CloseUI synchronizes controls immediately. Shop references fall back to the interacting player's configured ShopUI when no explicit scene reference is assigned.

## Extending actions

Create service actions with **Create > Dungeon Game > Dialogue > Service Action**. Supplied assets expose OpenShop, OpenBank, StartQuest, OpenService, and EndDialogue. OpenBank/StartQuest/OpenService raise DialogueNpc.onServiceRequested(service, serviceId). They deliberately keep dialogue open until an integration handles the request; no bank or quest system is included. The receiver can access DialogueNpc.ActiveController and its Controls property. A future modal service must retain its own control ownership before closing dialogue. StartQuest handlers may update quest state and choose the next conversation branch in a custom action.

For new behavior, derive a ScriptableObject from DialogueAction and implement Execute(controller, npc). Return false to follow the response's nextNode (an empty nextNode ends dialogue), or true when the action handles the transition. Scene references belong on DialogueNpc/its adapters, not on shared action assets.

## Authoring and verification tools

`AgentScripts/BuildMerchantDialogue.cs` builds the assets through Unity Editor APIs. It can be run in Edit Mode with `unity command script run --file AgentScripts/BuildMerchantDialogue.cs --entry BuildMerchantDialogue.Build`. It rewrites the dedicated generated merchant conversation, panel, variant, and test scene; use only when intentionally regenerating those assets.

`AgentScripts/VerifyMerchantDialogue.cs` exercises the Input System E binding, numeric and arrow/Enter choices, control blocking, scaled-time simulation, continuous shop handoff, closing, and reopening. `VerifyDialogueMouse.cs` tests actual UI pointer clicks. Both temporarily change Input System background routing for headless tests and restore it in finally. They do not change saved input settings. The test harness dismisses the pause menu and simulates player focus before injecting input.

Restore copies of the initially modified scripts and PlayerHolder are under `GameplayCheckpoints/Dialogue_20261112/`. Existing unrelated working-tree changes were preserved.

## Verification result

Verified in the connected Unity 6000.3.28f1 Editor using Pipeline 1.0.0 and Unity CLI 1.0.0-beta.17. Runtime code compiled successfully with no compilation errors.

| Acceptance check | Result |
| --- | --- |
| E on merchant opens dialogue through existing Interactable raycast | Passed |
| Movement, jumping, camera look, combat, inventory opening, and repeat E blocked during dialogue | Passed with injected keyboard/mouse input; no OnAttack event fired |
| World continues running | Passed: frame counter and scaled-time marker advance; timeScale stays 1 |
| Trade opens existing ShopUI with merchant stock | Passed with numeric response and mouse click; three stock entries visible |
| Closing shop restores controls | Passed with E |
| Dialogue can reopen | Passed; Escape and Goodbye also restore controls without pausing |
| No Console errors | Passed: final live Console consoleErrors=0, compilationFailed=false |

Additional checks passed: arrow/Enter navigation, wares/rumors branches, continuous input lock during shop handoff, cancelling in-flight attacks, panel/NPC disabling, unavailable shop fallback, and all three placeholder service events. The merchant asset is confirmed as a Shop prefab variant, all nine response branches validate, and there is exactly one EventSystem.

The mouse test uses a temporary Input System mouse paired to PlayerInput, waits for uGUI layout and actual branch transitions, and restores original devices/settings. This isolates tests from physical pointer movement. Real hardware interaction was not manually exercised; automated tests used the actual Input System actions, camera raycast, GraphicRaycaster, EventSystem and InputSystemUIInputModule. No standalone build or platform-specific test was run.

Pre-existing duplicate NavMesh menu-registration warnings remain. The Pipeline history also contains an earlier Unity AI relay connection error predating this work; the live Console has no errors and the dialogue tests produced none. Recorded machine-readable results are in GameplayCheckpoints/Dialogue_20261112/{keyboard-verification,mouse-verification,lifecycle-verification,console-status}.json. Screenshots are in Screenshots/dialogue-merchant.png and Screenshots/dialogue-shop.png.

Workflow used the installed unity-cli, unity-pipeline, ui, and ui-ugui skills. Read the installed Pipeline Documentation~/commands/scripts.md, editor-lifecycle-and-observability.md, runtime.md, build-and-compilation.md, and capture.md, plus unity-cli/references/playmode-verification-loop.md for the matching command and test behavior.

The active renderer is Built-in despite URP being installed. Generated test-only floor/merchant materials use Standard shaders to match it. Final screenshots were visually checked after correcting this. The dedicated test scene is left open in Edit Mode, Play Mode is stopped, and the temporary Editor auto-tick override is off.
