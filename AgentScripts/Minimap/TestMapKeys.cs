using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public static class TestMapKeys {
static async Task Press(Key key) {InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));await Task.Delay(100);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Task.Delay(100);}
public static async Task<string> Run() {
var map=Object.FindFirstObjectByType<MapController>();var state=map.target.GetComponent<PlayerControlState>();
state.SendMessage("OnApplicationFocus",true);map.SetExpanded(false);state.Synchronize();
if(!state.GameplayAllowed)return "Keyboard test blocked by another modal/death";
await Press(Key.M);if(!map.IsExpanded)throw new System.Exception("M did not open");
await Press(Key.Escape);if(map.IsExpanded || Time.timeScale==0)throw new System.Exception("Escape did not collapse without pause");
await Press(Key.M);await Press(Key.M);if(map.IsExpanded)throw new System.Exception("M did not toggle closed");
Debug.Log("[Minimap validation] PASS actual M open/toggle and Escape close");return "Keyboard checks passed";
}}
