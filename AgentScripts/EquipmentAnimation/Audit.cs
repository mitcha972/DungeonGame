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
public static class EquipmentAnimationAudit
{
 public static void Main()
 {
  Directory.CreateDirectory("Temp/EquipmentAnimation");
  var prefabs=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
  var characters=new List<object>();
  foreach(var path in prefabs)
  {
   var p=AssetDatabase.LoadAssetAtPath<GameObject>(path);var presentations=p.GetComponentsInChildren<CharacterAnimator>(true);var gear=p.GetComponentsInChildren<EquipmentManager>(true);
   if(presentations.Length==0&&gear.Length==0)continue;
   characters.Add(new {path,animation=presentations.Select(a=>new {objectPath=AnimationUtility.CalculateTransformPath(a.transform,p.transform),localId=Id(a),natural=AssetDatabase.GetAssetPath(a.defaultWeaponClass),available=(a.availableWeaponClasses??Array.Empty<WeaponClass>()).Select(AssetDatabase.GetAssetPath),light=AssetDatabase.GetAssetPath(a.replaceableLightAttackAnim),heavy=AssetDatabase.GetAssetPath(a.replaceableHeavyAttackAnim),ability=AssetDatabase.GetAssetPath(a.replaceableAbilityAnim),animators=a.GetComponentsInChildren<Animator>(true).Select(x=>new {controller=AssetDatabase.GetAssetPath(x.runtimeAnimatorController),layers=Enumerable.Range(0,x.layerCount).Select(x.GetLayerName)})}),equipment=gear.Select(e=>new{objectPath=AnimationUtility.CalculateTransformPath(e.transform,p.transform),type=e.GetType().Name,localId=Id(e),mesh=AssetDatabase.GetAssetPath(e.targetMesh),slots=e is PlayerEquipmentManager player?(player.equipmentSlotsUI??Array.Empty<EquipmentSlotUI>()).Length:0})});
  }
  var sets=AssetDatabase.FindAssets("t:WeaponClass").Select(AssetDatabase.GUIDToAssetPath).Select(path=>{var w=AssetDatabase.LoadAssetAtPath<WeaponClass>(path);return new{path,w.itemType,type=w.itemType.ToString(),w.isRanged,w.hitStart,w.hitEnd,w.attackDuration,w.cooldown,w.heavyDamageMultiplier,projectile=AssetDatabase.GetAssetPath(w.projectile),w.projectileSpeed,w.projectileRange,light=(w.lightAttackClips??Array.Empty<AnimationClip>()).Select(c=>new{asset=AssetDatabase.GetAssetPath(c),name=c!=null?c.name:null,id=Id(c)}),heavy=(w.heavyAttackClips??Array.Empty<AnimationClip>()).Select(c=>new{asset=AssetDatabase.GetAssetPath(c),name=c!=null?c.name:null,id=Id(c)})};}).ToArray();
  var items=AssetDatabase.FindAssets("t:Equipment").Select(AssetDatabase.GUIDToAssetPath).Select(path=>{var e=AssetDatabase.LoadAssetAtPath<Equipment>(path);return new{path,e.name,type=e.itemType.ToString(),slot=e.equipSlot.ToString(),mesh=AssetDatabase.GetAssetPath(e.mesh)};}).ToArray();
  var scene=SceneManager.GetActiveScene();var stage=PrefabStageUtility.GetCurrentPrefabStage();
  if(scene.isDirty)EditorSceneManager.SaveScene(scene,"Temp/EquipmentAnimation/initial-unsaved.unity",true);
  File.WriteAllText("Temp/EquipmentAnimation/audit.json",JsonConvert.SerializeObject(new{playing=EditorApplication.isPlaying,scene=scene.path,dirty=scene.isDirty,stage=stage!=null?stage.assetPath:null,stageDirty=stage!=null&&stage.scene.isDirty,characters,sets,items},Formatting.Indented));
 }
 private static long Id(UnityEngine.Object o){if(o==null)return 0;AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o,out string guid,out long id);return id;}
}

