using UnityEngine;

/// <summary>Adds player slot presentation to the shared equipment owner.</summary>
public class PlayerEquipment : CharacterEquipment
{
    public EquipmentSlotUI[] equipmentSlotsUI;
    protected override void Awake() { base.Awake(); EquipmentChanged += RefreshSlot; }
    protected override void Start()
    {
        base.Start();
        foreach (var item in Items.Values) RefreshSlot(item, null);
    }
    private void RefreshSlot(Equipment item, Equipment previous)
    {
        if (equipmentSlotsUI == null) return;
        var kind = item != null ? item.equipSlot : previous.equipSlot;
        foreach (var slot in equipmentSlotsUI)
            if (slot != null && slot.equipmentSlot == kind)
            { if (item != null) slot.AddItem(item); else slot.ClearSlot(); }
    }
    protected override void OnDestroy() { EquipmentChanged -= RefreshSlot; base.OnDestroy(); }
}
