using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public static class MigrateEquipmentAnimation
{
 static readonly List<string> changes=new List<string>();
 static void Backup(string p){string b="Temp/EquipmentAnimation/Baseline/"+p;Directory.CreateDirectory(Path.GetDirectoryName(b));if(!File.Exists(b))File.Copy(p,b);}
 static string Key(Transform t)=>t.parent==null?t.GetSiblingIndex()+":"+t.name:Key(t.parent)+"/"+t.GetSiblingIndex()+":"+t.name;
 static AnimationClip Clip(JToken token)
 {
  if(token==null||token["name"]?.Type==JTokenType.Null)return null;
  string path=(string)token["asset"];long id=(long)token["id"];
  return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c=>{AssetDatabase.TryGetGUIDAndLocalFileIdentifier(c,out string g,out long local);return id==local;});
 }
 static AnimationClip DefaultClip(string state)
 {
  var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Animators/Characters/NPC Animator.controller");
  foreach(var layer in controller.layers){var clip=Find(layer.stateMachine,state);if(clip!=null)return clip;}return null;
 }
 static AnimationClip Find(AnimatorStateMachine m,string state)
 {
  var s=m.states.FirstOrDefault(x=>x.state.name==state).state;
  if(s!=null&&s.motion is AnimationClip clip)return clip;
  foreach(var child in m.stateMachines){var nested=Find(child.stateMachine,state);if(nested!=null)return nested;}return null;
 }
 static void Motions(SerializedObject so,string name,JToken list,AnimationClip fallback)
 {
  var clips=list.Select(Clip).Where(c=>c!=null).ToList();
  if(clips.Count==0&&fallback!=null)clips.Add(fallback);
  var p=so.FindProperty(name);p.arraySize=clips.Count;
  for(int i=0;i<clips.Count;i++)p.GetArrayElementAtIndex(i).FindPropertyRelative("clip").objectReferenceValue=clips[i];
 }
 static bool SetNatural(CharacterEquipment equipment,WeaponAttackSet natural)
 {
  if(equipment==null)return false;var s=new SerializedObject(equipment);
  if(s.FindProperty("innateAttacks").objectReferenceValue==natural)return false;
  s.FindProperty("innateAttacks").objectReferenceValue=natural;s.ApplyModifiedPropertiesWithoutUndo();return true;
 }
 static bool ClearOverrides(GameObject root)
 {
  bool changed=false;
  var removed=new[]{"defaultWeaponClass","availableWeaponClasses","replaceableLightAttackAnim","replaceableHeavyAttackAnim","replaceableAbilityAnim","overrideController","currentEquipmentArray"};
  foreach(var t in root.GetComponentsInChildren<Transform>(true))
  {
   if(!PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject))continue;
   var mods=PrefabUtility.GetPropertyModifications(t.gameObject);if(mods==null)continue;
   var keep=mods.Where(m=>!removed.Any(n=>m.propertyPath==n||m.propertyPath.StartsWith(n+"."))).ToArray();
   if(keep.Length!=mods.Length){PrefabUtility.SetPropertyModifications(t.gameObject,keep);changed=true;}
  }
  return changed;
 }
 public static void Main()
 {
  if(EditorApplication.isPlaying)throw new Exception("Edit Mode required.");
  var audit=JObject.Parse(File.ReadAllText("Temp/EquipmentAnimation/audit.json"));
  var profiles=new Dictionary<string,WeaponAttackSet>();
  var fallbackLight=DefaultClip("LightAttack");var fallbackHeavy=DefaultClip("HeavyAttack");
  foreach(var entry in audit["sets"])
  {
   string path=(string)entry["path"];Backup(path);
   var profile=AssetDatabase.LoadAssetAtPath<WeaponAttackSet>(path);profiles[path]=profile;
   var so=new SerializedObject(profile);
   bool legacy=(string)entry["type"]=="Unarmed"&&entry["light"].Count()==0;
   Motions(so,"lightAttacks",entry["light"],legacy?fallbackLight:null);
   Motions(so,"heavyAttacks",entry["heavy"],legacy?fallbackHeavy:null);
   so.ApplyModifiedPropertiesWithoutUndo();
   if(legacy)changes.Add("Preserved existing controller fallback in "+path+": "+(fallbackLight!=null?fallbackLight.name:"no authored motion"));
  }
  foreach(var guid in AssetDatabase.FindAssets("t:Equipment"))
  {
   string path=AssetDatabase.GUIDToAssetPath(guid);var item=AssetDatabase.LoadAssetAtPath<Equipment>(path);if(item.equipSlot!=EquipmentSlot.MainHand)continue;
   var key=item.itemType==ItemType.Sword?"Assets/GamePlay/Combat/Attacks/Slash.asset":item.itemType==ItemType.Bow?"Assets/GamePlay/Combat/Attacks/Arrow.asset":null;
   if(key==null)throw new Exception("Explicit weapon assignment required: "+path);
   Backup(path);item.weaponAttacks=profiles[key];EditorUtility.SetDirty(item);changes.Add("Weapon assignment: "+path+" -> "+key);
  }
  var characters=audit["characters"].OrderBy(c=>((string)c["path"]).EndsWith("Player_Main.prefab")?0:((string)c["path"]).EndsWith("Enemy_Main.prefab")?1:2);
  foreach(var entry in characters)
  {
   string path=(string)entry["path"];Backup(path);var root=PrefabUtility.LoadPrefabContents(path);bool changed=false;
   try{
    foreach(var binding in entry["animation"])
    {
     string objectPath=(string)binding["objectPath"];var t=string.IsNullOrEmpty(objectPath)?root.transform:root.transform.Find(objectPath);
     if(t==null)throw new Exception("Character path changed: "+path+"/"+objectPath);
     var animation=t.GetComponent<CharacterAnimation>();var gear=t.GetComponentInChildren<CharacterEquipment>(true);
     string natural=(string)binding["natural"];
     if(!string.IsNullOrEmpty(natural))changed|=SetNatural(gear,profiles[natural]);
    }
    changed|=ClearOverrides(root);
    if(changed){PrefabUtility.SaveAsPrefabAsset(root,path,out bool saved);if(!saved)throw new Exception("Cannot save "+path);changes.Add("Prefab migrated: "+path);}
   }finally{PrefabUtility.UnloadPrefabContents(root);}
  }
  var open=Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();var active=SceneManager.GetActiveScene();
  var scenes=JArray.Parse(File.ReadAllText("Temp/EquipmentAnimation/scenes.json"));
  foreach(var group in scenes.GroupBy(x=>(string)x["scene"]))
  {
   var existing=open.FirstOrDefault(s=>s.path==group.Key);bool wasOpen=existing.IsValid()&&existing.isLoaded;
   var scene=wasOpen?existing:EditorSceneManager.OpenScene(group.Key,OpenSceneMode.Additive);
   try{
    bool changed=false;var gear=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CharacterEquipment>(true)).ToArray();
    foreach(var binding in group)
    {
     var target=gear.FirstOrDefault(g=>Key(g.transform)==(string)binding["key"]);string path=AssetDatabase.GUIDToAssetPath((string)binding["natural"]);
     if(!string.IsNullOrEmpty(path))changed|=SetNatural(target,AssetDatabase.LoadAssetAtPath<WeaponAttackSet>(path));
    }
    foreach(var root in scene.GetRootGameObjects())changed|=ClearOverrides(root);
    if(changed){
     if(wasOpen)changes.Add("Preserved open scene unsaved after binding migration: "+group.Key);
     else{Backup(group.Key);if(!EditorSceneManager.SaveScene(scene))throw new Exception("Cannot save scene "+group.Key);changes.Add("Scene migrated: "+group.Key);}
    }
   }finally{if(!wasOpen)EditorSceneManager.CloseScene(scene,true);}
  }
  if(active.IsValid())SceneManager.SetActiveScene(active);
  if(!AssetDatabase.IsValidFolder("Assets/Combat"))AssetDatabase.CreateFolder("Assets","Combat");
  if(!AssetDatabase.IsValidFolder("Assets/Combat/WeaponAttacks"))AssetDatabase.CreateFolder("Assets/Combat","WeaponAttacks");
  foreach(var pair in profiles)
  {
   string name=Path.GetFileName(pair.Key);
   if(name=="Arrow.asset")name="Bow_Attacks.asset";else if(name=="Slash.asset")name="Sword_Attacks.asset";else if(name=="Unarmed.asset")name="Unarmed_Attacks.asset";
   string destination="Assets/Combat/WeaponAttacks/"+name;
   string error=AssetDatabase.MoveAsset(pair.Key,destination);if(!string.IsNullOrEmpty(error))throw new Exception(error);
   pair.Value.name=Path.GetFileNameWithoutExtension(name);EditorUtility.SetDirty(pair.Value);changes.Add("Attack asset moved: "+pair.Key+" -> "+destination);
  }
  foreach(var path in new[]{"Assets/Animations/Animators/Characters/Player Animator.controller","Assets/Animations/Animators/Characters/NPC Animator.controller"})Backup(path);
  AssetDatabase.SaveAssets();WeaponAnimationAuthoring.Synchronize();
  Directory.CreateDirectory("Assets/Plan/Validation");
  File.WriteAllText("Assets/Plan/Validation/equipment-animation-migration.json",JsonConvert.SerializeObject(changes,Formatting.Indented));
 }
}