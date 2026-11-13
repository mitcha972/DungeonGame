using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor;
public static class BuildMap {
static RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 anchor,Vector2 pos) {
var t=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); t.SetParent(parent,false);t.anchorMin=t.anchorMax=anchor;t.sizeDelta=size;t.anchoredPosition=pos;return t;
}
static void Stretch(RectTransform t) {t.anchorMin=Vector2.zero;t.anchorMax=Vector2.one;t.offsetMin=Vector2.zero;t.offsetMax=Vector2.zero;}
static TMPro.TextMeshProUGUI Text(string name,Transform parent,string value,Vector2 size,Vector2 anchor,Vector2 pos,float font) {
var t=Rect(name,parent,size,anchor,pos).gameObject.AddComponent<TMPro.TextMeshProUGUI>();t.text=value;t.fontSize=font;t.alignment=TMPro.TextAlignmentOptions.Center;t.color=new Color(.96f,.87f,.62f);t.raycastTarget=false;return t;
}
public static string Run() {
if(EditorApplication.isPlaying) return "Refused during Play Mode";
var path="Assets/Prefabs/Player/PlayerHolder.prefab";
var root=PrefabUtility.LoadPrefabContents(path);
try {
var all=root.GetComponentsInChildren<Transform>(true);
var panel=all.First(t=>t.name=="MapDisplay" && t.parent.name=="UICanvas") as RectTransform;
var camera=all.First(t=>t.name=="MapCamera").GetComponent<Camera>();
var raw=panel.GetComponentInChildren<RawImage>(true);
var floor=panel.GetComponentsInChildren<TMPro.TMP_Text>(true).FirstOrDefault();
var map=panel.GetComponent<MapController>() ?? panel.gameObject.AddComponent<MapController>();
map.target=camera.transform.parent;map.mapCamera=camera;map.mapPanel=panel;map.display=raw;map.floorText=floor;
camera.cullingMask=(1<<0)|(1<<6)|(1<<9)|(1<<11)|(1<<12);
camera.targetDisplay=0;
var circle=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
var bg=panel.GetComponentInChildren<Image>(true);bg.sprite=circle;bg.color=new Color(.45f,.34f,.18f);bg.raycastTarget=false;Stretch(bg.rectTransform);
var viewport=Rect("CircularViewport",panel,new Vector2(208,208),new Vector2(.5f,.5f),Vector2.zero);
Stretch(viewport);viewport.offsetMin=new Vector2(5,5);viewport.offsetMax=new Vector2(-5,-5);
var image=viewport.gameObject.AddComponent<Image>();image.sprite=circle;image.raycastTarget=false;
viewport.gameObject.AddComponent<Mask>().showMaskGraphic=false;
raw.transform.SetParent(viewport,false);Stretch(raw.rectTransform);raw.raycastTarget=false;raw.color=Color.white;
var marker=Text("PlayerHeading",viewport,"▲",new Vector2(28,28),new Vector2(.5f,.5f),Vector2.zero,25);
map.playerMarker=marker.rectTransform;
var bubble=Rect("NorthBubble",panel,new Vector2(34,34),new Vector2(.5f,1),new Vector2(0,-4));
var b=bubble.gameObject.AddComponent<Image>();b.sprite=circle;b.color=new Color(.22f,.12f,.07f);b.raycastTarget=false;
Stretch(Text("North",bubble,"N",Vector2.zero,new Vector2(.5f,.5f),Vector2.zero,23).rectTransform);
if(floor!=null) { floor.rectTransform.anchorMin=floor.rectTransform.anchorMax=new Vector2(.5f,0);floor.rectTransform.anchoredPosition=new Vector2(0,12);floor.rectTransform.sizeDelta=new Vector2(180,26);floor.fontSize=18;floor.raycastTarget=false;}
Text("MapHint",panel,"M · Map",new Vector2(180,24),new Vector2(.5f,0),new Vector2(0,-14),16);
map.SetExpanded(false);
var state=root.GetComponentInChildren<PlayerControlState>(true);
var so=new SerializedObject(state);so.FindProperty("_map").objectReferenceValue=map;so.ApplyModifiedPropertiesWithoutUndo();
PrefabUtility.SaveAsPrefabAsset(root,path);
} finally {PrefabUtility.UnloadPrefabContents(root);}
var inputPath="Assets/GamePlay/Character/InputSystem/mehh.inputactions";
var asset=AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
var actions=asset.actionMaps.First(m=>m.FindAction("Journal",false)!=null);
if(actions.FindAction("Map",false)==null) {var action=actions.AddAction("Map",InputActionType.Button);action.AddBinding("<Keyboard>/m").WithGroup("KeyboardMouse");System.IO.File.WriteAllText(inputPath,asset.ToJson());AssetDatabase.ImportAsset(inputPath);}
AssetDatabase.SaveAssets();
return "Updated PlayerHolder minimap and Map input";
}}
