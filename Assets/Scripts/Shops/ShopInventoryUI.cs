using System.Collections.Generic;
using UnityEngine;

public class ShopInventoryUI : MonoBehaviour
{
    public Transform itemsParent;
    public ShopInventory inventory;
    public ShopInventorySlot[] slots;
    public GameObject[] inventoryItems;
    public ShopScript currentShop;
    [SerializeField] private PlayerControlState _controlState;
    public Inventory PlayerInventory { get; private set; }
    public bool Selling { get; private set; }
    private int page;
    private TMPro.TextMeshProUGUI heading, details;
    void Awake()
    {
        slots = itemsParent.GetComponentsInChildren<ShopInventorySlot>(true);
        // Expand existing prefab slots rather than introducing a second shop panel.
        if (slots.Length > 0)
        {
            for (int i = slots.Length; i < 9; i++) Instantiate(slots[0], slots[0].transform.parent);
            slots = itemsParent.GetComponentsInChildren<ShopInventorySlot>(true);
        }
        var content = slots.Length > 0 ? slots[0].transform.parent : itemsParent;
        var oldLayout = content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (oldLayout != null) DestroyImmediate(oldLayout);
        var fitter = content.GetComponent<UnityEngine.UI.ContentSizeFitter>(); if (fitter != null) fitter.enabled = false;
        var scroll = GetComponent<UnityEngine.UI.ScrollRect>(); if (scroll != null) { scroll.enabled = false; if (scroll.verticalScrollbar != null) scroll.verticalScrollbar.gameObject.SetActive(false); }
        var title = transform.Find("Title"); if (title != null) title.gameObject.SetActive(false);
        var rootRect = (RectTransform)transform; rootRect.anchorMin = rootRect.anchorMax = new Vector2(.5f,.5f); rootRect.pivot = new Vector2(.5f,.5f); rootRect.anchoredPosition = Vector2.zero; rootRect.sizeDelta = new Vector2(480, 520);
        var contentRect = content as RectTransform; if (contentRect != null) { contentRect.anchorMin = Vector2.zero; contentRect.anchorMax = Vector2.one; contentRect.offsetMin = Vector2.zero; contentRect.offsetMax = Vector2.zero; }
        var grid = content.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (grid == null) grid = content.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
        if (grid != null) { grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3; grid.cellSize = new Vector2(110, 98); grid.spacing = new Vector2(5, 5); grid.padding = new RectOffset(15,15,15,15); grid.childAlignment = TextAnchor.UpperCenter; }
        var parentRect = itemsParent as RectTransform;
        if (parentRect != null) { parentRect.anchorMin = parentRect.anchorMax = new Vector2(.5f, .5f); parentRect.pivot = new Vector2(.5f, .5f); parentRect.anchoredPosition = new Vector2(0, -5); parentRect.sizeDelta = new Vector2(370, 345); }
        heading = Label("ShopHeading", new Vector2(0, 205), new Vector2(440, 40), 22);
        details = Label("TradeDetails", new Vector2(0, -225), new Vector2(460, 65), 16);
        Button("Buy", new Vector2(-160, -175), () => Mode(false));
        Button("Sell", new Vector2(-80, -175), () => Mode(true));
        Button("Previous", new Vector2(10, -175), () => { page = Mathf.Max(0, page - 1); Refresh(); });
        Button("Next", new Vector2(100, -175), () => { page++; Refresh(); });
        Button("Close", new Vector2(190, -175), CloseUI);
    }
    TMPro.TextMeshProUGUI Label(string name, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TMPro.TextMeshProUGUI)); go.transform.SetParent(transform, false);
        var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size;
        var text = go.GetComponent<TMPro.TextMeshProUGUI>(); text.font = TMPro.TMP_Settings.defaultFontAsset; text.fontSize = fontSize; text.color = new Color(1, .92f, .72f); text.alignment = TMPro.TextAlignmentOptions.Center; text.raycastTarget = false; return text;
    }
    void Button(string title, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(title, typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button)); go.transform.SetParent(transform, false);
        var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(82, 32);
        var image = go.GetComponent<UnityEngine.UI.Image>(); image.color = new Color(.16f, .11f, .07f, .95f); image.raycastTarget = true;
        var child = Label(title + "Text", Vector2.zero, new Vector2(82, 32), 15); child.transform.SetParent(go.transform, false); child.text = title;
        var button = go.GetComponent<UnityEngine.UI.Button>(); button.targetGraphic = image; button.onClick.AddListener(action);
    }
    public void OpenShop(ShopScript shop, PlayerControlState state)
    {
        Unbind(); if (state != null) _controlState = state;
        gameObject.SetActive(true); currentShop = shop; inventory = shop.inventory; inventory.Initialize();
        PlayerInventory = state != null ? state.GetComponent<Inventory>() : slots.Length > 0 && slots[0].IU != null ? slots[0].IU.inventory : null;
        inventory.Changed += Refresh; if (PlayerInventory != null) PlayerInventory.onItemChangedCallback += Refresh;
        Selling = false; page = 0; Refresh(); if (_controlState != null) _controlState.Synchronize();
    }
    void Mode(bool sell) { Selling = sell; page = 0; details.text = sell ? "Select a bag item to sell one. Hover for restrictions." : "Select an item to buy one."; Refresh(); }
    public void Refresh()
    {
        if (inventory == null) return;
        var entries = new List<Item>();
        if (Selling && PlayerInventory != null) { foreach (var item in PlayerInventory.items) if (item != null && !entries.Contains(item)) entries.Add(item); }
        else entries.AddRange(inventory.items);
        page = Mathf.Clamp(page, 0, Mathf.Max(0, (entries.Count - 1) / 9));
        heading.text = (inventory.definition != null ? inventory.definition.shopName : currentShop.name) + " · " + (PlayerInventory != null ? PlayerInventory.Gold : 0) + " gold · " + (Selling ? "Sell" : "Buy") + " " + (page + 1);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].CUI = this; slots[i].ClearSlot(); int index = page * 9 + i;
            bool visible = i < 9 && index < entries.Count; slots[i].gameObject.SetActive(visible); if (visible) slots[i].AddItem(entries[index]);
        }
    }
    public void Describe(Item item) { details.text = item.name + ": " + item.description + (Selling ? "\n" + (inventory.Refusal(item) ?? "Merchant accepts this item.") : ""); }
    public void Trade(Item item) { if (inventory == null) return; inventory.Trade(PlayerInventory, item, !Selling, out var result); details.text = result; Refresh(); }
    void Unbind() { if (inventory != null) inventory.Changed -= Refresh; if (PlayerInventory != null) PlayerInventory.onItemChangedCallback -= Refresh; }
    void OnDisable() { Unbind(); }
    void Update() { if (_controlState == null && Input.GetButtonDown("Cancel")) CloseUI(); }
    public void CloseUI() { Unbind(); currentShop = null; inventory = null; PlayerInventory = null; gameObject.SetActive(false); if (_controlState != null) _controlState.Synchronize(); }
    public void AddItems() { Refresh(); }
}
