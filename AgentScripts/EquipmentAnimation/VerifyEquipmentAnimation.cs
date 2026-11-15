using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Newtonsoft.Json;
public static class VerifyEquipmentAnimation
{
 static readonly List<object> checks=new List<object>();
 static void C(bool pass,string name,object details=null)=>checks.Add(new{pass,name,details});
 static async Task Wait(int ms)=>await Task.Delay(ms);
 static void Set(object o,string name,object value)=>o.GetType().GetField(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(o,value);
 public static async Task<object> Run()
 {
  if(!Application.isPlaying)throw new Exception("Play Mode required.");
  checks.Clear();var errors=new List<string>();var cleanup=new List<UnityEngine.Object>();CharacterEquipment gear=null;NavMeshDataInstance nav=default;
  Application.LogCallback log=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception)errors.Add(m);};Application.logMessageReceived+=log;
  try{
   var p=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).First(x=>x.GetComponent<StarterAssets.ThirdPersonController>()!=null);
   foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None))ai.enabled=false;
   var state=p.GetComponent<PlayerControlState>();state.ClosePanels();state.SendMessage("OnApplicationFocus",true);
   var combat=p.GetComponent<PlayerCombat>();var animation=p.GetComponent<CharacterAnimation>();gear=p.GetComponent<CharacterEquipment>();var bag=p.GetComponent<Inventory>();var motor=p.GetComponent<StarterAssets.ThirdPersonController>();var animator=p.GetComponentInChildren<Animator>();
   p.maxHealth=p.currentHealth=10000;combat.CancelAttack();gear.UnequipAll();animation.SetBusy(false);
   C(gear.InnateAttacks!=null&&gear.ActiveAttackSet==gear.InnateAttacks,"Bare hands resolve explicit innate attacks");
   C(!(animator.runtimeAnimatorController is AnimatorOverrideController),"Player uses the authored controller without runtime overrides");
   C(typeof(CharacterAnimation).GetField("defaultWeaponClass")==null&&typeof(CharacterAnimation).GetField("replaceableLightAttackAnim")==null,"Animation has no weapon-class search or replaceable clip fields");
   var items=AssetDatabase.FindAssets("t:Equipment").Select(g=>AssetDatabase.LoadAssetAtPath<Equipment>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
   var sword=items.First(x=>x.itemType==ItemType.Sword&&x.equipSlot==EquipmentSlot.MainHand);var bow=items.First(x=>x.itemType==ItemType.Bow&&x.equipSlot==EquipmentSlot.MainHand);var shield=items.First(x=>x.itemType==ItemType.Shield);
   C(items.Where(x=>x.equipSlot==EquipmentSlot.MainHand).All(x=>x.weaponAttacks!=null),"All authored main-hand items explicitly reference attack sets");
   for(int i=0;i<bag.items.Count;i++)bag.items[i]=null;bag.items[0]=sword;bag.items[1]=bow;bag.NotifyExchange();
   int baseDamage=p.damage;bool coherent=false;bool reentrantRejected=false;
   Action<Equipment,Equipment> observer=(item,previous)=>{if(item==sword){coherent=gear.Items[EquipmentSlot.MainHand]==sword&&bag.items[0]==null;gear.Equip(bow);reentrantRejected=gear.Items[EquipmentSlot.MainHand]==sword;combat.Attack(false);C(!combat.IsExecutingAttack,"Equipment observers cannot begin a half-committed attack");}};
   gear.EquipmentChanged+=observer;gear.Equip(sword,0);gear.EquipmentChanged-=observer;
   C(coherent&&reentrantRejected,"Equipment/bag state commits before observers and rejects reentrant gear changes");
   C(p.damage==baseDamage+sword.damageModifier,"Weapon modifiers apply exactly once");
   C(gear.ActiveAttackSet==sword.weaponAttacks,"Sword selects its item's attack set");
   C(gear.Visuals.ContainsKey(EquipmentSlot.MainHand)&&combat.attackHitBox.WeaponVisual==gear.Visuals[EquipmentSlot.MainHand],"Weapon visual and sampled hit volume agree");
   int grip=animator.GetLayerIndex("Right Hand"),attackLayer=animator.GetLayerIndex("Override");
   C(animator.GetLayerWeight(grip)==1&&animator.GetLayerWeight(attackLayer)==1,"Equipment enables the grip without changing the attack layer");
   animation.SetBusy(false);gear.Equip(bow,1);
   C(bag.items[1]==sword&&gear.Items[EquipmentSlot.MainHand]==bow,"Indexed weapon swap returns the old weapon to its exact bag slot");
   C(p.damage==baseDamage+bow.damageModifier,"Weapon swap replaces rather than stacks modifiers");
   C(gear.ActiveAttackSet==bow.weaponAttacks&&gear.ActiveAttackSet.launchesProjectile,"Bow uses its explicit projectile settings");
   animation.SetBusy(false);combat.CancelAttack();
   for(int i=0;i<bag.items.Count;i++)bag.items[i]=shield;bag.NotifyExchange();
   var current=gear.Items[EquipmentSlot.MainHand];C(!gear.TryUnequipToInventory(EquipmentSlot.MainHand)&&gear.Items[EquipmentSlot.MainHand]==current,"Full inventory retains equipped gear and its visual");
   bag.items[3]=null;bag.NotifyExchange();
   C(gear.TryUnequipToInventory(EquipmentSlot.MainHand)&&bag.items[3]==bow&&!gear.Items.ContainsKey(EquipmentSlot.MainHand),"Unequip atomically commits the item into available bag space");
   C(gear.ActiveAttackSet==gear.InnateAttacks&&combat.attackHitBox.WeaponVisual==null,"Unequip restores innate attacks and clears the previous weapon binding");
   C(animator.GetLayerWeight(grip)==0&&animator.GetLayerWeight(attackLayer)==1,"Unequip clears the grip while preserving attack-layer weight");
   C(p.damage==baseDamage,"Unequip restores base damage");
   var renderer=new SerializedObject(gear).FindProperty("bodyRenderer").objectReferenceValue as SkinnedMeshRenderer;
   if(renderer!=null&&renderer.sharedMesh.blendShapeCount>0){
    var a=UnityEngine.Object.Instantiate(sword);a.equipSlot=EquipmentSlot.Chest;a.mesh=null;a.coveredMeshRegions=new[]{CharacterEquipment.BodyRegion.Torso};cleanup.Add(a);
    var b=UnityEngine.Object.Instantiate(a);b.equipSlot=EquipmentSlot.Legs;cleanup.Add(b);gear.Equip(a);gear.Equip(b);gear.Unequip(EquipmentSlot.Chest);
    C(renderer.GetBlendShapeWeight(0)==100,"Overlapping armor coverage survives removal of just one covering item");gear.Unequip(EquipmentSlot.Legs);C(renderer.GetBlendShapeWeight(0)==0,"Last covering item restores the visible body region");
   }
   var platform=GameObject.CreatePrimitive(PrimitiveType.Cube);cleanup.Add(platform);platform.transform.position=new Vector3(300,49.9f,300);platform.transform.localScale=new Vector3(30,.2f,30);
   var data=NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0),new List<NavMeshBuildSource>{new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Box,transform=Matrix4x4.TRS(platform.transform.position,Quaternion.identity,Vector3.one),size=platform.transform.localScale}},new Bounds(new Vector3(300,50,300),new Vector3(32,10,32)),Vector3.zero,Quaternion.identity);cleanup.Add(data);nav=NavMesh.AddNavMeshData(data);
   var controller=p.GetComponent<CharacterController>();controller.enabled=false;p.transform.position=new Vector3(300,50.1f,300);p.transform.rotation=Quaternion.identity;controller.enabled=true;motor.GroundLayers|=1;motor.FacingTarget=null;Set(motor,"_yaw",0f);Set(motor,"_pitch",0f);motor.FirstPerson=true;
   NavMesh.SamplePosition(p.transform.position+Vector3.forward*.8f,out var spawn,2,NavMesh.AllAreas);
   var enemy=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Main.prefab"),spawn.position,Quaternion.LookRotation(Vector3.back));cleanup.Add(enemy);
   await Wait(250);enemy.GetComponent<EnemyAi>().enabled=false;enemy.GetComponent<NavMeshAgent>().enabled=false;var victim=enemy.GetComponent<EnemyStats>();victim.maxHealth=victim.currentHealth=10000;victim.armor=0;
   C(!(enemy.GetComponentInChildren<Animator>().runtimeAnimatorController is AnimatorOverrideController),"NPC also uses an authored controller without runtime overrides");
   var snapshot=UnityEngine.Object.Instantiate(sword.weaponAttacks);cleanup.Add(snapshot);snapshot.damageWindowStart=.15f;snapshot.damageWindowEnd=.8f;snapshot.attackInterval=.8f;
   var custom=UnityEngine.Object.Instantiate(sword);cleanup.Add(custom);custom.weaponAttacks=snapshot;gear.Equip(custom);combat.CancelAttack();animation.SetBusy(false);await Wait(1300);
   enemy.transform.position=p.transform.position+Vector3.forward*.8f;enemy.transform.LookAt(p.transform.position);Physics.SyncTransforms();int hp=victim.currentHealth;combat.Attack(false);float duration=combat.AttackDuration;
   C(combat.IsExecutingAttack&&combat.CurrentAttackSet==snapshot,"Combat captures the item's selected attack and motion");
   await Wait(160);C(animator.GetCurrentAnimatorClipInfo(attackLayer).Any(x=>x.clip==combat.CurrentAttackMotion.clip),"Direct authored state plays the selected sword clip");
   snapshot.damageWindowStart=snapshot.damageWindowEnd=1f;snapshot.attackInterval=99;
   await Wait((int)(duration*1000)+400);C(victim.currentHealth<hp&&combat.attackIsReady,"An accepted attack keeps its captured window and cooldown after profile changes");
   snapshot.damageWindowStart=.15f;snapshot.damageWindowEnd=.8f;snapshot.attackInterval=.8f;
   combat.CancelAttack();combat.Attack(false);gear.Equip(bow);await Wait(180);
   C(!combat.IsExecutingAttack&&!animation.IsBusy,"Programmatic weapon swap cancels the old attack and its animation");
   C(animator.GetCurrentAnimatorStateInfo(attackLayer).shortNameHash==Animator.StringToHash("Empty"),"Cancellation clears even a just-requested weapon motion");
   combat.CancelAttack();animation.SetBusy(false);combat.Attack(false);var before=gear.Items[EquipmentSlot.MainHand];bag.items[0]=sword;bag.NotifyExchange();gear.Equip(sword,0);
   C(gear.Items[EquipmentSlot.MainHand]==before&&bag.items[0]==sword,"Interactive equipment swap is rejected while the action is busy");combat.CancelAttack();
   var ranged=UnityEngine.Object.Instantiate(bow.weaponAttacks);cleanup.Add(ranged);var sameCategory=UnityEngine.Object.Instantiate(sword);cleanup.Add(sameCategory);sameCategory.weaponAttacks=ranged;
   gear.Equip(sameCategory);combat.CancelAttack();animation.SetBusy(false);enemy.transform.position=p.transform.position+Vector3.forward*5;Physics.SyncTransforms();await Wait(250);hp=victim.currentHealth;combat.Attack(false);duration=combat.AttackDuration;ranged.projectilePrefab=null;ranged.projectileSpeed=1;ranged.projectileRange=1;
   C(combat.CurrentAttackSet==ranged&&sameCategory.itemType==sword.itemType,"Different attack sets work independently of the same item category");
   await Wait((int)(duration*1000)+500);C(victim.currentHealth<hp,"Projectile delivery retains the accepted prefab/speed/range snapshot");
   combat.CancelAttack();gear.Unequip(EquipmentSlot.MainHand);animation.SetBusy(false);enemy.transform.position=p.transform.position+Vector3.forward*.8f;Physics.SyncTransforms();combat.Attack(true);
   C(combat.IsExecutingAttack&&combat.CurrentAttackMotion.clip==gear.InnateAttacks.lightAttacks[0].clip,"Player legacy heavy request remains light-only after unequip");combat.CancelAttack();
   var ability=AssetDatabase.FindAssets("t:Abilities").Select(g=>AssetDatabase.LoadAssetAtPath<Abilities>(AssetDatabase.GUIDToAssetPath(g))).First(a=>a.startAnimation!=null);combat.abilities=ability;combat.Ability();await Wait(150);
   int abilityHash=Animator.StringToHash(ability.AnimationState);
   C(Enumerable.Range(0,animator.layerCount).Any(i=>animator.GetCurrentAnimatorStateInfo(i).shortNameHash==abilityHash||animator.GetNextAnimatorStateInfo(i).shortNameHash==abilityHash),"Existing ability animation plays its authored state without a replacement slot");
   await Wait((int)(ability.startAnimation.length*1000)+300);C(!animation.IsBusy,"Ability recovery remains tied to its authored clip");combat.CancelAttack();
  }catch(Exception e){errors.Add(e.ToString());}
  finally{if(gear!=null)gear.UnequipAll();foreach(var item in cleanup)if(item!=null)UnityEngine.Object.Destroy(item);if(nav.valid)nav.Remove();Application.logMessageReceived-=log;}
  var result=new{passed=checks.All(c=>(bool)c.GetType().GetProperty("pass").GetValue(c))&&errors.Count==0,checks,errors};
  Directory.CreateDirectory("Assets/Plan/Validation");File.WriteAllText("Assets/Plan/Validation/equipment-animation-runtime.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
}