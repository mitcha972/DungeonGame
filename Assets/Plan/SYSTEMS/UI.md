# UI and modal ownership

**Implemented uGUI**: Canvas/RectTransform, TMP and some legacy Text. No runtime UXML/USS migration exists. Files moved to UI/Menus, UI/Inventory, UI/HUD, UI/Feedback and UI/Tooltips. UICanvasController/UIPointerEvents are shared; DialoguePanel belongs to Dialogue.

PlayerHolder supplies UICanvas, EventSystem and InputSystemUIInputModule. Dialogue adds an inactive reusable panel; the existing ShopUI is directly under Canvas so inventory visibility cannot hide trading. Preserve GraphicRaycaster, Button events and serialized slot references.

PlayerControlState owns cursor/gameplay eligibility. Dialogue/inventory/shop/chest block input without pausing; SettingsMenu deliberately freezes time for ordinary pause. E/Escape close shop/chest; Escape closes dialogue. Do not add contradictory Escape pollers to centrally wired UI. Alternate prefabs can retain fallback paths; inspect _controlState assignments.

UIPointerEvents implements native click/enter/exit callbacks with an optional CodeMonkey Button_UI enabled/hover bridge. Removing direct application imports does not authorize removing serialized vendor presentation. Tooltip_ItemStats converts to canvas space and clamps corners; oversized/localized/world-space configurations still need visual checks.

Scene rename correction: SettingsMenu.Exit loads **Start**, the menu's new name, instead of **Main**, now dungeon gameplay. MenuController/LoadingScript guards use the relocated paths, preserving destination roles.

Dialogue/shop keyboard/mouse paths have historical evidence; organization checks are in [maintenance](../MAINTENANCE.md). Inventory drag/right-click, SlimUI settings, display cameras, accessibility/localization and alternate resolutions are not fully certified. Future bank/service panels need explicit input ownership; arbitrary modals are not automatically discovered.

Quest journal is added to PlayerHolder and centrally gated by PlayerControlState. J/Escape close it without pausing; gameplay remains blocked while open. See [quests](Quests.md) for the paged TMP/uGUI journal and persistence boundary.

Minimap: PlayerHolder now supplies a north-up circular minimap and M/Escape expanded view, with PlayerControlState modal gating. Configuration, validation and limitations: [Minimap](Minimap.md).
