using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;
public static class EquipmentAnimationAssetValidation
{
 [MenuItem("Tools/Validation/Equipment Animation/Audit Authored Assets")]
 public static void Audit()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop Play Mode before the asset audit.");
  var checks=new List<object>(); var issues=new List<string>();
  Action<bool,string,object> check=(pass,name,details)=>checks.Add(new{pass,name,details});
  var sets=AssetDatabase.FindAssets("t:WeaponAttackSet").Select(g=>AssetDatabase.LoadAssetAtPath<WeaponAttackSet>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
  var controllers=AssetDatabase.FindAssets("t:AnimatorController",new[]{"Assets/Animations/Animators/Characters"}).Select(g=>AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
  var hashes=controllers.ToDictionary(c=>AssetDatabase.GetAssetPath(c),c=>Hash128.Compute(File.ReadAllText(AssetDatabase.GetAssetPath(c))).ToString());
  WeaponAnimationAuthoring.Synchronize();
  check(hashes.All(x=>Hash128.Compute(File.ReadAllText(x.Key)).ToString()==x.Value),"Synchronization is idempotent for authored controllers",hashes.Keys);
  foreach(var c in controllers.Where(c=>c.layers.Any(l=>l.name=="Override"))){
   var machine=c.layers.First(x=>x.name=="Override").stateMachine;
   var motions=sets.SelectMany(s=>c.name=="Player Animator"?s.lightAttacks:s.lightAttacks.Concat(s.heavyAttacks)).Where(m=>m!=null&&m.clip!=null).ToArray();
   check(motions.All(m=>machine.states.Any(x=>x.state.name==m.AnimatorState&&x.state.motion==m.clip)),"All selected weapon motions have their direct authored states",c.name);
   check(!machine.states.Any(x=>new[]{"LightAttack","HeavyAttack","Ability"}.Contains(x.state.name)),"No active weapon placeholder states",c.name);
   var abilityAssets=AssetDatabase.FindAssets("t:Abilities").Select(g=>AssetDatabase.LoadAssetAtPath<Abilities>(AssetDatabase.GUIDToAssetPath(g))).Where(a=>a.startAnimation!=null);
   check(abilityAssets.All(a=>c.layers.Any(l=>l.stateMachine.states.Any(x=>x.state.name==a.AnimationState&&x.state.motion==a.startAnimation))),"Ability clips have direct authored states",c.name);
  }
  check(sets.Length==7&&sets.All(s=>s.lightAttacks.Any(m=>m!=null&&m.clip!=null&&!string.IsNullOrEmpty(m.AnimatorState))),"All seven migrated attack sets have bound light motions",sets.Select(s=>AssetDatabase.GetAssetPath(s)).ToArray());
  foreach(var g in AssetDatabase.FindAssets("t:Equipment")){
   var item=AssetDatabase.LoadAssetAtPath<Equipment>(AssetDatabase.GUIDToAssetPath(g));
   if(item.equipSlot==EquipmentSlot.MainHand)check(item.weaponAttacks!=null,"Main-hand item owns its attack set",AssetDatabase.GetAssetPath(item));
  }
  foreach(var name in new[]{"PlayerHolder","Player_Main","Enemy_Main"}){
   var path=AssetDatabase.FindAssets(name+" t:Prefab").Select(AssetDatabase.GUIDToAssetPath).First(p=>Path.GetFileNameWithoutExtension(p)==name);
   var root=AssetDatabase.LoadAssetAtPath<GameObject>(path); var gear=root.GetComponentsInChildren<CharacterEquipment>(true);
   check(gear.Length==1&&gear[0].InnateAttacks!=null,"Aggregate has one gear owner and explicit innate attacks",path);
   check(root.GetComponentsInChildren<CharacterAnimation>(true).Length==1,"Aggregate has one animation owner",path);
   foreach(var t in root.GetComponentsInChildren<Transform>(true)){
    if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)issues.Add(path+" / "+t.name+" missing script");
    foreach(var component in t.GetComponents<Component>()){
     if(component==null)continue;
     var so=new SerializedObject(component); var property=so.GetIterator();
     while(property.Next(true)){
      if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue==null&&property.objectReferenceInstanceIDValue!=0)
       issues.Add(path+" / "+t.name+" / "+component.GetType().Name+"."+property.propertyPath+" missing reference");
     }
    }
   }
  }
  check(issues.Count==0,"Production player/enemy prefab references resolve",issues);
  var renamed=new[]{"CharacterEquipment","PlayerEquipment","EnemyEquipment","CharacterAnimation","WeaponAttackSet"};
  var identities=renamed.Select(n=>{var p=AssetDatabase.FindAssets(n+" t:MonoScript").Select(AssetDatabase.GUIDToAssetPath).Single(x=>Path.GetFileNameWithoutExtension(x)==n);return new{name=n,path=p,guid=AssetDatabase.AssetPathToGUID(p)};}).ToArray();
  var backups=Directory.GetFiles("Temp/EquipmentAnimation/Baseline","*.meta",SearchOption.AllDirectories).Select(File.ReadAllText).ToArray();
  check(identities.All(x=>backups.Any(b=>b.Contains("guid: "+x.guid))),"All five renamed runtime scripts retain their original GUIDs",identities);
  var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  var result=new{passed=checks.All(c=>(bool)c.GetType().GetProperty("pass").GetValue(c)),checks,scene=new{active.path,active.isDirty},playMode=EditorApplication.isPlaying,compiling=EditorApplication.isCompiling,playModeStartScene=EditorSceneManager.playModeStartScene==null?null:AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene)};
  File.WriteAllText("Assets/Plan/Validation/equipment-animation-assets.json",JsonConvert.SerializeObject(result,Formatting.Indented));
 }
}