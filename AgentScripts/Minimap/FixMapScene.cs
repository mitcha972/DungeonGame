using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class FixMapScene {
public static string Run() {
if(EditorApplication.isPlaying) return "Refused in Play";
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(!scene.path.EndsWith("/Quest_Test.unity"))return "Wrong scene";
var map=Object.FindFirstObjectByType<MapController>(FindObjectsInactive.Include);
foreach(var t in map.GetComponentsInChildren<Transform>(true)) {
if(t.name=="MapDisplayBackground" || t.name=="FloorText" || t.name=="FloorNumText") {
PrefabUtility.RevertObjectOverride(t.gameObject,InteractionMode.AutomatedAction);
foreach(var c in t.GetComponents<Component>()) PrefabUtility.RevertObjectOverride(c,InteractionMode.AutomatedAction);
}}
EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
return "Reconciled only Quest_Test minimap presentation overrides";
}}
