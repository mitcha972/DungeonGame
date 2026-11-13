using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using DungeonGame.Dialogue;
public static class VerifyDialogueLifecycle {
 static void Check(bool pass,string text){if(!pass)throw new Exception(text);Debug.Log("Dialogue lifecycle PASS: "+text);}
 public static async Task<object> Run(){
 var s=UnityEngine.Object.FindFirstObjectByType<PlayerControlState>();s.SendMessage("OnApplicationFocus",true);var d=s.GetComponent<DialogueController>();var npc=UnityEngine.Object.FindFirstObjectByType<DialogueNpc>();var shop=npc.shop;
 if(s.ShopUI.gameObject.activeSelf)s.ShopUI.CloseUI();d.Close();
 var combat=s.GetComponent<PlayerCombat>();combat.Attack(false);Check(combat.IsExecutingAttack,"test begins with in-flight attack");Check(d.Open(npc),"dialogue can open during attack");Check(!combat.IsExecutingAttack && !combat.isBlocking,"opening dialogue cancels in-flight combat");
 d.panel.gameObject.SetActive(false);Check(!d.IsOpen && s.GameplayAllowed,"disabling panel restores control");
 Check(d.Open(npc),"dialogue reopens after panel disabled");npc.enabled=false;await Task.Delay(120);Check(!d.IsOpen && s.GameplayAllowed,"disabling NPC restores control");npc.enabled=true;
 Check(d.Open(npc),"dialogue reopens after NPC re-enabled");npc.shop=null;var action=AssetDatabase.LoadAssetAtPath<ServiceDialogueAction>("Assets/Dialogue/OpenShop.asset");action.Execute(d,npc);Check(d.IsOpen && !s.GameplayAllowed,"unavailable shop leaves dialogue safely open");npc.shop=shop;
 int requests=0;DialogueService last=DialogueService.EndDialogue;string id=null;UnityEngine.Events.UnityAction<DialogueService,string> callback=(service,key)=>{requests++;last=service;id=key;};npc.onServiceRequested.AddListener(callback);
 foreach(var service in new[]{DialogueService.OpenBank,DialogueService.StartQuest,DialogueService.OpenService}){var a=ScriptableObject.CreateInstance<ServiceDialogueAction>();a.service=service;a.serviceId="test-service";a.Execute(d,npc);Check(last==service && id=="test-service" && d.IsOpen && npc.ActiveController==d,"service hook: "+service);UnityEngine.Object.Destroy(a);}
 npc.onServiceRequested.RemoveListener(callback);Check(requests==3,"all extension hooks delivered");d.Close();Check(npc.ActiveController==null && s.GameplayAllowed,"close clears NPC session and restores gameplay");
 return new {passed=true,serviceRequests=requests};
 }
}
