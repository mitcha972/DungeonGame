using System;using System.IO;using System.Linq;using System.Threading.Tasks;using System.Collections.Generic;using UnityEngine;using UnityEngine.InputSystem;using UnityEngine.InputSystem.LowLevel;using Unity.Cinemachine;using Newtonsoft.Json;
public static class VerifyMouseFacingCamera{
 static List<object> checks=new List<object>();static List<string> errors=new List<string>();
 static void C(bool ok,string name,object details=null)=>checks.Add(new{pass=ok,name,details});
 static async Task Wait(float seconds=.3f){float end=Time.time+seconds,limit=Time.realtimeSinceStartup+15;while(Time.time<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Gameplay clock stopped");await Task.Delay(20);}}
 public static async Task<object> Run(){
 if(!Application.isPlaying)throw new Exception("Play Mode required.");checks.Clear();errors.Clear();
 var p=UnityEngine.Object.FindObjectsByType<StarterAssets.ThirdPersonController>(FindObjectsSortMode.None).First();var stats=p.GetComponent<PlayerStats>();stats.maxHealth=stats.currentHealth=10000;
 var state=p.GetComponent<PlayerControlState>();var input=p.GetComponent<PlayerInput>();var animator=p.GetComponentInChildren<Animator>();var combat=p.GetComponent<PlayerCombat>();var anim=p.GetComponent<CharacterAnimation>();
 var oldSettings=InputSystem.settings;var settings=UnityEngine.Object.Instantiate(oldSettings);settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;InputSystem.settings=settings;
 var oldDevices=input.devices.ToArray();string oldScheme=input.currentControlScheme;
 var kb=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();GameObject platform=null,dummy=null;
 Application.LogCallback log=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception)errors.Add(m);};Application.logMessageReceived+=log;
 Action<Key[]> keys=k=>InputSystem.QueueStateEvent(kb,new KeyboardState(k));
 Action<Vector2,bool,bool> look=(delta,left,right)=>{var v=new MouseState{delta=delta};if(left)v=v.WithButton(MouseButton.Left);if(right)v=v.WithButton(MouseButton.Right);InputSystem.QueueStateEvent(mouse,v);};
 var brain=UnityEngine.Object.FindFirstObjectByType<CinemachineBrain>();
 Func<float> cameraPitch=()=>Mathf.DeltaAngle(0,Camera.main.transform.eulerAngles.x);
 try{
 input.SwitchCurrentControlScheme("KeyboardMouse",kb,mouse);state.ClosePanels();state.SendMessage("OnApplicationFocus",true);await Wait(.4f);
 foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None))ai.enabled=false;
 C(state.GameplayAllowed,"Gameplay look eligible at cold start");
 C(brain.ActiveVirtualCamera.Name==p.tpCamera.name,"Cold start selects configured third-person camera",brain.ActiveVirtualCamera.Name);
 C(p.tpCamera.GetComponent<CinemachineCamera>().Follow==p.CinemachineCameraTarget.transform&&p.fpCamera.GetComponent<CinemachineCamera>().Follow==p.CinemachineCameraTarget.transform,"Both modes follow the motor-driven view anchor");
 C(UnityEngine.Object.FindObjectsByType<CinemachineCamera>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(c=>c.name=="ThirdPersonFollowCamera").All(c=>!c.enabled),"Legacy third-person camera cannot win a startup priority tie");
 float yaw=p.ViewYaw,pitch=cameraPitch();look(new Vector2(20,10),false,false);await Wait(.6f);
 C(Mathf.Abs(Mathf.DeltaAngle(yaw,p.ViewYaw))>1,"Native mouse X changes view yaw on first enable",new{before=yaw,after=p.ViewYaw});
 C(Mathf.Abs(Mathf.DeltaAngle(pitch,cameraPitch()))>1,"Native mouse Y changes the real camera on first enable",new{before=pitch,after=cameraPitch()});
 C(Mathf.Abs(Mathf.DeltaAngle(p.transform.eulerAngles.y,p.ViewYaw))<1,"Idle body facing follows mouse yaw");
 C(Mathf.Abs(Mathf.DeltaAngle(cameraPitch(),p.CinemachineCameraTarget.transform.eulerAngles.x))<1,"Displayed pitch follows the driven target");
 look(new Vector2(0,1000),false,false);await Wait(.3f);C(Mathf.Abs(Mathf.DeltaAngle(cameraPitch(),p.BottomClamp))<1,"Upward look respects lower pitch limit",cameraPitch());
 look(new Vector2(0,-1000),false,false);await Wait(.3f);C(Mathf.Abs(Mathf.DeltaAngle(cameraPitch(),p.TopClamp))<1,"Downward look respects upper pitch limit",cameraPitch());
 C(Vector3.Angle(p.transform.up,Vector3.up)<.1f,"Vertical aim keeps the body upright");
 look(new Vector2(0,p.TopClamp),false,false);await Wait(.3f);
 platform=GameObject.CreatePrimitive(PrimitiveType.Cube);platform.transform.position=new Vector3(400,49.9f,400);platform.transform.localScale=new Vector3(80,.2f,80);
 var cc=p.GetComponent<CharacterController>();cc.enabled=false;p.transform.position=new Vector3(400,50.1f,400);cc.enabled=true;p.GroundLayers|=1;Physics.SyncTransforms();await Wait(.3f);
 foreach(var pair in new[]{(Key.W,Vector3.forward,"forward"),(Key.S,Vector3.back,"backward"),(Key.A,Vector3.left,"left"),(Key.D,Vector3.right,"right")}){
 var start=p.transform.position;float facing=p.transform.eulerAngles.y;var expected=Quaternion.Euler(0,p.ViewYaw,0)*pair.Item2;keys(new[]{pair.Item1});await Wait(.5f);var delta=p.transform.position-start;delta.y=0;
 C(delta.magnitude>.1f&&Vector3.Dot(delta.normalized,expected)>.8f,"Native "+pair.Item3+" movement follows view-relative direction",delta.ToString());
 C(Mathf.Abs(Mathf.DeltaAngle(facing,p.transform.eulerAngles.y))<1,"Native "+pair.Item3+" input does not steer body facing");
 if(pair.Item1!=Key.W){string clip=pair.Item1==Key.S?"Backward":pair.Item1==Key.A?"Left":"Right";C(animator.GetCurrentAnimatorClipInfo(0).Any(x=>x.weight>.05f&&x.clip.name.Contains(clip)),"Directional "+pair.Item3+" animation plays",string.Join(",",animator.GetCurrentAnimatorClipInfo(0).Select(x=>x.clip.name+":"+x.weight)));}
 keys(new Key[0]);await Wait(.2f);
 }
 keys(new[]{Key.W,Key.D});await Wait(.8f);
 var velocity=(Vector3)typeof(StarterAssets.ThirdPersonController).GetField("_horizontalVelocity",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(p);
 C(velocity.magnitude<=p.MoveSpeed+.05f&&velocity.magnitude>p.MoveSpeed*.9f,"Diagonal movement keeps the configured speed",velocity.magnitude);
 C(Mathf.Abs(Mathf.DeltaAngle(p.transform.eulerAngles.y,p.ViewYaw))<1,"Diagonal movement does not steer facing");
 keys(new Key[0]);await Wait(.2f);keys(new[]{Key.W,Key.LeftShift});await Wait(.9f);
 velocity=(Vector3)typeof(StarterAssets.ThirdPersonController).GetField("_horizontalVelocity",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(p);
 C(velocity.magnitude>p.MoveSpeed&&Mathf.Abs(velocity.magnitude-p.SprintSpeed)<.1f,"Native sprint retains configured speed",velocity.magnitude);
 keys(new Key[0]);await Wait(.2f);
 dummy=new GameObject("Camera focus test");dummy.transform.position=p.transform.position+Vector3.left*5;p.FacingTarget=dummy.transform;look(new Vector2(15,0),false,false);await Wait(.6f);
 C(Mathf.Abs(Mathf.DeltaAngle(p.transform.eulerAngles.y,p.ViewYaw))<1,"Enemy focus cannot override mouse-facing");p.FacingTarget=null;
 keys(new[]{Key.V});await Wait(.1f);keys(new Key[0]);await Wait(1.3f);C(p.FirstPerson&&brain.ActiveVirtualCamera.Name==p.fpCamera.name,"V activates first-person camera");
 pitch=cameraPitch();look(new Vector2(0,8),false,false);await Wait(.3f);C(Mathf.Abs(Mathf.DeltaAngle(pitch,cameraPitch()))>1,"First-person vertical look uses the same driven anchor");
 keys(new[]{Key.V});await Wait(.1f);keys(new Key[0]);await Wait(1.3f);C(!p.FirstPerson&&brain.ActiveVirtualCamera.Name==p.tpCamera.name,"V restores third-person camera");
 yaw=p.ViewYaw;state.InventoryUI.inv.SetActive(true);state.Synchronize();look(new Vector2(25,10),false,false);await Wait(.3f);C(!state.GameplayAllowed&&Mathf.Abs(Mathf.DeltaAngle(yaw,p.ViewYaw))<.01f,"Inventory blocks gameplay look");
 state.ClosePanels();state.Synchronize();look(new Vector2(8,4),false,false);await Wait(.4f);C(state.GameplayAllowed&&Mathf.Abs(Mathf.DeltaAngle(yaw,p.ViewYaw))>1,"Closing inventory restores fresh mouse look");
 yaw=p.ViewYaw;state.SendMessage("OnApplicationFocus",false);look(new Vector2(25,10),false,false);await Wait(.2f);C(!state.GameplayAllowed&&Mathf.Abs(Mathf.DeltaAngle(yaw,p.ViewYaw))<.01f,"Focus loss blocks view input");
 state.SendMessage("OnApplicationFocus",true);look(new Vector2(8,0),false,false);await Wait(.4f);C(state.GameplayAllowed&&Mathf.Abs(Mathf.DeltaAngle(yaw,p.ViewYaw))>1,"Focus regain restores view input");
 combat.CancelAttack();anim.SetBusy(false);look(Vector2.zero,true,false);keys(new[]{Key.W});await Wait(.2f);yaw=p.ViewYaw;look(new Vector2(10,0),true,false);await Wait(.3f);C(Mathf.Abs(Mathf.DeltaAngle(yaw,p.ViewYaw))>1&&p.canMove&&p.canRotate,"Attack retains mouse-facing and movement");look(Vector2.zero,false,false);keys(new Key[0]);combat.CancelAttack();
 var shield=UnityEditor.AssetDatabase.FindAssets("t:Equipment").Select(g=>UnityEditor.AssetDatabase.LoadAssetAtPath<Equipment>(UnityEditor.AssetDatabase.GUIDToAssetPath(g))).First(e=>e.itemType==ItemType.Shield);p.GetComponent<CharacterEquipment>().Equip(shield);anim.SetBusy(false);
 look(Vector2.zero,false,true);await Wait(.2f);yaw=p.ViewYaw;look(new Vector2(10,0),false,true);await Wait(.3f);C(combat.isBlocking&&Mathf.Abs(Mathf.DeltaAngle(yaw,p.ViewYaw))>1,"Shield block retains mouse-facing");look(Vector2.zero,false,false);await Wait(.1f);combat.CancelAttack();
 p.gameObject.SetActive(false);await Task.Delay(100);p.gameObject.SetActive(true);await Wait(.4f);pitch=cameraPitch();look(new Vector2(0,8),false,false);await Wait(.4f);C(Mathf.Abs(Mathf.DeltaAngle(pitch,cameraPitch()))>1&&brain.ActiveVirtualCamera.Name==p.tpCamera.name,"Re-enable preserves camera selection and vertical input");
 }catch(Exception e){errors.Add(e.ToString());}
 finally{if(input!=null&&oldDevices.All(d=>d.added))input.SwitchCurrentControlScheme(oldScheme,oldDevices);InputSystem.settings=oldSettings;InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(kb);UnityEngine.Object.Destroy(settings);if(platform!=null)UnityEngine.Object.Destroy(platform);if(dummy!=null)UnityEngine.Object.Destroy(dummy);Application.logMessageReceived-=log;}
 var result=new{passed=checks.All(c=>(bool)c.GetType().GetProperty("pass").GetValue(c))&&errors.Count==0,checks,errors};Debug.Log("[CameraValidation] passed="+result.passed+", checks="+checks.Count+", errors="+errors.Count);File.WriteAllText("Assets/Plan/Validation/camera-runtime.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
 }
}