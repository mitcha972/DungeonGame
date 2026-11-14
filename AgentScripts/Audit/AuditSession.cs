using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AuditSession
{
    const string Key = "DungeonGame.FullAudit.StartScene";
    public static object Prepare(string path)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Preserve existing Play Mode");
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        if (scene == null) throw new Exception("Scene does not exist: " + path);
        if (!SessionState.GetBool(Key + ".saved", false))
        {
            SessionState.SetString(Key, EditorSceneManager.playModeStartScene == null ? "" : AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            SessionState.SetBool(Key + ".saved", true);
        }
        EditorSceneManager.playModeStartScene = scene;
        return new { prepared = true, path, editScene = SceneManager.GetActiveScene().path, dirty = SceneManager.GetActiveScene().isDirty };
    }
    public static object Restore()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Stop owned tests first");
        if (SessionState.GetBool(Key + ".saved", false))
        {
            string path = SessionState.GetString(Key, "");
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            SessionState.EraseString(Key); SessionState.EraseBool(Key + ".saved");
        }
        return new { restored = true, scene = SceneManager.GetActiveScene().path, dirty = SceneManager.GetActiveScene().isDirty };
    }
    public static object Snapshot()
    {
        return new { scene = SceneManager.GetActiveScene().path, frame = Time.frameCount, time = Time.time,
            floor = GameController.floor, difficulty = GameController.difficulty,
            selectedName = GameController.playerName, selectedClass = GameController.playerClass == null ? null : GameController.playerClass.name,
            players = UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).Select(p => new { objectName = p.gameObject.name, playerName = p.name, playerClass = p.playerClass == null ? null : p.playerClass.name, p.currentHealth, p.maxHealth, p.damage, p.armor }).ToArray(),
            controllers = UnityEngine.Object.FindObjectsByType<GameController>(FindObjectsSortMode.None).Select(g => new { g.m_floor, g.m_difficulty, hasPlayer = g.playerContent != null }).ToArray(),
            generators = UnityEngine.Object.FindObjectsByType<DungeonGenerator>(FindObjectsSortMode.None).Select(g => new { g.IsReady, g.IsGenerating, g.LastFailure, cells = g.Cells.Count, position = g.PlayerSpawnPosition.ToString() }).ToArray() };
    }
    public static object PlaySelection()
    {
        var menu = UnityEngine.Object.FindFirstObjectByType<MenuController>();
        if (menu == null) throw new Exception("Menu missing");
        var selected = AssetDatabase.FindAssets("t:Class").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Class>).First(c => c != null && c != menu.pClass);
        menu.NewClass(selected); menu.name = "Audit Adventurer";
        GameController.floor = 7; GameController.difficulty = 3;
        menu.Play();
        return new { selected = selected.name, expectedName = "Audit Adventurer" };
    }
}
