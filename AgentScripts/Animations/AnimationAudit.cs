using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Newtonsoft.Json.Linq;
public static class AnimationAudit {
 static string P(UnityEngine.Object o)=>o==null?null:AssetDatabase.GetAssetPath(o);
 static JArray States(AnimatorStateMachine sm){var a=new JArray();foreach(var s in sm.states)a.Add(new JObject{["name"]=s.state.name,["motion"]=P(s.state.motion),["motionName"]=s.state.motion==null?null:s.state.motion.name,["speed"]=s.state.speed,["transitions"]=new JArray(s.state.transitions.Select(t=>new JObject{["to"]=t.destinationState==null?"EXIT":t.destinationState.name,["exit"]=t.hasExitTime,["conditions"]=new JArray(t.conditions.Select(c=>c.parameter+" "+c.mode+" "+c.threshold))}))});foreach(var child in sm.stateMachines)a.Merge(States(child.stateMachine));return a;}
 public static object Run(){var r=new JObject();r["controllers"]=new JArray(AssetDatabase.FindAssets("t:AnimatorController",new[]{"Assets/Animations"}).Select(g=>{var p=AssetDatabase.GUIDToAssetPath(g);var c=AssetDatabase.LoadAssetAtPath<AnimatorController>(p);return new JObject{["path"]=p,["layers"]=new JArray(c.layers.Select(l=>new JObject{["name"]=l.name,["weight"]=l.defaultWeight,["mask"]=P(l.avatarMask),["states"]=States(l.stateMachine)}))};}));
 r["weapons"]=new JArray(AssetDatabase.FindAssets("t:WeaponClass").Select(g=>{var w=AssetDatabase.LoadAssetAtPath<WeaponClass>(AssetDatabase.GUIDToAssetPath(g));return new JObject{["path"]=P(w),["type"]=w.itemType.ToString(),["light"]=new JArray((w.lightAttackClips??new AnimationClip[0]).Select(P)),["heavy"]=new JArray((w.heavyAttackClips??new AnimationClip[0]).Select(P))};}));
 r["abilities"]=new JArray(AssetDatabase.FindAssets("t:Abilities").Select(g=>{var w=AssetDatabase.LoadAssetAtPath<Abilities>(AssetDatabase.GUIDToAssetPath(g));return new JObject{["path"]=P(w),["start"]=P(w.startAnimation),["cast"]=P(w.castAnimation)};}));
 r["events"]=new JArray(AssetDatabase.FindAssets("t:AnimationClip",new[]{"Assets/Animations"}).SelectMany(g=>AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(g)).OfType<AnimationClip>()).Distinct().Where(c=>c.events.Length>0).Select(c=>new JObject{["path"]=P(c),["clip"]=c.name,["human"]=c.humanMotion,["events"]=new JArray(c.events.Select(e=>e.functionName))}));
 r["models"]=new JArray(AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/Prefabs/")&&p.EndsWith(".prefab")).Select(p=>AssetDatabase.LoadAssetAtPath<GameObject>(p)).Where(go=>go!=null&&go.GetComponentsInChildren<Animator>(true).Length>0).Select(go=>new JObject{["path"]=P(go),["animators"]=new JArray(go.GetComponentsInChildren<Animator>(true).Select(a=>new JObject{["name"]=a.name,["controller"]=P(a.runtimeAnimatorController),["avatar"]=P(a.avatar),["valid"]=a.avatar!=null&&a.avatar.isValid,["human"]=a.isHuman,["rootMotion"]=a.applyRootMotion,["receivers"]=new JArray(a.GetComponents<MonoBehaviour>().Where(x=>x!=null).Select(x=>x.GetType().Name))}))}));
 File.WriteAllText("Assets/Plan/Validation/animation-audit.json",r.ToString());return new{controllers=((JArray)r["controllers"]).Count,weapons=r["weapons"],abilities=r["abilities"],events=r["events"],models=r["models"]};}
 public static object Prepare(string scene){if(EditorApplication.isPlaying)throw new Exception("User playing");const string key="AnimationAudit.start";if(!SessionState.GetBool(key+"saved",false)){SessionState.SetString(key,P(UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene)??"");SessionState.SetBool(key+"saved",true);}UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Test_Scenes/"+scene+".unity");return scene;}
 public static object Restore(){var p=SessionState.GetString("AnimationAudit.start","");UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(p)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(p);SessionState.EraseBool("AnimationAudit.startsaved");return true;}
}


