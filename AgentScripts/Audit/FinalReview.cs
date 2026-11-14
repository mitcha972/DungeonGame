using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public static class FinalReview
{
    static string Hash(string path){using(var algorithm=SHA256.Create())return BitConverter.ToString(algorithm.ComputeHash(File.ReadAllBytes(path))).Replace("-","");}
    public static object Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Stopped Editor required");
        const string prefab="Assets/Prefabs/Dungeons/Rooms/DEnd.prefab";
        Func<string,string[]> ids=p=>Regex.Matches(File.ReadAllText(p),@"(?m)^--- !u!\d+ &(-?\d+)").Cast<Match>().Select(m=>m.Groups[1].Value).ToArray();
        var before=ids("Temp/AuditRestore/DEnd.prefab");var after=ids(prefab);
        var missing=before.Except(after).ToArray();
        var prefabs=new[]{prefab,"Assets/Prefabs/Player/PlayerHolder.prefab","Assets/Prefabs/Enemies/Enemy_Main.prefab"}.Select(p=>{
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(p);
            return new {path=p,missingScripts=root.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject))};
        }).ToArray();
        var rootEnd=AssetDatabase.LoadAssetAtPath<GameObject>(prefab);var exit=rootEnd.GetComponentInChildren<DungeonExit>(true);
        string meshPath="Assets/Models/Building/Tiles/Dungeon_Big_col.fbx";
        bool readable=((ModelImporter)AssetImporter.GetAtPath(meshPath)).isReadable&&AssetDatabase.LoadAllAssetsAtPath(meshPath).OfType<Mesh>().All(m=>m.isReadable);
        bool scenePreserved=Hash("Temp/AuditRestore/preexisting.patch")==Hash("Temp/AuditRestore/final-preexisting.patch");
        var scene=SceneManager.GetActiveScene();
        var enabledScenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        bool buildSequence=new[]{"Assets/Scenes/Game/Start.unity","Assets/Scenes/Game/Loading.unity","Assets/Scenes/Game/Main.unity"}.SequenceEqual(enabledScenes.Take(3));
        bool identities=AssetDatabase.AssetPathToGUID(prefab)=="8bc49777625f54b4c88577dc68a171f5"&&AssetDatabase.AssetPathToGUID(meshPath)=="77adebcf3b6bcd94894ae188ae3a6b24";
        var report=new {passed=missing.Length==0&&prefabs.All(p=>p.missingScripts==0)&&readable&&scenePreserved&&buildSequence&&identities&&exit!=null&&!scene.isDirty&&scene.path=="Assets/Scenes/Test_Scenes/Combat_Test.unity",
            completedUtc=DateTime.UtcNow.ToString("o"),originalPrefabIds=before.Length,currentPrefabIds=after.Length,missingOriginalIds=missing,prefabs,readable,sceneDiffPreserved=scenePreserved,sceneDiffHash=Hash("Temp/AuditRestore/final-preexisting.patch"),identities,buildSequence,enabledScenes,
            scene=scene.path,scene.isDirty,playMode=EditorApplication.isPlaying,startSceneOverride=EditorSceneManager.playModeStartScene==null?null:AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene),windowsBuildSupportAvailable=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64),standaloneBuildExecuted=false};
        File.WriteAllText("Assets/Plan/Validation/audit-final-review.json",JsonConvert.SerializeObject(report,Formatting.Indented));return report;
    }
}
