using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using DungeonGame.Dialogue;
using DungeonGame.Quests;
public static class VerifyResourceNodes
{
 static readonly List<string> checks=new List<string>();
 static void Check(bool b,string s){if(!b)throw new Exception("FAIL: "+s);checks.Add(s);}
 static async Task Wait(float seconds){float end=Time.time+seconds;float deadline=Time.realtimeSinceStartup+seconds+8;while(Time.time<end){if(Time.realtimeSinceStartup>deadline)throw new Exception("World time stopped");await Task.Delay(30);}}
 static async Task Press(Key key){InputSystem.EnableDevice(Keyboard.current);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));await Task.Delay(100);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Task.Delay(100);}
 static void Empty(Inventory inv){for(int i=0;i<inv.items.Count;i++)inv.items[i]=null;inv.NotifyExchange();}
 static void Move(PlayerGathering g,Vector3 p){var c=g.GetComponent<CharacterController>();if(c!=null)c.enabled=false;g.transform.position=p;if(c!=null)c.enabled=true;Physics.SyncTransforms();}
 static ResourceNode Clone(ResourceNode source,Vector3 pos,List<UnityEngine.Object> cleanup){var n=UnityEngine.Object.Instantiate(source);n.transform.position=pos;n.definition=UnityEngine.Object.Instantiate(source.definition);n.definition.collectionDuration=.2f;n.definition.respawnDuration=.6f;n.RestoreState(new ResourceNode.Snapshot{charges=1});cleanup.Add(n.gameObject);cleanup.Add(n.definition);return n;}
 public static async Task<object> Run()
 {
 if(!Application.isPlaying)throw new Exception("Requires test Play Mode");checks.Clear();var cleanup=new List<UnityEngine.Object>();
 var settings=InputSystem.settings;var bg=settings.backgroundBehavior;var edit=settings.editorInputBehaviorInPlayMode;
 settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
 bool passed=false;string error=null;
 try{
 var g=UnityEngine.Object.FindFirstObjectByType<PlayerGathering>();Check(g!=null,"real player contains gathering component");var inv=g.GetComponent<Inventory>();var pc=g.GetComponent<PlayerController>();var state=g.GetComponent<PlayerControlState>();var stats=g.GetComponent<PlayerStats>();var anim=g.GetComponentInChildren<Animator>();var combat=g.GetComponent<CharacterCombat>();
 foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None))ai.enabled=false;
 foreach(var c in UnityEngine.Object.FindObjectsByType<EnemyCombat>(FindObjectsSortMode.None))c.enabled=false;
 g.Cancel("");combat.CancelAttack();anim.Rebind();state.ClosePanels();var pause=UnityEngine.Object.FindFirstObjectByType<SettingsMenu>();if(pause!=null)pause.Resume();state.SendMessage("OnApplicationFocus",true);await Wait(.3f);
 int frames=Time.frameCount;await Wait(.2f);Check(Time.frameCount>frames,"frames and scaled world time advance");
 var nodes=UnityEngine.Object.FindObjectsByType<ResourceNode>(FindObjectsSortMode.None).OrderBy(n=>n.definition.collectionDuration).ToArray();Check(nodes.Length==3,"test scene contains exactly three configured resource nodes");foreach(var n in nodes)n.RestoreState(new ResourceNode.Snapshot{charges=n.definition.maxCharges});
 foreach(var n in nodes){Check(n.definition.IsValid&&n.GetComponent<Collider>()!=null,"node has valid reward and interaction collider: "+n.name);Check(anim.HasState(0,Animator.StringToHash(n.definition.animationState)),"existing player controller has gathering animation: "+n.name);}
 Empty(inv);
 // Drive the real E event and camera raycast; pin camera during synthetic press only.
 var first=nodes[0];Move(g,first.transform.position+new Vector3(0,.1f,-2.3f));
 var cam=Camera.main;Vector3 cp=g.transform.position+Vector3.up*1.35f;Quaternion cr=Quaternion.LookRotation(first.transform.position+Vector3.up*.8f-cp);
 UnityEditor.EditorApplication.CallbackFunction pin=()=>{if(cam!=null){cam.transform.position=cp;cam.transform.rotation=cr;}};
 UnityEditor.EditorApplication.update+=pin;
 try{pin();Check(pc.LookedAtResource()==first,"camera-center raycast resolves available source in player range");await Press(Key.E);}finally{UnityEditor.EditorApplication.update-=pin;}
 Check(g.IsGathering,"E through existing input starts timed gathering");Check(inv.Count(first.definition.resourceItem)==0,"reward is delayed at action start");Check(!state.GameplayAllowed&&Time.timeScale==1,"gathering locks gameplay while world time continues");
 Check(!g.TryBegin(nodes[1]),"second gathering action is rejected");await Wait(.25f);Check(anim.GetCurrentAnimatorStateInfo(0).IsName(first.definition.animationState)||anim.GetNextAnimatorStateInfo(0).IsName(first.definition.animationState),"humanoid gathering animation plays during timer");Check(g.Progress>0&&g.Progress<1,"progress advances before reward");
 await Wait(first.definition.collectionDuration);Check(!g.IsGathering&&inv.Count(first.definition.resourceItem)==1,"configured timer awards exactly one item through inventory");Check(first.IsDepleted&&first.Charges==0,"successful source collection consumes one charge and depletes");Check(!g.TryBegin(first)&&inv.Count(first.definition.resourceItem)==1,"depleted node rejects repeated interaction without reward");Check(state.GameplayAllowed,"controls recover after successful collection");
 // Other starter nodes retain their authored 3s / 4s durations.
 foreach(var n in nodes.Skip(1)){Move(g,n.transform.position+new Vector3(0,.1f,-2));await Wait(.2f);Check(g.TryBegin(n),"starter gathering begins: "+n.name);int before=inv.Count(n.definition.resourceItem);await Wait(.25f);Check(inv.Count(n.definition.resourceItem)==before,"starter reward remains delayed: "+n.name);await Wait(n.definition.collectionDuration);Check(inv.Count(n.definition.resourceItem)==before+1&&n.IsDepleted,"starter timer rewards and depletes: "+n.name);}
 Check(first.IsDepleted&&first.RespawnRemaining>30,"starter source retains its independent 60-second respawn deadline");
 var one=Clone(nodes[1],new Vector3(-5,0,-5),cleanup);var two=Clone(nodes[2],new Vector3(-3,0,-5),cleanup);Empty(inv);Move(g,new Vector3(-4,.1f,-6));await Wait(.2f);
 Check(g.TryBegin(one),"short-timer fixture begins");await Wait(.3f);Check(one.IsDepleted&&!two.IsDepleted,"depletion timers are independent per instance");Check(!one.availableVisual.activeSelf&&one.depletedVisual.activeSelf,"depleted appearance replaces available visual");await Wait(.7f);Check(!one.IsDepleted&&one.availableVisual.activeSelf&&!one.depletedVisual.activeSelf,"automatic respawn restores charges and visuals");Check(g.TryBegin(one),"respawned node can be gathered again");await Wait(.3f);Check(inv.Count(one.definition.resourceItem)==2,"second collection grants exactly one further reward");await Wait(.7f);
 Empty(inv);Check(g.TryBegin(one),"cancel fixture begins");await Press(Key.Escape);Check(!g.IsGathering&&!one.IsDepleted&&inv.Count(one.definition.resourceItem)==0&&state.GameplayAllowed&&Time.timeScale==1,"Escape cancels without reward charge loss or pause");
 one.definition.collectionDuration=.5f;Check(g.TryBegin(one),"capacity-race fixture begins");for(int i=0;i<inv.items.Count;i++)inv.items[i]=two.definition.resourceItem;await Wait(.6f);Check(!g.IsGathering&&!one.IsDepleted&&inv.Count(one.definition.resourceItem)==0,"inventory becoming full mid-action preserves node and gives no partial reward");Check(!g.TryBegin(one)&&g.Feedback.Contains("full"),"full inventory gives immediate feedback");Empty(inv);
 one.definition.quantity=2;one.definition.collectionDuration=.2f;for(int i=1;i<inv.items.Count;i++)inv.items[i]=two.definition.resourceItem;Check(!g.TryBegin(one)&&inv.Count(one.definition.resourceItem)==0,"multi-item reward requires full atomic capacity");Empty(inv);one.definition.quantity=1;
 one.definition.requiredItem=two.definition.resourceItem;Check(!g.TryBegin(one),"required tool is checked before start");inv.Add(two.definition.resourceItem);one.definition.collectionDuration=.5f;Check(g.TryBegin(one),"required-item fixture starts with tool");inv.Remove(inv.items.IndexOf(two.definition.resourceItem));await Wait(.1f);Check(!g.IsGathering&&!one.IsDepleted,"losing required tool cancels without consuming charge");one.definition.requiredItem=null;
 Check(g.TryBegin(one),"range cancellation fixture starts");Move(g,new Vector3(7,.1f,-7));await Wait(.1f);Check(!g.IsGathering&&!one.IsDepleted&&state.GameplayAllowed,"leaving configured player interaction range cancels safely");Move(g,new Vector3(-4,.1f,-6));await Wait(.2f);
 Check(g.TryBegin(one),"node-disable fixture starts");one.gameObject.SetActive(false);Check(!g.IsGathering&&state.GameplayAllowed,"disabling node immediately releases player action ownership");one.gameObject.SetActive(true);await Wait(.2f);
 Check(g.TryBegin(one),"player-disable fixture starts");g.enabled=false;Check(!g.IsGathering&&!one.IsDepleted&&state.GameplayAllowed,"disabling gatherer releases reservation and controls");g.enabled=true;await Wait(.2f);
 Check(g.TryBegin(one),"modal ownership fixture starts");var journal=g.GetComponentInChildren<QuestJournal>(true);if(journal==null)journal=UnityEngine.Object.FindFirstObjectByType<QuestJournal>();journal.panel.SetActive(true);state.Synchronize();await Wait(.1f);Check(!g.IsGathering&&!state.GameplayAllowed&&journal.IsOpen,"cancellation does not unlock gameplay owned by quest journal");journal.Close();state.Synchronize();Check(state.GameplayAllowed,"journal close restores normal gameplay");
 int incomingReturn=anim.GetCurrentAnimatorStateInfo(0).fullPathHash;Check(g.TryBegin(one),"damage cancellation fixture starts");await Wait(.15f);anim.CrossFade("GatheringMine",.1f,0);anim.Update(.01f);stats.currentHealth=Mathf.Max(1,stats.currentHealth-1);await Wait(.08f);Check(!g.IsGathering&&!one.IsDepleted,"damage interrupts gathering without consuming resource");Check(anim.GetCurrentAnimatorStateInfo(0).IsName("GatheringMine")||anim.GetNextAnimatorStateInfo(0).IsName("GatheringMine"),"gathering cleanup preserves a valid incoming base-layer action transition");anim.CrossFade(incomingReturn,.05f,0);g.GetComponent<CharacterAnimator>().SetBusy(false);await Wait(.2f);
 one.RestoreState(new ResourceNode.Snapshot{charges=0,remainingSeconds=.3f});var snap=one.CaptureState();Check(snap.charges==0&&snap.remainingSeconds>0,"snapshot exposes depletion and remaining scaled seconds");one.gameObject.SetActive(false);await Wait(.4f);one.gameObject.SetActive(true);Check(!one.IsDepleted,"disabled nodes resume using elapsed game time without per-frame polling");
 one.definition.maxCharges=2;one.RestoreState(new ResourceNode.Snapshot{charges=2});one.definition.collectionDuration=.2f;Empty(inv);Check(g.TryBegin(one),"multi-charge fixture starts");await Wait(.3f);Check(one.Charges==1&&!one.IsDepleted,"first collection preserves availability when charges remain");
 bool reentryRejected=false;Inventory.OnItemChanged reentry=()=>reentryRejected=!g.TryBegin(one);inv.onItemChangedCallback+=reentry;int events=0;Action<ResourceNode,Item,int> collected=(n,item,count)=>events++;g.ResourceCollected+=collected;Check(g.TryBegin(one),"final-charge fixture starts");await Wait(.3f);g.ResourceCollected-=collected;inv.onItemChangedCallback-=reentry;Check(reentryRejected,"inventory observers cannot reenter and duplicate a reward");Check(one.IsDepleted&&events==1,"final charge depletes and emits one successful collection event");
 Check(!state.ActionLocked&&state.GameplayAllowed,"all action locks released after verification");passed=true;
 }catch(Exception e){error=e.ToString();}
 finally{foreach(var o in cleanup)if(o!=null)UnityEngine.Object.Destroy(o);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=edit;}
 var report=new{passed,error,checks=checks.ToArray(),frameCount=Time.frameCount,worldTime=Time.time,scope="Live Editor assertions: synthetic E/Escape, real player/Inventory/Animator, authored starter timers plus shortened cloned respawn fixtures. Not manual combat, hardware or durable save certification."};System.IO.Directory.CreateDirectory("Assets/Plan/Validation");System.IO.File.WriteAllText("Assets/Plan/Validation/resource-nodes-playmode.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}





