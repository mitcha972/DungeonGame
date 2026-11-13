using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
public static class OrganizeProjectAssets {
 [Serializable] public class Entry {public string @old;public string @new;public string guid;public string sha256;}
 [Serializable] public class Manifest {public List<Entry> scriptMoves;public List<Entry> assetDocuments;}
 static string Hash(string path){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
 static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;int i=path.LastIndexOf('/');Folder(path.Substring(0,i));string guid=AssetDatabase.CreateFolder(path.Substring(0,i),path.Substring(i+1));if(string.IsNullOrEmpty(guid))throw new Exception("Cannot create "+path);}
 public static object Run(){
 if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Edit Mode required; no assets moved");
 AssetDatabase.Refresh();
 var m=Newtonsoft.Json.JsonConvert.DeserializeObject<Manifest>(File.ReadAllText("Assets/Plan/ARCHIVE/Organization/organization-manifest.json"));
 foreach(var e in m.scriptMoves){if(Hash(e.@old)!=e.sha256 || AssetDatabase.AssetPathToGUID(e.@old)!=e.guid)throw new Exception("Concurrent script change: "+e.@old);if(File.Exists(e.@new))throw new Exception("Destination exists: "+e.@new);}
 foreach(var e in m.assetDocuments)if(Hash(e.@old)!=e.sha256)throw new Exception("Concurrent document change: "+e.@old);
 foreach(var e in m.scriptMoves)Folder(Path.GetDirectoryName(e.@new).Replace('\\','/') );
 foreach(var e in m.assetDocuments)Folder(Path.GetDirectoryName(e.@new).Replace('\\','/') );
 AssetDatabase.Refresh();
 var journal=new List<string>();
 AssetDatabase.DisallowAutoRefresh();AssetDatabase.StartAssetEditing();
 try {
 foreach(var e in m.scriptMoves){Folder(Path.GetDirectoryName(e.@new).Replace('\\','/'));var error=AssetDatabase.MoveAsset(e.@old,e.@new);if(!string.IsNullOrEmpty(error))throw new Exception(error);journal.Add(e.@old+" -> "+e.@new);}
 foreach(var e in m.assetDocuments){Folder(Path.GetDirectoryName(e.@new).Replace('\\','/'));var error=AssetDatabase.MoveAsset(e.@old,e.@new);if(!string.IsNullOrEmpty(error))throw new Exception(error);journal.Add(e.@old+" -> "+e.@new);}
 var scenes=EditorBuildSettings.scenes;int reconciled=0;
 for(int i=0;i<scenes.Length;i++){string path=AssetDatabase.GUIDToAssetPath(scenes[i].guid.ToString());if(string.IsNullOrEmpty(path)||!File.Exists(path))throw new Exception("Unresolved build scene: "+scenes[i].path);if(path!=scenes[i].path){scenes[i]=new EditorBuildSettingsScene(path,scenes[i].enabled);reconciled++;}}
 EditorBuildSettings.scenes=scenes;
 File.WriteAllLines("Assets/Plan/ARCHIVE/Organization/asset-move-journal.txt",journal);
 return new{movedScripts=m.scriptMoves.Count,movedDocuments=m.assetDocuments.Count,buildScenePathsReconciled=reconciled};
 }finally{File.WriteAllLines("Assets/Plan/ARCHIVE/Organization/asset-move-journal.txt",journal);AssetDatabase.StopAssetEditing();AssetDatabase.AllowAutoRefresh();AssetDatabase.SaveAssets();}
 }
}


