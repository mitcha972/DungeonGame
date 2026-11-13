using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using DungeonGame.Dialogue;
using DungeonGame.Quests;
public static class AlignQuestPrefabs
{
 public static object Run()
 {
 if(EditorApplication.isPlaying)throw new Exception("Stop test Play Mode first");
 const string enemyPath="Assets/Prefabs/Enemies/Enemy_Main.prefab";
 var enemy=PrefabUtility.LoadPrefabContents(enemyPath);
 try{var stats=enemy.GetComponentInChildren<EnemyStats>(true);if(stats==null)throw new Exception("Enemy_Main has no EnemyStats");if(stats.GetComponent<QuestEnemyBridge>()==null)stats.gameObject.AddComponent<QuestEnemyBridge>();PrefabUtility.SaveAsPrefabAsset(enemy,enemyPath);}finally{PrefabUtility.UnloadPrefabContents(enemy);}
 var oldMerchant=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WorldInteract/DialogueMerchant.prefab");
 // Permanent NPC variant: inherits Enemy_Main's model, animator, equipment and agent.
 var preview=EditorSceneManager.NewPreviewScene();
 try{
 var npc=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(enemyPath),preview);npc.name="NPC_Main";
 foreach(var t in npc.GetComponentsInChildren<Transform>(true))t.gameObject.tag="Untagged";
 foreach(var c in npc.GetComponentsInChildren<EnemyAi>(true))c.enabled=false;
 foreach(var c in npc.GetComponentsInChildren<EnemyCombat>(true))c.enabled=false;
 foreach(var c in npc.GetComponentsInChildren<EnemyStats>(true))c.enabled=false;
 foreach(var c in npc.GetComponentsInChildren<EnemyInteract>(true))UnityEngine.Object.DestroyImmediate(c);
 foreach(var c in npc.GetComponentsInChildren<QuestEnemyBridge>(true))c.enabled=false;
 var shop=npc.AddComponent<ShopScript>();EditorUtility.CopySerialized(oldMerchant.GetComponent<ShopScript>(),shop);
 var inventory=npc.AddComponent<ShopInventory>();EditorUtility.CopySerialized(oldMerchant.GetComponent<ShopInventory>(),inventory);shop.inventory=inventory;shop.shopInventoryUI=null;
 var dialogue=npc.AddComponent<DialogueNpc>();dialogue.speakerName="Rowan the Merchant";dialogue.conversation=oldMerchant.GetComponent<DialogueNpc>().conversation;dialogue.shop=shop;shop.dialogue=dialogue;
 PrefabUtility.SaveAsPrefabAsset(npc,"Assets/Prefabs/WorldInteract/NPC_Main.prefab");
 }finally{EditorSceneManager.ClosePreviewScene(preview);}
 // Keep the existing merchant's GUID and serialized component IDs. It is still the original
 // merchant fixture; NPC_Main is the permanent Enemy_Main-based NPC used by quest content.
 return new{enemy=enemyPath,npc="Assets/Prefabs/WorldInteract/NPC_Main.prefab"};
 }
}
