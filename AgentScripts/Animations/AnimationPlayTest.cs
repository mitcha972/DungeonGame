using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json.Linq;
public static class AnimationPlayTest {
 static async Task Wait(int ms){await Task.Delay(ms);} static void MouseEvent(MouseState value){InputSystem.QueueStateEvent(Mouse.current,value);InputSystem.Update();}
 public static async Task<object> Run(){if(!EditorApplication.isPlaying)throw new Exception("Requires Play Mode");var originalInput=InputSystem.settings;var testInput=UnityEngine.Object.Instantiate(originalInput);testInput.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;testInput.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings=testInput;var checks=new JArray();var errors=new List<string>();Application.LogCallback log=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception)errors.Add(m);};Application.logMessageReceived+=log;
 var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;int frame=Time.frameCount;
 try{await Wait(300);checks.Add(new JObject{["check"]="Frames advance",["pass"]=Time.frameCount>frame});
 var p=UnityEngine.Object.FindFirstObjectByType<PlayerCombat>();if(p!=null){var state=p.GetComponent<PlayerControlState>();state?.ClosePanels();state?.SendMessage("OnApplicationFocus",true);var a=p.GetComponentInChildren<Animator>();var presentation=p.GetComponent<CharacterAnimator>();var enemies=UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None);foreach(var e in enemies)e.enabled=false;
 var attacks=new List<bool>();p.OnAttack+=attacks.Add;InputSystem.EnableDevice(Mouse.current);InputSystem.EnableDevice(Keyboard.current);
 MouseEvent(new MouseState());await Wait(100);MouseEvent(new MouseState().WithButton(MouseButton.Left));await Wait(80);MouseEvent(new MouseState());await Wait(140);
 checks.Add(new JObject{["check"]="Mouse tap routes attack",["pass"]=attacks.Count>0,["weapon"]=presentation.CurrentWeapon==null?null:presentation.CurrentWeapon.name,["states"]=new JArray(Enumerable.Range(0,a.layerCount).Select(i=>new JObject{["layer"]=i,["hash"]=a.GetCurrentAnimatorStateInfo(i).shortNameHash,["clips"]=new JArray(a.GetCurrentAnimatorClipInfo(i).Select(c=>c.clip.name)),["weight"]=a.GetLayerWeight(i)}))});
 await Wait((int)(Mathf.Max(presentation.CurrentAttackDuration, 1f)*1000+600));checks.Add(new JObject{["check"]="Attack recovers",["pass"]=!p.IsExecutingAttack&&!presentation.AnimationIsBusy()});
 int n=attacks.Count;MouseEvent(new MouseState().WithButton(MouseButton.Left));await Wait(400);MouseEvent(new MouseState());await Wait(100);checks.Add(new JObject{["check"]="Mouse hold routes supported power attack or light fallback",["pass"]=attacks.Count>n,["heavy"]=attacks.Count>n&&attacks.Last(),["supported"]=p.SupportsPowerAttack});await Wait((int)(Mathf.Max(presentation.CurrentAttackDuration, 1f)*1000+600));
 n=attacks.Count;MouseEvent(new MouseState().WithButton(MouseButton.Right));await Wait(130);bool block=p.isBlocking;MouseEvent(new MouseState());await Wait(150);checks.Add(new JObject{["check"]="Right mouse block or unshielded light fallback",["pass"]=p.HasShield?block&&!p.isBlocking:attacks.Count>n,["shield"]=p.HasShield});await Wait((int)(Mathf.Max(presentation.CurrentAttackDuration, 1f)*1000+600));
 checks.Add(new JObject{["check"]="Player humanoid avatar",["pass"]=a.avatar!=null&&a.avatar.isValid&&a.isHuman});
 foreach(var e in UnityEngine.Object.FindObjectsByType<CharacterCombat>(FindObjectsSortMode.None).Where(x=>x!=p)){e.CancelAttack();var ca=e.GetComponent<CharacterAnimator>();if(ca==null)continue;ca.SetBusy(false);e.Attack(false);await Wait(1200);checks.Add(new JObject{["check"]="Enemy attack recovery: "+e.name,["pass"]=!e.IsExecutingAttack,["animators"]=new JArray(e.GetComponentsInChildren<Animator>().Select(x=>new JObject{["name"]=x.name,["human"]=x.isHuman,["valid"]=x.avatar!=null&&x.avatar.isValid,["clips"]=new JArray(x.GetCurrentAnimatorClipInfo(0).Select(c=>c.clip.name))}))});}
 p.OnAttack-=attacks.Add;
 }else checks.Add(new JObject{["check"]="Player combat present",["pass"]=false});
 }catch(Exception ex){errors.Add(ex.ToString());}finally{Application.logMessageReceived-=log;InputSystem.settings=originalInput;UnityEngine.Object.Destroy(testInput);}
 var r=new JObject{["scene"]=scene,["checks"]=checks,["errors"]=new JArray(errors.Distinct()),["passed"]=checks.All(c=>(bool)c["pass"])&&errors.Count==0};File.WriteAllText("Assets/Plan/Validation/animations-"+scene+".json",r.ToString());return r;}
}




