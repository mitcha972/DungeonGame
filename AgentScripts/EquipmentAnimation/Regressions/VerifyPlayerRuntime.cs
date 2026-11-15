using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json;

public static class VerifyPlayerRuntime
{
    private static readonly List<object> checks=new List<object>();
    private static Keyboard keyboard;
    private static UnityEngine.InputSystem.Mouse pointer;
    private static void Check(bool pass,string name,object details=null) => checks.Add(new { pass,name,details });
    private static async Task Wait(int ms) => await Task.Delay(ms);
    private static void Keys(params Key[] keys) { if(keyboard!=null){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys)); InputSystem.Update();} }
    private static void Mouse(MouseState value) { if(pointer!=null){InputSystem.QueueStateEvent(pointer,value); InputSystem.Update();} }
    private static async Task Press(Key key) { Keys(key); await Wait(100); Keys(); await Wait(150); }
    private static void Teleport(PlayerStats p, Vector3 position)
    {
        var cc=p.GetComponent<CharacterController>(); cc.enabled=false; p.transform.position=position; cc.enabled=true; Physics.SyncTransforms();
    }
    private static void Field(object target,string name,object value) => target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
    public static async Task<object> Run()
    {
        if(!Application.isPlaying) throw new Exception("Requires task-owned Play Mode.");
        checks.Clear();
        var original=InputSystem.settings;
        var settings=UnityEngine.Object.Instantiate(original);
        settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings=settings;
        var errors=new List<string>();
        Application.LogCallback logger=(message,stack,type)=>{ if(type==LogType.Error||type==LogType.Exception) errors.Add(message); };
        Application.logMessageReceived+=logger;
        var cleanup=new List<UnityEngine.Object>();
        PlayerStats p=null;
        PlayerInput playerInput=null; InputDevice[] oldDevices=null; string oldScheme=null;
        try
        {
            int firstFrame=Time.frameCount; await Wait(250); Check(Time.frameCount>firstFrame,"Player loop advances");
            p=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).First(x=>x.GetComponent<StarterAssets.ThirdPersonController>()!=null);
            playerInput=p.GetComponent<PlayerInput>(); oldDevices=playerInput.devices.ToArray(); oldScheme=playerInput.currentControlScheme;
            keyboard=InputSystem.AddDevice<Keyboard>(); pointer=InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>();
            playerInput.SwitchCurrentControlScheme("KeyboardMouse",keyboard,pointer); await Wait(100);
            foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None)) ai.enabled=false;
            p.maxHealth=p.currentHealth=10000;
            var controls=p.GetComponent<PlayerControlState>(); var motor=p.GetComponent<StarterAssets.ThirdPersonController>();
            var combat=p.GetComponent<PlayerCombat>(); var presentation=p.GetComponent<CharacterAnimation>(); var animator=p.GetComponentInChildren<Animator>(); var equipment=p.GetComponent<PlayerEquipment>();
            // Isolate locomotion from Combat_Test's spawn geometry and nearby enemies.
            var platform=GameObject.CreatePrimitive(PrimitiveType.Cube); cleanup.Add(platform); platform.name="Temporary locomotion platform";
            platform.transform.position=new Vector3(200,49.9f,200); platform.transform.localScale=new Vector3(30,.2f,30);
            motor.GroundLayers|=1; Teleport(p,new Vector3(200,50.15f,200));
            controls.ClosePanels(); controls.SendMessage("OnApplicationFocus",true); controls.Synchronize();
            InputSystem.EnableDevice(keyboard); InputSystem.EnableDevice(pointer); Keys(); Mouse(new MouseState());
            Check(controls.GameplayAllowed,"Gameplay input initially allowed");
            await Wait(250); var start=p.transform.position; Keys(Key.W); await Wait(500); float walked=Vector3.Distance(start,p.transform.position); Keys(); await Wait(150);
            Check(walked>.1f,"Native W moves the real CharacterController",walked);
            Keys(Key.W,Key.LeftShift); await Wait(350); float sprint=p.GetComponent<CharacterController>().velocity.magnitude; Keys(); await Wait(150);
            Check(sprint>motor.MoveSpeed,"Sprint exceeds walk speed",sprint);
            float yaw=motor.ViewYaw; InputSystem.QueueStateEvent(pointer,new MouseState{delta=new Vector2(24,0)}); await Wait(100); Mouse(new MouseState());
            Check(Mathf.Abs(Mathf.DeltaAngle(yaw,motor.ViewYaw))>.01f,"Mouse delta rotates view");
            bool mode=motor.FirstPerson; await Press(Key.V); Check(motor.FirstPerson!=mode,"V changes camera mode"); await Press(Key.V); Check(motor.FirstPerson==mode,"V restores camera mode");
            float y=p.transform.position.y; await Press(Key.Space); Check(p.transform.position.y<=y+.05f,"Space cannot jump");
            await Press(Key.Tab); Check(!controls.GameplayAllowed&&controls.InventoryUI.inv.activeSelf&&Cursor.lockState==CursorLockMode.None,"Inventory owns input and cursor");
            start=p.transform.position; Keys(Key.W); Mouse(new MouseState().WithButton(MouseButton.Left)); await Wait(250); Keys(); Mouse(new MouseState());
            Check(Vector3.Distance(start,p.transform.position)<.06f&&!combat.IsExecutingAttack,"Inventory blocks movement and attack");
            await Press(Key.Tab); Check(controls.GameplayAllowed,"Inventory close restores input");
            await Press(Key.Escape); Check(Time.timeScale==0&&!controls.GameplayAllowed,"Pause freezes gameplay"); await Press(Key.Escape); Check(Time.timeScale==1&&controls.GameplayAllowed,"Resume restores gameplay");
            await Press(Key.J); Check(!controls.GameplayAllowed,"Journal gates input"); await Press(Key.J); Check(controls.GameplayAllowed,"Journal releases input");
            await Press(Key.M); Check(!controls.GameplayAllowed,"Expanded map gates input"); await Press(Key.M); Check(controls.GameplayAllowed,"Expanded map releases input");
            var attacks=new List<bool>(); combat.OnAttack+=attacks.Add;
            combat.CancelAttack(); Mouse(new MouseState().WithButton(MouseButton.Left)); await Wait(350);
            Check(attacks.Count==1&&!attacks[0],"One mouse press starts one light attack");
            start=p.transform.position; Keys(Key.W); await Wait(200); Keys(); Check(Vector3.Distance(start,p.transform.position)>.05f,"Player can move during a light attack");
            await Wait((int)(combat.AttackDuration*1000)+500);
            Check(attacks.Count==1,"Holding attack never repeats or charges"); Mouse(new MouseState()); await Wait(150); Check(attacks.Count==1,"Attack release never attacks");
            combat.CancelAttack(); combat.Attack(true); Check(attacks.Count==2&&!attacks.Last(),"Legacy heavy request executes a light attack"); combat.CancelAttack();
            var shield=AssetDatabase.FindAssets("t:Equipment").Select(g=>AssetDatabase.LoadAssetAtPath<Equipment>(AssetDatabase.GUIDToAssetPath(g))).First(x=>x.itemType==ItemType.Shield);
            equipment.Equip(shield); presentation.SetBusy(false); Mouse(new MouseState().WithButton(MouseButton.Right)); await Wait(200); Check(combat.isBlocking,"Native right mouse raises equipped shield",new{controls.BlockHeld,combat.HasShield,combat.canBlock,busy=presentation.IsBusy,controls.GameplayAllowed});
            controls.InventoryUI.ToogleInvetoryUI(); await Wait(100); Check(!combat.isBlocking&&!controls.BlockHeld,"Modal opening clears shield intent"); controls.InventoryUI.ToogleInvetoryUI(); await Wait(100); Check(!combat.isBlocking,"Held shield cannot leak across a modal boundary"); Mouse(new MouseState()); await Wait(100);
            Mouse(new MouseState().WithButton(MouseButton.Right)); await Wait(150); Check(combat.isBlocking,"Shield works again after release and fresh press"); Mouse(new MouseState()); await Wait(100); Check(!combat.isBlocking,"Shield release lowers block");
            controls.SendMessage("OnApplicationFocus",false); Check(!controls.GameplayAllowed&&controls.Move==Vector2.zero,"Focus loss clears intentions"); controls.SendMessage("OnApplicationFocus",true); Check(controls.GameplayAllowed,"Focus regain restores eligibility");
            int health=p.currentHealth; p.TakeDamage(10); Check(p.currentHealth<health,"Player damage uses shared health resolution"); int damaged=p.currentHealth; p.Heal(5); Check(p.currentHealth>damaged,"Player healing remains functional");
            var holder=p.transform.root; var tree=holder.GetComponentInChildren<UI_SkillTree>(true); await Press(Key.Tab); tree.gameObject.SetActive(true); await Wait(100); Check(typeof(UI_SkillTree).GetField("playerSkills",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(tree)==p.GetPlayerSkills(),"Skill tree binds directly to PlayerStats when its panel opens"); await Press(Key.Tab);
            var gathering=p.GetComponent<PlayerGathering>();
            var def=AssetDatabase.FindAssets("t:ResourceNodeDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>(AssetDatabase.GUIDToAssetPath(g))).First();
            var nodeObject=new GameObject("Cleanup gathering fixture"); cleanup.Add(nodeObject); nodeObject.transform.position=p.transform.position+Vector3.forward;
            var node=nodeObject.AddComponent<ResourceNode>(); var copy=UnityEngine.Object.Instantiate(def); cleanup.Add(copy); copy.collectionDuration=.2f; copy.requiredItem=null; copy.requirements=Array.Empty<ResourceNodeRequirement>();
            node.definition=copy; node.RestoreState(new ResourceNode.Snapshot{charges=1});
            var bag=p.GetComponent<Inventory>(); for(int i=0;i<bag.items.Count;i++) bag.items[i]=null; bag.NotifyExchange();
            combat.CancelAttack(); presentation.SetBusy(false);
            bool began=gathering.TryBegin(node); Check(began&&!controls.GameplayAllowed,"Gathering acquires exclusive action ownership");
            await Wait(500); Check(!gathering.IsGathering&&controls.GameplayAllowed&&bag.Count(copy.resourceItem)>=copy.quantity,"Gathering completes through existing inventory reward path");
            motor.FirstPerson=true; await Wait(200); var camera=Camera.main; var ray=camera.ViewportPointToRay(new Vector3(.5f,.5f));
            node.transform.position=ray.GetPoint(1.8f); nodeObject.AddComponent<BoxCollider>().size=new Vector3(.6f,.6f,.6f); copy.collectionDuration=2f; node.RestoreState(new ResourceNode.Snapshot{charges=1}); Physics.SyncTransforms();
            await Press(Key.E); Check(gathering.IsGathering,"Native E uses the shared camera-center interaction ray");
            await Press(Key.Escape); Check(!gathering.IsGathering&&!controls.ActionLocked&&controls.GameplayAllowed,"Escape cancels gathering and releases only its action lease");
            combat.OnAttack-=attacks.Add;
        }
        catch(Exception e) { errors.Add(e.ToString()); }
        finally
        {
            Keys(); Mouse(new MouseState()); InputSystem.settings=original; UnityEngine.Object.Destroy(settings);
            if(playerInput!=null&&oldDevices!=null&&oldDevices.Length>0)playerInput.SwitchCurrentControlScheme(oldScheme,oldDevices);
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard); if(pointer!=null)InputSystem.RemoveDevice(pointer); keyboard=null; pointer=null;
            foreach(var item in cleanup) if(item!=null) UnityEngine.Object.Destroy(item);
            Application.logMessageReceived-=logger;
        }
        var result=new { passed=checks.All(c=>(bool)c.GetType().GetProperty("pass").GetValue(c))&&errors.Count==0,checks,errors,frameCount=Time.frameCount };
        Directory.CreateDirectory("Assets/Plan/Validation"); File.WriteAllText("Assets/Plan/Validation/equipment-animation-regression-runtime.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        return result;
    }
}
