# Dialogue and service integration

**Dialogue implemented; quest/bank/service backends planned only.** Sources stay in Assets/Scripts/Dialogue; the fixture marker moved to Assets/Scripts/Tests/Runtime. [Architecture](../ARCHITECTURE.md) owns input relationships; [shops](Shops.md) own stock/purchase behavior.

DialogueConversation is a ScriptableObject graph of string node IDs, NPC text, responses, next-node IDs and DialogueAction references. DialogueController keeps per-session state on the player and reads shared assets without modifying progress. An inactive DialoguePanel lives under the existing Canvas. DialogueNpc stores speaker/data/optional shop and service events. DialogueInteractable adapts ordinary NPCs to Interactable; ShopScript delegates to its optional DialogueNpc rather than adding a competing Interactable.

`Assets/Prefabs/Dialogue/DialoguePanel.prefab` uses TMP labels, response Buttons/layout and the existing EventSystem. Mouse, arrows/Enter and number shortcuts select; Escape closes through PlayerControlState. Opening blocks gameplay/cancels in-flight combat. Closing, panel/controller/NPC disable or death releases the session. Long/localized text, more than the fixture's four choices, accessibility and all screen sizes remain unverified.

ServiceDialogueAction supplies OpenShop, OpenBank, StartQuest, OpenService and EndDialogue. OpenShop activates the existing shop before closing dialogue, preserving the input lock. Missing shop leaves dialogue open with feedback. EndDialogue closes. Other actions raise onServiceRequested(service, serviceId) and retain dialogue until a handler takes responsibility. ActiveController/Controls expose session context. There is no journal, quest reward logic, bank storage or arbitrary-modal ownership implementation; a future service adapter must acquire ownership before closing dialogue.

Subclass DialogueAction: return false to follow nextNode (empty ends), or true when the action owns the transition. Keep scene references on NPC/adapters, not action assets. Validate unique node IDs/resolvable edges.

## Merchant and tests

`Assets/Scenes/Test_Scenes/DialogueMerchant_Test.unity` contains Rowan: Trade, Ask About Wares, Ask About Rumors and Goodbye. A Shop variant has three guaranteed drops and an explicit PlayerHolder ShopUI. The rotating marker witnesses scaled time.

Historical tests passed E/input blocking/world time, keyboard/mouse branching, uninterrupted shop handoff, close/reopen, disabling, attack cancellation and service callbacks. Retained evidence: [keyboard](../ARCHIVE/Evidence/Dialogue_20261112/keyboard-verification.json), [mouse](../ARCHIVE/Evidence/Dialogue_20261112/mouse-verification.json), [lifecycle](../ARCHIVE/Evidence/Dialogue_20261112/lifecycle-verification.json). [Maintenance](../MAINTENANCE.md) records organization reruns.

Ephemeral tools are in `AgentScripts/Dialogue/`. Run VerifyMerchantDialogue.Run, VerifyDialogueMouse.Run and VerifyDialogueLifecycle.Run via Unity CLI script run. They temporarily adjust background routing/paired devices and restore them. They test actual Input System/uGUI paths, not manual hardware/platform behavior. BuildMerchantDialogue.Build regenerates the dedicated assets/scene; do not run it merely to test an existing fixture.

Organization rerun: keyboard, mouse and lifecycle suites actually passed after all script moves; see [current validation](../MAINTENANCE.md). No dialogue runtime algorithm changed in this pass.
