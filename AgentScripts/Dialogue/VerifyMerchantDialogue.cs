using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using DungeonGame.Dialogue;
public static class VerifyMerchantDialogue
{
 public static async Task<object> Run() {
 var settings=InputSystem.settings;var bg=settings.backgroundBehavior;var editor=settings.editorInputBehaviorInPlayMode;
 try { settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; return await RunCore(); }
 finally { InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); if(Mouse.current!=null)InputSystem.QueueStateEvent(Mouse.current,new MouseState());InputSystem.Update();settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=editor; }
 }
 static void Check(bool value,string label){if(!value)throw new Exception("FAIL: "+label);Debug.Log("Dialogue verification PASS: "+label);}
 static async Task KeyPress(Key key){InputSystem.EnableDevice(Keyboard.current);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));await Task.Delay(180);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Task.Delay(180);}
 public static async Task<object> RunCore()
 {
 var state=UnityEngine.Object.FindFirstObjectByType<PlayerControlState>();var controller=state.GetComponent<DialogueController>();
 var npc=UnityEngine.Object.FindFirstObjectByType<DialogueNpc>();var world=UnityEngine.Object.FindFirstObjectByType<DialogueWorldMarker>();
 controller.Close(); UnityEngine.Object.FindFirstObjectByType<SettingsMenu>().Resume();
 state.SendMessage("OnApplicationFocus", true);
 await Task.Delay(500);
 Check(Time.timeScale==1f,"world begins at normal time scale");Check(state.GameplayAllowed,"gameplay initially enabled");
 Check(npc.shop.inventory.items.Count==3,"three guaranteed merchant items populated");
 await KeyPress(Key.E);
 Check(controller.IsOpen,"E opens merchant dialogue through Interactable raycast");
 Check(!state.GameplayAllowed && Cursor.visible && Cursor.lockState==CursorLockMode.None,"dialogue owns UI and gameplay controls");
 Check(controller.CurrentNode.id=="greeting","merchant greeting and four responses");
 var movement=state.GetComponent<StarterAssets.ThirdPersonController>();var inputs=state.GetComponent<StarterAssets.StarterAssetsInputs>();
 int attacks=0; Action<bool> attackObserver=heavy=>attacks++;state.GetComponent<PlayerCombat>().OnAttack+=attackObserver;
 var start=state.transform.position;var rotation=movement.CinemachineCameraTarget.transform.rotation;float clock=world.SimulatedSeconds;int frame=Time.frameCount;
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W,Key.Space,Key.E,Key.Tab));
 if(Mouse.current!=null)InputSystem.QueueStateEvent(Mouse.current,new MouseState{delta=new Vector2(70,30),buttons=3});
 await Task.Delay(600);
 InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());if(Mouse.current!=null)InputSystem.QueueStateEvent(Mouse.current,new MouseState());await Task.Delay(200);
 Check(!state.GameplayAllowed && controller.IsOpen,"movement combat inventory and E cannot take dialogue ownership");
 Check(Vector2.Distance(new Vector2(start.x,start.z),new Vector2(state.transform.position.x,state.transform.position.z))<.01f,"movement blocked while dialogue open");
 Check(Quaternion.Angle(rotation,movement.CinemachineCameraTarget.transform.rotation)<.1f && inputs.look==Vector2.zero,"camera look blocked while dialogue open");
 state.GetComponent<PlayerCombat>().OnAttack-=attackObserver;Check(attacks==0 && !state.GetComponent<PlayerCombat>().isBlocking,"attack and block inputs ignored");
 Check(Time.timeScale==1f && world.SimulatedSeconds>clock && Time.frameCount>frame,"world simulation advances during dialogue");
 await KeyPress(Key.Digit2);Check(controller.CurrentNode.id=="wares","number key selects wares branch");
 // Standard UI navigation and Submit go through the existing InputSystemUIInputModule.
 await KeyPress(Key.DownArrow);await KeyPress(Key.Enter);Check(controller.CurrentNode.id=="greeting","arrows and Enter return to greeting");
 await KeyPress(Key.Digit3);Check(controller.CurrentNode.id=="rumors","rumors branch available");await KeyPress(Key.Digit1);Check(controller.CurrentNode.id=="greeting","rumors returns to greeting");
 bool transferredWithoutGameplay=true;Action<bool> observer=allowed=>{if(allowed)transferredWithoutGameplay=false;};state.GameplayChanged+=observer;
 await KeyPress(Key.Digit1);state.GameplayChanged-=observer;
 Check(!controller.IsOpen && state.ShopUI.gameObject.activeInHierarchy,"Trade opens existing ShopUI");
 Check(transferredWithoutGameplay && !state.GameplayAllowed,"dialogue transfers directly to shop without gameplay unlock");
 Check(state.ShopUI.currentShop==npc.shop && state.ShopUI.slots.Count(s=>s.gameObject.activeInHierarchy)==3,"ShopUI shows merchant stock");
 await KeyPress(Key.E);Check(!state.ShopUI.gameObject.activeInHierarchy && state.GameplayAllowed,"E closes shop and restores gameplay");
 await KeyPress(Key.E);Check(controller.IsOpen,"merchant dialogue reopens");
 await KeyPress(Key.Escape);Check(!controller.IsOpen && state.GameplayAllowed && Time.timeScale==1f,"Escape closes dialogue without pausing world");
 await KeyPress(Key.E);await KeyPress(Key.Digit4);Check(!controller.IsOpen && state.GameplayAllowed,"Goodbye restores gameplay");
 await KeyPress(Key.E);Check(controller.IsOpen,"dialogue ready for visual inspection");
 return new {passed=true,frameCount=Time.frameCount,worldSeconds=world.SimulatedSeconds,timeScale=Time.timeScale};
 }
}





