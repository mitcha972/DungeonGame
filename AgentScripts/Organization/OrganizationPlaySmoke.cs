using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class OrganizationPlaySmoke {
 const string Key="DungeonGame.Organization.SceneSetup";
 public static object Prepare(){
 if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("A user's Play session is active; smoke test not started");
 for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new Exception("Unsaved scene; smoke test not started");
 SessionState.SetString(Key,JsonConvert.SerializeObject(EditorSceneManager.GetSceneManagerSetup()));
 EditorSceneManager.OpenScene("Assets/Scenes/Test_Scenes/DialogueMerchant_Test.unity",OpenSceneMode.Single);
 return new{scene=SceneManager.GetActiveScene().path};
 }
 public static object Restore(){
 if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Stop owned test session first");
 var json=SessionState.GetString(Key,"");if(string.IsNullOrEmpty(json))return "No saved scene setup";
 EditorSceneManager.RestoreSceneManagerSetup(JsonConvert.DeserializeObject<SceneSetup[]>(json));SessionState.EraseString(Key);
 return new{restoredScene=SceneManager.GetActiveScene().path};
 }
}
