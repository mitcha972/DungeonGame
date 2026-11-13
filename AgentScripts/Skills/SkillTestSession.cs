using System;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class SkillTestSession
{
 const string Key="DungeonGame.Skills.PreviousStartScene";
 public static object Prepare(){if(EditorApplication.isPlaying)throw new Exception("Preserve user Play Mode");if(!SessionState.GetBool(Key+".saved",false)){SessionState.SetString(Key,EditorSceneManager.playModeStartScene==null?"":AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));SessionState.SetBool(Key+".saved",true);}EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Test_Scenes/Gameplay_Test.unity");return new{prepared=EditorSceneManager.playModeStartScene!=null};}
 public static object Restore(){if(EditorApplication.isPlaying)throw new Exception("Stop tests first");if(SessionState.GetBool(Key+".saved",false)){var path=SessionState.GetString(Key,"");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(path)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(path);SessionState.EraseString(Key);SessionState.EraseBool(Key+".saved");}return new{restored=true};}
}
