using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Unity.Cinemachine;
using Newtonsoft.Json;
public static class CameraStartupDiagnostic {
 static async Task Wait(int frames=8){int end=Time.frameCount+frames;float limit=Time.realtimeSinceStartup+15;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Frames stopped");await Task.Delay(20);}}
 public static async Task<object> Run(){
 var p=UnityEngine.Object.FindObjectsByType<StarterAssets.ThirdPersonController>(FindObjectsSortMode.None).First();
 var input=p.GetComponent<PlayerInput>();var devices=input.devices.ToArray();var scheme=input.currentControlScheme;var settings=InputSystem.settings;var test=UnityEngine.Object.Instantiate(settings);test.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;test.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings=test;
 var kb=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();
 try{
 input.SwitchCurrentControlScheme("KeyboardMouse",kb,mouse);var control=p.GetComponent<PlayerControlState>();control.ClosePanels();control.SendMessage("OnApplicationFocus",true);await Wait();
 Func<object> snap=()=>new{frame=Time.frameCount,look=control.Look.ToString(),pitch=typeof(StarterAssets.ThirdPersonController).GetField("_pitch",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(p),target=p.CinemachineCameraTarget.transform.eulerAngles.ToString(),realCamera=Camera.main.transform.eulerAngles.ToString(),live=UnityEngine.Object.FindFirstObjectByType<CinemachineBrain>().ActiveVirtualCamera.Name,cameras=UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(c=>new{c.name,active=c.gameObject.activeInHierarchy,follow=c.Follow==null?null:c.Follow.name,followRotation=c.Follow==null?null:c.Follow.eulerAngles.ToString()}).ToArray()};
 var cold=snap();InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(0,12)});InputSystem.Update();await Wait();var coldLook=snap();
 p.gameObject.SetActive(false);await Wait(3);p.gameObject.SetActive(true);await Wait();var enabled=snap();
 InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(0,-12)});InputSystem.Update();await Wait();var enabledLook=snap();
 var result=new{cold,coldLook,enabled,enabledLook};File.WriteAllText("Assets/Plan/Validation/camera-startup-before.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }finally{if(input!=null&&devices.All(d=>d.added))input.SwitchCurrentControlScheme(scheme,devices);InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(kb);InputSystem.settings=settings;UnityEngine.Object.Destroy(test);}
 }
}