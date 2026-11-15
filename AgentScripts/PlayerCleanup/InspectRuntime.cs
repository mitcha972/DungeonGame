using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
public static class InspectCharacterRuntime
{
    public static void Main()
    {
        var p=Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).First(x=>x.GetComponent<StarterAssets.ThirdPersonController>()!=null);
        var state=p.GetComponent<PlayerControlState>();
        var trees=p.transform.root.GetComponentsInChildren<UI_SkillTree>(true).Select(t=>new{t.name,active=t.gameObject.activeInHierarchy,player=new SerializedObject(t).FindProperty("playerStats").objectReferenceValue?.name,correctPlayer=new SerializedObject(t).FindProperty("playerStats").objectReferenceValue==p,initialized=typeof(UI_SkillTree).GetField("playerSkills",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(t)!=null}).ToArray();
        var slots=p.transform.root.GetComponentsInChildren<HotKeySlot>(true).Select(s=>new{s.name,key=s.keyCode.ToString(),active=s.gameObject.activeInHierarchy,s.enabled,correctState=new SerializedObject(s).FindProperty("controls").objectReferenceValue==state,slot=s.inventorySlot.slotIndex,item=s.inventorySlot.item?.name,correctBag=s.inventorySlot.inventory==p.GetComponent<Inventory>()}).ToArray();
        File.WriteAllText("Temp/PlayerCleanup/binding-debug.json",JsonConvert.SerializeObject(new{trees,slots},Formatting.Indented));
    }
}
