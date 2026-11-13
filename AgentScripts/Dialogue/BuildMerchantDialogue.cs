using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using DungeonGame.Dialogue;
public static class BuildMerchantDialogue
{
 const string Data="Assets/Dialogue";
 const string PanelPath="Assets/Prefabs/Dialogue/DialoguePanel.prefab";
 const string MerchantPath="Assets/Prefabs/WorldInteract/DialogueMerchant.prefab";
 const string ScenePath="Assets/Scenes/Test_Scenes/DialogueMerchant_Test.unity";
 static TMP_FontAsset font;
 static void Folder(string p) { if(AssetDatabase.IsValidFolder(p))return; var k=p.LastIndexOf('/');Folder(p.Substring(0,k));AssetDatabase.CreateFolder(p.Substring(0,k),p.Substring(k+1)); }
 static T Asset<T>(string p) where T:ScriptableObject {var a=AssetDatabase.LoadAssetAtPath<T>(p);if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,p);}return a;}
 static RectTransform Rect(string n,Transform parent) {var g=new GameObject(n,typeof(RectTransform));g.transform.SetParent(parent,false);return (RectTransform)g.transform;}
 static TextMeshProUGUI Text(string n,Transform parent,string text,int size,int height) {var r=Rect(n,parent);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=text;t.fontSize=size;t.color=new Color(.94f,.89f,.76f);t.raycastTarget=false;var l=r.gameObject.AddComponent<LayoutElement>();l.preferredHeight=height;l.minHeight=height;return t;}
 static ServiceDialogueAction Action(DialogueService kind) {var a=Asset<ServiceDialogueAction>(Data+"/"+kind+".asset");a.service=kind;EditorUtility.SetDirty(a);return a;}
 public static object Build()
 {
 if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first.");
 Folder(Data);Folder("Assets/Prefabs/Dialogue");
 font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Plugins/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
 var trade=Action(DialogueService.OpenShop);var end=Action(DialogueService.EndDialogue);
 Action(DialogueService.OpenBank);Action(DialogueService.StartQuest);Action(DialogueService.OpenService);
 var conversation=Asset<DialogueConversation>(Data+"/MerchantConversation.asset");
 conversation.entryNode="greeting";
 conversation.nodes=new[] {
 new DialogueNode{id="greeting",text="Welcome, traveller. Steel, supplies, or a story for the road? Take your time - the dungeon will not wait for either of us.",responses=new[] {
 new DialogueResponse{text="Trade",action=trade},new DialogueResponse{text="Ask About Wares",nextNode="wares"},new DialogueResponse{text="Ask About Rumors",nextNode="rumors"},new DialogueResponse{text="Goodbye",action=end}}},
 new DialogueNode{id="wares",text="A sturdy wooden shield, an iron sword, and a potion for when courage runs thin. Simple tools, honestly sold. Have a look before you head below.",responses=new[] {new DialogueResponse{text="Let me see your wares.",action=trade},new DialogueResponse{text="Something else...",nextNode="greeting"},new DialogueResponse{text="Goodbye",action=end}}},
 new DialogueNode{id="rumors",text="They say the lower halls remember every adventurer who never came back. I say bring a shield, watch the corners, and never trust a quiet doorway.",responses=new[] {new DialogueResponse{text="Back to business.",nextNode="greeting"},new DialogueResponse{text="Goodbye",action=end}}}};
 EditorUtility.SetDirty(conversation);
 var preview=EditorSceneManager.NewPreviewScene();
 try {
 var r=Rect("DialoguePanel",null);SceneManager.MoveGameObjectToScene(r.gameObject,preview);
 r.anchorMin=new Vector2(.5f,.03f);r.anchorMax=r.anchorMin;r.pivot=new Vector2(.5f,0);r.sizeDelta=new Vector2(1080,450);
 var image=r.gameObject.AddComponent<Image>();image.color=new Color(.10f,.085f,.065f,.98f);
 var v=r.gameObject.AddComponent<VerticalLayoutGroup>();v.padding=new RectOffset(28,28,18,18);v.spacing=10;v.childControlWidth=true;v.childControlHeight=true;v.childForceExpandHeight=false;
 var panel=r.gameObject.AddComponent<DialoguePanel>();
 panel.speakerText=Text("Speaker",r,"Merchant",28,36);panel.speakerText.color=new Color(.94f,.70f,.31f);
 panel.npcText=Text("NpcText",r,"NPC dialogue",23,100);
 var responses=Rect("Responses",r);var layout=responses.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=6;layout.childControlHeight=true;layout.childControlWidth=true;layout.childForceExpandHeight=false;
 var le=responses.gameObject.AddComponent<LayoutElement>();le.preferredHeight=192;le.flexibleHeight=1;
 var b=Rect("ResponseTemplate",responses);b.gameObject.AddComponent<LayoutElement>().preferredHeight=42;
 var bi=b.gameObject.AddComponent<Image>();bi.color=new Color(.22f,.18f,.12f);
 var button=b.gameObject.AddComponent<Button>();button.targetGraphic=bi;var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.86f,.56f);colors.selectedColor=colors.highlightedColor;button.colors=colors;
 var label=Text("Label",b,"Response",22,40);var lr=label.rectTransform;lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;lr.offsetMin=new Vector2(14,0);lr.offsetMax=new Vector2(-14,0);label.alignment=TextAlignmentOptions.MidlineLeft;
 panel.responseTemplate=button;panel.responseParent=responses;button.gameObject.SetActive(false);
 Text("Help",r,"Click a response | Arrows + Enter | 1-9 | Esc to leave",17,26).color=new Color(.72f,.67f,.55f);
 PrefabUtility.SaveAsPrefabAsset(r.gameObject,PanelPath);
 } finally {EditorSceneManager.ClosePreviewScene(preview);}
 var holder=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Player/PlayerHolder.prefab");
 try {
 var state=holder.GetComponentInChildren<PlayerControlState>(true);
 var controller=state.GetComponent<DialogueController>()??state.gameObject.AddComponent<DialogueController>();
 var shopUI=holder.GetComponentInChildren<ShopInventoryUI>(true);
 // ShopUI must live outside InventoryUIs, which is hidden while exploring.
 var shopRect=(RectTransform)shopUI.transform;shopRect.SetParent(shopUI.GetComponentInParent<Canvas>(true).transform,false);shopRect.anchorMin=shopRect.anchorMax=new Vector2(.5f,.5f);shopRect.pivot=new Vector2(.5f,.5f);shopRect.anchoredPosition=Vector2.zero;shopRect.sizeDelta=new Vector2(350,380);shopUI.gameObject.SetActive(false);
 if(controller.panel==null) {
 var p=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath),shopUI.GetComponentInParent<Canvas>(true).transform);
 controller.panel=p.GetComponent<DialoguePanel>();p.SetActive(false);
 }
 var so=new SerializedObject(state);so.FindProperty("_dialogue").objectReferenceValue=controller;so.FindProperty("_shopUI").objectReferenceValue=shopUI;so.ApplyModifiedPropertiesWithoutUndo();
 PrefabUtility.SaveAsPrefabAsset(holder,"Assets/Prefabs/Player/PlayerHolder.prefab");
 } finally {PrefabUtility.UnloadPrefabContents(holder);}
 preview=EditorSceneManager.NewPreviewScene();
 try {
 var m=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WorldInteract/Shop.prefab"),preview);
 m.name="DialogueMerchant";m.transform.position=Vector3.zero;m.transform.localScale=Vector3.one;
 var shop=m.GetComponent<ShopScript>();var npc=m.GetComponent<DialogueNpc>()??m.AddComponent<DialogueNpc>();npc.speakerName="Rowan the Merchant";npc.conversation=conversation;npc.shop=shop;shop.dialogue=npc;
 var so=new SerializedObject(shop);var drops=so.FindProperty("itemDrops");drops.arraySize=3;
 var stock=new[]{"Assets/Prefabs/Items/Weapons/Sword-Iron.prefab","Assets/Prefabs/Items/Weapons/Shield-Wood.prefab","Assets/Prefabs/Items/Consumables/Potion.prefab"};
 for(int i=0;i<stock.Length;i++) {var item=AssetDatabase.LoadAssetAtPath<GameObject>(stock[i]);if(item.GetComponent<ItemInteract>()?.item==null)throw new Exception("Invalid merchant stock: "+stock[i]);var d=drops.GetArrayElementAtIndex(i);d.FindPropertyRelative("lootItem").objectReferenceValue=item;d.FindPropertyRelative("dropChance").intValue=100;}
 so.ApplyModifiedPropertiesWithoutUndo();
 // Original Shop is a cube: keep its collider and add a recognisable merchant silhouette.
 foreach(var renderer in m.GetComponentsInChildren<Renderer>())renderer.enabled=false;
 var body=GameObject.CreatePrimitive(PrimitiveType.Capsule);body.name="Merchant";body.transform.SetParent(m.transform,false);body.transform.localPosition=new Vector3(0,1,0);body.transform.localScale=new Vector3(.65f,1,.65f);UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());
 var material=AssetDatabase.LoadAssetAtPath<Material>(Data+"/MerchantCoat.mat");if(material==null){material=new Material(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline==null?Shader.Find("Standard"):Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,Data+"/MerchantCoat.mat");}material.color=new Color(.29f,.40f,.25f);EditorUtility.SetDirty(material);body.GetComponent<Renderer>().sharedMaterial=material;
 var collider=m.GetComponent<BoxCollider>();if(collider!=null){collider.center=new Vector3(0,1,0);collider.size=new Vector3(.8f,2,.8f);}
 PrefabUtility.SaveAsPrefabAsset(m,MerchantPath);
 } finally {EditorSceneManager.ClosePreviewScene(preview);}
 // Author a separate scene without touching the user's existing clean scene.
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
 var player=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/PlayerHolder.prefab"),scene);
 player.transform.position=Vector3.zero;var ps=player.GetComponentInChildren<PlayerControlState>(true);ps.transform.position=new Vector3(0,.05f,0);ps.transform.rotation=Quaternion.identity;
 var merchant=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(MerchantPath),scene);merchant.transform.position=new Vector3(0,0,2.8f);
 merchant.GetComponent<ShopScript>().shopInventoryUI=player.GetComponentInChildren<ShopInventoryUI>(true);PrefabUtility.RecordPrefabInstancePropertyModifications(merchant.GetComponent<ShopScript>());
 var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);SceneManager.MoveGameObjectToScene(floor,scene);floor.name="TradingPostFloor";floor.transform.position=new Vector3(0,-.3f,0);floor.transform.localScale=new Vector3(30,.5f,30);
 var floorMat=AssetDatabase.LoadAssetAtPath<Material>(Data+"/TradingPostFloor.mat");if(floorMat==null){floorMat=new Material(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline==null?Shader.Find("Standard"):Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(floorMat,Data+"/TradingPostFloor.mat");}floorMat.color=new Color(.25f,.28f,.30f);EditorUtility.SetDirty(floorMat);floor.GetComponent<Renderer>().sharedMaterial=floorMat;
 var light=new GameObject("Sun",typeof(Light));SceneManager.MoveGameObjectToScene(light,scene);light.GetComponent<Light>().type=LightType.Directional;light.GetComponent<Light>().intensity=2;light.transform.rotation=Quaternion.Euler(45,-30,0);
 var marker=GameObject.CreatePrimitive(PrimitiveType.Cube);SceneManager.MoveGameObjectToScene(marker,scene);marker.name="WorldSimulationMarker";marker.transform.position=new Vector3(2,1,3);marker.transform.localScale=Vector3.one*.4f;marker.AddComponent<DialogueWorldMarker>();
 var instruction=new GameObject("TestInstructions",typeof(TextMeshPro));SceneManager.MoveGameObjectToScene(instruction,scene);instruction.transform.position=new Vector3(0,2.8f,2.9f);instruction.transform.rotation=Quaternion.identity;var txt=instruction.GetComponent<TextMeshPro>();txt.font=font;txt.text="ROWAN'S TRADING POST\nLook at Rowan and press E";txt.fontSize=3;txt.alignment=TextAlignmentOptions.Center;txt.rectTransform.sizeDelta=new Vector2(6,1);
 EditorSceneManager.SaveScene(scene,ScenePath);EditorSceneManager.CloseScene(scene,true);AssetDatabase.SaveAssets();
 return new {panel=PanelPath,merchant=MerchantPath,scene=ScenePath,stock=3};
 }
}



