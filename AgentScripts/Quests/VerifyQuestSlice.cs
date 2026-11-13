using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using DungeonGame.Dialogue;
using DungeonGame.Quests;
public static class VerifyQuestSlice
{
 static readonly List<string> checks=new List<string>();
 static void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);checks.Add(label);Debug.Log("Quest PASS: "+label);}
 static async Task Press(Key key){InputSystem.EnableDevice(Keyboard.current);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));await Task.Delay(180);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Task.Delay(180);}
 static Button[] Responses(DialogueController d)=>d.panel.responseParent.GetComponentsInChildren<Button>().Where(b=>b!=d.panel.responseTemplate).ToArray();
 public static async Task<object> Run()
 {
 if(!Application.isPlaying)throw new Exception("Requires Play Mode");checks.Clear();
 var settings=InputSystem.settings;var bg=settings.backgroundBehavior;var edit=settings.editorInputBehaviorInPlayMode;
 settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
 try {
 var owner=UnityEngine.Object.FindFirstObjectByType<QuestController>();owner.Initialize();var service=owner.Service;var quest=owner.quests.Single();var state=owner.GetComponent<PlayerControlState>();var d=owner.GetComponent<DialogueController>();var stats=owner.GetComponent<PlayerStats>();var inv=owner.GetComponent<Inventory>();var npc=UnityEngine.Object.FindFirstObjectByType<DialogueNpc>();var journal=UnityEngine.Object.FindFirstObjectByType<QuestJournal>();
 UnityEngine.Object.FindFirstObjectByType<SettingsMenu>().Resume();state.SendMessage("OnApplicationFocus",true);d.Close();await Task.Delay(400);
 int frame=Time.frameCount;float time=Time.time;await Task.Delay(300);Check(Time.frameCount>frame&&Time.time>time,"Play Mode frames and scaled world time advance");
 var enemies=UnityEngine.Object.FindObjectsByType<EnemyStats>(FindObjectsSortMode.None).Where(e=>e.isActiveAndEnabled).OrderBy(e=>e.name).ToArray();Check(enemies.Length==2,"fixture contains two Enemy_Main quest enemies");foreach(var e in enemies){Check(e.GetComponent<EnemyAi>()!=null&&e.GetComponent<UnityEngine.AI.NavMeshAgent>()!=null,"quest enemy retains Enemy_Main AI and agent");var agent=e.GetComponent<UnityEngine.AI.NavMeshAgent>();Check(agent!=null&&agent.isOnNavMesh,"Enemy_Main agent is on baked fixture NavMesh");e.GetComponent<EnemyAi>().enabled=false;}Check(npc.GetComponent<EnemyStats>()!=null,"quest giver retains Enemy_Main character components");Check(npc.GetComponent<UnityEngine.AI.NavMeshAgent>()!=null&&!npc.GetComponent<EnemyAi>().enabled,"NPC retains agent with hostile AI disabled");
 var before=UnityEngine.Object.Instantiate(enemies[0].gameObject);before.GetComponent<EnemyStats>().TakeDamage(1000,owner.transform,0);Check(service.Status(quest)==QuestStatus.NotStarted,"kills before acceptance cannot start quest");UnityEngine.Object.Destroy(before);
 await Press(Key.E);Check(d.IsOpen&&d.CurrentNode.id=="offer","E raycast opens Rowan quest offer");Check(!state.GameplayAllowed&&Time.timeScale==1,"dialogue blocks controls without pausing dungeon");
 await Press(Key.Digit1);Check(service.Status(quest)==QuestStatus.Active&&d.CurrentNode.id=="raiders","number-key acceptance starts quest and changes dialogue immediately");Check(!service.TryStart(quest,out _),"duplicate acceptance rejected");
 await Task.Delay(100);Check(Responses(d).Length==2&&Responses(d)[0].GetComponentInChildren<TMPro.TMP_Text>().text.StartsWith("1. I'll"),"ineligible turn-in hidden and visible choices renumbered");
 Check(!service.TryAdvance(quest,"clear-watch",out _),"incomplete kill stage cannot advance");
 await Press(Key.Digit1);Check(!d.IsOpen,"filtered number shortcut maps to Goodbye rather than hidden turn-in");
 var child=new GameObject("AttackOwnerChild");child.transform.SetParent(owner.transform,false);
 enemies[0].TakeDamage(1000,child.transform,0);enemies[0].Die(child.transform);Check(service.Count(quest,quest.stages[0].objectives[0])==1,"child-owner kill credited once despite repeated death call");
 var wrong=UnityEngine.Object.Instantiate(enemies[1].gameObject);wrong.GetComponent<EnemyStats>().TakeDamage(1000,null,0);Check(service.Count(quest,quest.stages[0].objectives[0])==1,"environmental deaths do not count as player kills");UnityEngine.Object.Destroy(wrong);
 enemies[1].TakeDamage(1000,owner.transform,0);Check(service.Ready(quest),"two attributed enemy deaths make stage ready");
 await Press(Key.J);Check(journal.IsOpen&&!state.GameplayAllowed&&journal.body.text.Contains("2/2"),"J journal shows kill progress and owns gameplay input");await Press(Key.Escape);Check(!journal.IsOpen&&state.GameplayAllowed&&Time.timeScale==1,"Escape closes journal without pausing");
 Check(d.Open(npc),"return to Rowan opens active-stage conversation");await Task.Delay(150);var selected=Responses(d)[0];ExecuteEvents.Execute(selected.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerClickHandler);await Task.Delay(150);
 Check(service.CurrentStage(quest)?.id=="recover-ledger"&&d.CurrentNode.id=="recover","uGUI pointer click turns in kill stage and updates Rowan dialogue");
 Check(!service.TryAdvance(quest,"clear-watch",out _),"stale previous-stage turn-in rejected");d.Close();
 var controller=owner.GetComponent<CharacterController>();controller.enabled=false;owner.transform.position=new Vector3(7,1,10);controller.enabled=true;Physics.SyncTransforms();await Task.Delay(500);
 Check(service.Count(quest,quest.stages[1].objectives[0])==1,"native trigger overlap discovers watch post");await Task.Delay(250);Check(service.Count(quest,quest.stages[1].objectives[0])==1,"repeated trigger stays cannot overcount discovery");
 var pickup=GameObject.Find("LostLedgerPickup").GetComponent<ItemInteract>();var potion=quest.itemRewards[0].item;for(int i=0;i<inv.items.Count;i++)inv.AddToSlot(potion,i);
 pickup.Interact(owner.transform);await Task.Delay(50);Check(pickup!=null&&inv.Count(quest.stages[1].objectives[1].item)==0,"full inventory preserves world pickup and objective remains incomplete");
 inv.Remove(0);pickup.Interact(owner.transform);pickup.Interact(owner.transform);await Task.Delay(100);Check(pickup==null&&inv.Count(quest.stages[1].objectives[1].item)==1&&service.Ready(quest),"successful pickup counts once and readies final stage");
 string activeSave=service.CaptureJson();var restored=new QuestService(owner.quests,inv,()=>stats.QuestLevel,null);Check(restored.TryRestoreJson(activeSave,out _)&&restored.Ready(quest),"active quest JSON round trip restores stage and objective progress");
 inv.Remove(inv.items.IndexOf(quest.stages[1].objectives[1].item));Check(!service.Ready(quest),"removing collected item revokes possession readiness");inv.Add(quest.stages[1].objectives[1].item);
 controller.enabled=false;owner.transform.position=new Vector3(0,.1f,0);controller.enabled=true;Physics.SyncTransforms();Check(d.Open(npc),"final turn-in conversation opens");
 int xpBefore=stats.CurrentExperience,levelBefore=stats.QuestLevel;d.Choose(0);Check(service.Status(quest)==QuestStatus.Active&&inv.Count(quest.stages[1].objectives[1].item)==1&&stats.CurrentExperience==xpBefore&&stats.QuestLevel==levelBefore,"insufficient net reward capacity leaves items XP and completion untouched");
 inv.Remove(inv.items.FindIndex(i=>i==potion));
 bool reentryRejected=false;Inventory.OnItemChanged reentry=()=>{if(service.Status(quest)==QuestStatus.Completed)reentryRejected=!service.TryAdvance(quest,"recover-ledger",out _);};inv.onItemChangedCallback+=reentry;
 d.Choose(0);inv.onItemChangedCallback-=reentry;
 Check(service.Status(quest)==QuestStatus.Completed&&service.RewardsClaimed(quest),"final turn-in records completed status and reward receipt");Check(inv.Count(quest.stages[1].objectives[1].item)==0&&inv.Count(potion)==inv.items.Count,"delivery removes ledger and grants two potions using freed capacity");
 Check((stats.QuestLevel-levelBefore)*100+stats.CurrentExperience-xpBefore==125,"quest XP reward awarded through PlayerStats exactly once");Check(reentryRejected,"inventory callback cannot reenter completion and duplicate rewards");
 Check(d.CurrentNode.id=="completed"&&d.panel.npcText.text.Contains("ledger is safe"),"end-quest action immediately presents completed dialogue");
 string completeSave=service.CaptureJson();Check(restored.TryRestoreJson(completeSave,out _)&&restored.Status(quest)==QuestStatus.Completed&&restored.RewardsClaimed(quest),"completed tracking and reward receipt survive JSON round trip");
 Check(!restored.TryRestoreJson(completeSave.Replace("\"version\": 1","\"version\": 999"),out _)&&restored.Status(quest)==QuestStatus.Completed,"unsupported save version rejected without destroying progress");
 Check(!service.TryAdvance(quest,"recover-ledger",out _)&&!service.TryStart(quest,out _),"completed nonrepeatable quest cannot restart or reward twice");
 d.Close();Check(d.Open(npc)&&d.CurrentNode.id=="completed","reopening NPC remembers completed quest dialogue");await Press(Key.Digit1);Check(!d.IsOpen&&state.ShopUI.gameObject.activeInHierarchy&&!state.GameplayAllowed,"completed dialogue still hands off to existing shop");await Press(Key.E);Check(state.GameplayAllowed,"shop closes and restores controls after quest completion");
 await Press(Key.J);Check(journal.body.text.Contains("Completed")&&journal.body.text.Contains("Rewards received"),"journal displays completion tracking snapshot");var closeButton=journal.panel.GetComponentsInChildren<Button>().Single(b=>b.name=="Close");Check(closeButton.onClick.GetPersistentEventCount()==1,"journal close button has serialized persistent callback");closeButton.onClick.Invoke();Check(!journal.IsOpen&&state.GameplayAllowed,"journal Close button releases gameplay ownership");
 CoreChecks(inv,potion);
 var report=new {passed=true,checks=checks.ToArray(),frameCount=Time.frameCount,worldTime=Time.time,scope="Play Mode integration with simulated input, native trigger, ItemInteract and EnemyStats damage/death; quest DTO only, no disk gameplay save"};
 System.IO.Directory.CreateDirectory("Assets/Plan/Validation");System.IO.File.WriteAllText("Assets/Plan/Validation/quest-playmode.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }finally{InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=edit;}
 }
 static void CoreChecks(Inventory inventory,Item item)
 {
 var quest=ScriptableObject.CreateInstance<QuestDefinition>();var prerequisite=ScriptableObject.CreateInstance<QuestDefinition>();
 try{
 prerequisite.id="prerequisite";prerequisite.title="Prerequisite";prerequisite.stages=new[]{new QuestStage{id="talk",objectives=new[]{new QuestObjective{id="npc",type=QuestObjectiveType.TalkToNpc,targetId="contact"}}}};
 quest.id="core-test";quest.title="Core test";quest.minimumLevel=5;quest.prerequisites=new[]{prerequisite};quest.stages=new[]{new QuestStage{id="one",objectives=new[]{new QuestObjective{id="npc",type=QuestObjectiveType.TalkToNpc,targetId="contact"}}},new QuestStage{id="two",objectives=new[]{new QuestObjective{id="npc2",type=QuestObjectiveType.TalkToNpc,targetId="contact"}}}};
 int level=1;var service=new QuestService(new[]{quest,prerequisite},inventory,()=>level,_=>{});Check(!service.TryStart(quest,out _),"minimum-level requirement enforced");level=5;Check(!service.TryStart(quest,out _),"quest prerequisite completion enforced");service.TryStart(prerequisite,out _);service.ReportNpc("wrong");Check(!service.Ready(prerequisite),"wrong NPC action cannot satisfy objective");service.ReportNpc("contact");Check(service.TryAdvance(prerequisite,"talk",out _),"authored NPC action satisfies talk objective");Check(service.TryStart(quest,out _),"completed prerequisite unlocks adventure");
 service.ReportNpc("contact");service.TryAdvance(quest,"one",out _);Check(!service.Ready(quest),"event credit cannot leak into newly entered stage");
 var second=new QuestService(new[]{quest,prerequisite},inventory,()=>level,_=>{});Check(second.Status(quest)==QuestStatus.NotStarted,"two player services never share progress through assets");
 bool duplicate=false;try{new QuestService(new[]{prerequisite,prerequisite},inventory,()=>1,null);}catch(ArgumentException){duplicate=true;}Check(duplicate,"duplicate quest catalog IDs rejected");
 prerequisite.prerequisites=new[]{quest};Check(!quest.Validate(out _),"cyclic prerequisites rejected");prerequisite.prerequisites=Array.Empty<QuestDefinition>();
 quest.stages[0].objectives=new[]{new QuestObjective{id="same",type=QuestObjectiveType.PossessItem,item=item},new QuestObjective{id="same",type=QuestObjectiveType.PossessItem,item=item}};Check(!quest.Validate(out _),"duplicate objective IDs rejected");
 }finally{UnityEngine.Object.Destroy(quest);UnityEngine.Object.Destroy(prerequisite);}
 }
}



