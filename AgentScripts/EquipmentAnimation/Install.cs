using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class InstallEquipmentAnimation
{
 private static string Key(Transform t){return t.parent==null?t.GetSiblingIndex()+":"+t.name:Key(t.parent)+"/"+t.GetSiblingIndex()+":"+t.name;}
 private static void Backup(string p){string b="Temp/EquipmentAnimation/Baseline/"+p;Directory.CreateDirectory(Path.GetDirectoryName(b));if(!File.Exists(b))File.Copy(p,b);}
 public static void Main()
 {
  if(EditorApplication.isPlaying)throw new Exception("Installation requires Edit Mode.");
  var sources=JsonConvert.DeserializeObject<Source[]>(File.ReadAllText("Temp/EquipmentAnimation/sources.json"));
  var bindings=new List<object>();var open=Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
  foreach(var path in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath))
  {
   var existing=open.FirstOrDefault(s=>s.path==path);bool loaded=existing.IsValid()&&existing.isLoaded;
   var scene=loaded?existing:EditorSceneManager.OpenPreviewScene(path);
   try{foreach(var a in scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CharacterAnimator>(true))){
    var gear=a.GetComponentInChildren<EquipmentManager>(true);
    if(gear!=null)bindings.Add(new{scene=path,key=Key(gear.transform),natural=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(a.defaultWeaponClass))});
   }}finally{if(!loaded)EditorSceneManager.ClosePreviewScene(scene);}
  }
  File.WriteAllText("Temp/EquipmentAnimation/scenes.json",JsonConvert.SerializeObject(bindings,Formatting.Indented));
  // Consolidate the empty menu-preview specialization without changing its component identity.
  var current=SceneManager.GetActiveScene();
  var menu=SceneManager.GetSceneByPath("Assets/Scenes/Game/Start.unity");
  bool menuWasOpen=menu.IsValid()&&menu.isLoaded;
  if(menuWasOpen&&menu.isDirty)throw new Exception("Preserve unsaved menu before migration.");
  if(!menuWasOpen)menu=EditorSceneManager.OpenScene("Assets/Scenes/Game/Start.unity",OpenSceneMode.Additive);
  try{
   bool changed=false;
   foreach(var component in menu.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<EquipmentManagerUI>(true)).ToArray()){
    Backup(menu.path);
    var serialized=new SerializedObject(component);
    serialized.FindProperty("m_Script").objectReferenceValue=AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Inventory/Equipment/EquipmentManager.cs");
    serialized.ApplyModifiedPropertiesWithoutUndo();changed=true;
   }
   if(changed&&!EditorSceneManager.SaveScene(menu))throw new Exception("Cannot save menu equipment migration.");
  }finally{if(!menuWasOpen)EditorSceneManager.CloseScene(menu,true);if(current.IsValid())SceneManager.SetActiveScene(current);}
  var unused=new[]{"Assets/Scripts/Inventory/Equipment/EquipmentManagerUI.cs","Assets/Scripts/Inventory/Equipment/EquipmentManager1.cs"};
  var consumers=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"}).Concat(AssetDatabase.FindAssets("t:Scene",new[]{"Assets"})).Select(AssetDatabase.GUIDToAssetPath).Where(p=>AssetDatabase.GetDependencies(p,true).Intersect(unused).Any()).ToArray();
  if(consumers.Length>0)throw new Exception("Shells referenced: "+string.Join(",",consumers));
  foreach(var s in sources){Backup(s.old);Backup(s.old+".meta");}
  foreach(var p in unused){Backup(p);Backup(p+".meta");}
  AssetDatabase.StartAssetEditing();
  try{
   foreach(var p in unused)if(!AssetDatabase.DeleteAsset(p))throw new Exception(p);
   foreach(var s in sources){
    if(s.old!=s.@new){string e=AssetDatabase.MoveAsset(s.old,s.@new);if(!string.IsNullOrEmpty(e))throw new Exception(e);}
    File.WriteAllText(s.@new,File.ReadAllText("Temp/EquipmentAnimation/Staged/"+s.@new));
   }
  }finally{AssetDatabase.StopAssetEditing();}
  AssetDatabase.Refresh();
  Debug.Log("Equipment/animation source installation completed.");
 }
 private sealed class Source{public string old;public string @new;}
}