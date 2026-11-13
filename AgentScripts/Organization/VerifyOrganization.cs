using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
public static class VerifyOrganization {
 public static object Run(){
 if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Edit Mode required");
 var m=JObject.Parse(File.ReadAllText("Assets/Plan/ARCHIVE/Organization/organization-manifest.json"));int scripts=0;
 foreach(var e in m["scriptInventory"]){string p=(string)e["new"];if(AssetDatabase.AssetPathToGUID(p)!=(string)e["guid"]||AssetDatabase.LoadAssetAtPath<MonoScript>(p)==null)throw new Exception("Unresolved script identity: "+p);scripts++;}
 var expected=new[]{"Assets/Scenes/Game/Start.unity","Assets/Scenes/Game/Loading.unity","Assets/Scenes/Game/Main.unity","Assets/Scenes/Test_Scenes/Combat_Test.unity"};
 var build=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();if(!build.SequenceEqual(expected))throw new Exception("Unexpected build scene order: "+string.Join(",",build));
 var prefabChecks=new JArray();
 foreach(var p in new[]{"Assets/Prefabs/Player/PlayerHolder.prefab","Assets/Prefabs/Player/PlayerContent.prefab","Assets/Prefabs/WorldInteract/DialogueMerchant.prefab","Assets/Prefabs/WorldInteract/Shop.prefab","Assets/Prefabs/UI/Progression/UI_SkillTree.prefab"}){
 var root=PrefabUtility.LoadPrefabContents(p);
 try {
 int missing=root.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));if(missing!=0)throw new Exception(p+" has "+missing+" missing scripts");
 if(p.Contains("PlayerHolder")){var s=root.GetComponentInChildren<PlayerControlState>(true);var d=s.GetComponent<DungeonGame.Dialogue.DialogueController>();if(s.ShopUI==null||d==null||d.panel==null||s.ShopUI.transform.parent!=d.panel.transform.parent)throw new Exception("PlayerHolder dialogue/shop reference mismatch");}
 if(p.Contains("DialogueMerchant")){var s=root.GetComponent<ShopScript>();var n=root.GetComponent<DungeonGame.Dialogue.DialogueNpc>();if(s.dialogue!=n||n.shop!=s||n.conversation==null||s.inventory==null)throw new Exception("Merchant references missing");var data=new SerializedObject(s);if(data.FindProperty("itemDrops").arraySize!=3)throw new Exception("Merchant stock changed");}
 prefabChecks.Add(new JObject{["path"]=p,["missingScripts"]=missing,["dependencies"]=AssetDatabase.GetDependencies(p,true).Length});
 }finally{PrefabUtility.UnloadPrefabContents(root);}
 }
 var sceneChecks=new JArray();foreach(var p in Directory.GetFiles("Assets/Scenes","*.unity",SearchOption.AllDirectories)){var path=p.Replace('\\','/');if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)==null)throw new Exception("Scene missing: "+path);sceneChecks.Add(new JObject{["path"]=path,["guid"]=AssetDatabase.AssetPathToGUID(path),["dependencies"]=AssetDatabase.GetDependencies(path,true).Length});}
 var report=new JObject{["passed"]=true,["scriptsResolved"]=scripts,["prefabs"]=prefabChecks,["scenes"]=sceneChecks,["buildScenes"]=new JArray(build)};
 File.WriteAllText("Assets/Plan/ARCHIVE/Organization/editor-reference-validation.json",report.ToString());return report;
 }
 public static object CleanupEmptyFolders(){
 var removed=new List<string>();foreach(var root in new[]{"Assets/Scripts/Controllers","Assets/Scripts/Dungeon","Assets/Scripts/GamePlay","Assets/Scripts/Systems","Assets/Plans","Assets/Doc"}){
 if(!Directory.Exists(root))continue;
 var dirs=Directory.GetDirectories(root,"*",SearchOption.AllDirectories).Append(root).OrderByDescending(p=>p.Length).ToArray();
 foreach(var dir in dirs){var p=dir.Replace('\\','/');if(Directory.Exists(p)&&Directory.GetFileSystemEntries(p).Length==0){if(!AssetDatabase.DeleteAsset(p))throw new Exception("Cannot remove empty folder: "+p);removed.Add(p);}}
 }return new{removedEmptyFolders=removed};
 }
}
