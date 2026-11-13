using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
public static class RepairAnimationEvents {
 public static object Run(){if(EditorApplication.isPlaying)throw new Exception("Stop first");var removed=new JArray();var obsolete=new[]{"EnableMovement","DisableMovement","EnableDamage","DisableDamage","EnableProjectile","AttackHitEvent"};foreach(var g in AssetDatabase.FindAssets("t:AnimationClip",new[]{"Assets/Animations"})){var path=AssetDatabase.GUIDToAssetPath(g);if(!path.EndsWith(".anim"))continue;var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);var events=AnimationUtility.GetAnimationEvents(clip);var keep=events.Where(e=>!obsolete.Contains(e.functionName)).ToArray();if(keep.Length==events.Length)continue;removed.Add(new JObject{["path"]=path,["removed"]=new JArray(events.Where(e=>obsolete.Contains(e.functionName)).Select(e=>e.functionName))});AnimationUtility.SetAnimationEvents(clip,keep);EditorUtility.SetDirty(clip);}AssetDatabase.SaveAssets();File.WriteAllText("Assets/Plan/Validation/animation-event-repairs.json",removed.ToString());return removed;}
}
