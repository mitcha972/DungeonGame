using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json.Linq;
public static class AnimationHandoff {
 public static object RestoreScene(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Stop test session first");var s=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(s.path!="Assets/Scenes/Test_Scenes/Combat_Test.unity")throw new Exception("Unexpected editing scene; preserve it");EditorSceneManager.OpenScene(s.path,OpenSceneMode.Single);AssetDatabase.Refresh();var now=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var r=new JObject{["scene"]=now.path,["dirty"]=now.isDirty,["playing"]=EditorApplication.isPlaying,["playModeStartScene"]=EditorSceneManager.playModeStartScene==null?null:AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene),["userApprovedReload"]=true};File.WriteAllText("Assets/Plan/Validation/animation-handoff.json",r.ToString());return r;}
}
