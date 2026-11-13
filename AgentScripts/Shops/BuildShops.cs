using System;
using UnityEngine;
using UnityEditor;
public static class BuildShops
{
 static T Asset<T>(string path) where T:ScriptableObject { var a=AssetDatabase.LoadAssetAtPath<T>(path); if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);}return a; }
 public static object Run()
 {
 if(EditorApplication.isPlaying)throw new Exception("Preserve Play Mode");
 if(!AssetDatabase.IsValidFolder("Assets/Shops"))AssetDatabase.CreateFolder("Assets","Shops");
 var sword=AssetDatabase.LoadAssetAtPath<Item>("Assets/GamePlay/ScriptableObjects/Items/Weapons/Sword-Iron.asset");
 var shield=AssetDatabase.LoadAssetAtPath<Item>("Assets/GamePlay/ScriptableObjects/Items/Weapons/Shield-Wood.asset");
 var potion=AssetDatabase.LoadAssetAtPath<Item>("Assets/GamePlay/ScriptableObjects/Items/Consumables/Health Potion.asset");
 var names=new[]{"GeneralStore","Blacksmith","Alchemist"};
 for(int k=0;k<3;k++) {
 var d=Asset<ShopDefinition>("Assets/Shops/"+names[k]+".asset");d.shopName=k==0?"General Store":names[k]; d.category=names[k];
 var stock=k==0?new[]{sword,shield,potion}:k==1?new[]{sword,shield}:new[]{potion};
 d.stock=Array.ConvertAll(stock,i=>new ShopStockRule{item=i,initial=k==2?5:3,maximum=10,restockAmount=k==2?2:1,purchasePrice=i==potion?12:40,salePrice=i==potion?4:15});
 d.buyItems=stock; d.sellItems=stock; d.buyCategories=k==2?new[]{ItemType.Potion}:k==1?new[]{ItemType.Sword,ItemType.Bow,ItemType.Head,ItemType.Chest,ItemType.Legs,ItemType.Shield}:new[]{ItemType.Potion};
 d.sellCategories=d.buyCategories; d.restockInterval= k==0?60:k==1?90:30; d.purchaseMultiplier=k==0?1.25f:1.5f;d.saleMultiplier=.5f;EditorUtility.SetDirty(d);
 var path=k==0?"Assets/Prefabs/WorldInteract/DialogueMerchant.prefab":"Assets/Prefabs/WorldInteract/"+names[k]+"Merchant.prefab";
 var source=k==0?path:"Assets/Prefabs/WorldInteract/DialogueMerchant.prefab";
 var root=PrefabUtility.LoadPrefabContents(source);
 try {root.name=k==0?"DialogueMerchant":names[k]+"Merchant";var inv=root.GetComponent<ShopInventory>();inv.definition=d;inv.space=9;PrefabUtility.SaveAsPrefabAsset(root,path);} finally {PrefabUtility.UnloadPrefabContents(root);}
 }
 AssetDatabase.SaveAssets(); return "Created three definitions; extended DialogueMerchant and two merchant prefab variants.";
 }
 public static object Verify()
 {
 var go=new GameObject("ShopVerification"); var bag=new GameObject("BagVerification"); var other=new GameObject("OtherShopVerification");
 try {
 var inv=go.AddComponent<ShopInventory>();inv.definition=AssetDatabase.LoadAssetAtPath<ShopDefinition>("Assets/Shops/GeneralStore.asset");inv.Initialize();
 var second=other.AddComponent<ShopInventory>();second.definition=inv.definition;second.Initialize();var p=bag.AddComponent<Inventory>();
 // Awake is not invoked on EditMode components: use explicit empty slots.
 p.items.Clear();for(int i=0;i<20;i++)p.items.Add(null);
 var item=inv.items[0];int stock=inv.Quantity(item),gold=p.Gold;string msg;
 if(!inv.Trade(p,item,true,out msg)||p.Count(item)!=1||inv.Quantity(item)!=stock-1||p.Gold!=gold-inv.Price(item,true))throw new Exception("Purchase: "+msg);
 if(!inv.Trade(p,item,false,out msg)||p.Count(item)!=0||inv.Quantity(item)!=stock)throw new Exception("Sale: "+msg);
 if(second.Quantity(item)!=stock)throw new Exception("Shared stock");
 while(inv.Quantity(item)>0)inv.Remove(item);
 int before=p.Gold;if(inv.Trade(p,item,true,out msg)||p.Gold!=before)throw new Exception("Zero stock");
 inv.Restock(Time.timeAsDouble+180);if(inv.Quantity(item)<=0||inv.Quantity(item)>inv.Maximum(item))throw new Exception("Restock");
 p.items.Clear();for(int i=0;i<20;i++)p.items.Add(item);before=p.Gold;stock=inv.Quantity(item);
 if(inv.Trade(p,item,true,out msg)||p.Gold!=before||inv.Quantity(item)!=stock)throw new Exception("Full bag atomicity");
 var alchemy=AssetDatabase.LoadAssetAtPath<ShopDefinition>("Assets/Shops/Alchemist.asset");if(alchemy.Accepts(item))throw new Exception("Restrictions");
 inv.Initialize();if(inv.Quantity(item)!=stock)throw new Exception("Reset");
 var scratch=ScriptableObject.CreateInstance<Item>();var restricted=ScriptableObject.CreateInstance<Item>();
 try {
 scratch.name="Verification item";scratch.itemType=ItemType.Potion;restricted.itemType=ItemType.Potion;restricted.tradeRestricted=true;
 if(inv.definition.Accepts(restricted))throw new Exception("Restricted category bypass");
 // Fill every merchant type slot and reject a tenth without removing it or paying.
 while(inv.items.Count<9){var filler=ScriptableObject.CreateInstance<Item>();filler.name="Filler";inv.Add(filler);}
 p.items[0]=scratch;before=p.Gold;if(inv.Trade(p,scratch,false,out msg)||p.items[0]!=scratch||p.Gold!=before)throw new Exception("Full merchant atomicity");
 while(inv.Quantity(item)<inv.Maximum(item))inv.Add(item);
 p.items[0]=item;before=p.Gold;if(inv.Trade(p,item,false,out msg)||p.items[0]!=item||p.Gold!=before)throw new Exception("Maximum atomicity");
 p.items[0]=null;
 var goldField=typeof(Inventory).GetField("gold",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);goldField.SetValue(p,0);
 stock=inv.Quantity(item);if(inv.Trade(p,item,true,out msg)||inv.Quantity(item)!=stock||p.Count(item)!=19)throw new Exception("Insufficient gold");
 goldField.SetValue(p,1000);int attempts=0;while(inv.Trade(p,item,true,out msg))attempts++;
 if(attempts!=1||p.Count(item)!=20||inv.Quantity(item)!=stock-1)throw new Exception("Repeated purchases");
 }finally{foreach(var filler in inv.items)if(filler!=null&&filler.name=="Filler")UnityEngine.Object.DestroyImmediate(filler);UnityEngine.Object.DestroyImmediate(scratch);UnityEngine.Object.DestroyImmediate(restricted);}
 return "PASS: purchase, sale, gold, item transfer, depletion, zero-stock rejection, independent stock, restock bounds, full bag atomicity, restrictions, initialization preserves stock, full merchant, stock maximum, restricted category rejection, insufficient gold, repeated purchases.";
 } finally {UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(bag);UnityEngine.Object.DestroyImmediate(other);}
 }
}

