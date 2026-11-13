using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
public static class InspectResources {
 public static object Run(){
 var scene=SceneManager.GetActiveScene();var roots=scene.GetRootGameObjects();var pc=roots.SelectMany(x=>x.GetComponentsInChildren<PlayerController>(true)).First();
 var a=pc.GetComponentInChildren<Animator>(true);
 return new{player=pc.name,pos=pc.transform.position.ToString(),controller=AssetDatabase.GetAssetPath(a.runtimeAnimatorController),avatar=AssetDatabase.GetAssetPath(a.avatar),prompt=pc.InteractionText==null?"missing":pc.InteractionText.name,canvas=pc.transform.root.GetComponentInChildren<Canvas>(true).name,
 controllerClips=a.runtimeAnimatorController.animationClips.Select(x=>x.name).ToArray(),
 clips=new[]{"Assets/Animations/Misc/Gathering.anim","Assets/Animations/Misc/MiningLoop.anim"}.Select(p=>{var c=AssetDatabase.LoadAssetAtPath<AnimationClip>(p);return new{p,c.length,c.isHumanMotion,curves=AnimationUtility.GetCurveBindings(c).Length};}).ToArray(),
 nodes=roots.SelectMany(x=>x.GetComponentsInChildren<ResourceNode>(true)).Select(x=>new{x.name,path=AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(x)),root=x.transform.root.name}).ToArray(),
 bounds=roots.SelectMany(x=>x.GetComponentsInChildren<Collider>(true)).Where(x=>x.bounds.size.x>10).Select(x=>new{x.name,center=x.bounds.center.ToString(),size=x.bounds.size.ToString()}).ToArray()}; }
}
