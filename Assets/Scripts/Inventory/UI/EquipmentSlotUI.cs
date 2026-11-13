using UnityEngine;

public class EquipmentSlotUI : UISlot
{
    public EquipmentSlot equipmentSlot;

    private void Start()
    {
        var button = UIPointerEvents.GetOrAdd(this);
        if (button != null) button.LeftClicked = () => ItemSelect();
    }

    public override void ItemSelect()
    {
        if (item == null || inventory == null || equipmentManager == null) return;
        if (!equipmentManager.currentEquipment.TryGetValue(equipmentSlot, out Equipment equipped) || equipped != item)
        {
            ClearSlot();
            return;
        }
        if (!inventory.Add(equipped)) return;
        equipmentManager.Unequip(equipmentSlot);
        ClearSlot();
    }

    public void Unequip() => ItemSelect();
}
