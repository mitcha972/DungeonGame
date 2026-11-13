using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json.Linq;
public static class AnimationCoverage {
 static JArray checks=new JArray();static void C(bool b,string s,object detail=null){checks.Add(new JObject{["check"]=s,["pass"]=b,["detail"]=detail==null?null:JToken.FromObject(detail)});}
 static async Task Wait(float sec){await Task.Delay((int)(sec*1000));}
 static async Task PressKey(Key key,float duration){InputSystem.EnableDevice(Keyboard.current);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));await Wait(duration);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Wait(.15f);}
 public static async Task<object> Run(){if(!EditorApplication.isPlaying)throw new Exception("Requires Play Mode");var p=UnityEngine.Object.FindFirstObjectByType<PlayerCombat>();var a=p.GetComponentInChildren<Animator>();var ca=p.GetComponent<CharacterAnimator>();var em=p.GetComponent<EquipmentManager>();var st=p.GetComponent<PlayerControlState>();st?.ClosePanels();st?.SendMessage("OnApplicationFocus",true);foreach(var e in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None))e.enabled=false;
 var errors=new JArray();Application.LogCallback logger=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception)errors.Add(m);};Application.logMessageReceived+=logger;
 try{
 p.CancelAttack();var weapon=em.currentEquipment.ContainsKey(EquipmentSlot.MainHand)?em.currentEquipment[EquipmentSlot.MainHand]:null;var shield=em.currentEquipment.ContainsKey(EquipmentSlot.OffHand)?em.currentEquipment[EquipmentSlot.OffHand]:null;
 if(weapon!=null){ca.OnEquipmentChanged(weapon,null);C(a.GetLayerWeight(2)==1,"Main hand enables right grip");ca.OnEquipmentChanged(null,weapon);C(a.GetLayerWeight(2)==0&&a.GetLayerWeight(1)==1,"Main hand removal preserves attack layer");ca.OnEquipmentChanged(weapon,null);}if(shield!=null){ca.OnEquipmentChanged(shield,null);C(a.GetLayerWeight(3)==1,"Offhand enables left grip");ca.OnEquipmentChanged(null,shield);C(a.GetLayerWeight(3)==0,"Offhand removal clears left grip");ca.OnEquipmentChanged(shield,null);}
 var previous=ca.CurrentWeapon;var field=typeof(CharacterAnimator).GetField("<CurrentWeapon>k__BackingField",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
 foreach(var w in AssetDatabase.FindAssets("t:WeaponClass").Select(g=>AssetDatabase.LoadAssetAtPath<WeaponClass>(AssetDatabase.GUIDToAssetPath(g))).Where(w=>w.itemType==ItemType.Unarmed||w.itemType==ItemType.Bow||w.itemType==ItemType.Sword).Where(w=>w.lightAttackClips!=null&&w.lightAttackClips.Length>0)){
 field.SetValue(ca,w);p.currentProjectile=w.projectile;
 foreach(bool heavy in new[]{false,true}){var clips=heavy?w.heavyAttackClips:w.lightAttackClips;if(clips==null||!clips.Any(c=>c!=null))continue;p.CancelAttack();p.Attack(heavy);C(p.IsExecutingAttack,w.name+(heavy?" heavy":" light")+" accepted");await Wait(.2f);var shown=a.GetCurrentAnimatorClipInfo(1).Select(x=>x.clip.name).ToArray();C(shown.Any(n=>clips.Any(c=>c!=null&&c.name==n)),w.name+(heavy?" heavy":" light")+" correct clip",shown);await Wait(ca.CurrentAttackDuration+.3f);C(!p.IsExecutingAttack&&!ca.AnimationIsBusy(),w.name+" recovery");}}
 field.SetValue(ca,previous);p.currentProjectile=previous!=null?previous.projectile:null;
 var before=p.transform.position;await PressKey(Key.W,.55f);C(Vector3.Distance(before,p.transform.position)>.05f,"W input moves player");
 var ability=AssetDatabase.FindAssets("t:Abilities").Select(g=>AssetDatabase.LoadAssetAtPath<Abilities>(AssetDatabase.GUIDToAssetPath(g))).FirstOrDefault();if(ability!=null){p.CancelAttack();p.abilities=ability;p.Ability();await Wait(.2f);C(Enumerable.Range(0,a.layerCount).Any(i=>a.GetCurrentAnimatorStateInfo(i).shortNameHash==Animator.StringToHash("Ability")||a.GetNextAnimatorStateInfo(i).shortNameHash==Animator.StringToHash("Ability")),"Direct ability API enters Ability state (no production ability input)");await Wait(.25f);C(ca.AnimationIsBusy(),"Ability remains busy beyond old 0.3-second recovery",new{clip=ability.startAnimation.name,length=ability.startAnimation.length});await Wait(ability.startAnimation.length+.2f);p.CancelAttack();}
 }catch(Exception ex){errors.Add(ex.ToString());}finally{Application.logMessageReceived-=logger;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.QueueStateEvent(Mouse.current,new MouseState());}
 var r=new JObject{["checks"]=checks,["errors"]=errors,["passed"]=checks.All(x=>(bool)x["pass"])&&errors.Count==0};File.WriteAllText("Assets/Plan/Validation/animation-coverage.json",r.ToString());return r;}
}



