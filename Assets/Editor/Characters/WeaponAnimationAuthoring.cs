using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

/// <summary>Bakes direct clip states into character controllers; runtime never replaces placeholder clips.</summary>
public sealed class WeaponAnimationAuthoring : AssetPostprocessor, IPreprocessBuildWithReport
{
    private static bool busy, queued;
    public int callbackOrder => 0;
    [MenuItem("Tools/Characters/Synchronize Weapon Animations")]
    public static void Synchronize()
    {
        if (busy || EditorApplication.isPlaying) return;
        busy = true;
        try
        {
            var weapons = new Dictionary<string, AnimationClip>();
            var playerWeapons = new Dictionary<string, AnimationClip>();
            var abilities = new Dictionary<string, AnimationClip>();
            foreach (var id in AssetDatabase.FindAssets("t:WeaponAttackSet"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<WeaponAttackSet>(AssetDatabase.GUIDToAssetPath(id));
                var serialized = new SerializedObject(asset);
                BindMotions(serialized.FindProperty("lightAttacks"), weapons);
                BindMotions(serialized.FindProperty("lightAttacks"), playerWeapons);
                BindMotions(serialized.FindProperty("heavyAttacks"), weapons);
                if (serialized.hasModifiedProperties) { serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(asset); }
            }
            foreach (var id in AssetDatabase.FindAssets("t:Abilities"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Abilities>(AssetDatabase.GUIDToAssetPath(id));
                if (asset.startAnimation == null) continue;
                string state = StateName(asset.startAnimation, "Ability_");
                var serialized = new SerializedObject(asset);
                if (serialized.FindProperty("animationState").stringValue != state) serialized.FindProperty("animationState").stringValue = state;
                if (serialized.hasModifiedProperties) { serialized.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(asset); }
                abilities[state] = asset.startAnimation;
            }
            // Before the one-time data migration the new arrays are empty; do not strip their old states yet.
            if (weapons.Count == 0) return;
            foreach (var id in AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/Animations" }))
            {
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(id));
                var attack = controller.layers.FirstOrDefault(l => string.Equals(l.name, "Override", StringComparison.OrdinalIgnoreCase));
                if (attack == null) continue;
                var ability = controller.layers.FirstOrDefault(l => l.stateMachine.states.Any(s => s.state.name == "Ability" || s.state.name.StartsWith("Ability_")))
                    ?? controller.layers.FirstOrDefault(l => l.name == "UpperBody") ?? attack;
                bool changed = Bake(attack.stateMachine, controller.name == "Player Animator" ? playerWeapons : weapons, "Weapon_", new[] { "LightAttack", "HeavyAttack" });
                changed |= Bake(ability.stateMachine, abilities, "Ability_", new[] { "Ability" });
                if (changed) { EditorUtility.SetDirty(controller); AssetDatabase.SaveAssetIfDirty(controller); }
            }
        }
        finally { busy = false; }
    }
    private static void BindMotions(SerializedProperty array, Dictionary<string, AnimationClip> bindings)
    {
        if (array == null) return;
        for (int i = 0; i < array.arraySize; i++)
        {
            var motion = array.GetArrayElementAtIndex(i);
            var clip = motion.FindPropertyRelative("clip").objectReferenceValue as AnimationClip;
            if (clip == null) continue;
            string name = StateName(clip, "Weapon_");
            if (motion.FindPropertyRelative("animatorState").stringValue != name) motion.FindPropertyRelative("animatorState").stringValue = name;
            bindings[name] = clip;
        }
    }
    private static string StateName(AnimationClip clip, string prefix)
    {
        if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out string guid, out long id)) throw new InvalidOperationException("Save the motion clip as an asset first: " + clip.name);
        return prefix + System.Text.RegularExpressions.Regex.Replace(clip.name, "[^A-Za-z0-9_]", "_") + "_" + guid + "_" + id.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
    private static bool Bake(AnimatorStateMachine machine, Dictionary<string, AnimationClip> clips, string prefix, string[] obsolete)
    {
        bool changed = false;
        var empty = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == "Empty");
        if (empty == null) throw new InvalidOperationException("Character action layer requires an Empty state: " + machine.name);
        var stale = machine.states.Select(s => s.state).Where(s => obsolete.Contains(s.name) || (s.name.StartsWith(prefix) && !clips.ContainsKey(s.name))).ToArray();
        foreach (var state in machine.states.Select(s => s.state))
            foreach (var transition in state.transitions.Where(t => stale.Contains(t.destinationState)).ToArray()) { state.RemoveTransition(transition); changed = true; }
        foreach (var transition in machine.anyStateTransitions.Where(t => stale.Contains(t.destinationState)).ToArray()) { machine.RemoveAnyStateTransition(transition); changed = true; }
        foreach (var state in stale) { machine.RemoveState(state); changed = true; }
        if (machine.defaultState == null) { machine.defaultState = empty; changed = true; }
        int index = 0;
        foreach (var binding in clips)
        {
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == binding.Key);
            if (state == null)
            {
                state = machine.AddState(binding.Key, new Vector3(300, 90 + 55 * index));
                var exit = state.AddTransition(empty);
                exit.hasExitTime = true; exit.exitTime = 1f; exit.duration = .1f; exit.hasFixedDuration = true;
                changed = true;
            }
            if (state.motion != binding.Value) { state.motion = binding.Value; changed = true; }
            index++;
        }
        return changed;
    }
    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] oldPaths)
    {
        if (busy || queued || EditorApplication.isPlaying) return;
        if (!imported.Concat(moved).Any(p => p.EndsWith(".asset") && (AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(WeaponAttackSet) || AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Abilities)))) return;
        queued = true;
        EditorApplication.delayCall += () => { queued = false; Synchronize(); };
    }
    public void OnPreprocessBuild(BuildReport report)
    {
        Synchronize();
        foreach (var id in AssetDatabase.FindAssets("t:Equipment"))
        {
            var item = AssetDatabase.LoadAssetAtPath<Equipment>(AssetDatabase.GUIDToAssetPath(id));
            if (item.equipSlot == EquipmentSlot.MainHand && (item.weaponAttacks == null || !item.weaponAttacks.lightAttacks.Any(m => m != null && m.clip != null)))
                throw new BuildFailedException("Assign an authored light attack set to main-hand equipment: " + AssetDatabase.GetAssetPath(item));
        }
    }
}
