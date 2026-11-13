using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;
public static class OrganizeAnimations {
 static void Folder(string p){if(AssetDatabase.IsValidFolder(p))return;int n=p.LastIndexOf('/');Folder(p.Substring(0,n));AssetDatabase.CreateFolder(p.Substring(0,n),p.Substring(n+1));}
 static JArray Snapshot(){return new JArray(AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/")&&!AssetDatabase.IsValidFolder(p)&&(p.EndsWith(".prefab")||p.EndsWith(".unity")||p.EndsWith(".controller")||p.EndsWith(".overrideController")||p.EndsWith(".asset"))).Select(p=>new JObject{["guid"]=AssetDatabase.AssetPathToGUID(p),["path"]=p,["dependencies"]=new JArray(AssetDatabase.GetDependencies(p,true).Select(AssetDatabase.AssetPathToGUID).OrderBy(x=>x))}));}
 public static object Run(){if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()!=null)throw new Exception("Preserve Play Mode/prefab stage");var moves=JArray.Parse(File.ReadAllText("Assets/Plan/Validation/animation-moves.json"));foreach(var m in moves)if(AssetDatabase.AssetPathToGUID((string)m["old"])!=(string)m["guid"]||File.Exists((string)m["new"]))throw new Exception("Preflight: "+m);
 var before=Snapshot();File.WriteAllText("Temp/animation-dependencies-before.json",before.ToString());var identities=new JArray();foreach(var m in moves)foreach(var o in AssetDatabase.LoadAllAssetsAtPath((string)m["old"]))if(o!=null&&AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o,out string guid,out long id))identities.Add(new JObject{["guid"]=guid,["id"]=id,["type"]=o.GetType().FullName});File.WriteAllText("Temp/animation-subassets-before.json",identities.ToString());
 foreach(var m in moves){string to=(string)m["new"];Folder(to.Substring(0,to.LastIndexOf('/')));var error=AssetDatabase.MoveAsset((string)m["old"],to);if(error!="")throw new Exception(error);}
 // Only remove now-empty folders under the two authorized animation roots.
 foreach(var root in new[]{"Assets/Animations","Assets/Animators"})if(Directory.Exists(root))foreach(var dir in Directory.GetDirectories(root,"*",SearchOption.AllDirectories).OrderByDescending(x=>x.Length))if(Directory.GetFileSystemEntries(dir).Length==0)AssetDatabase.DeleteAsset(dir.Replace('\\','/'));
 if(Directory.Exists("Assets/Animators")&&Directory.GetFileSystemEntries("Assets/Animators").Length==0)AssetDatabase.DeleteAsset("Assets/Animators");return Verify();}
 public static object Verify(){var before=JArray.Parse(File.ReadAllText("Temp/animation-dependencies-before.json"));var errors=new JArray();foreach(var b in before){var path=AssetDatabase.GUIDToAssetPath((string)b["guid"]);var deps=new JArray(AssetDatabase.GetDependencies(path,true).Select(AssetDatabase.AssetPathToGUID).OrderBy(x=>x));if(string.IsNullOrEmpty(path)||!JToken.DeepEquals(deps,b["dependencies"]))errors.Add(b["path"]);}
 var ids=JArray.Parse(File.ReadAllText("Temp/animation-subassets-before.json"));foreach(var group in ids.GroupBy(x=>(string)x["guid"])){var found=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(group.Key)).Where(o=>o!=null).Select(o=>{AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o,out string g,out long id);return id;}).ToArray();foreach(var o in group)if(!found.Contains((long)o["id"]))errors.Add("Missing local file ID: "+o);}
 var r=new JObject{["passed"]=errors.Count==0,["dependencyAssets"]=before.Count,["subassets"]=ids.Count,["moves"]=JArray.Parse(File.ReadAllText("Assets/Plan/Validation/animation-moves.json")).Count,["errors"]=errors};File.WriteAllText("Assets/Plan/Validation/animation-organization.json",r.ToString());return r;}
}
