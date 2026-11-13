using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json.Linq;
public static class InputDiagnosis {
 public static async Task<object> Run(){if(!EditorApplication.isPlaying)throw new Exception("Play required");var p=UnityEngine.Object.FindFirstObjectByType<PlayerCombat>();var pi=p.GetComponent<UnityEngine.InputSystem.PlayerInput>();var pc=p.GetComponent<PlayerController>();var state=p.GetComponent<PlayerControlState>();state.SendMessage("OnApplicationFocus",true);p.CancelAttack();var trace=new JArray();Action<InputAction.CallbackContext> log=c=>trace.Add(c.action.name+":"+c.phase+":"+state.GameplayAllowed+":"+p.canAttack+":"+p.attackIsReady+":"+p.GetComponent<CharacterAnimator>().AnimationIsBusy());pi.onActionTriggered+=log;var attack=pi.actions.FindAction("Attack");attack.performed+=log;int count=0;Action<bool> onAttack=h=>count++;p.OnAttack+=onAttack;
 try{InputSystem.EnableDevice(Mouse.current);InputSystem.QueueStateEvent(Mouse.current,new MouseState());InputSystem.Update();await Task.Delay(50);InputSystem.QueueStateEvent(Mouse.current,new MouseState().WithButton(MouseButton.Left));InputSystem.Update();await Task.Delay(100);InputSystem.QueueStateEvent(Mouse.current,new MouseState());InputSystem.Update();await Task.Delay(250);return new {trace,count,action=attack.enabled,binding=attack.bindings.Select(b=>b.effectivePath).ToArray(),controls=attack.controls.Select(c=>c.path).ToArray(), pc.enabled,fields=typeof(PlayerController).GetFields(System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Where(f=>f.Name=="_combat"||f.Name=="_stats"||f.Name=="_playerInput").Select(f=>new{f.Name,value=f.GetValue(pc)==null?"null":f.GetValue(pc).ToString()}).ToArray()};}finally{pi.onActionTriggered-=log;attack.performed-=log;p.OnAttack-=onAttack;}}
}
