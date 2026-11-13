using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
internal static class CombatCheckpointRunner
{
    private static readonly List<string> Logs = new List<string>();
    private static bool _success = true, _running;
    private static double _start, _phaseStart;
    private static int _phase;
    private static GameObject _player;
    private static Component _stats, _combat, _equipment, _ui, _movement, _enemyStats, _enemyAi;
    private static NavMeshAgent _enemy;
    private static Vector3 _position;
    private static int _healthBefore, _expectedDamage;
    private static int _projectileCount;
    static CombatCheckpointRunner()
    {
        if (SessionState.GetString("CombatCheckpoint.State", "Idle") == "Entering")
        {
            AttachHandlers();
            if (EditorApplication.isPlaying) Begin();
        }
    }
    public static void Arm()
    {
        if (_running) return;
        Logs.Clear();
        _success = true;
        _phase = 0;
        _projectileCount = 0;
        SessionState.SetString("CombatCheckpoint.State", "Entering");
        AttachHandlers();
        if (EditorApplication.isPlaying) Begin();
        else
        {
            EditorApplication.delayCall -= EnterPlayMode;
            EditorApplication.delayCall += EnterPlayMode;
        }
    }
    private static void AttachHandlers()
    {
        Application.logMessageReceived -= Capture;
        EditorApplication.playModeStateChanged -= Changed;
        Application.logMessageReceived += Capture;
        EditorApplication.playModeStateChanged += Changed;
    }

    private static void EnterPlayMode()
    {
        if (SessionState.GetString("CombatCheckpoint.State", "Idle") == "Entering" && !EditorApplication.isPlayingOrWillChangePlaymode)
            EditorApplication.isPlaying = true;
    }

