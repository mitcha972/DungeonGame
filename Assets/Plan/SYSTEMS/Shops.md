# Shops

**Implemented stock display/item grants; partial economy.** ShopScript, ShopInventory, ShopInventoryUI and ShopInventorySlot now live together under Assets/Scripts/Shops. [Dialogue](Dialogue.md) owns conversation behavior.

ShopScript inherits Interactable. Start rolls itemDrops (0 never drops, 100 always drops), reads each pickup prefab's root ItemInteract.item and adds to local ShopInventory up to capacity. ShopInventory.instance remains compatibility state; OpenShop explicitly supplies the selected merchant. Interact delegates to an assigned DialogueNpc, otherwise opens directly. TryOpenShop uses explicit scene UI or the interacting player's PlayerControlState.ShopUI.

ShopInventoryUI activates before binding so Awake discovers inactive slots, sets currentShop/inventory, clears stale icons, activates stocked rows and selects a button. CloseUI clears bindings/slots and synchronizes controls. PlayerHolder's same ShopUI sits directly under UICanvas, outside the hidden InventoryUIs container; existing components/slots/events remain.

Base `Assets/Prefabs/WorldInteract/Shop.prefab` intentionally remains unchanged with no UI assignment/drop entries. `DialogueMerchant.prefab` is a variant with DialogueNpc, MerchantConversation and three 100% entries: iron sword, wooden shield and potion. The test scene explicitly binds that merchant to its PlayerHolder ShopUI. Other shops' stock was not populated/overwritten.

**Known limitation:** ShopInventorySlot.UseItem calls Inventory.Add, ignores failure, does not charge currency and does not remove stock. Prices can be zero. Economy completion requires explicit affordability, transfer-failure, currency, depletion/restock and persistence policies; organization does not redesign this behavior.

Historical keyboard/mouse tests verified visible stock, atomic dialogue→shop lock and close/reopen. [Maintenance](../MAINTENANCE.md) records current checks. Multiple-merchant singleton behavior and alternate player UI hierarchies remain separate integration work.
