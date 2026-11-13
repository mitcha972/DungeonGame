using UnityEngine;

public class ChestInventorySlot : UISlot
{
    public InventoryUI IU;
    public ChestInventoryUI CUI;

    private void Start()
    {
        var button = UIPointerEvents.GetOrAdd(this);
        if (button != null) button.LeftClicked = () => ItemSelect();
    }

    public override void ItemSelect()
    {
        if (item == null || IU == null || IU.inventory == null || CUI == null || CUI.currentChest == null) return;
        var source = CUI.currentChest.inventory;
        if (source == null || !source.items.Contains(item) || !IU.inventory.Add(item)) return;
        source.Remove(item);
        ClearSlot();
    }
}
