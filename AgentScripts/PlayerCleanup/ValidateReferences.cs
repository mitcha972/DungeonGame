using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class ValidateCharacterReferences
{
    public static void Main()
    {
        if(EditorApplication.isPlaying)throw new Exception("Reference validation requires Edit Mode.");
        var deleted=JsonConvert.DeserializeObject<Deleted[]>(File.ReadAllText("Temp/PlayerCleanup/deleted.json")).Select(x=>x.guid).ToList();
        var issues=new List<string>(); var callbacks=new List<string>(); int objects=0,references=0;
        var paths=new[]{"Assets/Prefabs/Player/PlayerHolder.prefab","Assets/Prefabs/Player/Player_Main.prefab","Assets/Prefabs/Enemies/Enemy_Main.prefab","Assets/Prefabs/NPC/NPC_Main.prefab"};
        foreach(var path in paths)
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
            {
                objects++;
                foreach(var component in t.GetComponents<Component>())
                {
                    if(component==null){issues.Add(path+"/"+t.name+": missing script");continue;}
                    if(!(component is MonoBehaviour))continue;
                    var so=new SerializedObject(component);var p=so.GetIterator();
                    while(p.Next(true))
                    {
                        if(p.propertyType==SerializedPropertyType.ObjectReference)
                        {
                            references++;
                            if(p.objectReferenceValue==null&&p.objectReferenceInstanceIDValue!=0)issues.Add(path+"/"+t.name+"."+p.propertyPath+": broken reference");
                        }
                        if(p.propertyType==SerializedPropertyType.String&&p.name=="m_MethodName"&&!string.IsNullOrEmpty(p.stringValue))
                        {
                            string parent=p.propertyPath.Substring(0,p.propertyPath.LastIndexOf('.'));
                            var target=so.FindProperty(parent+".m_Target")?.objectReferenceValue;
                            string description=path+"/"+t.name+": "+target?.GetType().Name+"."+p.stringValue;
                            callbacks.Add(description);
                            if(target==null)issues.Add(description+" has no target");
                            else if(!target.GetType().GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Any(m=>m.Name==p.stringValue))issues.Add(description+" has no method");
                        }
                    }
                }
            }
        }
        var open=Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
        var scenes=open.Select(s=>new{s.path,s.isDirty,s.isLoaded}).ToArray();
        Directory.CreateDirectory("Assets/Plan/Validation");
        File.WriteAllText("Assets/Plan/Validation/player-cleanup-references.json",JsonConvert.SerializeObject(new{passed=issues.Count==0,objects,references,callbacks,issues,scenes},Formatting.Indented));
        if(issues.Count>0)throw new Exception(string.Join("; ",issues.Take(12)));
    }
    private sealed class Deleted{public string path; public string guid;}
}
