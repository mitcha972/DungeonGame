using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
public static class RepairEndRoom
{
    const string Path = "Assets/Prefabs/Dungeons/Rooms/DEnd.prefab";
    public static object Preview()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
        return Candidates(root).Select(c => new { c.name, pos = c.transform.position.ToString(), center = Center(c).ToString(), parent = c.transform.parent.name }).ToArray();
    }
    static MeshCollider[] Candidates(GameObject root) => root.GetComponentsInChildren<MeshCollider>(true).Where(c => {
        var p = root.transform.InverseTransformPoint(Center(c));
        return c.name.StartsWith("Dungeon_Wall_") &&
            ((Mathf.Abs(Mathf.Abs(p.x)-15.8f)<.3f && Mathf.Abs(p.z)<3.9f) || (Mathf.Abs(Mathf.Abs(p.z)-15.8f)<.3f && Mathf.Abs(p.x)<3.9f));
    }).ToArray();
    static Vector3 Center(MeshCollider c) => c.transform.TransformPoint(c.sharedMesh.bounds.center);
    public static object Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Edit Mode required");
        System.IO.File.Copy(Path,"Temp/AuditRestore/DEnd.prefab",true);
        var root=PrefabUtility.LoadPrefabContents(Path);
        try
        {
            var room=root.GetComponent<RoomBehaviour>(); var walls=Candidates(root);
            if(walls.Length!=8 || root.GetComponentInChildren<DungeonExit>(true)!=null) throw new Exception("Unexpected end-room baseline");
            foreach(var wall in walls)
            {
                var p=root.transform.InverseTransformPoint(Center(wall));
                int direction=Mathf.Abs(p.z)>Mathf.Abs(p.x)?(p.z>0?0:1):(p.x>0?2:3);
                // These inner wall sections must follow the same passage flag as the outer cap.
                wall.transform.SetParent(room.walls[direction].transform,true);
            }
            var original=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dungeons/Rooms/DBig.prefab").GetComponentInChildren<DungeonExit>(true);
            var exit=UnityEngine.Object.Instantiate(original.gameObject,root.transform);
            exit.name="ExitObject"; exit.transform.localPosition=Vector3.zero; exit.transform.localRotation=Quaternion.identity;
            PrefabUtility.SaveAsPrefabAsset(root,Path);
            return new {repaired=true, wallSections=walls.Length, reusedExit=true};
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    public static object FixCorridorParents()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Edit Mode required");
        var root=PrefabUtility.LoadPrefabContents(Path);
        try
        {
            var r=root.GetComponent<RoomBehaviour>(); int moved=0;
            foreach(var source in r.doors)
            foreach(var floor in source.GetComponentsInChildren<Collider>(true).Where(c=>c.name=="FloorTIle").ToArray())
            {
                var p=root.transform.InverseTransformPoint(floor.transform.position);
                int direction=Mathf.Abs(p.z)>Mathf.Abs(p.x)?(p.z>0?0:1):(p.x>0?2:3);
                if(source==r.doors[direction]) continue;
                var section=floor.transform.parent;
                section.SetParent(r.doors[direction].transform,true); moved++;
            }
            if(moved!=2) throw new Exception("Unexpected corridor baseline: "+moved);
            PrefabUtility.SaveAsPrefabAsset(root,Path);
            return new {movedSections=moved};
        }
        finally {PrefabUtility.UnloadPrefabContents(root);}
    }
}
