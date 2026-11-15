using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class MergeQuickSlots
{
    private const string Script="Assets/Scripts/Inventory/UI/HotKeySlot.cs";
    private static bool Merge(GameObject root)
    {
        var slots=root.GetComponentsInChildren<HotKeySlot>(true); if(slots.Length==0)return false;
        var ui=root.GetComponentInChildren<HotKeysUI>(true);
        if(ui!=null)
        {
            var so=new SerializedObject(ui); var array=so.FindProperty("quickSlots"); array.arraySize=6;
            foreach(var slot in slots) { int i=(int)slot.keyCode-(int)KeyCode.Alpha1; if(i>=0&&i<6)array.GetArrayElementAtIndex(i).objectReferenceValue=slot.inventorySlot; }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        bool changed=ui!=null;
        foreach(var slot in slots)
            if(!PrefabUtility.IsPartOfPrefabInstance(slot)||PrefabUtility.IsAddedComponentOverride(slot)) { UnityEngine.Object.DestroyImmediate(slot,true); changed=true; }
        return changed;
    }
    public static void Main()
    {
        if(EditorApplication.isPlaying)throw new Exception("Exit task-owned Play Mode before merging assets.");
        var prefabs=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>AssetDatabase.GetDependencies(p,true).Contains(Script)).ToArray();
        foreach(var path in prefabs)
        {
            string backup="Temp/PlayerCleanup/Baseline/"+path; Directory.CreateDirectory(Path.GetDirectoryName(backup)); if(!File.Exists(backup))File.Copy(path,backup);
            var root=PrefabUtility.LoadPrefabContents(path);
            try { if(Merge(root)){ PrefabUtility.SaveAsPrefabAsset(root,path,out bool saved); if(!saved)throw new Exception("Save failed: "+path); } }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        var active=SceneManager.GetActiveScene();
        foreach(var path in AssetDatabase.FindAssets("t:Scene",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>AssetDatabase.GetDependencies(p,true).Contains(Script)).ToArray())
        {
            var scene=SceneManager.GetSceneByPath(path); bool wasOpen=scene.IsValid()&&scene.isLoaded;
            if(wasOpen&&scene.isDirty)throw new Exception("Preserve dirty scene: "+path);
            if(!wasOpen)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            bool changed=false; foreach(var root in scene.GetRootGameObjects())changed|=Merge(root);
            if(changed){string backup="Temp/PlayerCleanup/Baseline/"+path;Directory.CreateDirectory(Path.GetDirectoryName(backup));if(!File.Exists(backup))File.Copy(path,backup);EditorSceneManager.SaveScene(scene);}
            if(!wasOpen)EditorSceneManager.CloseScene(scene,true);
        }
        if(active.IsValid())SceneManager.SetActiveScene(active);
        var remaining=AssetDatabase.FindAssets("t:Prefab",new[]{"Assets"}).Concat(AssetDatabase.FindAssets("t:Scene",new[]{"Assets"})).Select(AssetDatabase.GUIDToAssetPath).Where(p=>AssetDatabase.GetDependencies(p,true).Contains(Script)).ToArray();
        if(remaining.Length>0)throw new Exception("HotKeySlot remains referenced: "+string.Join(",",remaining));
        AssetDatabase.DeleteAsset(Script); AssetDatabase.SaveAssets();
    }
}
