# UI and modal ownership

Reusable prefab templates now live in `Assets/Prefabs/UI/{Inventory,HUD,Feedback,Menus,Progression,Tooltips}` (2026-11-13). DialoguePanel retains its Dialogue path; embedded PlayerHolder UI was not rewritten. [Organization record](../MAINTENANCE.md#prefab-organization--2026-11-13) records byte/reference validation and exclusions.

**Implemented uGUI**: Canvas/RectTransform, TMP and some legacy Text. No runtime UXML/USS migration exists. Files moved to UI/Menus, UI/Inventory, UI/HUD, UI/Feedback and UI/Tooltips. UICanvasController/UIPointerEvents are shared; DialoguePanel belongs to Dialogue.

PlayerHolder supplies UICanvas, EventSystem and InputSystemUIInputModule. Dialogue adds an inactive reusable panel; the existing ShopUI is directly under Canvas so inventory visibility cannot hide trading. Preserve GraphicRaycaster, Button events and serialized slot references.

PlayerControlState owns cursor/gameplay eligibility. Dialogue/inventory/shop/chest block input without pausing; SettingsMenu deliberately freezes time for ordinary pause. E/Escape close shop/chest; Escape closes dialogue. Do not add contradictory Escape pollers to centrally wired UI. Alternate prefabs can retain fallback paths; inspect _controlState assignments.

UIPointerEvents implements native click/enter/exit callbacks with an optional CodeMonkey Button_UI enabled/hover bridge. Removing direct application imports does not authorize removing serialized vendor presentation. Tooltip_ItemStats converts to canvas space and clamps corners; oversized/localized/world-space configurations still need visual checks.

Scene rename correction: SettingsMenu.Exit loads **Start**, the menu's new name, instead of **Main**, now dungeon gameplay. MenuController/LoadingScript guards use the relocated paths, preserving destination roles.

Dialogue/shop keyboard/mouse paths have historical evidence; organization checks are in [maintenance](../MAINTENANCE.md). Inventory drag/right-click, SlimUI settings, display cameras, accessibility/localization and alternate resolutions are not fully certified. Future bank/service panels need explicit input ownership; arbitrary modals are not automatically discovered.

Quest journal is added to PlayerHolder and centrally gated by PlayerControlState. J/Escape close it without pausing; gameplay remains blocked while open. See [quests](Quests.md) for the paged TMP/uGUI journal and persistence boundary.

Minimap: PlayerHolder now supplies a north-up circular minimap and M/Escape expanded view, with PlayerControlState modal gating. Configuration, validation and limitations: [Minimap](Minimap.md).

Resource feedback reuses the player's interaction TMP prompt and adds GatheringProgressHUD under the existing screen Canvas for a narrow gold progress bar. It has no input/modal ownership or independent Canvas. See [resource nodes](ResourceNodes.md) for setup and tested scope.

SkillFeedbackHUD now listens to independent skill XP on the existing UICanvas: fixed eight-popup TMP pool, projected action/HUD positions, fading rise, nonmodal level-up banner and optional Inspector-enabled debug text (hidden by default). No new Canvas or input owner. See [Skills](Skills.md).


## HUD redesign reverted - 2026-11-13

At the user's request, the sidebar redesign was fully removed. PlayerHolder and all changed UI/input scripts were restored to their pre-redesign contents; Combat_Test was restored from its exact pre-HUD snapshot, preserving its earlier gameplay changes. Added sidebar scripts, backups, test fixtures and captures were removed. Previous redesign test results do not describe the restored interface. Rollback validation: original prefab matches the pre-task Git contents, the combat scene was restored byte-for-byte from its pre-HUD snapshot, and Unity compilation completed without errors. No new gameplay features remain.

Final Editor scene-reload verification timed out after 30 seconds; subsequent CLI status reported the Editor unreachable. Disk rollback and successful restored-script compilation were verified before that timeout. No post-reload visual/gameplay test is claimed.

PlayerHolder now adds StatusEffectHUD below health on the existing UICanvas: symbol icons, ceil-seconds TMP countdowns and local hover details. No modal/input owner added. See [status effects](StatusEffects.md).

Warning cleanup (2026-11-14): removed 325 unnecessary CanvasRenderers from 3D TextMeshPro objects across seven existing prefabs and Start.unity. TextMeshProUGUI renderers, text content, layout, meshes/materials, callbacks and surviving serialized records remain unchanged. Editor reload audit found zero remaining candidates in those assets. HealthChange now uses the supported, InstanceID-sorted Canvas lookup; MenuController explicitly retains its serialized name field hiding. No UI redesign or manual visual/gameplay certification. [Maintenance](../MAINTENANCE.md#warning-cleanup--2026-11-14) owns affected assets, evidence and follow-ups.

Player cleanup (2026-11-15): quick-slot forwarding components and the separate skill-tree bootstrap were consolidated into existing owners. UI hierarchy/layout was preserved apart from obsolete components and an unused aim child; hidden quick-slot visuals no longer control whether gameplay shortcuts work. PlayerControlState remains the modal/cursor owner. See [cleanup report](../Player_Cleanup_Report.md).
