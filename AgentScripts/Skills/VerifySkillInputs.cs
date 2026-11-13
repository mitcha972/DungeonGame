using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
public static class VerifySkillInputs
{
 static List<string> checks=new List<string>();static void Check(bool b,string s){if(!b)throw new Exception(s);checks.Add(s);}
 static async Task Wait(float s){await Task.Delay((int)(s*1000));}
 static async Task KeyPress(Key key){InputSystem.EnableDevice(Keyboard.current);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Wait(.05f);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));await Wait(.12f);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Wait(.12f);}
 public static async Task<object> Run()
 {
  bool passed=false;string error=null;var cleanup=new List<UnityEngine.Object>();
  try{
   var p=UnityEngine.Object.FindFirstObjectByType<PlayerSkillProgress>();var stats=p.GetComponent<PlayerStats>();var controls=p.GetComponent<PlayerControlState>();var bag=p.GetComponent<Inventory>();var combat=p.GetComponent<CharacterCombat>();var animator=p.GetComponent<CharacterAnimator>();var supply=UnityEngine.Object.FindFirstObjectByType<SkillbookTestSupply>();
   controls.ClosePanels();controls.SendMessage("OnApplicationFocus",true);combat.CancelAttack();animator.SetBusy(false);p.RestoreState(new SkillProgressSnapshot());for(int i=0;i<bag.items.Count;i++)bag.items[i]=null;bag.NotifyExchange();
   var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=new Vector3(0,.08f,.21f);cc.enabled=true;Physics.SyncTransforms();await Wait(.1f);
   var cam=Camera.main;Vector3 cameraPosition=p.transform.position+Vector3.up*1.3f;Quaternion cameraRotation=Quaternion.LookRotation(supply.transform.position-cameraPosition);UnityEditor.EditorApplication.CallbackFunction pin=()=>{cam.transform.position=cameraPosition;cam.transform.rotation=cameraRotation;};EditorApplication.update+=pin;
   try{pin();await KeyPress(Key.E);}finally{EditorApplication.update-=pin;}
   Check(supply.books.All(b=>bag.Count(b)==1),"Synthetic E and camera raycast obtain all ten books through Interactable");
   for(int i=0;i<bag.items.Count;i++)bag.items[i]=supply.books[0];var before=bag.items.ToArray();supply.Interact(p.transform);Check(before.SequenceEqual(bag.items),"Full bag rejects entire supply without partial changes");
   for(int i=0;i<bag.items.Count;i++)bag.items[i]=null;bag.Add(supply.books[0]);int slot=bag.items.IndexOf(supply.books[0]);int health=stats.currentHealth;stats.currentHealth=0;supply.books[0].Use(p.gameObject,slot);Check(bag.items[slot]==supply.books[0]&&p.GetXP(RpgSkill.Magic)==0,"Dead-health player cannot consume a book");stats.currentHealth=health;
   p.enabled=false;supply.books[0].Use(p.gameObject,slot);Check(bag.items[slot]==supply.books[0],"Disabled skill owner retains book");p.enabled=true;
   var orphan=new GameObject("No skill owner",typeof(Inventory));cleanup.Add(orphan);orphan.GetComponent<Inventory>().Add(supply.books[0]);supply.books[0].Use(orphan,0);Check(orphan.GetComponent<Inventory>().items[0]==supply.books[0],"Missing skill owner retains book");
   var curve=ScriptableObject.CreateInstance<SkillXpCurve>();cleanup.Add(curve);curve.firstLevelCost=int.MaxValue;curve.costIncrease=int.MaxValue;Check(curve.Threshold(30)>curve.Threshold(29)&&curve.Threshold(30)>0,"Extreme authored curve remains monotonic without overflow");curve.firstLevelCost=int.MinValue;curve.costIncrease=int.MinValue;Check(curve.Threshold(2)==1&&curve.Threshold(30)>curve.Threshold(29),"Negative authored curve values become safe increasing costs");
   for(int i=0;i<10;i++){p.RestoreState(new SkillProgressSnapshot());p.AwardXP((RpgSkill)i,p.curve.Threshold(30));Check(p.GetLevel((RpgSkill)i)==30&&p.XPToNextLevel((RpgSkill)i)==0,"Skill reaches cap with no next cost: "+(RpgSkill)i);}
   p.RestoreState(new SkillProgressSnapshot());var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Main.prefab");var enemyGO=UnityEngine.Object.Instantiate(prefab,new Vector3(100,10,100),Quaternion.identity);cleanup.Add(enemyGO);var enemy=enemyGO.GetComponentInChildren<EnemyStats>();var ai=enemy.GetComponent<EnemyAi>();if(ai!=null)ai.enabled=false;var agent=enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();if(agent!=null)agent.enabled=false;enemy.GetComponent<CharacterCombat>().CancelAttack();enemy.maxHealth=enemy.currentHealth=1000;enemy.armor=0;
   cc.enabled=false;p.transform.position=new Vector3(0,.08f,.21f);cc.enabled=true;await Wait(.15f);enemy.armor=0;enemy.currentHealth=1000;enemy.GetComponent<CharacterCombat>().enabled=true;var collider=enemy.GetComponentsInChildren<Collider>().First(c=>!c.isTrigger);Physics.SyncTransforms();
   var hitGO=new GameObject("Native overlap skill fixture",typeof(BoxCollider));hitGO.transform.SetParent(p.transform,false);hitGO.transform.position=collider.bounds.center;cleanup.Add(hitGO);hitGO.GetComponent<BoxCollider>().size=Vector3.one*1.3f;var hit=hitGO.AddComponent<AttackHitBox>();hit.owner=p.gameObject;hit.myStats=stats;typeof(CharacterCombat).GetField("_attackDamage",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(combat,9);hit.BeginAttack();hit.EnableDamage();hit.SampleDamage();hit.DisableDamage();Check(p.GetXP(RpgSkill.Combat)==9,"Native Physics.OverlapBox melee hit awards Combat XP: actual="+p.GetXP(RpgSkill.Combat));
   hitGO.transform.position=Vector3.up*100;hit.BeginAttack();hit.EnableDamage();hit.SampleDamage();hit.DisableDamage();Check(p.GetXP(RpgSkill.Combat)==9,"Native melee miss awards no XP");
   Vector3 point=collider.bounds.center;var projectileGO=new GameObject("Native swept projectile",typeof(Rigidbody));cleanup.Add(projectileGO);projectileGO.transform.position=point+Vector3.back*3;projectileGO.transform.rotation=Quaternion.identity;var body=projectileGO.GetComponent<Rigidbody>();body.useGravity=false;body.position=projectileGO.transform.position;Physics.SyncTransforms();var projectile=projectileGO.AddComponent<Projectile>();projectile.StartUp(stats,p.gameObject,13,20,20);await Wait(1f);Check(p.GetXP(RpgSkill.Archery)==13&&p.GetXP(RpgSkill.Combat)==9,"Native swept projectile awards Archery without Combat double credit: archery="+p.GetXP(RpgSkill.Archery)+",combat="+p.GetXP(RpgSkill.Combat)+",projectile="+(projectileGO==null?"destroyed":projectileGO.transform.position.ToString())+",target="+point+",health="+enemy.currentHealth);
   Check(Time.timeScale==1,"Input stress preserves running world");passed=true;
  }catch(Exception ex){error=ex.ToString();}finally{foreach(var item in cleanup)if(item!=null)UnityEngine.Object.Destroy(item);}
  var report=new{passed,count=checks.Count,checks,error};System.IO.File.WriteAllText("Assets/Plan/Validation/skills-inputs.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));return report;
 }
}






