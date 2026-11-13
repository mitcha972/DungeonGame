using UnityEngine;
using UnityEngine.EventSystems;

public class ShopInventorySlot : MonoBehaviour, IPointerEnterHandler
{
    public UnityEngine.UI.Image icon;
    public InventoryUI IU;
    public ShopInventoryUI CUI;
    public UnityEngine.UI.Text itemPriceText;
    private Item item;
    private TMPro.TextMeshProUGUI label;
    public void AddItem(Item value)
    {
        item = value; if (icon != null) { icon.sprite = item.icon; icon.enabled = true; }
        var buyButton = transform.Find("BuyButton"); if (buyButton != null) buyButton.gameObject.SetActive(false);
        var itemButton = transform.Find("ItemButton") as RectTransform;
        if (itemButton != null) { itemButton.anchorMin = Vector2.zero; itemButton.anchorMax = Vector2.one; itemButton.offsetMin = Vector2.zero; itemButton.offsetMax = Vector2.zero; var button = itemButton.GetComponent<UnityEngine.UI.Button>(); if (button != null) { button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent(); button.onClick.AddListener(UseItem); } }
        if (icon != null) { var ir = icon.rectTransform; ir.anchorMin = ir.anchorMax = new Vector2(.5f, 1); ir.pivot = new Vector2(.5f,1); ir.anchoredPosition = new Vector2(0,-5); ir.sizeDelta = new Vector2(36,36); }
        if (itemPriceText != null) itemPriceText.enabled = false;
        if (label == null)
        {
            var go = new GameObject("TradeLabel", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            go.transform.SetParent(transform, false); label = go.GetComponent<TMPro.TextMeshProUGUI>();
            var r = (RectTransform)go.transform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(3, 0); r.offsetMax = new Vector2(-3, -43);
            label.font = TMPro.TMP_Settings.defaultFontAsset; label.fontSize = 12; label.color = new Color(1, .92f, .72f); label.alignment = TMPro.TextAlignmentOptions.Center; label.raycastTarget = false;
        }
        int count = CUI.Selling ? CUI.PlayerInventory.Count(item) : CUI.inventory.Quantity(item);
        label.text = item.name + "\n" + count + " in stock · " + CUI.inventory.Price(item, !CUI.Selling) + "g";
        if (!CUI.Selling && count == 0) label.text += "\nSold out";
    }
    public void ClearSlot() { item = null; if (icon != null) { icon.sprite = null; icon.enabled = false; } if (label != null) label.text = ""; }
    public void UseItem() { if (item != null && CUI != null) CUI.Trade(item); }
    public void OnPointerEnter(PointerEventData data) { if (item != null && CUI != null) CUI.Describe(item); }
}
