using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
public static class RepairSerializedCharacterReferences
{
    public static void Main()
    {
        if(EditorApplication.isPlaying)throw new Exception("Edit Mode required.");
        var repairs=new List<string>();
        foreach(var path in new[]{"Assets/Prefabs/Player/Player_Main.prefab","Assets/Prefabs/Player/PlayerHolder.prefab"})
        {
            var root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach(var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if(component==null)continue;
                    var so=new SerializedObject(component); var p=so.GetIterator();
                    var broken=new List<string>();var invalid=new Dictionary<string,List<int>>();
                    while(p.Next(true))
                    {
                        if(p.propertyType==SerializedPropertyType.ObjectReference&&p.objectReferenceValue==null&&p.objectReferenceInstanceIDValue!=0)broken.Add(p.propertyPath);
                        if(p.propertyType!=SerializedPropertyType.String||p.name!="m_MethodName"||string.IsNullOrEmpty(p.stringValue))continue;
                        string parent=p.propertyPath.Substring(0,p.propertyPath.LastIndexOf('.'));
                        var target=so.FindProperty(parent+".m_Target")?.objectReferenceValue;
                        if(target!=null&&p.stringValue=="OnRemoveButton"&&target is InventorySlot)
                        {
                            p.stringValue="RemoveFromInventory";repairs.Add(path+"/"+component.name+": repaired inventory remove callback");continue;
                        }
                        bool exists=target!=null&&target.GetType().GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Any(m=>m.Name==p.stringValue);
                        if(exists)continue;
                        var match=Regex.Match(p.propertyPath,@"^(.*\.m_Calls)\.Array\.data\[(\d+)\]\.m_MethodName$");
                        if(!match.Success)throw new Exception("Unknown callback layout: "+p.propertyPath);
                        string array=match.Groups[1].Value;
                        if(!invalid.ContainsKey(array))invalid[array]=new List<int>();invalid[array].Add(int.Parse(match.Groups[2].Value));
                        repairs.Add(path+"/"+component.name+": removed invalid callback "+p.stringValue);
                    }
                    foreach(var field in broken){so.FindProperty(field).objectReferenceValue=null;repairs.Add(path+"/"+component.name+": cleared missing optional asset "+field);}
                    foreach(var array in invalid)
                        foreach(int i in array.Value.Distinct().OrderByDescending(i=>i))so.FindProperty(array.Key).DeleteArrayElementAtIndex(i);
                    if(component is UnityEngine.InputSystem.PlayerInput)
                    {
                        var events=so.FindProperty("m_ActionEvents");
                        if(events!=null&&events.arraySize>0){events.arraySize=0;repairs.Add(path+"/"+component.name+": removed obsolete UnityEvent input forwarding array");}
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root,path,out bool saved);if(!saved)throw new Exception("Cannot save reference repair: "+path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        File.WriteAllText("Assets/Plan/Validation/player-cleanup-reference-repairs.json",JsonConvert.SerializeObject(repairs,Formatting.Indented));
    }
}
