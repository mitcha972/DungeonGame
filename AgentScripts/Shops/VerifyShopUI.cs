using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
public static class VerifyShopUI
{
 public static object Run()
 {
 var state=UnityEngine.Object.FindFirstObjectByType<PlayerControlState>();var shop=UnityEngine.Object.FindFirstObjectByType<ShopScript>();
 var dialogue=state.GetComponent<DungeonGame.Dialogue.DialogueController>();dialogue.Close();shop.TryOpenShop(state.transform);var ui=state.ShopUI;
 if(ui.slots.Count(s=>s.gameObject.activeInHierarchy)>9||state.GameplayAllowed||Time.timeScale!=1)throw new Exception("Shop ownership/slot cap");
 var grid=ui.slots[0].transform.parent.GetComponent<UnityEngine.UI.GridLayoutGroup>(); if(grid==null||grid.constraintCount!=3)throw new Exception("3-column grid"); var item=shop.inventory.items[0];var bag=state.GetComponent<Inventory>();int gold=bag.Gold,qty=shop.inventory.Quantity(item),count=bag.Count(item);
 ui.slots[0].transform.Find("ItemButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();if(bag.Gold!=gold-shop.inventory.Price(item,true)||bag.Count(item)!=count+1||shop.inventory.Quantity(item)!=qty-1)throw new Exception("UI purchase");
 ui.CloseUI();shop.TryOpenShop(state.transform);if(shop.inventory.Quantity(item)!=qty-1)throw new Exception("Reopening resets stock");
 var sell=ui.transform.Find("Sell").GetComponent<UnityEngine.UI.Button>();sell.onClick.Invoke();
 var slot=ui.slots.First(s=>s.gameObject.activeInHierarchy && (Item)typeof(ShopInventorySlot).GetField("item", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(s)==item);slot.transform.Find("ItemButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();if(bag.Count(item)!=count)throw new Exception("UI sell");
 ui.transform.Find("Buy").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
 return new{passed=true,slots=ui.slots.Count(s=>s.gameObject.activeInHierarchy),gold=bag.Gold,uiSize=((RectTransform)ui.transform).rect.size.ToString(),stockSize=((RectTransform)ui.itemsParent).rect.size.ToString()};
 }
}



