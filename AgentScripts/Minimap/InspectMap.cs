using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public static class InspectMap {
public static string Run() {
var root=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/PlayerHolder.prefab");
return string.Join("\n",root.GetComponentsInChildren<Transform>(true).Where(t=>t.name.ToLower().Contains("map")).Select(t=>t.name+" parent="+t.parent?.name+" active="+t.gameObject.activeSelf+" components="+string.Join(",",t.GetComponents<Component>().Select(c=>c.GetType().Name))+" children="+string.Join(",",t.Cast<Transform>().Select(c=>c.name))))+"\nSCENE dirty="+UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty+"\n"+string.Join("\n",root.GetComponentsInChildren<RawImage>(true).Select(i=>i.name+" tex="+i.texture?.name))+"\n"+string.Join("\n",Enumerable.Range(0,32).Select(i=>i+":"+LayerMask.LayerToName(i)));
}}
