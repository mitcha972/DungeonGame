using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Isolated regression checks; does not open or save user scenes.</summary>
public static class GameplayModernizationValidation
{
    private static readonly List<string> Results = new List<string>();

    [MenuItem("Tools/Validation/Gameplay Modernization")]
    public static void Run()
    {
        Results.Clear();
        try
        {
            CheckScenes();
            CheckExperience();
            CheckCombatIntervals();
            CheckLayouts();
            Results.Add("PASS: all checks completed.");
            WriteResults();
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Results.Add("FAIL: " + exception);
            WriteResults();
            Debug.LogException(exception);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else throw;
        }
    }

    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        Results.Add("PASS: " + description);
    }

    private static void WriteResults()
    {
        Directory.CreateDirectory("Assets/Plan/ARCHIVE/Evidence/Current");
        File.WriteAllLines("Assets/Plan/ARCHIVE/Evidence/Current/GAMEPLAY_MODERNIZATION_VALIDATION.txt", Results);
        Debug.Log(string.Join("\n", Results));
    }

    private static void CheckScenes()
    {
        int dungeon = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Game/Main.unity");
        int main = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Game/Start.unity");
        int loading = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Game/Loading.unity");
        Check(main >= 0 && loading >= 0 && dungeon >= 0, "Primary sequence is enabled in Build Settings");
        Check(LoadingScript.IsValidDestination(dungeon), "Dungeon accepted as loading destination");
        Check(!LoadingScript.IsValidDestination(main) && !LoadingScript.IsValidDestination(loading), "Menu and loading recursion rejected");
        Check(!LoadingScript.IsValidDestination(-1) && !LoadingScript.IsValidDestination(SceneManager.sceneCountInBuildSettings), "Out-of-range destinations rejected");
    }

    private static void CheckExperience()
    {
        var model = new LevelSystem();
        int levelEvents = 0, xpEvents = 0;
        model.OnLevelChanged += (_, __) => levelEvents++;
        model.OnExperienceChanged += (_, __) => xpEvents++;
        model.AddExperience(-1);
        model.AddExperience(0);
        Check(model.GetExperience() == 0 && xpEvents == 0, "Nonpositive awards ignored");
        model.AddExperience(100);
        Check(model.GetLevelNumber() == 1 && model.GetExperience() == 0 && levelEvents == 1 && xpEvents == 1, "Exact threshold and events");
        model.AddExperience(250);
        Check(model.GetLevelNumber() == 3 && model.GetExperience() == 50 && levelEvents == 3 && xpEvents == 2, "Multi-level award preserves remainder");
        model.AddExperience(int.MaxValue);
        Check(model.IsMaxLevel() && model.GetLevelNumber() == 39 && model.GetExperience() == 0 && model.GetExperienceNormalized() == 1f, "Overflow-sized award reaches bounded cap");
        Check(levelEvents == 39 && xpEvents == 3, "One level event per threshold and one XP event per award");
        model.AddExperience(int.MaxValue);
        Check(levelEvents == 39 && xpEvents == 3 && model.GetExperience() == 0, "Max-level awards are idempotent");
    }

    private static void CheckCombatIntervals()
    {
        Check(!CharacterCombat.CrossesDamageWindow(0f, 0.2f, 0.3f, 0.6f), "Before hit window stays inactive");
        Check(CharacterCombat.CrossesDamageWindow(0.2f, 0.4f, 0.3f, 0.6f), "Entering hit window activates");
        Check(CharacterCombat.CrossesDamageWindow(0.2f, 0.9f, 0.3f, 0.6f), "Skipped hit window activates");
        Check(CharacterCombat.CrossesDamageWindow(0f, 1f, 0.3f, 0.6f), "Entire attack skipped by one frame still samples");
        Check(!CharacterCombat.CrossesDamageWindow(0.7f, 0.9f, 0.3f, 0.6f), "After hit window stays inactive");
        Check(CharacterCombat.CrossesDamageWindow(0.2f, 0.8f, 0.5f, 0.5f), "Zero-width hit window is crossed");
        Check(CharacterCombat.CrossesDamageWindow(0.9f, 1f, 1f, 1f), "Final attack boundary is sampled");
    }

    private static void CheckLayouts()
    {
        var state = UnityEngine.Random.state;
        var owner = new GameObject("ModernizationLayoutValidation");
        try
        {
            owner.SetActive(false);
            var generator = owner.AddComponent<DungeonGenerator>();
            var build = typeof(DungeonGenerator).GetMethod("BuildLayout", BindingFlags.NonPublic | BindingFlags.Instance);
            if (build == null) throw new MissingMethodException("DungeonGenerator.BuildLayout");
            foreach (int width in new[] { 2, 5, 16 })
            {
                generator.size = new Vector2Int(width, width);
                for (int seed = 0; seed < 100; seed++)
                {
                    UnityEngine.Random.InitState(seed);
                    generator.startPos = seed % (width * width);
                    build.Invoke(generator, null);
                    ValidateGraph(generator, width);
                }
            }
            Check(true, "300 seeded maze graphs: connected, reciprocal, bounded, unique start/exit, farthest exit, spanning tree");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(owner);
            UnityEngine.Random.state = state;
        }
    }

    private static void ValidateGraph(DungeonGenerator generator, int width)
    {
        var cells = generator.Cells;
        int start = -1, exit = -1, starts = 0, exits = 0, edges = 0;
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].startCell) { start = i; starts++; }
            if (cells[i].endCell) { exit = i; exits++; }
        }
        if (starts != 1 || exits != 1 || start == exit) throw new Exception("Invalid start/exit");
        var distances = new int[cells.Count];
        for (int i = 0; i < distances.Length; i++) distances[i] = -1;
        var queue = new Queue<int>();
        queue.Enqueue(start);
        distances[start] = 0;
        int visited = 0, furthest = 0;
        int[] steps = { -width, width, 1, -1 };
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            visited++;
            if (!cells[current].visited) throw new Exception("Cell not generated");
            for (int d = 0; d < 4; d++)
            {
                if (!cells[current].status[d]) continue;
                edges++;
                int next = current + steps[d];
                if (next < 0 || next >= cells.Count || (d == 2 && current % width == width - 1) || (d == 3 && current % width == 0)) throw new Exception("Out-of-bounds passage");
                if (!cells[next].status[d ^ 1]) throw new Exception("Nonreciprocal passage");
                if (distances[next] >= 0) continue;
                distances[next] = distances[current] + 1;
                furthest = Math.Max(furthest, distances[next]);
                queue.Enqueue(next);
            }
        }
        if (visited != cells.Count || edges != 2 * (cells.Count - 1) || distances[exit] != furthest) throw new Exception("Invalid connected maze or exit distance");
    }
}
