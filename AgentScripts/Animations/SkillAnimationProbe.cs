using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json.Linq;
public static class SkillAnimationProbe {
 public static async Task<object> Run(){if(!EditorApplication.isPlaying)throw new Exception("Play Mode required");int frame=Time.frameCount;await Task.Delay(250);var messages=new JArray();Application.LogCallback log=(m,s,t)=>messages.Add(m);Application.logMessageReceived+=log;try{if(Keyboard.current!=null){InputSystem.EnableDevice(Keyboard.current);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.E));await Task.Delay(120);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Task.Delay(180);}}finally{Application.logMessageReceived-=log;}
 var r=new JObject{["scene"]=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,["framesAdvance"]=Time.frameCount>frame,["skillTrees"]=UnityEngine.Object.FindObjectsByType<UI_SkillTree>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,["playerCombat"]=UnityEngine.Object.FindObjectsByType<PlayerCombat>(FindObjectsSortMode.None).Length,["legacySkillPollers"]=UnityEngine.Object.FindObjectsByType<PlayerSword>(FindObjectsSortMode.None).Length,["animators"]=new JArray(UnityEngine.Object.FindObjectsByType<Animator>(FindObjectsSortMode.None).Select(a=>new JObject{["name"]=a.name,["controller"]=a.runtimeAnimatorController==null?null:a.runtimeAnimatorController.name,["avatar"]=a.avatar==null?null:a.avatar.name})),["inputActions"]=new JArray(UnityEngine.Object.FindObjectsByType<UnityEngine.InputSystem.PlayerInput>(FindObjectsSortMode.None).SelectMany(p=>p.actions==null?new string[0]:p.actions.Select(a=>a.name))),["messagesAfterE"]=messages,["scope"]="SkillTree demo inspection and synthetic E only; not complete ability effects."};File.WriteAllText("Assets/Plan/Validation/animation-skilltree.json",r.ToString());return r;}
}
