using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class GatheringQuestSession
{
 const string Key="GatheringQuest.PreviousStartScene";
 static object Select(string path){if(EditorApplication.isPlaying)throw new Exception("Preserve active Play Mode");if(!SessionState.GetBool(Key+".saved",false)){SessionState.SetString(Key,EditorSceneManager.playModeStartScene==null?"":AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));SessionState.SetBool(Key+".saved",true);}var scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(path);if(scene==null)throw new Exception("Missing scene: "+path);EditorSceneManager.playModeStartScene=scene;return new{prepared=true,path,editScene=SceneManager.GetActiveScene().path,dirty=SceneManager.GetActiveScene().isDirty};}
 public static object Prepare()=>Select("Assets/Scenes/Test_Scenes/GatheringQuest_Test.unity");
 public static object PrepareOriginalQuest()=>Select("Assets/Scenes/Test_Scenes/Quest_Test.unity");
 public static object PrepareMerchant()=>Select("Assets/Scenes/Test_Scenes/DialogueMerchant_Test.unity");
 public static object Restore(){if(EditorApplication.isPlaying)throw new Exception("Stop test Play Mode first");if(SessionState.GetBool(Key+".saved",false)){string p=SessionState.GetString(Key,"");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(p)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(p);SessionState.EraseString(Key);SessionState.EraseBool(Key+".saved");}return new{restored=true,scene=SceneManager.GetActiveScene().path,dirty=SceneManager.GetActiveScene().isDirty};}
}
