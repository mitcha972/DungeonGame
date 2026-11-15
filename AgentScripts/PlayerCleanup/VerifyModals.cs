using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json;
using DungeonGame.Dialogue;
public static class VerifyCharacterModals
{
    public static async Task<object> Run()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required.");
        var checks=new List<object>(); Action<bool,string> check=(pass,name)=>checks.Add(new{pass,name}); var errors=new List<string>(); var cleanup=new List<GameObject>();
        var original=InputSystem.settings; var settings=UnityEngine.Object.Instantiate(original); settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus; settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; InputSystem.settings=settings;
        Func<Key,Task> press=async key=>{InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));InputSystem.Update();await Task.Delay(80);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();await Task.Delay(100);};
        try
        {
            var p=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).First(x=>x.GetComponent<StarterAssets.ThirdPersonController>()!=null); p.maxHealth=p.currentHealth=10000;
            foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None))ai.enabled=false;
            var state=p.GetComponent<PlayerControlState>(); state.ClosePanels(); state.SendMessage("OnApplicationFocus",true); var dialogue=p.GetComponent<DialogueController>(); var combat=p.GetComponent<PlayerCombat>(); combat.CancelAttack();
            var merchant=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NPC/DialogueMerchant.prefab"));cleanup.Add(merchant);var npc=merchant.GetComponent<DialogueNpc>();
            combat.Attack(false);check(combat.IsExecutingAttack,"An attack is active before dialogue opens");
            bool opened=dialogue.Open(npc);check(opened&&dialogue.IsOpen&&!state.GameplayAllowed&&!combat.IsExecutingAttack,"Dialogue owns input and cancels an in-flight attack");
            await press(Key.E); await press(Key.Tab);check(dialogue.IsOpen&&!state.InventoryUI.inv.activeSelf,"E and Tab are ignored while talking");
            await press(Key.Escape);check(!dialogue.IsOpen&&state.GameplayAllowed&&Time.timeScale==1,"Escape closes dialogue without pausing the world");
            var chestObject=new GameObject("Temporary chest fixture");cleanup.Add(chestObject);var inventory=chestObject.AddComponent<ChestInventory>();var chest=chestObject.AddComponent<ChestScript>();chest.inventory=inventory;
            var item=AssetDatabase.FindAssets("t:Consumable").Select(g=>AssetDatabase.LoadAssetAtPath<Item>(AssetDatabase.GUIDToAssetPath(g))).First();inventory.Add(item);
            chest.Interact(p.transform);state.Synchronize();check(state.InventoryUI.chestInventoryUI.currentChest==chest&&!state.GameplayAllowed,"Chest interaction resolves canvas through PlayerControlState");
            var chestUI=state.InventoryUI.chestInventoryUI;var slot=chestUI.slots.First(s=>s.item==item);int before=p.GetComponent<Inventory>().Count(item);slot.ItemSelect();check(p.GetComponent<Inventory>().Count(item)==before+1&&!inventory.items.Contains(item),"Existing chest-to-player item transfer still works");
            await press(Key.E);check(!chestUI.gameObject.activeInHierarchy&&state.GameplayAllowed,"E closes chest and restores gameplay");
            bool shop=merchant.GetComponent<ShopScript>().TryOpenShop(p.transform);check(shop&&state.ShopUI.gameObject.activeInHierarchy&&!state.GameplayAllowed,"Existing shop opens using the same modal owner");
            await press(Key.Escape);check(!state.ShopUI.gameObject.activeInHierarchy&&state.GameplayAllowed&&Time.timeScale==1,"Escape closes shop without opening pause");
        }
        catch(Exception e){errors.Add(e.ToString());}
        finally{InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();InputSystem.settings=original;UnityEngine.Object.Destroy(settings);foreach(var go in cleanup)UnityEngine.Object.Destroy(go);}
        var result=new{passed=checks.All(c=>(bool)c.GetType().GetProperty("pass").GetValue(c))&&errors.Count==0,checks,errors};File.WriteAllText("Assets/Plan/Validation/player-cleanup-modals.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
}
