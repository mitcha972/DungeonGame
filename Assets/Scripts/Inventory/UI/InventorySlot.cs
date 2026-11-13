using UnityEngine;

public class InventorySlot : UISlot
{
    private void Start()
    {
        var ui = GetComponentInParent<InventoryUI>();
        if (ui != null)
        {
            if (inventory == null) inventory = ui.inventory;
            if (ui.owner != null) equipmentManager = ui.owner.GetComponent<EquipmentManager>();
        }
        var button = UIPointerEvents.GetOrAdd(this);
        if (button == null) return;
        button.LeftClicked = () => ItemSelect();
        button.RightClicked = () => RemoveFromInventory();
    }

    public override void ItemSelect() => base.ItemSelect();
}
