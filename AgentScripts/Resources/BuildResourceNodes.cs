using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
public static class BuildResourceNodes
{
 const string Data="Assets/ResourceNodes";
 const string Prefabs="Assets/Prefabs/WorldInteract";
 public const string TestScene="Assets/Scenes/Test_Scenes/ResourceNodes_Test.unity";
 static void Folder(string p){if(AssetDatabase.IsValidFolder(p))return;int i=p.LastIndexOf('/');Folder(p.Substring(0,i));AssetDatabase.CreateFolder(p.Substring(0,i),p.Substring(i+1));}
 static T Asset<T>(string name) where T:ScriptableObject {string p=Data+"/"+name+".asset";var a=AssetDatabase.LoadAssetAtPath<T>(p);if(a==null){a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,p);}return a;}
 static Material Mat(string name,Color c){string p=Data+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,p);}m.color=c;EditorUtility.SetDirty(m);return m;}
 static GameObject Shape(string name,Transform parent,PrimitiveType type,Vector3 pos,Vector3 scale,Material m){var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=m;return g;}
 static GameObject Child(string name,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);return g;}
 static GameObject Model(string path,Transform parent,float height){var p=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(p==null)throw new Exception(path);var g=(GameObject)PrefabUtility.InstantiatePrefab(p);g.transform.SetParent(parent,false);g.transform.localPosition=Vector3.zero;foreach(var c in g.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(c);var renderers=g.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);float s=height/Mathf.Max(.01f,b.size.y);g.transform.localScale*=s;g.transform.localPosition-=new Vector3(b.center.x,b.min.y,b.center.z)*s;return g;}
 static void Controller(AnimatorController c,AnimationClip collect,AnimationClip mine){if(c==null)return;var sm=c.layers[0].stateMachine;string[] names={"GatheringCollect","GatheringChop","GatheringMine"};for(int i=0;i<3;i++){var s=sm.states.Select(x=>x.state).FirstOrDefault(x=>x.name==names[i])??sm.AddState(names[i]);s.motion=i==0?collect:mine;s.writeDefaultValues=true;}EditorUtility.SetDirty(c);}
 static void WirePlayer(GameObject root)
 {
 var pc=root.GetComponentInChildren<PlayerController>(true);if(pc==null)throw new Exception("PlayerController missing");
 var gathering=pc.GetComponent<PlayerGathering>();if(gathering==null)gathering=pc.gameObject.AddComponent<PlayerGathering>();
 var canvas=root.GetComponentsInChildren<Canvas>(true).First(x=>x.renderMode!=RenderMode.WorldSpace);
 var hud=canvas.GetComponentInChildren<GatheringProgressHUD>(true);
 if(hud==null){var go=new GameObject("GatheringHUD",typeof(RectTransform),typeof(GatheringProgressHUD));go.transform.SetParent(canvas.transform,false);hud=go.GetComponent<GatheringProgressHUD>();var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.35f);rt.sizeDelta=new Vector2(300,10);rt.anchoredPosition=Vector2.zero;
 var bar=new GameObject("Progress",typeof(RectTransform),typeof(UnityEngine.UI.Image));bar.transform.SetParent(rt,false);var br=(RectTransform)bar.transform;br.anchorMin=Vector2.zero;br.anchorMax=Vector2.one;br.offsetMin=br.offsetMax=Vector2.zero;var bg=bar.GetComponent<UnityEngine.UI.Image>();bg.color=new Color(.12f,.09f,.055f,.95f);bg.raycastTarget=false;
 var fill=new GameObject("Fill",typeof(RectTransform),typeof(UnityEngine.UI.Image));fill.transform.SetParent(br,false);var fr=(RectTransform)fill.transform;fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=new Vector2(2,2);fr.offsetMax=new Vector2(-2,-2);hud.progressFill=fill.GetComponent<UnityEngine.UI.Image>();hud.progressFill.color=new Color(.73f,.57f,.24f);hud.progressFill.raycastTarget=false;hud.progressRoot=bar;bar.SetActive(false);}
 hud.owner=gathering;
 var prompt=pc.InteractionText;if(prompt!=null){prompt.raycastTarget=false;prompt.color=new Color(1,.88f,.53f);prompt.fontSize=22;prompt.alignment=TextAlignmentOptions.Center;prompt.textWrappingMode=TextWrappingModes.Normal;var rt=prompt.rectTransform;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.4f);rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=Vector2.zero;rt.sizeDelta=new Vector2(650,80);}
 }
 public static object Build()
 {
 if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode before authoring");
 var scene=SceneManager.GetActiveScene();if(!scene.path.EndsWith("Gameplay_Test.unity"))throw new Exception("Expected user's Gameplay_Test scene");
 Folder(Data);
 var potion=AssetDatabase.LoadAssetAtPath<Item>("Assets/GamePlay/ScriptableObjects/Items/Consumables/Health Potion.asset");if(potion==null)throw new Exception("Existing healing potion missing");
 var wood=Asset<Item>("Logs");wood.name="Logs";wood.description="A sturdy log, freshly cut. Useful supplies for future crafting.";wood.itemType=ItemType.Resource;wood.price=4;wood.icon=AssetDatabase.LoadAssetAtPath<Sprite>(Data+"/LogsIcon.png")??potion.icon;EditorUtility.SetDirty(wood);
 var ore=Asset<Item>("CopperOre");ore.name="Copper ore";ore.description="A rough piece of copper-bearing ore.";ore.itemType=ItemType.Resource;ore.price=5;ore.icon=AssetDatabase.LoadAssetAtPath<Sprite>(Data+"/CopperOreIcon.png")??potion.icon;EditorUtility.SetDirty(ore);
 var collect=AssetDatabase.LoadAssetAtPath<AnimationClip>(Data+"/GatheringCollectLoop.anim");if(collect==null){collect=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Misc/Gathering.anim"));collect.name="GatheringCollectLoop";var settings=AnimationUtility.GetAnimationClipSettings(collect);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(collect,settings);AssetDatabase.CreateAsset(collect,Data+"/GatheringCollectLoop.anim");}
 var mine=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Misc/MiningLoop.anim");
 var holder=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Player/PlayerHolder.prefab");try{WirePlayer(holder);var a=holder.GetComponentInChildren<PlayerController>(true).GetComponentInChildren<Animator>(true);Controller(a.runtimeAnimatorController as AnimatorController,collect,mine);PrefabUtility.SaveAsPrefabAsset(holder,"Assets/Prefabs/Player/PlayerHolder.prefab");}finally{PrefabUtility.UnloadPrefabContents(holder);}
 var livePlayer=scene.GetRootGameObjects().First(x=>x.GetComponentInChildren<PlayerController>(true)!=null);WirePlayer(livePlayer);var liveAnimator=livePlayer.GetComponentInChildren<PlayerController>(true).GetComponentInChildren<Animator>(true);Controller(liveAnimator.runtimeAnimatorController as AnimatorController,collect,mine);
 Material stone=Mat("WeatheredStone",new Color(.34f,.32f,.28f)),copper=Mat("Copper",new Color(.66f,.32f,.15f)),bark=Mat("Bark",new Color(.29f,.17f,.08f)),water=Mat("HealingWater",new Color(.6f,.13f,.25f));
 string[] names={"HealingPotionSource","WoodcuttingTree","CopperDeposit"};string[] displays={"Healing spring","Tree","Copper rocks"};string[] verbs={"Fill potion from","Chop","Mine"};string[] states={"GatheringCollect","GatheringChop","GatheringMine"};Item[] rewards={potion,wood,ore};float[] durations={2,3,4},respawns={60,45,90};
 for(int i=0;i<3;i++){
 var d=Asset<ResourceNodeDefinition>(names[i]);d.displayName=displays[i];d.actionLabel=verbs[i];d.resourceItem=rewards[i];d.quantity=1;d.collectionDuration=durations[i];d.respawnDuration=respawns[i];d.maxCharges=1;d.animationState=states[i];EditorUtility.SetDirty(d);
 string path=Prefabs+"/"+(i==1?"ResourceNode_Tree":"ResourceNode_"+names[i])+".prefab";
 var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);var root=existing!=null?PrefabUtility.LoadPrefabContents(path):new GameObject("ResourceNode_"+names[i]);
 try{
 // Existing tree is the user's unconfigured cube placeholder; retain prefab GUID/root identity.
 foreach(Transform child in root.transform.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
 foreach(var old in root.GetComponents<ResourceNode>())UnityEngine.Object.DestroyImmediate(old);
 root.transform.position=Vector3.zero;var node=root.AddComponent<ResourceNode>();node.definition=d;node.audioSource=root.GetComponent<AudioSource>();if(node.audioSource==null)node.audioSource=root.AddComponent<AudioSource>();node.audioSource.playOnAwake=false;
 var available=Child("Available",root.transform);var depleted=Child("Depleted",root.transform);node.availableVisual=available;node.depletedVisual=depleted;
 var collider=root.GetComponent<BoxCollider>();if(collider==null)collider=root.AddComponent<BoxCollider>();collider.center=new Vector3(0,.85f,0);collider.size=new Vector3(1.4f,1.7f,1.4f);
 if(i==0){Shape("Basin",root.transform,PrimitiveType.Cylinder,new Vector3(0,.4f,0),new Vector3(1.5f,.4f,1.5f),stone);Shape("HealingPool",available.transform,PrimitiveType.Cylinder,new Vector3(0,.82f,0),new Vector3(1.25f,.025f,1.25f),water);Model("Assets/Prefabs/Props/Potions/Elixir_Of_Health.prefab",available.transform,.55f).transform.localPosition+=new Vector3(0,.85f,0);Shape("EmptyBasin",depleted.transform,PrimitiveType.Cylinder,new Vector3(0,.81f,0),new Vector3(1.2f,.02f,1.2f),stone);}
 if(i==1){Model("Assets/Plugins/NavMeshComponents-master/Assets/Examples/Prefabs/BanyanTree.prefab",available.transform,4.2f);Shape("Stump",depleted.transform,PrimitiveType.Cylinder,new Vector3(0,.23f,0),new Vector3(.8f,.23f,.8f),bark);collider.center=new Vector3(0,1,0);collider.size=new Vector3(1.1f,2,1.1f);}
 if(i==2){Model("Assets/Prefabs/Props/RocksAndCrystals/Stone_1.prefab",root.transform,1.2f);for(int k=0;k<5;k++){var g=Shape("CopperVein",available.transform,PrimitiveType.Cube,new Vector3((k%2==0?-.32f:.32f),.7f+k*.1f, k*.13f-.3f),new Vector3(.2f,.25f,.24f),copper);g.transform.localRotation=Quaternion.Euler(k*23,30,20);}Shape("SpentOre",depleted.transform,PrimitiveType.Sphere,new Vector3(0,.8f,0),new Vector3(.35f,.2f,.35f),stone);}
 depleted.SetActive(false);PrefabUtility.SaveAsPrefabAsset(root,path);
 }finally{if(existing!=null)PrefabUtility.UnloadPrefabContents(root);else UnityEngine.Object.DestroyImmediate(root);}
 }
 Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Add resource gathering test area");
 var positions=new[]{new Vector3(-3,0,-1),new Vector3(1.60102f,0,3.78819f),new Vector3(3,0,-1)};
 for(int i=0;i<3;i++){string path=Prefabs+"/"+(i==1?"ResourceNode_Tree":"ResourceNode_"+names[i])+".prefab";var existing=scene.GetRootGameObjects().FirstOrDefault(x=>x.name==(i==1?"ResourceNode_Tree":"ResourceNode_"+names[i]));if(existing!=null)continue;var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),scene);Undo.RegisterCreatedObjectUndo(g,"Place resource node");g.transform.position=positions[i];PrefabUtility.RecordPrefabInstancePropertyModifications(g.transform);}
 Undo.CollapseUndoOperations(group);EditorSceneManager.MarkSceneDirty(scene);
 AssetDatabase.SaveAssets();
 // Save a separate copy; never commit unrelated unsaved work to Gameplay_Test.
 if(!EditorSceneManager.SaveScene(scene,TestScene,true))throw new Exception("Test scene copy save failed");
 return new{implemented=true,prefabs=names,sceneCopy=TestScene,originalScene=scene.path,originalStillDirty=scene.isDirty};
 }
}




