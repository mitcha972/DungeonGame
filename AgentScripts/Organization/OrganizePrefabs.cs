using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Newtonsoft.Json.Linq;
public static class OrganizePrefabs {
 const string Manifest="Assets/Plan/ARCHIVE/Organization/prefab-organization.json";
 static JObject State(){return new JObject{["playing"]=EditorApplication.isPlayingOrWillChangePlaymode,["scenes"]=new JArray(Enumerable.Range(0,UnityEngine.SceneManagement.SceneManager.sceneCount).Select(i=>{var s=UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);return new JObject{["path"]=s.path,["dirty"]=s.isDirty};}))};}
 static JArray Audit(){var a=new JArray();foreach(var p in AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/")&&!p.StartsWith("Assets/Plugins/")&&(p.EndsWith(".prefab")||p.EndsWith(".unity")))){
 var go=AssetDatabase.LoadAssetAtPath<GameObject>(p);int missing=go==null?0:go.GetComponentsInChildren<Transform>(true).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
 a.Add(new JObject{["path"]=p,["guid"]=AssetDatabase.AssetPathToGUID(p),["resolved"]=AssetDatabase.LoadMainAssetAtPath(p)!=null,["missingScripts"]=missing,["dependencies"]=new JArray(AssetDatabase.GetDependencies(p,true).Select(AssetDatabase.AssetPathToGUID).Where(x=>!string.IsNullOrEmpty(x)).Distinct().OrderBy(x=>x))});}return a;}
 public static object Before(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Preserve active Play Mode; no moves performed");var r=new JObject{["state"]=State(),["assets"]=Audit()};File.WriteAllText("Temp/prefab-editor-before.json",r.ToString());return new{assets=((JArray)r["assets"]).Count,state=r["state"],missing=((JArray)r["assets"]).Where(x=>(int)x["missingScripts"]>0).ToArray()};}
 static void Folder(string p){if(AssetDatabase.IsValidFolder(p))return;Folder(p.Substring(0,p.LastIndexOf('/')));var parent=p.Substring(0,p.LastIndexOf('/'));if(string.IsNullOrEmpty(AssetDatabase.CreateFolder(parent,p.Substring(p.LastIndexOf('/')+1))))throw new Exception("Cannot create "+p);}
 public static object Move(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Play Mode active");if(PrefabStageUtility.GetCurrentPrefabStage()!=null)throw new Exception("Prefab Stage open; preserve editing state");var m=JObject.Parse(File.ReadAllText(Manifest));var moves=(JArray)m["moves"];
 foreach(var e in moves){string from=(string)e["old"],to=(string)e["new"];if(AssetDatabase.AssetPathToGUID(from)!=(string)e["guid"]||File.Exists(to))throw new Exception("Preflight failed: "+from);}
 foreach(var e in moves){string from=(string)e["old"],to=(string)e["new"];Folder(to.Substring(0,to.LastIndexOf('/')));string error=AssetDatabase.MoveAsset(from,to);if(error!="")throw new Exception(error);File.AppendAllText("Temp/prefab-moves.log",from+" -> "+to+"\n");}
 return new{moved=moves.Count};}
 public static object Cleanup(){var removed=new JArray();var root="Assets/Prefabs/Props/DestructibleObjects_Prefabs";if(Directory.Exists(root))foreach(var p in Directory.GetDirectories(root,"*",SearchOption.AllDirectories).Append(root).OrderByDescending(x=>x.Length)){if(Directory.GetFileSystemEntries(p).Length==0){if(!AssetDatabase.DeleteAsset(p.Replace('\\','/')))throw new Exception("Cannot remove empty "+p);removed.Add(p);}}AssetDatabase.ImportAsset("Assets/Prefabs/Player/Variants/StarterAssets_PlayerContent.prefab",ImportAssetOptions.ForceUpdate);return removed;}
 public static object After(){var before=JObject.Parse(File.ReadAllText("Temp/prefab-editor-before.json"));var after=Audit();var errors=new JArray();foreach(var b in before["assets"]){var a=after.FirstOrDefault(x=>(string)x["guid"]==(string)b["guid"]);if(a==null||!(bool)a["resolved"]||(int)a["missingScripts"]!=(int)b["missingScripts"]||!JToken.DeepEquals(a["dependencies"],b["dependencies"]))errors.Add(b["path"]);}
 var state=State();if(!JToken.DeepEquals(before["state"],state))errors.Add("Scene/Play Mode state changed");var result=new JObject{["passed"]=errors.Count==0,["errors"]=errors,["assetsChecked"]=after.Count,["state"]=state,["preexistingMissingScripts"]=new JArray(after.Where(x=>(int)x["missingScripts"]>0))};File.WriteAllText("Assets/Plan/ARCHIVE/Organization/prefab-editor-validation.json",result.ToString());if(errors.Count>0)throw new Exception(result.ToString());return result;}
}
