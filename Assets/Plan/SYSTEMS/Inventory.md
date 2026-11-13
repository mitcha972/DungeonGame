# Inventory, equipment and chests

**Implemented prototype.** Files moved to Assets/Scripts/Inventory/{Items,Equipment,UI,Chests}; Interactable/world pickups belong to World/Interactions. [Shops](Shops.md) describe merchant storage. No inventory or purchase algorithm changed in organization.

Inventory.Awake creates null slot entries and derives capacity from configured UI slots. The capacity-with-zero-count audit finding is superseded. Add fills an empty slot and returns failure at capacity; indexed operations guard bounds, refresh UI and notify. ItemInteract destroys pickups only after successful Add. ChestInventorySlot checks source membership and successful destination Add before removal. ScriptableObject item definitions have no quantity/unique-instance/save model.

EquipmentManager owns slot/equipment/mesh dictionaries and modifiers/notifications. ReplaceEquipment registers state before one callback; old event-order findings are superseded. Indexed swaps return replaced gear into the source slot. PlayerEquipmentManager adds UI; EnemyEquipmentManager loads defaults. UnequipAll is a reset operation used by class selection; gameplay U uses UnequipAllToInventory and retains equipment if inventory is full. Preserve these distinct semantics.

Health Consumable validates owner/slot/health, heals through PlayerStats.Heal and removes the consumed entry. Damage modifier behavior is not implemented. Dropping remains equipment-specific: TryDropItem rejects unsupported items before UISlot removes anything. Do not claim every item type can be dropped.

UIPointerEvents supplies native callbacks while retaining vendor presentation compatibility. Slot/index ordering and actual references still matter. Source verifies initialization/transfer/event/consumption paths; [maintenance](../MAINTENANCE.md) lists executed tests. Full-capacity gear swaps, hotkeys, tooltip/right-click behavior, class resets and every prefab variant need targeted checks. Next design dependencies are identity/quantities/persistence/economy, not another slot initialization rewrite.

Quest turn-ins use generic InventoryItemAmount/Count/CanExchange/TryExchange APIs. Exchange builds a candidate slot list before mutation; reward capacity includes consumed slots. Quest transactions defer inventory notification until state/XP commit. Existing slot storage/equipment behavior is retained; stacks and durable item identity remain unimplemented. See [quests](Quests.md).

Merchant integration (2026-11-13): Inventory owns serialized gold (default 100); ShopInventory uses deferred TryExchange notification to commit stock, gold and bag together. Player bag remains one item per slot. See [shops](Shops.md) for rules and validation.
