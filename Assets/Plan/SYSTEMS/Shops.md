# Shops

Implemented 2026-11-13 by extending the existing ShopScript/ShopInventory/ShopInventoryUI/ShopInventorySlot and merchant prefab. Dialogue and PlayerControlState retain modal ownership; world time continues.

## Configuration and ownership

`ShopDefinition` assets in `Assets/Shops/` describe name/category, authored stock, initial/maximum counts, per-interval restock amount, interval, purchase/sale multipliers, optional per-item price overrides (-1 uses Item.price), and separate purchase/sale item/category allowlists. Authored stock is also a sale allowlist. Do not duplicate an item in stock: initialization ignores duplicate definitions and entries beyond nine. Empty allowlists refuse player sales. Item.tradeRestricted blocks category acceptance; an explicit buyItems entry allows it. Mark quest/special item definitions accordingly; there is no automatic quest-item classification.

Each ShopInventory owns quantities independently of its shared definition. Nine distinct types maximum, including sold-out reserved types and purchased items. Player sales of a new type require a free slot; existing stock cannot exceed its maximum. Purchased types use purchasedItemCapacity if absent from authored stock. Only authored types restock. Shop-specific sell rules can prevent resale of purchased items.

Restocking uses scaled gameplay time, advances while the panel is closed, catches up missed intervals and clamps to maximum. An interval of zero disables it. Reopening never initializes stock again. Stock survives only for that merchant component's lifetime; scene reload/destroy resets it. No disk gameplay save system exists. Future saving must persist stable merchant identity, item identities, quantities, next-restock clock policy and Inventory gold with the rest of the player state.

## Transactions and currency

Inventory now owns serialized nonnegative gold (default starting balance 100). No preexisting currency implementation was found. Shop transactions use Inventory.TryExchange with deferred notification: validate stock/allowlists/gold/capacity, commit bag contents, gold and stock, then notify. Reentrant trades on the same merchant are rejected during commit/notification. Each input trades one item. Prices are nonnegative, rounded up consistently, and bounded to int.MaxValue; sale gold overflow rejects the trade. Sample assets use explicit positive prices because existing item values can be zero. Custom zero-priced stock is intentionally free.

Player inventory remains fixed slots, one item per slot; no new player stack/instance model was introduced. Merchant quantities share a type slot regardless of quantity. Selling scans only bag contents, so equipped gear is unavailable; identical unequipped copies remain sellable. Existing Inventory/Equipment/Quest APIs remain in use.

## UI and prefab setup

The existing ShopUI expands its existing serialized slots at Awake if fewer than nine, binds them explicitly to the active merchant, and uses a 3x3 grid. TMP labels show name, quantity, price and sold-out status. Buy/Sell/Previous/Next/Close buttons, shop name, gold, hover details/restrictions and transaction feedback are added to the existing panel. Sell mode pages all distinct bag items in groups of nine, including refused items so their restriction feedback is accessible. Player inventory capacity is unchanged. UI refreshes on inventory and merchant changes, and unsubscribes when closed.

`DialogueMerchant.prefab` now references GeneralStore.asset. BlacksmithMerchant and AlchemistMerchant extend that merchant prefab using existing dialogue, pickup item definitions and components. No duplicate item definitions or separate shop panel. Existing DialogueMerchant_Test scene inherits the updated general merchant. Specialist merchants are available as prefabs for placement; no production merchant placement changed. Legacy merchants without a definition retain rolled finite stock, base-price purchases and refuse sales/restocking. ShopInventory.instance remains compatibility only; UI trades with its explicitly selected merchant.

## Verification and handoff

Repeatable checks: `AgentScripts/Shops/BuildShops.cs` (Run builds definitions/prefabs; Verify exercises transactions), `VerifyShopUI.cs` (Play Mode fixture). Do not run builders just to validate. Existing dialogue integration regression is in AgentScripts/Dialogue.

Validation results are recorded below after execution. Remaining integration: production-flow testing, different resolutions/controller navigation and durable economy saves; no claim of a player stacking system or production economy balance.

Executed validation (2026-11-13):
- Unity 6000.3.28f1 compilation completed without compile errors.
- BuildShops.Verify passed actual definition-based purchase/sale/gold/item transfer, zero-stock rejection, depletion, independent merchants, catch-up restock bounds, full bag/merchant/stock rejection with unchanged funds/items, explicit restricted/category rules, insufficient gold, repeated purchases and idempotent initialization.
- VerifyMerchantDialogue.Run passed in the existing merchant fixture: E interaction/raycast, dialogue choices, movement/look/combat lock, uninterrupted shop handoff, visible stock, E/Escape close and gameplay restoration while timeScale remains 1.
- VerifyShopUI.Run passed in Play Mode using the retained item Button callbacks and Sell/Buy controls: purchase and sale, stock preservation across closing/reopening, maximum nine visible types and actual three-column GridLayoutGroup.
- Visual inspection of the captured Game view confirmed existing fantasy stock cards and the three-column layout. Capture screen scaling compresses the view; broader resolution/accessibility checks remain unverified. No shop errors in the final run; an earlier UI initialization defect was fixed and rerun. Existing unrelated animation-event console history and one earlier UnityEditor.Connect exception were not changed.
- Test Play Mode was exited and original edit scene/start-scene setting restored; no user scene changes saved.

Next integration requirements: tag future quest/special Item assets with tradeRestricted, assign definitions to new merchants, and balance starter gold/prices. Specialist prefabs retain the shared Rowan dialogue; customize their conversations when placed. No specialist scene placement or production-flow certification is claimed. Gold and stock are not persisted to disk.
