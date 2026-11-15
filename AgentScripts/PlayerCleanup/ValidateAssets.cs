using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
public static class ValidateCharacterAssets
{
    public static void Main()
    {
        var checks = new List<string>();
        Action<bool,string> check = (ok,label) => { if (!ok) throw new Exception(label); checks.Add(label); };
        var root = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/PlayerHolder.prefab");
        var player = root.GetComponentInChildren<PlayerStats>(true);
        check(player != null && AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(player)).EndsWith("Player_Main.prefab"), "Holder contains the user's nested Player_Main");
        var state = player.GetComponent<PlayerControlState>();
        var movement = player.GetComponent<StarterAssets.ThirdPersonController>();
        check(player.GetComponents<StarterAssets.ThirdPersonController>().Length == 1 && player.GetComponents<CharacterController>().Length == 1, "Exactly one motor and one CharacterController");
        check(player.GetComponent<CharacterAnimator>() != null && player.GetComponentInChildren<Animator>(true) != null, "Shared character animation retained");
        check(movement.tpCamera != null && movement.fpCamera != null && movement.CinemachineCameraTarget != null, "Holder supplies both view modes and orbit target");
        var so = new SerializedObject(state);
        foreach (var field in new[]{"_inventoryUI","_pauseMenu","_shopUI","_dialogue","_questJournal","_map"}) check(so.FindProperty(field).objectReferenceValue != null,"State wiring: "+field);
        var quickSlots=new SerializedObject(root.GetComponentInChildren<HotKeysUI>(true)).FindProperty("quickSlots");
        check(quickSlots.arraySize==6 && Enumerable.Range(0,6).All(i=>quickSlots.GetArrayElementAtIndex(i).objectReferenceValue!=null),"All six quick slots retain their inventory bindings");
        var actions=player.GetComponent<PlayerInput>();
        check(actions != null && actions.notificationBehavior==PlayerNotifications.InvokeCSharpEvents,"PlayerInput uses a single C# input subscriber");
        check(actions.actions.FindAction("Jump")==null && actions.actions.FindAction("Slide")==null && actions.actions.FindAction("HeavyAttack")==null,"Obsolete actions absent");
        foreach(var action in new[]{"Move","Look","Sprint","ToggleCamera","Attack","Block","Interact","Inventory","Pause","Journal","Map","Grab","Unequip","QuickSlot1","QuickSlot6"}) check(actions.actions.FindAction(action)!=null,"Binding retained: "+action);
        var prefabs=new[]{"Assets/Prefabs/Player/PlayerHolder.prefab","Assets/Prefabs/Player/Player_Main.prefab","Assets/Prefabs/Enemies/Enemy_Main.prefab","Assets/Prefabs/NPC/NPC_Main.prefab","Assets/Prefabs/NPC/Quartermaster_Nessa.prefab"};
        var descriptions=new List<object>();
        foreach(var path in prefabs)
        {
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach(var t in asset.GetComponentsInChildren<Transform>(true)) check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0,"No missing scripts: "+path+"/"+t.name);
            descriptions.Add(new { path, objects=asset.GetComponentsInChildren<Transform>(true).Select(t=>new { path=AnimationUtility.CalculateTransformPath(t,asset.transform), components=t.GetComponents<Component>().Select(c=>c.GetType().FullName).ToArray() }) });
        }
        var enemy=AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[2]);
        check(enemy.GetComponent<CharacterAnimator>()!=null && enemy.GetComponent<EnemyAi>()!=null && enemy.GetComponent<EnemyCombat>()!=null,"Enemy keeps shared presentation and its own AI/combat");
        var npc=AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[3]);
        check(npc.GetComponent<CharacterAnimator>()!=null && !npc.GetComponent<EnemyAi>().enabled,"Friendly NPC retains shared presentation with hostile AI disabled");
        var controller=AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/Animations/Animators/Characters/Player Animator.controller");
        check(!controller.parameters.Any(p=>p.name=="Jump"),"Player animator has no jump parameter");
        check(!AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(controller)).OfType<UnityEditor.Animations.AnimatorState>().Any(s=>s.name.Contains("Jump")||s.name.Contains("Heavy")||s.name.Contains("Slide")),"Player animator has no jump/heavy/slide states");
        var original=JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(File.ReadAllText("Temp/PlayerCleanup/discovery.json"));
        var before=original["prefabs"].First(p=>(string)p["path"]==prefabs[0]);
        var oldScripts=before["objects"].SelectMany(o=>o["components"]).Select(c=>(string)c["script"]).Where(p=>p!=null&&p.StartsWith("Assets/Scripts/")).Distinct().ToArray();
        var nowScripts=root.GetComponentsInChildren<MonoBehaviour>(true).Select(m=>AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(m))).Where(p=>p.StartsWith("Assets/Scripts/")).Distinct().ToArray();
        File.WriteAllText("Temp/PlayerCleanup/assets.json",JsonConvert.SerializeObject(new { passed=true, checks, beforeCount=oldScripts.Length, afterCount=nowScripts.Length, originalScripts=oldScripts, finalScripts=nowScripts, prefabs=descriptions },Formatting.Indented));
        Debug.Log("Character asset validation passed: "+checks.Count+" checks; custom player-hierarchy script types "+oldScripts.Length+" -> "+nowScripts.Length);
    }
}
