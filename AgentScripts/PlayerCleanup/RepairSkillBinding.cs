using System;
using UnityEngine;
using UnityEditor;
public static class RepairSkillBinding
{
    public static void Main()
    {
        foreach(var path in new[]{"Assets/Prefabs/Player/PlayerHolder.prefab","Assets/Prefabs/Player/Variants/PlayerContent.prefab"})
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                var stats=root.GetComponentInChildren<PlayerStats>(true);
                foreach(var tree in root.GetComponentsInChildren<UI_SkillTree>(true))
                {
                    var so=new SerializedObject(tree); so.FindProperty("playerStats").objectReferenceValue=stats; so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root,path,out bool saved); if(!saved) throw new Exception("Cannot save skill binding.");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
