using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
public static class FinishMap {
public static string Inspect() {
var root=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/PlayerHolder.prefab");
var map=root.GetComponentInChildren<MapController>(true);
return string.Join("\n",map.GetComponentsInChildren<Transform>(true).Select(t=>t.name+" "+string.Join(",",t.GetComponents<Component>().Select(c=>c.GetType().Name))))+"\n"+string.Join("\n",root.GetComponentsInChildren<Button>(true).Where(b=>b.name.StartsWith("MinMap")).Select(b=>b.name+" "+string.Join(",",Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Select(i=>b.onClick.GetPersistentTarget(i)+"."+b.onClick.GetPersistentMethodName(i)))));
}
public static string Run() {
var path="Assets/Prefabs/Player/PlayerHolder.prefab";var root=PrefabUtility.LoadPrefabContents(path);
try {
var map=root.GetComponentInChildren<MapController>(true);
var bg=map.mapPanel.Find("MapDisplayBackground").GetComponent<Image>();
bg.material=null;bg.type=Image.Type.Simple;bg.color=new Color(.48f,.36f,.18f,1);
foreach(var text in map.mapPanel.GetComponentsInChildren<UnityEngine.UI.Text>(true)) text.gameObject.SetActive(false);
map.GetComponentsInChildren<Transform>(true).First(t=>t.name=="FloorNumText").gameObject.SetActive(false);
map.floorText.alignment=TMPro.TextAlignmentOptions.Center;
map.floorText.color=new Color(.96f,.87f,.62f);
map.floorText.rectTransform.localScale=Vector3.one;
PrefabUtility.SaveAsPrefabAsset(root,path);
}finally{PrefabUtility.UnloadPrefabContents(root);}
return "Cleaned inherited minimap presentation";
}}


