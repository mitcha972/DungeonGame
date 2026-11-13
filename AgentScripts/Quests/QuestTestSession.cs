using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class QuestTestSession
{
 const string Key="DungeonGame.QuestTest.PreviousStartScene";
 public static object Prepare()=>PrepareScene("Assets/Scenes/Test_Scenes/Quest_Test.unity");
 public static object PrepareMerchant()=>PrepareScene("Assets/Scenes/Test_Scenes/DialogueMerchant_Test.unity");
 private static object PrepareScene(string path)
 {
 if(EditorApplication.isPlaying)throw new Exception("Preserve active Play Mode");
 SessionState.SetString(Key,EditorSceneManager.playModeStartScene!=null?AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene):"");
 EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
 return new{prepared=true,startScene=path,editSceneUnchanged=SceneManager.GetActiveScene().path,dirty=SceneManager.GetActiveScene().isDirty};
 }
 public static object Restore()
 {
 if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first");
 var previous=SessionState.GetString(Key,"");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(previous)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);SessionState.EraseString(Key);
 return new{restored=true,scene=SceneManager.GetActiveScene().path,dirty=SceneManager.GetActiveScene().isDirty};
 }
}
