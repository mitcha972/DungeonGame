using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
public static class BuildSkills
{
 static T Asset<T>(string path) where T:ScriptableObject { var asset=AssetDatabase.LoadAssetAtPath<T>(path); if(asset==null){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}return asset; }
 static void Wire(GameObject player, SkillXpCurve curve, SkillDefinition[] definitions)
 {
  var progress=player.GetComponent<PlayerSkillProgress>();if(progress==null)progress=player.AddComponent<PlayerSkillProgress>();progress.curve=curve;progress.definitions=definitions;
  var canvas=player.transform.root.GetComponentsInChildren<Canvas>(true).FirstOrDefault(c=>c.renderMode!=RenderMode.WorldSpace);
  if(canvas==null)throw new Exception("No existing screen Canvas on "+player.name);
  var hud=player.transform.root.GetComponentInChildren<SkillFeedbackHUD>(true);
  if(hud==null){var go=new GameObject("SkillFeedbackHUD",typeof(RectTransform));go.transform.SetParent(canvas.transform,false);var rect=(RectTransform)go.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;hud=go.AddComponent<SkillFeedbackHUD>();}
  hud.progress=progress;hud.canvas=canvas;hud.worldCamera=player.transform.root.GetComponentsInChildren<Camera>(true).FirstOrDefault(c=>c.CompareTag("MainCamera"));
  hud.font=player.transform.root.GetComponentsInChildren<TMP_Text>(true).Select(t=>t.font).FirstOrDefault(f=>f!=null);
  hud.levelUpSound=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/UI_Click.mp3");hud.showDebug=false;
 }
 public static object Build()
 {
  if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new Exception("Requires stopped and compiled Editor");
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(!scene.path.EndsWith("/Gameplay_Test.unity"))throw new Exception("Expected Gameplay_Test");
  System.IO.Directory.CreateDirectory("Assets/Skills");System.IO.Directory.CreateDirectory("Assets/Items/Skillbooks");AssetDatabase.Refresh();
  var curve=Asset<SkillXpCurve>("Assets/Skills/SharedSkillXpCurve.asset");
  var definitions=new SkillDefinition[10];var books=new Skillbook[10];
  // Existing item icon serves as a consistent temporary placeholder.
  var icon=AssetDatabase.FindAssets("t:Item").Select(g=>AssetDatabase.LoadAssetAtPath<Item>(AssetDatabase.GUIDToAssetPath(g))).Where(i=>i!=null&&i.icon!=null).Select(i=>i.icon).First();
  for(int i=0;i<10;i++) {string name=((RpgSkill)i).ToString();definitions[i]=Asset<SkillDefinition>("Assets/Skills/"+name+".asset");definitions[i].skill=(RpgSkill)i;definitions[i].displayName=name;EditorUtility.SetDirty(definitions[i]);books[i]=Asset<Skillbook>("Assets/Items/Skillbooks/Skillbook_"+name+".asset");books[i].skill=(RpgSkill)i;books[i].name="Skillbook: "+name;books[i].description="Consume to gain exactly 100 "+name+" XP. One book per inventory slot.";books[i].icon=icon;books[i].itemType=ItemType.Resource;books[i].price=0;books[i].tradeRestricted=true;EditorUtility.SetDirty(books[i]);}
  var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Player/PlayerHolder.prefab");try{var owner=root.GetComponentInChildren<PlayerStats>(true);if(owner==null)throw new Exception("PlayerHolder stats missing");Wire(owner.gameObject,curve,definitions);PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Player/PlayerHolder.prefab");}finally{PrefabUtility.UnloadPrefabContents(root);}
  var players=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsInactive.Include,FindObjectsSortMode.None);foreach(var player in players)Wire(player.gameObject,curve,definitions);
  var copper=AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>("Assets/ResourceNodes/CopperDeposit.asset");if(copper==null)throw new Exception("Copper definition missing");copper.awardsSkillXP=true;copper.xpSkill=RpgSkill.Mining;copper.skillXP=25;EditorUtility.SetDirty(copper);
  var playerTransform=players.First().transform;var supply=UnityEngine.Object.FindFirstObjectByType<SkillbookTestSupply>();
  if(supply==null){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Skillbooks - E grants all ten (10 free slots)";go.transform.position=playerTransform.position+playerTransform.forward*2.3f+playerTransform.right*1.5f+Vector3.up*.5f;go.transform.localScale=new Vector3(1.2f,.8f,.8f);supply=go.AddComponent<SkillbookTestSupply>();
   var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Plugins/DungeonGenerator/LowPoly Dungeon/Prefabs/Book_09.prefab");if(model!=null){var visual=(GameObject)PrefabUtility.InstantiatePrefab(model);visual.transform.SetParent(go.transform,false);visual.transform.localPosition=Vector3.up*.7f;}
   var label=new GameObject("Skillbook testing sign",typeof(TextMeshPro));label.transform.SetParent(go.transform,false);label.transform.localPosition=Vector3.up*1.4f;label.transform.localScale=Vector3.one*.02f;label.transform.rotation=Quaternion.LookRotation(label.transform.position-playerTransform.position);var text=label.GetComponent<TextMeshPro>();text.text="SKILLBOOK TEST SUPPLY\nE: receive all 10 books\nUse books from inventory\nRequires 10 free slots";text.fontSize=18;text.alignment=TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(40,14);
  }supply.books=books;
  EditorUtility.SetDirty(curve);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  return new{created=true,scene=scene.path,players=players.Length,books=books.Length,supply=supply.transform.position.ToString(),thresholds=Enumerable.Range(1,30).Select(curve.Threshold).ToArray()};
 }
}


