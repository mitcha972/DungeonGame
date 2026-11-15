using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class FinishAssets
{
    public static void Main()
    {
        if (EditorApplication.isPlaying) throw new Exception("Stop only task-owned Play Mode before asset migration.");
        // These parent assets only inherited changed NPCs. Restore our incidental reserialization.
        foreach (var path in new[] { "Assets/Prefabs/Dungeons/Rooms/D1.prefab", "Assets/Prefabs/Dungeons/Rooms/D3.prefab", "Assets/Prefabs/Dungeons/Rooms/DEnd.prefab", "Assets/Prefabs/Dungeons/Rooms/DHall2.prefab", "Assets/Prefabs/NPC/NPC_Main.prefab" })
        {
            File.Copy("Temp/PlayerCleanup/Baseline/" + path, path, true);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        var active = SceneManager.GetActiveScene();
        string demoPath = "Assets/Scenes/Test_Scenes/SkillTree_Test.unity";
        string backup = "Temp/PlayerCleanup/Baseline/" + demoPath;
        Directory.CreateDirectory(Path.GetDirectoryName(backup));
        if (!File.Exists(backup)) File.Copy(demoPath, backup);
        var demo = EditorSceneManager.OpenScene(demoPath, OpenSceneMode.Additive);
        try
        {
            foreach (var p in demo.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Player>(true))) UnityEngine.Object.DestroyImmediate(p, true);
            EditorSceneManager.SaveScene(demo);
        }
        finally { EditorSceneManager.CloseScene(demo, true); SceneManager.SetActiveScene(active); }
        string controllerPath = "Assets/Animations/Animators/Characters/Player Animator.controller";
        backup = "Temp/PlayerCleanup/Baseline/" + controllerPath;
        Directory.CreateDirectory(Path.GetDirectoryName(backup));
        if (!File.Exists(backup)) File.Copy(controllerPath, backup);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        foreach (var layer in controller.layers) Clean(layer.stateMachine);
        controller.parameters = controller.parameters.Where(p=>!Obsolete(p.name)).ToArray();
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(controllerPath))
        {
            if (asset is AnimatorState state && Obsolete(state.name)) UnityEngine.Object.DestroyImmediate(state, true);
            else if (asset is AnimatorStateTransition t && t.conditions.Any(c=>Obsolete(c.parameter))) UnityEngine.Object.DestroyImmediate(t, true);
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Removed player jump/heavy states and retained falling/landing transitions. NPC controller unchanged.");
    }
    private static bool Obsolete(string name) => name.IndexOf("Jump", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Slide", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Heavy", StringComparison.OrdinalIgnoreCase) >= 0;
    private static void Clean(AnimatorStateMachine sm)
    {
        var removed = sm.states.Where(s=>Obsolete(s.state.name)).Select(s=>s.state).ToArray();
        foreach (var state in sm.states.Select(s=>s.state))
            foreach (var t in state.transitions.Where(t=>removed.Contains(t.destinationState) || t.conditions.Any(c=>Obsolete(c.parameter))).ToArray()) state.RemoveTransition(t);
        foreach (var t in sm.anyStateTransitions.Where(t=>removed.Contains(t.destinationState) || t.conditions.Any(c=>Obsolete(c.parameter))).ToArray()) sm.RemoveAnyStateTransition(t);
        foreach (var state in removed) sm.RemoveState(state);
        var empty = sm.states.FirstOrDefault(s=>s.state.name=="Empty").state;
        var air = sm.states.FirstOrDefault(s=>s.state.name=="InAir").state;
        if (empty != null && air != null)
        {
            var fall = empty.AddTransition(air); fall.hasExitTime=false; fall.duration=.1f; fall.AddCondition(AnimatorConditionMode.If,0,"FreeFall");
            var land = air.AddTransition(empty); land.hasExitTime=false; land.duration=.1f; land.AddCondition(AnimatorConditionMode.If,0,"Grounded");
        }
        foreach (var sub in sm.stateMachines) Clean(sub.stateMachine);
    }
}
