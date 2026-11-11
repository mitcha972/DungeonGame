using System.Collections;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject owner;
    public Transform itemsParent;
    public GameObject inventoryUI;
    public Inventory inventory;
    public InventorySlot[] slots;

    private void Start()
    {
        if (itemsParent != null) slots = itemsParent.GetComponentsInChildren<InventorySlot>(true);
        RefreshSlots();
    }

    private void UIAdd() => RefreshSlots();
    private void UIUpdate() => RefreshSlots();
    private void UIRemove(int slot) => RefreshSlots();

    private void RefreshSlots()
    {
        if (inventory == null || slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            if (i < inventory.items.Count && inventory.items[i] != null) slots[i].AddItem(inventory.items[i]);
            else slots[i].ClearSlot();
        }
    }

    public IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(0.01f);
        RefreshSlots();
    }
}
