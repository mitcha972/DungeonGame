using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
public static class PolishCharacterPrefab
{
    public static void Main()
    {
        const string path="Assets/Prefabs/Player/PlayerHolder.prefab";
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var aim=root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="PlayerAim");
            if(aim!=null&&aim.childCount==0&&aim.GetComponents<Component>().Length==1) UnityEngine.Object.DestroyImmediate(aim.gameObject,true);
            PrefabUtility.SaveAsPrefabAsset(root,path,out bool saved); if(!saved)throw new Exception("Cannot save holder cleanup.");
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
}
