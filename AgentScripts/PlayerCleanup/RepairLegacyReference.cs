using System;
using UnityEngine;
using UnityEditor;
public static class RepairLegacyReference
{
    public static void Main()
    {
        const string path="Assets/Prefabs/Player/Variants/StarterAssets_PlayerContent.prefab";
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true)) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            foreach(var c in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if(c.GetType().Name=="PlayerAnimator")
                {
                    var so=new SerializedObject(c); so.FindProperty("m_Script").objectReferenceValue=AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Combat/Animation/CharacterAnimator.cs"); so.ApplyModifiedPropertiesWithoutUndo();
                }
                else if(c.GetType().Name=="CharacterAnimationEventReceiver") UnityEngine.Object.DestroyImmediate(c,true);
            }
            PrefabUtility.SaveAsPrefabAsset(root,path,out bool saved);
            if(!saved) throw new Exception("Legacy prefab could not be saved.");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Removed the pre-existing missing camera script so the legacy prefab can retain shared CharacterAnimator references.");
    }
}
