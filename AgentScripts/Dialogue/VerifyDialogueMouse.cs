using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using DungeonGame.Dialogue;
public static class VerifyDialogueMouse
{
 public static async Task<object> Run(){
 var settings=InputSystem.settings;var bg=settings.backgroundBehavior;var ed=settings.editorInputBehaviorInPlayMode;
 Mouse testMouse=null;PlayerInput playerInput=null;InputDevice[] previousDevices=null;string previousScheme=null;
 try {
 settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
 var state=UnityEngine.Object.FindFirstObjectByType<PlayerControlState>();state.SendMessage("OnApplicationFocus",true);var dialogue=state.GetComponent<DialogueController>();playerInput=state.GetComponent<PlayerInput>();previousDevices=playerInput.devices.ToArray();previousScheme=playerInput.currentControlScheme;testMouse=InputSystem.AddDevice<Mouse>("DialogueTestMouse");playerInput.SwitchCurrentControlScheme("KeyboardMouse",Keyboard.current,testMouse);await Task.Delay(250);
 if(state.ShopUI.gameObject.activeSelf)state.ShopUI.CloseUI();dialogue.Close();dialogue.Open(UnityEngine.Object.FindFirstObjectByType<DialogueNpc>());
 Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)dialogue.panel.transform);await Task.Delay(250);
 var button=dialogue.panel.responseParent.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<TMPro.TMP_Text>().text.Contains("Ask About Wares"));
 var pos=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);
 var pointer=new PointerEventData(EventSystem.current){position=pos};var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
 InputSystem.EnableDevice(Mouse.current);InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=pos});await Task.Delay(180);
 InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=pos,buttons=1});await Task.Delay(180);InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=pos});await Task.Delay(250);
 for(int wait=0;wait<40 && dialogue.CurrentNode?.id!="wares";wait++)await Task.Delay(50);
 if(dialogue.CurrentNode?.id!="wares")throw new Exception("Mouse response selection failed; hits="+string.Join(",",hits.Select(h=>h.gameObject.name)));
 Debug.Log("Dialogue verification PASS: mouse click selects wares through GraphicRaycaster and InputSystemUIInputModule");
 // Use the visible Trade response's actual pointer path as well.
 button=dialogue.panel.responseParent.GetComponentsInChildren<Button>().First();pos=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);
 InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=pos});await Task.Delay(180);InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=pos,buttons=1});await Task.Delay(180);InputSystem.QueueStateEvent(Mouse.current,new MouseState{position=pos});await Task.Delay(250);
 for(int wait=0;wait<40 && dialogue.IsOpen;wait++)await Task.Delay(50);
 if(dialogue.IsOpen || !state.ShopUI.gameObject.activeInHierarchy || state.GameplayAllowed)throw new Exception("Mouse Trade failed");
 Debug.Log("Dialogue verification PASS: mouse Trade opens the existing ShopUI and retains control lock");
 return new {passed=true,stock=state.ShopUI.inventory.items.Count};
 }finally{if(playerInput!=null && previousDevices!=null)playerInput.SwitchCurrentControlScheme(previousScheme,previousDevices);if(testMouse!=null)InputSystem.RemoveDevice(testMouse);settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=ed;}
 }
}



