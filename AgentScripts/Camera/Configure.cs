using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.Animations;using UnityEditor.SceneManagement;using Unity.Cinemachine;using Newtonsoft.Json;
public static class ConfigureMouseFacingCamera {
 static List<string> changes=new List<string>();
 static void Bind(GameObject root){
 var m=root.GetComponentInChildren<StarterAssets.ThirdPersonController>(true);if(m==null)return;
 var stable=m.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="StablePlayerViewTarget");
 if(m.CinemachineCameraTarget!=stable.gameObject){stable.rotation=m.CinemachineCameraTarget.transform.rotation;m.CinemachineCameraTarget=stable.gameObject;EditorUtility.SetDirty(m);EditorUtility.SetDirty(stable);}
 foreach(var go in new[]{m.tpCamera,m.fpCamera})if(go!=null){var c=go.GetComponent<CinemachineCamera>();c.Follow=stable;EditorUtility.SetDirty(c);PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
 foreach(var c in root.GetComponentsInChildren<CinemachineCamera>(true))if(c.name=="ThirdPersonFollowCamera"&&c.gameObject!=m.tpCamera&&c.gameObject!=m.fpCamera){c.enabled=false;EditorUtility.SetDirty(c);PrefabUtility.RecordPrefabInstancePropertyModifications(c);}
 PrefabUtility.RecordPrefabInstancePropertyModifications(m);PrefabUtility.RecordPrefabInstancePropertyModifications(stable);changes.Add(root.name+": shared view target="+stable.name);
 }
 static AnimationClip Clip(string p,string n)=>AssetDatabase.LoadAllAssetsAtPath(p).OfType<AnimationClip>().First(c=>c.name==n);
 static BlendTree Dir(AnimatorController c,string n,Motion f,Motion b,Motion l,Motion r,float rate){
 var t=new BlendTree{name=n,blendType=BlendTreeType.SimpleDirectional2D,blendParameter="MoveX",blendParameterY="MoveY",useAutomaticThresholds=false};
 t.children=new[]{new ChildMotion{motion=f,position=Vector2.up,timeScale=1},new ChildMotion{motion=b,position=Vector2.down,timeScale=rate},new ChildMotion{motion=l,position=Vector2.left,timeScale=rate},new ChildMotion{motion=r,position=Vector2.right,timeScale=rate}};AssetDatabase.AddObjectToAsset(t,c);return t;
 }
 public static object Run(){
 if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new Exception("Stopped compiled Editor required.");
 changes.Clear();Directory.CreateDirectory("AgentScripts/Camera/Recovery");
 foreach(var p in new[]{"Assets/Prefabs/Player/Player_Main.prefab","Assets/Prefabs/Player/PlayerHolder.prefab","Assets/Animations/Animators/Characters/Player Animator.controller"}){var b="AgentScripts/Camera/Recovery/"+Path.GetFileName(p)+".before";if(!File.Exists(b))File.Copy(p,b,false);}
 foreach(var p in new[]{"Assets/Prefabs/Player/Player_Main.prefab","Assets/Prefabs/Player/PlayerHolder.prefab"}){var r=PrefabUtility.LoadPrefabContents(p);try{Bind(r);PrefabUtility.SaveAsPrefabAsset(r,p);}finally{PrefabUtility.UnloadPrefabContents(r);}}
 var c=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Animators/Characters/Player Animator.controller");
 foreach(var n in new[]{"MoveX","MoveY"})if(!c.parameters.Any(p=>p.name==n))c.AddParameter(n,AnimatorControllerParameterType.Float);
 var t=c.layers[0].stateMachine.states.Select(s=>s.state.motion).OfType<BlendTree>().First();var a=t.children;
 if(!a.Any(x=>x.motion!=null&&x.motion.name=="Walk directions")){
 if(a.Length!=3)throw new Exception("Unexpected locomotion tree.");
 var l=Clip("Assets/Animations/Humanoid/Pack/Movement/RunLeft.fbx","RunLeft");var r=Clip("Assets/Animations/Humanoid/Pack/Movement/RunRight.fbx","RunRight");var b=Clip("Assets/Animations/Humanoid/Locomotion/RunBackward.anim","RunBackward");
 a[1].motion=Dir(c,"Walk directions",a[1].motion,b,l,r,.6f);a[2].motion=Dir(c,"Run directions",a[2].motion,b,l,r,1);t.children=a;EditorUtility.SetDirty(t);EditorUtility.SetDirty(c);AssetDatabase.SaveAssetIfDirty(c);
 }
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();foreach(var r in scene.GetRootGameObjects())if(r.GetComponentInChildren<StarterAssets.ThirdPersonController>(true)!=null)Bind(r);EditorSceneManager.MarkSceneDirty(scene);
 var result=new{changes,scene=new{scene.path,scene.isDirty},savedPrefabs=true,sceneSaved=false};File.WriteAllText("Assets/Plan/Validation/camera-authoring.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
}