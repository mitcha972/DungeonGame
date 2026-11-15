using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public static class MigrateCharacters
{
    private static readonly List<string> changes = new List<string>();
    private static readonly string[] removed = { "PlayerCameraController", "PlayerAim", "PlayerSword", "Player", "Testing", "PlayerPickUpDrop", "CharacterAnimationEventReceiver" };
    private static MonoScript sharedAnimator;
    private static void Backup(string path)
    {
        string target = "Temp/PlayerCleanup/Baseline/" + path;
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        if (!File.Exists(target)) File.Copy(path, target);
    }
    public static void Prepare()
    {
        if (EditorApplication.isPlaying) throw new Exception("Leave the user's Play Mode untouched.");
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && stage.scene.isDirty) throw new Exception("The open prefab has unsaved changes: " + stage.assetPath);
        // Copy holder-owned camera/UI overrides before removing their source components.
        foreach (var path in new[] { "Assets/Prefabs/Player/PlayerHolder.prefab", "Assets/Prefabs/Player/Player_Main.prefab" })
        {
            Backup(path);
            var root = PrefabUtility.LoadPrefabContents(path);
            try { Configure(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        File.WriteAllText("Temp/PlayerCleanup/prepared.json", JsonConvert.SerializeObject(changes, Formatting.Indented));
    }
    private static void Configure(GameObject root)
    {
        foreach (var camera in root.GetComponentsInChildren<PlayerCameraController>(true))
        {
            var movement = camera.GetComponent<StarterAssets.ThirdPersonController>();
            if (movement == null) continue;
            var from = new SerializedObject(camera); var to = new SerializedObject(movement);
            foreach (var name in new[] { "tpCamera", "fpCamera", "isFirstPerson", "_firstPersonHiddenRenderers" })
                to.CopyFromSerializedProperty(from.FindProperty(name));
            to.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(movement);
            changes.Add("View configuration: " + root.name + "/" + movement.name);
        }
        foreach (var controls in root.GetComponentsInChildren<PlayerControlState>(true))
        {
            var old = controls.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (old != null) { var so = new SerializedObject(controls); so.FindProperty("analogMovement").boolValue = old.analogMovement; so.ApplyModifiedPropertiesWithoutUndo(); }
            foreach (var slot in root.GetComponentsInChildren<HotKeySlot>(true))
            {
                var so = new SerializedObject(slot); so.FindProperty("controls").objectReferenceValue = controls; so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        foreach (var testing in root.GetComponentsInChildren<Testing>(true))
        {
            var from = new SerializedObject(testing);
            var player = from.FindProperty("player").objectReferenceValue as Component;
            var tree = from.FindProperty("uiSkillTree").objectReferenceValue as UI_SkillTree;
            if (tree != null)
            {
                var to = new SerializedObject(tree);
                to.FindProperty("playerStats").objectReferenceValue = player != null ? player.GetComponent<PlayerStats>() : null;
                to.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        foreach (var grab in root.GetComponentsInChildren<PlayerPickUpDrop>(true))
        {
            var controller = grab.GetComponent<PlayerController>();
            if (controller == null) continue;
            var from = new SerializedObject(grab); var to = new SerializedObject(controller);
            to.CopyFromSerializedProperty(from.FindProperty("objectGrabPointTransform"));
            to.CopyFromSerializedProperty(from.FindProperty("pickUpLayerMask"));
            to.ApplyModifiedPropertiesWithoutUndo();
        }
    }
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new Exception("Cannot migrate while playing.");
        var open = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
        if (open.Any(s => s.isDirty)) throw new Exception("Preserve unsaved scenes before asset migration.");
        var active = SceneManager.GetActiveScene();
        sharedAnimator = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Combat/Animation/CharacterAnimator.cs");
        var uses = JsonConvert.DeserializeObject<Use[]>(File.ReadAllText("Temp/PlayerCleanup/uses.json"));
        var prefabs = uses.Select(x => x.path).Where(p => p.EndsWith(".prefab") && File.Exists(p)).Concat(new[] { "Assets/Prefabs/Player/Player_Main.prefab", "Assets/Prefabs/Player/Variants/PlayerContent.prefab" }).Distinct().ToArray();
        // Bases first: removing inherited components on a holder should not create redundant overrides.
        foreach (var path in prefabs.OrderBy(p => p.EndsWith("Player_Main.prefab") ? 0 : p.EndsWith("PlayerHolder.prefab") ? 2 : 1))
        {
            Backup(path);
            var root = PrefabUtility.LoadPrefabContents(path);
            try { Configure(root); Clean(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        foreach (var path in uses.Select(x => x.path).Where(p => p.EndsWith(".unity")).Distinct())
        {
            var existing = open.FirstOrDefault(s => s.path == path);
            bool wasOpen = existing.IsValid();
            var scene = wasOpen ? existing : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            bool changed = false;
            foreach (var root in scene.GetRootGameObjects()) { Configure(root); changed |= Clean(root); }
            if (changed || scene.isDirty) { Backup(path); EditorSceneManager.SaveScene(scene); changes.Add("Scene migration: " + path); }
            if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
        }
        if (active.IsValid() && active.isLoaded) SceneManager.SetActiveScene(active);
        AssetDatabase.SaveAssets();
        File.WriteAllText("Temp/PlayerCleanup/migration.json", JsonConvert.SerializeObject(changes, Formatting.Indented));
        Debug.Log("Character migration complete: " + changes.Count + " changes.");
    }
    private static bool Clean(GameObject root)
    {
        bool changed = false;
        foreach (var c in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (c == null || (PrefabUtility.IsPartOfPrefabInstance(c) && !PrefabUtility.IsAddedComponentOverride(c))) continue;
            string type = c.GetType().Name;
            if (type == "PlayerAnimator" || type == "EnemyAnimator")
            {
                string label = root.name + "/" + c.name;
                var so = new SerializedObject(c); so.FindProperty("m_Script").objectReferenceValue = sharedAnimator; so.ApplyModifiedPropertiesWithoutUndo();
                changes.Add("Shared CharacterAnimator: " + label); changed = true;
            }
            else if (removed.Contains(type) || (type == "StarterAssetsInputs" && c.GetComponent<PlayerControlState>() != null))
            {
                changes.Add("Removed " + type + ": " + root.name + "/" + c.name);
                UnityEngine.Object.DestroyImmediate(c, true); changed = true;
            }
        }
        return changed;
    }
    private sealed class Use { public string path; public string[] scripts; }
}
