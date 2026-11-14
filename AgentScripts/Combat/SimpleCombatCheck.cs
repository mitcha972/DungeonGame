using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.AI;
public static class CombatRegression
{
 static List<string> results=new List<string>();
 static void Check(bool ok,string message) { results.Add((ok?"PASS ":"FAIL ")+message); }
 static async Task Wait(float seconds) { float end=Time.time+seconds; while(Time.time<end) await Task.Delay(16); }
 public static string Main() { EditorApplication.delayCall += Run; return "Armed combat matrix"; }
 static async void Run()
 {
 try {
  results.Clear(); foreach(var old in UnityEngine.Object.FindObjectsByType<EnemyCombat>(FindObjectsSortMode.None)) UnityEngine.Object.Destroy(old.gameObject); await Wait(.1f); int frame=Time.frameCount; await Wait(.1f); Check(Time.frameCount>frame,"Live frames advancing");
  var p=UnityEngine.Object.FindFirstObjectByType<PlayerCombat>();
  var controller=p.GetComponent<CharacterController>(); controller.enabled=false; p.transform.position=new Vector3(0,-.01f,-4.39f); controller.enabled=true; await Wait(.2f);
  var ps=p.myStats; var pe=p.GetComponent<EquipmentManager>(); var pa=p.GetComponent<CharacterAnimator>();
  var movement=p.GetComponent<StarterAssets.ThirdPersonController>();
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Main.prefab");
  var enemy=UnityEngine.Object.Instantiate(prefab,p.transform.position+Vector3.forward,Quaternion.Euler(0,180,0));
  await Wait(.3f); var ai=enemy.GetComponent<EnemyAi>(); ai.enabled=false;
  var agent=enemy.GetComponent<NavMeshAgent>(); agent.enabled=false;
  var ec=enemy.GetComponent<EnemyCombat>(); var es=ec.myStats; var ea=enemy.GetComponent<CharacterAnimator>();
  ps.maxHealth=ps.currentHealth=100000; es.maxHealth=es.currentHealth=100000; ps.damage=es.damage=30;
  // Controlled, oversized target collider observes each authored moving damage volume without camera/AI noise.
  var hitTarget=enemy.AddComponent<BoxCollider>(); hitTarget.size=new Vector3(5,4,5); hitTarget.center=Vector3.up;
  var weapons=new[]{"Sword-Wood","Sword-Iron","Bow"};
  pe.Unequip(EquipmentSlot.OffHand);
  foreach(var name in new[]{"Unarmed"}.Concat(weapons))
  {
   p.CancelAttack(); ec.CancelAttack();
   if(name=="Unarmed") pe.Unequip(EquipmentSlot.MainHand);
   else pe.Equip(AssetDatabase.LoadAssetAtPath<Equipment>("Assets/GamePlay/ScriptableObjects/Items/Weapons/"+name+".asset"));
   await Wait(.15f); p.CancelAttack();
   p.transform.rotation=Quaternion.identity; enemy.transform.position=p.transform.position+Vector3.forward;
   if(name=="Bow") {
    var cam=Camera.main; enemy.transform.position=cam.ViewportPointToRay(new Vector3(.5f,.5f)).GetPoint(8);
   }
   Physics.SyncTransforms(); int before=es.currentHealth; bool visible=false; bool mobile=true; int attacks=0; int contacts=0; string debug="";
   Action<bool> accepted=h=>{attacks++; Check(!h,name+" uses light attack");}; p.OnAttack+=accepted;
   InputSystem.QueueStateEvent(Mouse.current,new MouseState{buttons=1});
   float end=Time.time+3; string overlap="";
   while(Time.time<end) {  mobile &= movement.canMove && movement.canRotate; var v=GameObject.Find("Active Attack Box"); if(v!=null) { var b=v.GetComponent<Renderer>().bounds; overlap=" visual="+b+" overlaps="+string.Join(",",Physics.OverlapBox(b.center,b.extents).Select(c=>c.name+":"+c.GetComponentInParent<CharacterStats>())); } contacts=Mathf.Max(contacts,p.attackHitBox.collidedCharacters.Count); debug=" health="+es.currentHealth+" enabled="+es.enabled+" pos="+enemy.transform.position+" damage="+p.AttackDamage; visible|=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Any(r=>r.enabled&&r.gameObject.activeInHierarchy&&(r.name=="Active Attack Box"||r.name=="Projectile Attack Box")); await Task.Delay(16); }
   InputSystem.QueueStateEvent(Mouse.current,new MouseState()); await Wait(.1f); p.OnAttack-=accepted;
   Check(pa.CurrentWeapon != null && (name=="Bow" ? pa.CurrentWeapon.isRanged : !pa.CurrentWeapon.isRanged),name+" selects expected melee/ranged data");
   Check(attacks==1,name+" held click produces exactly one attack ("+attacks+")");
   Check(es.currentHealth<before,name+" damages enemy ("+(before-es.currentHealth)+") contacts="+contacts+debug+overlap);
   Check(visible,name+" visible attack volume"); Check(mobile,name+" retains movement/rotation");
  }
  // Enemy attacks use the same authored player weapon classes, through the actual EnemyCombat/AI gate.
  hitTarget.enabled=false;
  var playerTarget=p.gameObject.AddComponent<BoxCollider>(); playerTarget.size=new Vector3(3,3,3); playerTarget.center=Vector3.up;
  foreach(var weapon in pa.availableWeaponClasses)
  {
   p.CancelAttack(); ec.CancelAttack(); ea.OnEquipmentChanged(null,null);
   typeof(CharacterAnimator).GetProperty("CurrentWeapon").GetSetMethod(true).Invoke(ea,new object[]{weapon});
   enemy.transform.position=p.transform.position+Vector3.forward*1.5f; enemy.transform.rotation=Quaternion.Euler(0,180,0);
   es.range=10; es.sightRange=100; ai.target=p.transform; ai.enabled=true;
   int before=ps.currentHealth; bool accepted=false; Action<bool> accept=h=>accepted=true; ec.OnAttack+=accept;
   ec.Attack(false); await Wait(3); ec.OnAttack-=accept; ai.enabled=false;
   Check(accepted,"Enemy "+weapon.name+" accepts attack"); Check(ps.currentHealth<before,"Enemy "+weapon.name+" damages player ("+(before-ps.currentHealth)+")");
  }
  playerTarget.enabled=false;
  p.CancelAttack(); pe.Unequip(EquipmentSlot.OffHand); Vector3 start=p.transform.position;
  InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W));
  p.Attack(false); await Wait(.5f);
  InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());
  Check(Vector3.Distance(start,p.transform.position)>.1f,"W moves player while attacking");
  await Wait(1);
  foreach(var name in new[]{"Shield-Wood","Shield-Iron"})
  {
   p.CancelAttack(); pe.Equip(AssetDatabase.LoadAssetAtPath<Equipment>("Assets/GamePlay/ScriptableObjects/Items/Weapons/"+name+".asset")); p.CancelAttack();
   int before=ps.currentHealth; p.TakeDamage(30,enemy.transform,0,p.transform.position,true); int inactive=before-ps.currentHealth;
   p.CancelAttack(); p.SetShieldBlock(true); Check(p.isBlocking,name+" block activates");
   before=ps.currentHealth; p.TakeDamage(30,enemy.transform,0,p.transform.position,true); int active=before-ps.currentHealth;
   Check(active>0&&active<inactive,name+" active block reduces without negating: "+inactive+" -> "+active);
   p.SetShieldBlock(false); Check(!p.isBlocking,name+" release drops block");
  }
  p.CancelAttack(); p.SetShieldBlock(true); int hp=ps.currentHealth;
  p.TakeDamage(1,enemy.transform,0,p.transform.position,true);
  Check(hp-ps.currentHealth==1,"Active block retains minimum one damage");
  p.CancelAttack(); pe.Unequip(EquipmentSlot.OffHand); hp=ps.currentHealth;
  p.TakeDamage(30,enemy.transform,0,p.transform.position,true);
  Check(hp-ps.currentHealth==Mathf.Max(1,30-Mathf.Max(0,ps.armor)),"No shield cannot reduce damage via blocked flag");
  UnityEngine.Object.Destroy(enemy); p.CancelAttack();
  string report=string.Join("\n",results); System.IO.File.WriteAllText("Assets/Plan/ARCHIVE/Evidence/SimpleCombatValidation.txt",report); Debug.Log(report);
 } catch(Exception e) { System.IO.File.WriteAllText("Assets/Plan/ARCHIVE/Evidence/SimpleCombatValidation.txt",e.ToString()+"\n"+string.Join("\n",results)); }
 }
}
