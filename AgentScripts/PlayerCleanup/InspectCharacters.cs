using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;

public static class InspectCharacters
{
    public static void Main()
    {
        var paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Player", "Assets/Prefabs/Enemies", "Assets/Prefabs/NPC" }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(x => x).ToArray();
        var prefabs = new List<object>();
        foreach (var path in paths)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var objects = new List<object>();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var components = new List<object>();
                foreach (var c in t.GetComponents<Component>())
                {
                    if (c == null) { components.Add(new { type = "MISSING" }); continue; }
                    var refs = new List<object>();
                    if (c is MonoBehaviour || c is Animator)
                    {
                        var so = new SerializedObject(c);
                        var p = so.GetIterator();
                        while (p.Next(true))
                            if (p.propertyType == SerializedPropertyType.ObjectReference && p.objectReferenceValue != null)
                                refs.Add(new { field = p.propertyPath, target = p.objectReferenceValue.name, type = p.objectReferenceValue.GetType().Name, asset = AssetDatabase.GetAssetPath(p.objectReferenceValue) });
                    }
                    components.Add(new { type = c.GetType().FullName, enabled = c is Behaviour b ? b.enabled : true, script = c is MonoBehaviour m ? AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(m)) : "", refs });
                }
                objects.Add(new { path = AnimationUtility.CalculateTransformPath(t, root.transform), active = t.gameObject.activeSelf, components });
            }
            prefabs.Add(new { path, objects, dependencies = AssetDatabase.GetDependencies(path, true) });
        }
        var scenes = Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCount).Select(i => UnityEngine.SceneManagement.SceneManager.GetSceneAt(i)).Select(s => new { s.path, s.isDirty, s.isLoaded }).ToArray();
        Directory.CreateDirectory("Temp/PlayerCleanup");
        File.WriteAllText("Temp/PlayerCleanup/discovery.json", JsonConvert.SerializeObject(new { playing = EditorApplication.isPlaying, scenes, prefabs }, Formatting.Indented));
        var candidates = new[] { "PlayerAnimator", "EnemyAnimator", "StarterAssetsInputs", "PlayerCameraController", "PlayerAim", "PlayerSword", "Player", "Testing", "PlayerMotor", "PlayerManager", "FPC", "CameraController", "CharacterAnimationEventReceiver", "BasicRigidBodyPush", "PlayerContent" };
        var scripts = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/Scripts" }).Select(AssetDatabase.GUIDToAssetPath).Where(p => candidates.Contains(Path.GetFileNameWithoutExtension(p))).ToArray();
        var assets = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/") && (p.EndsWith(".prefab") || p.EndsWith(".unity") || p.EndsWith(".asset") || p.EndsWith(".controller"))).ToArray();
        var uses = assets.Select(p => new { path = p, scripts = AssetDatabase.GetDependencies(p, true).Intersect(scripts).ToArray() }).Where(x => x.scripts.Length > 0).ToArray();
        File.WriteAllText("Temp/PlayerCleanup/uses.json", JsonConvert.SerializeObject(uses, Formatting.Indented));
        Debug.Log("Character discovery written: " + prefabs.Count + " prefabs; open scenes " + JsonConvert.SerializeObject(scenes));
    }
}
