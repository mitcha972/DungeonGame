using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
public static class DeleteObsolete
{
    public static void Main()
    {
        if (EditorApplication.isPlaying) throw new Exception("Cannot change assets during Play Mode.");
        var paths = new[] {
            "Assets/Scripts/StarterAssetsInputs.cs", "Assets/Scripts/Combat/Animation/CharacterAnimationEventReceiver.cs",
            "Assets/Scripts/NPC/Enemies/EnemyAnimator.cs", "Assets/Scripts/Player/PlayerAim.cs", "Assets/Scripts/Player/PlayerAnimator.cs",
            "Assets/Scripts/Player/PlayerCameraController.cs", "Assets/Scripts/Player/Legacy/FPC.cs", "Assets/Scripts/Progression/Skills/Player.cs",
            "Assets/Scripts/Progression/Skills/PlayerSword.cs", "Assets/Scripts/Progression/Skills/Testing.cs", "Assets/Scripts/World/Grabbing/PlayerPickUpDrop.cs" };
        var assets = AssetDatabase.FindAssets("t:Prefab", new[]{"Assets"}).Concat(AssetDatabase.FindAssets("t:Scene", new[]{"Assets"})).Select(AssetDatabase.GUIDToAssetPath).Distinct().ToArray();
        var references = assets.SelectMany(p=>AssetDatabase.GetDependencies(p,true).Intersect(paths).Select(s=>p+" -> "+s)).ToArray();
        if (references.Length > 0) throw new Exception("Still referenced: " + string.Join("; ", references));
        var deleted = paths.Select(p=>new { path=p, guid=AssetDatabase.AssetPathToGUID(p) }).ToArray();
        File.WriteAllText("Temp/PlayerCleanup/deleted.json", JsonConvert.SerializeObject(deleted, Formatting.Indented));
        foreach (var path in paths) if (!AssetDatabase.DeleteAsset(path)) throw new Exception("Delete failed: " + path);
        string error = AssetDatabase.MoveAsset("Assets/Scripts/ThirdPersonController.cs", "Assets/Scripts/Player/ThirdPersonController.cs");
        if (!string.IsNullOrEmpty(error)) throw new Exception(error);
        Debug.Log("Deleted eleven unreferenced scripts and preserved the movement script GUID while moving it into Player.");
    }
}
