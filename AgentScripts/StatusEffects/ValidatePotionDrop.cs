using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
var results=new List<string>(); Action<bool,string> Check=(ok,n)=>results.Add((ok?"PASS ":"FAIL ")+n);
var p=Object.FindFirstObjectByType<PlayerStats>();var bag=p.GetComponent<Inventory>();var manager=p.GetComponent<EquipmentManager>();
var poison=AssetDatabase.LoadAssetAtPath<Consumable>("Assets/GamePlay/ScriptableObjects/Items/Consumables/Poison Potion.asset");
var healthPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Items/Consumables/Potion.prefab");
Check(poison.worldPrefab!=healthPrefab,"Poison has independent pickup prefab");
Check(poison.worldPrefab.GetComponent<ItemInteract>().item==poison,"Poison pickup authored with Poison item");
Check(healthPrefab.GetComponent<ItemInteract>().item!=poison,"original health pickup unchanged");
Check(poison.worldPrefab.GetComponentInChildren<MeshFilter>().sharedMesh==healthPrefab.GetComponentInChildren<MeshFilter>().sharedMesh,"reuses potion mesh");
Check(poison.worldPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterials.SequenceEqual(healthPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterials),"reuses potion materials");
int index=bag.items.FindIndex(i=>i==poison); if(index<0){bag.Add(poison);index=bag.items.FindIndex(i=>i==poison);}
int count=bag.Count(poison); var go=new GameObject("Drop validation slot");var slot=go.AddComponent<UISlot>();slot.inventory=bag;slot.equipmentManager=manager;slot.item=poison;slot.slotIndex=index;
var before=Object.FindObjectsByType<ItemInteract>(FindObjectsSortMode.None);slot.RemoveFromInventory();
var drops=Object.FindObjectsByType<ItemInteract>(FindObjectsSortMode.None).Except(before).ToArray();
Check(drops.Length==1 && drops[0].item==poison,"UI drop creates exactly one poison pickup");
Check(bag.Count(poison)==count-1 && bag.items[index]==null,"UI drop removes exact inventory entry");
Check(drops.Length==1 && drops[0].GetComponent<Rigidbody>()!=null && drops[0].GetComponent<Collider>()!=null,"drop retains potion physics");
slot.RemoveFromInventory();Check(Object.FindObjectsByType<ItemInteract>(FindObjectsSortMode.None).Except(before).Count()==1,"repeated stale drop cannot duplicate");
if(drops.Length==1){drops[0].Interact(p.transform);drops[0].Interact(p.transform);Check(bag.Count(poison)==count,"pickup returns Poison exactly once");}
Check(!manager.TryDropItem(null),"null drop rejected");
Object.Destroy(go);
return new {passed=results.Count(x=>x.StartsWith("PASS")),failed=results.Count(x=>x.StartsWith("FAIL")),results};
