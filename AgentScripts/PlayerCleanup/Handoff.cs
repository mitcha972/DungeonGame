using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class CharacterCleanupHandoff
{
    public static void Main()
    {
        AssetDatabase.Refresh();
        var stage=PrefabStageUtility.GetCurrentPrefabStage();
        var scenes=Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).Select(s=>new{s.path,s.isDirty,s.isLoaded}).ToArray();
        var result=new { playing=EditorApplication.isPlaying,compiling=EditorApplication.isCompiling,compilationFailed=EditorUtility.scriptCompilationFailed,scenes,prefabStage=stage!=null?stage.assetPath:null,prefabStageDirty=stage!=null&&stage.scene.isDirty,selection=Selection.activeObject!=null?AssetDatabase.GetAssetPath(Selection.activeObject):null,playModeStartScene=EditorSceneManager.playModeStartScene!=null?AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene):null };
        Directory.CreateDirectory("Assets/Plan/Validation");File.WriteAllText("Assets/Plan/Validation/player-cleanup-handoff.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        AssetDatabase.ImportAsset("Assets/Plan/Validation/player-cleanup-handoff.json");
    }
}