    private static void Changed(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) Begin();
        else if (state == PlayModeStateChange.EnteredEditMode &&
                 (_running || SessionState.GetString("CombatCheckpoint.State", "Idle") == "Entering"))
            Finish("Play mode stopped before checkpoint completion.");
    }
    private static void Begin()
    {
        if (_running) return;
        _running = true;
        _start = _phaseStart = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
    }
    private static void Capture(string message, string stack, LogType type)
    {
        if (message.Contains("[CombatTest]") || type == LogType.Error || type == LogType.Exception)
        {
            string entry = type + ": " + message;
            if (!Logs.Contains(entry)) Logs.Add(entry);
            if (type == LogType.Error || type == LogType.Exception) _success = false;
        }
    }
    private static Component Find(string name)
    {
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var c in root.GetComponentsInChildren<MonoBehaviour>()) if (c != null && c.isActiveAndEnabled && c.GetType().Name == name) return c;
        return null;
    }
    private static object Field(Component c, string name) => c.GetType().GetField(name).GetValue(c);
    private static int Stat(Component c, string name) => (int)Field(c, name);
    private static void Set(Component c, string name, object value) => c.GetType().GetField(name).SetValue(c, value);
    private static bool Dead(Component c) => (bool)c.GetType().GetProperty("IsDead").GetValue(c);
    private static void Check(bool pass, string text) { _success &= pass; Debug.Log("[CombatTest] " + (pass ? "PASS " : "FAIL ") + text); }
    private static void Invoke(Component c, string name) => c.GetType().GetMethod(name, Type.EmptyTypes).Invoke(c, null);
    private static float _phaseGameStart;
    private static void Advance(int phase) { _phase = phase; _phaseStart = EditorApplication.timeSinceStartup; _phaseGameStart = Time.time; }
    private static void Key(Key key, bool press)
    {
        using (StateEvent.From(Keyboard.current, out var e)) { Keyboard.current[key].WriteValueIntoEvent(press ? 1f : 0f, e); InputSystem.QueueEvent(e); }
    }
    private static void Click(bool press) => InputSystem.QueueStateEvent(Mouse.current, new MouseState { buttons = press ? (ushort)1 : (ushort)0 });
    private static void PlaceEnemy(float distance)
    {
        Vector3 desired = _player.transform.position + Vector3.forward * distance;
        if (!NavMesh.SamplePosition(desired, out var hit, 2f, NavMesh.AllAreas)) throw new Exception("No local combat spawn NavMesh");
        _enemy.Warp(hit.position);
        _enemy.isStopped = true;
        ((Behaviour)_enemyAi).enabled = false;
        _enemy.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }
    private static void Tick()
    {
        try
        {
            double age = _phase >= 8 ? Time.time - _phaseGameStart : EditorApplication.timeSinceStartup - _phaseStart;
            if (EditorApplication.timeSinceStartup - _start > 45) { Finish("Timeout at phase " + _phase); return; }
            if (_phase == 0)
            {
                var generator = Find("DungeonGenerator");
                if (generator == null || !(bool)generator.GetType().GetProperty("IsReady").GetValue(generator)) return;
                _player = (GameObject)Field(generator, "player");
                _stats = _player.GetComponent("PlayerStats"); _combat = _player.GetComponent("PlayerCombat"); _equipment = _player.GetComponent("PlayerEquipmentManager");
                _ui = Find("UICanvasController"); _movement = _player.GetComponent("ThirdPersonController");
                _position = _player.transform.position;
                Key(UnityEngine.InputSystem.Key.I, true); Advance(1); return;
            }
            if (_phase == 1 && age > 0.15) { Key(UnityEngine.InputSystem.Key.I, false); Advance(2); return; }
            if (_phase == 2 && age > 0.2)
            {
                Check(((GameObject)Field(_ui, "inv")).activeSelf && Cursor.lockState == CursorLockMode.None, "Inventory opens and owns cursor");
                Key(UnityEngine.InputSystem.Key.W, true); _position = _player.transform.position; Advance(3); return;
            }
            if (_phase == 3 && age > 0.6)
            {
                Check(Vector3.Distance(_position, _player.transform.position) < 0.05f, "Inventory blocks movement: distance=" + Vector3.Distance(_position, _player.transform.position));
                Key(UnityEngine.InputSystem.Key.W, false); Key(UnityEngine.InputSystem.Key.I, true); Advance(4); return;
            }
            if (_phase == 4 && age > 0.2) { Key(UnityEngine.InputSystem.Key.I, false); Key(UnityEngine.InputSystem.Key.Escape, true); Advance(5); return; }
            if (_phase == 5 && age > 0.2) { Key(UnityEngine.InputSystem.Key.Escape, false); Check(Time.timeScale == 0f && Cursor.lockState == CursorLockMode.None, "Pause freezes game"); Advance(6); return; }
            if (_phase == 6 && age > 0.2) { Key(UnityEngine.InputSystem.Key.Escape, true); Advance(7); return; }
            if (_phase == 7 && age > 0.2)
            {
                Key(UnityEngine.InputSystem.Key.Escape, false); Check(Time.timeScale == 1f && Cursor.lockState == CursorLockMode.Locked, "Resume restores game");
                _enemy = Find("EnemyAi").GetComponent<NavMeshAgent>(); _enemyAi = _enemy.GetComponent("EnemyAi"); _enemyStats = _enemy.GetComponent("EnemyStats");
                PlaceEnemy(1f); _player.transform.rotation = Quaternion.identity;
                Set(_enemyStats, "currentHealth", 100);
                _healthBefore = 100; _expectedDamage = Mathf.Max(1, Stat(_stats, "damage") - Mathf.Max(0, Stat(_enemyStats, "armor")));
                Click(true); Advance(8); return;
            }
            if (_phase == 8 && age > 0.1) { Click(false); Advance(9); return; }
            if (_phase == 9 && age > 0.2 && (bool)Field(_combat, "attackIsReady"))
            {
                var presentation = _player.GetComponent("PlayerAnimator");
                Debug.Log("[CombatTest] Sword execution damage=" + _combat.GetType().GetProperty("AttackDamage").GetValue(_combat) + " duration=" + presentation.GetType().GetProperty("CurrentAttackDuration").GetValue(presentation) + " gameTime=" + Time.time + " animation=" + _player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).shortNameHash);
                int lost = _healthBefore - Stat(_enemyStats, "currentHealth");
                Check(lost == _expectedDamage, "Sword deals exactly one hit: lost=" + lost + " expected=" + _expectedDamage);
                Set(_enemyStats, "currentHealth", _expectedDamage); Click(true); Advance(10); return;
            }
            if (_phase == 10 && age > 0.1) { Click(false); Advance(11); return; }
            if (_phase == 11 && age > 0.2 && (bool)Field(_combat, "attackIsReady"))
            {
                Check(Dead(_enemyStats), "Sword kills enemy via attack timeline");
                var inventory = _player.GetComponent("Inventory"); var items = (System.Collections.IList)Field(inventory, "items"); var bow = items[0];
                var method = _equipment.GetType().GetMethods().First(m => m.Name == "Equip" && m.GetParameters().Length == 2);
                method.Invoke(_equipment, new object[] { bow, 0 });
                Advance(12); return;
            }
            if (_phase == 12 && age > 1.2)
            {
                var presentation = _player.GetComponent("PlayerAnimator"); var weapon = presentation.GetType().GetProperty("CurrentWeapon").GetValue(presentation) as UnityEngine.Object;
                Debug.Log("[CombatTest] Selected weapon=" + (weapon != null ? weapon.name : "NULL") + " busy=" + _player.GetComponent<Animator>().GetBool("isBusy") + " inventory0=" + ((System.Collections.IList)Field(_player.GetComponent("Inventory"), "items"))[0]);
                Check(weapon != null && weapon.name == "Arrow", "Bow equipment selects ranged weapon data");
                var candidates = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<NavMeshAgent>()).Where(a=>a.isActiveAndEnabled && a.gameObject.GetComponent("EnemyStats") != null && !Dead(a.gameObject.GetComponent("EnemyStats"))).ToArray();
                _enemy = candidates.First(); _enemyAi = _enemy.GetComponent("EnemyAi"); _enemyStats = _enemy.GetComponent("EnemyStats"); PlaceEnemy(4f);
                Set(_enemyStats, "currentHealth", Mathf.Max(1, Stat(_stats, "damage") - Stat(_enemyStats, "armor")));
                _movement.GetType().GetField("_cinemachineTargetYaw", BindingFlags.Instance|BindingFlags.NonPublic).SetValue(_movement, 0f);
                _movement.GetType().GetField("_cinemachineTargetPitch", BindingFlags.Instance|BindingFlags.NonPublic).SetValue(_movement, 6f);
                Advance(13); return;
            }
            if (_phase == 13 && age > 0.3) { Click(true); Advance(14); return; }
            if (_phase == 14 && age > 0.1) { Click(false); Advance(15); return; }
            if (_phase == 15)
            {
                if (Find("Projectile") != null) _projectileCount++;
                if (age < 0.2 || (!Dead(_enemyStats) && !(bool)Field(_combat, "attackIsReady"))) return;
                Check(Dead(_enemyStats), "Bow projectile kills enemy; projectile frames observed=" + _projectileCount);
                Finish(null);
                return;
            }
            if (_phase == 16 && age > 3.0)
            {
                Check(Vector3.Distance(_position, _enemy.transform.position) > 0.2f, "Enemy AI detects and pursues locally: distance=" + Vector3.Distance(_position, _enemy.transform.position));
                Check(Stat(_stats,"currentHealth") < _healthBefore, "Enemy melee damages player: health=" + Stat(_stats,"currentHealth") + " before=" + _healthBefore);
                Finish(null);
            }
        }
        catch (Exception exception) { Finish(exception.ToString()); }
    }
    private static void Finish(string error)
    {
        if (error != null) { _success = false; Logs.Add(error); }
        Time.timeScale = 1f;
        _running = false;
        EditorApplication.delayCall -= EnterPlayMode;
        EditorApplication.playModeStateChanged -= Changed;
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= Capture;
        string text = "success=" + _success + "\n" + string.Join("\n", Logs);
        SessionState.SetString("CombatCheckpoint.Result", text);
        SessionState.SetString("CombatCheckpoint.State", "Done");
        try
        {
            System.IO.Directory.CreateDirectory("Assets/Plan/ARCHIVE/Evidence/Current");
            System.IO.File.WriteAllText("Assets/Plan/ARCHIVE/Evidence/CombatTest.txt", text);
        }
        finally
        {
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }
    }
}
