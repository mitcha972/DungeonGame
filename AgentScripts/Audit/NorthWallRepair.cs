using System;using System.Linq;using UnityEngine;using UnityEditor;public static class NorthWallRepair { public static object Run(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Edit Mode required");
string path="Assets/Prefabs/Dungeons/Rooms/DEnd.prefab";var root=PrefabUtility.LoadPrefabContents(path);
try{var room=root.GetComponent<RoomBehaviour>();var walls=root.GetComponentsInChildren<MeshCollider>(true).Where(c=>c.name=="Dungeon_Wall_Var1 (37)"||c.name=="Dungeon_Wall_Var3 (48)").ToArray();
if(walls.Length!=2)throw new Exception("Unexpected north wall baseline");foreach(var c in walls)c.transform.SetParent(room.walls[0].transform,true);PrefabUtility.SaveAsPrefabAsset(root,path);return new{wallSections=walls.Length};}
finally{PrefabUtility.UnloadPrefabContents(root);}}}