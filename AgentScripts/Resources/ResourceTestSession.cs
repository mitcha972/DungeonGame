using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class ResourceTestSession
{
 const string Key="ResourceNodes.PreviousStartScene";
 public static object Prepare(){if(EditorApplication.isPlaying)throw new Exception("User's Play Mode must be preserved");if(!SessionState.GetBool(Key+".saved",false)){SessionState.SetString(Key,EditorSceneManager.playModeStartScene==null?"":AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));SessionState.SetBool(Key+".saved",true);}EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Test_Scenes/ResourceNodes_Test.unity");if(EditorSceneManager.playModeStartScene==null)throw new Exception("Test scene missing");return new{prepared=true,editScene=SceneManager.GetActiveScene().path,dirty=SceneManager.GetActiveScene().isDirty};}
 public static object Restore(){if(EditorApplication.isPlaying)throw new Exception("Stop test Play Mode first");if(SessionState.GetBool(Key+".saved",false)){var p=SessionState.GetString(Key,"");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(p)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(p);SessionState.EraseString(Key);SessionState.EraseBool(Key+".saved");}return new{restored=true,editScene=SceneManager.GetActiveScene().path,dirty=SceneManager.GetActiveScene().isDirty};}
}
