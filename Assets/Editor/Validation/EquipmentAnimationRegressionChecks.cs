namespace EquipmentAnimationRegression {
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
    private static async Task Wait(int ms) {
        if (Time.timeScale <= 0f) { await Task.Delay(ms); return; }
        float end = Time.time + ms / 1000f;
        float deadline = Time.realtimeSinceStartup + Mathf.Max(15f, ms / 1000f * 5f);
        while (Time.time < end) {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Gameplay clock did not advance during validation.");
            await Task.Delay(20);
        }
    }
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
            var sprintStart=p.transform.position; float sprintTime=Time.time; Keys(Key.W,Key.LeftShift); await Wait(1000); float sprint=Vector3.Distance(sprintStart,p.transform.position)/Mathf.Max(.001f,Time.time-sprintTime);
            float requestedSprint=((Vector3)typeof(StarterAssets.ThirdPersonController).GetField("_horizontalVelocity",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(motor)).magnitude; bool sprintHeld=controls.Sprint; Keys(); await Wait(150);
            Check(sprintHeld&&requestedSprint>motor.MoveSpeed*p.MovementMultiplier,"Native sprint raises the motor movement speed",new{sprintHeld,requestedSprint,sampledTravelSpeed=sprint,walk= motor.MoveSpeed*p.MovementMultiplier,multiplier=p.MovementMultiplier});
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

}
namespace EquipmentAnimationRegression {
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json;
public static class VerifyNpcIntegration
{
    private static readonly List<object> checks=new List<object>();
    private static void Check(bool pass,string name,object details=null)=>checks.Add(new{pass,name,details});
    private static async Task Wait(int ms) {
        if (Time.timeScale <= 0f) { await Task.Delay(ms); return; }
        float end = Time.time + ms / 1000f;
        float deadline = Time.realtimeSinceStartup + Mathf.Max(15f, ms / 1000f * 5f);
        while (Time.time < end) {
            if (Time.realtimeSinceStartup > deadline) throw new Exception("Gameplay clock did not advance during validation.");
            await Task.Delay(20);
        }
    }
    public static async Task<object> Run()
    {
        if(!Application.isPlaying) throw new Exception("Play Mode required.");
        checks.Clear(); PlayerInput pairedInput=null; InputDevice[] oldDevices=null; string oldScheme=null; Keyboard keyboard=null; UnityEngine.InputSystem.Mouse pointer=null; var cleanup=new List<UnityEngine.Object>(); var errors=new List<string>(); NavMeshDataInstance navInstance=default;
        Application.LogCallback log=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception)errors.Add(m);}; Application.logMessageReceived+=log;
        var original=InputSystem.settings; var settings=UnityEngine.Object.Instantiate(original); settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus; settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; InputSystem.settings=settings;
        try
        {
            var p=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).First(x=>x.GetComponent<StarterAssets.ThirdPersonController>()!=null);pairedInput=p.GetComponent<PlayerInput>();oldDevices=pairedInput.devices.ToArray();oldScheme=pairedInput.currentControlScheme;
            keyboard=InputSystem.AddDevice<Keyboard>();pointer=InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>();pairedInput.SwitchCurrentControlScheme("KeyboardMouse",keyboard,pointer);
            InputSystem.EnableDevice(keyboard); InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            var controls=p.GetComponent<PlayerControlState>(); controls.ClosePanels(); controls.SendMessage("OnApplicationFocus",true); p.maxHealth=p.currentHealth=10000;
            var motor=p.GetComponent<StarterAssets.ThirdPersonController>(); motor.GroundLayers|=1;
            var combat=p.GetComponent<PlayerCombat>(); var presentation=p.GetComponent<CharacterAnimation>(); var equipment=p.GetComponent<PlayerEquipment>(); var bag=p.GetComponent<Inventory>();
            var platform=GameObject.CreatePrimitive(PrimitiveType.Cube); cleanup.Add(platform); platform.transform.position=new Vector3(250,49.9f,250); platform.transform.localScale=new Vector3(30,.2f,30);
            var data=NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0),new List<NavMeshBuildSource>{new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Box,transform=Matrix4x4.TRS(platform.transform.position,Quaternion.identity,Vector3.one),size=platform.transform.localScale,area=0}},new Bounds(new Vector3(250,50,250),new Vector3(32,10,32)),Vector3.zero,Quaternion.identity);
            cleanup.Add(data); navInstance=NavMesh.AddNavMeshData(data);
            var cc=p.GetComponent<CharacterController>(); cc.enabled=false; p.transform.position=new Vector3(250,50.1f,250); p.transform.rotation=Quaternion.identity; cc.enabled=true;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Main.prefab");
            NavMesh.SamplePosition(p.transform.position+Vector3.forward*5,out var spawn,2,NavMesh.AllAreas);
            var enemy=UnityEngine.Object.Instantiate(prefab,spawn.position,Quaternion.LookRotation(Vector3.back)); cleanup.Add(enemy);
            var ai=enemy.GetComponent<EnemyAi>(); var ec=enemy.GetComponent<EnemyCombat>(); var es=enemy.GetComponent<EnemyStats>(); var agent=enemy.GetComponent<NavMeshAgent>();
            await Wait(150); ec.canAttack=false; ai.target=p.transform; Vector3 start=enemy.transform.position; await Wait(1300);
            Check(agent.isOnNavMesh&&Vector3.Distance(start,enemy.transform.position)>.1f,"Enemy AI follows player on a native NavMesh",Vector3.Distance(start,enemy.transform.position));
            int enemyAttacks=0; ec.OnAttack+=h=>enemyAttacks++; ec.canAttack=true; agent.Warp(p.transform.position+Vector3.forward*.8f); enemy.transform.LookAt(p.transform.position); ai.target=p.transform; ec.CancelAttack(); ec.Attack(false); await Wait(150);
            Check(enemyAttacks>0&&ec.IsExecutingAttack,"Enemy accepts an attack through its existing AI target gates");
            var ea=enemy.GetComponentInChildren<Animator>(); Check(ec.CurrentAttackMotion!=null && ea.GetCurrentAnimatorClipInfo(1).Any(c=>c.clip==ec.CurrentAttackMotion.clip),"NPC animation matches its equipped attack set");
            ai.enabled=false; ec.CancelAttack(); Check(!ec.IsExecutingAttack&&!enemy.GetComponent<CharacterAnimation>().IsBusy,"Enemy cancellation clears animation and attack");
            agent.enabled=false; es.maxHealth=es.currentHealth=5000; es.armor=0;
            var allEquipment=AssetDatabase.FindAssets("t:Equipment").Select(g=>AssetDatabase.LoadAssetAtPath<Equipment>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            var sword=allEquipment.First(x=>x.itemType==ItemType.Sword&&x.equipSlot==EquipmentSlot.MainHand);
            equipment.Equip(sword); combat.CancelAttack(); motor.FacingTarget=enemy.transform; motor.FirstPerson=false;
            enemy.transform.position=p.transform.position+Vector3.forward*.8f; enemy.transform.LookAt(p.transform.position); Physics.SyncTransforms();
            int old=es.currentHealth; combat.Attack(false); await Wait((int)(combat.AttackDuration*1000)+400);
            Check(es.currentHealth<old,"Player sword hit removes enemy health through the shared damage path",old-es.currentHealth);
            Check(!combat.IsExecutingAttack&&!presentation.IsBusy,"Player attack recovers normally");
            var bow=allEquipment.First(x=>x.itemType==ItemType.Bow&&x.equipSlot==EquipmentSlot.MainHand);
            equipment.Equip(bow); motor.FirstPerson=true;
            typeof(StarterAssets.ThirdPersonController).GetField("_yaw",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(motor,0f);
            typeof(StarterAssets.ThirdPersonController).GetField("_pitch",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(motor,0f);
            enemy.transform.position=p.transform.position+Vector3.forward*5; Physics.SyncTransforms(); await Wait(250); combat.CancelAttack(); old=es.currentHealth; combat.Attack(false); await Wait((int)(combat.AttackDuration*1000)+500);
            Check(equipment.ActiveAttackSet!=null&&equipment.ActiveAttackSet.launchesProjectile&&es.currentHealth<old,"Bow releases an initialized projectile that damages the NPC",old-es.currentHealth);
            equipment.Equip(sword); combat.CancelAttack();
            int health=es.currentHealth; es.Heal(10); Check(es.currentHealth>health,"Shared healing also works for humanoid NPC stats");
            var slow=AssetDatabase.FindAssets("t:StatusEffectDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<StatusEffectDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(x=>x.effectId=="slow");
            es.StatusEffects.TryApplyEffect(slow,p.transform); Check(es.MovementMultiplier<1,"NPC retains shared status effects"); es.StatusEffects.ClearAllEffects(); Check(es.MovementMultiplier==1,"NPC status removal restores derived movement");
            var npc=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NPC/NPC_Main.prefab"),p.transform.position+Vector3.right*3,Quaternion.identity); cleanup.Add(npc); await Wait(100);
            Check(npc.GetComponent<CharacterAnimation>()!=null&&!npc.GetComponent<EnemyAi>().enabled&&!npc.GetComponent<EnemyCombat>().enabled,"Friendly humanoid keeps animation with hostile AI/combat disabled");
            var potion=AssetDatabase.FindAssets("t:Consumable").Select(g=>AssetDatabase.LoadAssetAtPath<Consumable>(AssetDatabase.GUIDToAssetPath(g))).First(x=>x.statusEffect==null&&x.StatAmount>0&&x.statModifier==StatModifier.Health);
            var quick=p.transform.root.GetComponentInChildren<HotKeysUI>(true); var slot=(new SerializedObject(quick)).FindProperty("quickSlots").GetArrayElementAtIndex(0).objectReferenceValue as InventorySlot; int index=slot.slotIndex;
            bag.items[index]=potion; bag.NotifyExchange(); p.currentHealth=100; combat.CancelAttack(); presentation.SetBusy(false);
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Digit1)); InputSystem.Update(); await Wait(100); InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); InputSystem.Update(); await Wait(100);
            Check(p.currentHealth>100&&bag.items[index]==null,"Quick slot 1 uses its item through centralized input");
            combat.CancelAttack(); presentation.SetBusy(false);
            var enemyGear=enemy.GetComponentInChildren<CharacterEquipment>(); int gearCount=enemyGear.Items.Count;
            for(int i=0;i<bag.items.Count;i++)bag.items[i]=null; bag.NotifyExchange();
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.U)); InputSystem.Update(); await Wait(100); InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); InputSystem.Update(); await Wait(100);
            Check(equipment.Items.Count==0&&bag.Count(sword)==1,"U returns player equipment to inventory"); Check(enemyGear.Items.Count==gearCount,"U never unequips NPC equipment");
            bool died=false; es.OnDeath+=killer=>died=killer==p.transform; es.TakeDamage(es.currentHealth+es.armor+1000,p.transform,0); Check(es.IsDead&&died,"Enemy death preserves attributed death notifications");
        }
        catch(Exception e){errors.Add(e.ToString());}
        finally
        {
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); InputSystem.Update(); InputSystem.settings=original; if(pairedInput!=null&&oldDevices!=null&&oldDevices.All(d=>d!=null&&d.added))pairedInput.SwitchCurrentControlScheme(oldScheme,oldDevices);
            if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(pointer!=null&&pointer.added)InputSystem.RemoveDevice(pointer); UnityEngine.Object.Destroy(settings);
            foreach(var item in cleanup)if(item!=null)UnityEngine.Object.Destroy(item); if(navInstance.valid)navInstance.Remove(); Application.logMessageReceived-=log;
        }
        var result=new{passed=checks.All(c=>(bool)c.GetType().GetProperty("pass").GetValue(c))&&errors.Count==0,checks,errors}; Directory.CreateDirectory("Assets/Plan/Validation");File.WriteAllText("Assets/Plan/Validation/equipment-animation-regression-integration.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
}

}
namespace EquipmentAnimationRegression {
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json;
using DungeonGame.Dialogue;
public static class VerifyCharacterModals
{
    public static async Task<object> Run()
    {
        if(!Application.isPlaying)throw new Exception("Play Mode required.");
        PlayerInput pairedInput=null; InputDevice[] oldDevices=null; string oldScheme=null; Keyboard keyboard=null; UnityEngine.InputSystem.Mouse pointer=null; var checks=new List<object>(); Action<bool,string> check=(pass,name)=>checks.Add(new{pass,name}); var errors=new List<string>(); var cleanup=new List<GameObject>();
        var original=InputSystem.settings; var settings=UnityEngine.Object.Instantiate(original); settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus; settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; InputSystem.settings=settings;
        Func<Key,Task> press=async key=>{InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));InputSystem.Update();await Task.Delay(80);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();await Task.Delay(100);};
        try
        {
            var p=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).First(x=>x.GetComponent<StarterAssets.ThirdPersonController>()!=null);pairedInput=p.GetComponent<PlayerInput>();oldDevices=pairedInput.devices.ToArray();oldScheme=pairedInput.currentControlScheme;
            keyboard=InputSystem.AddDevice<Keyboard>();pointer=InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>();pairedInput.SwitchCurrentControlScheme("KeyboardMouse",keyboard,pointer);
            InputSystem.EnableDevice(keyboard); InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update(); p.maxHealth=p.currentHealth=10000;
            foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None))ai.enabled=false;
            var state=p.GetComponent<PlayerControlState>(); state.ClosePanels(); state.SendMessage("OnApplicationFocus",true); var dialogue=p.GetComponent<DialogueController>(); var combat=p.GetComponent<PlayerCombat>(); combat.CancelAttack();
            var merchant=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NPC/DialogueMerchant.prefab"));cleanup.Add(merchant);var npc=merchant.GetComponent<DialogueNpc>();
            combat.Attack(false);check(combat.IsExecutingAttack,"An attack is active before dialogue opens");
            bool opened=dialogue.Open(npc);check(opened&&dialogue.IsOpen&&!state.GameplayAllowed&&!combat.IsExecutingAttack,"Dialogue owns input and cancels an in-flight attack");
            await press(Key.E); await press(Key.Tab);check(dialogue.IsOpen&&!state.InventoryUI.inv.activeSelf,"E and Tab are ignored while talking");
            await press(Key.Escape);check(!dialogue.IsOpen&&state.GameplayAllowed&&Time.timeScale==1,"Escape closes dialogue without pausing the world");
            var chestObject=new GameObject("Temporary chest fixture");cleanup.Add(chestObject);var inventory=chestObject.AddComponent<ChestInventory>();var chest=chestObject.AddComponent<ChestScript>();chest.inventory=inventory;
            var item=AssetDatabase.FindAssets("t:Consumable").Select(g=>AssetDatabase.LoadAssetAtPath<Item>(AssetDatabase.GUIDToAssetPath(g))).First();inventory.Add(item);
            chest.Interact(p.transform);state.Synchronize();check(state.InventoryUI.chestInventoryUI.currentChest==chest&&!state.GameplayAllowed,"Chest interaction resolves canvas through PlayerControlState");
            var chestUI=state.InventoryUI.chestInventoryUI;var slot=chestUI.slots.First(s=>s.item==item);int before=p.GetComponent<Inventory>().Count(item);slot.ItemSelect();check(p.GetComponent<Inventory>().Count(item)==before+1&&!inventory.items.Contains(item),"Existing chest-to-player item transfer still works");
            await press(Key.E);check(!chestUI.gameObject.activeInHierarchy&&state.GameplayAllowed,"E closes chest and restores gameplay");
            bool shop=merchant.GetComponent<ShopScript>().TryOpenShop(p.transform);check(shop&&state.ShopUI.gameObject.activeInHierarchy&&!state.GameplayAllowed,"Existing shop opens using the same modal owner");
            await press(Key.Escape);check(!state.ShopUI.gameObject.activeInHierarchy&&state.GameplayAllowed&&Time.timeScale==1,"Escape closes shop without opening pause");
        }
        catch(Exception e){errors.Add(e.ToString());}
        finally{InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());InputSystem.Update();InputSystem.settings=original;if(pairedInput!=null&&oldDevices!=null&&oldDevices.All(d=>d!=null&&d.added))pairedInput.SwitchCurrentControlScheme(oldScheme,oldDevices);
            if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);if(pointer!=null&&pointer.added)InputSystem.RemoveDevice(pointer);UnityEngine.Object.Destroy(settings);foreach(var go in cleanup)UnityEngine.Object.Destroy(go);}
        var result=new{passed=checks.All(c=>(bool)c.GetType().GetProperty("pass").GetValue(c))&&errors.Count==0,checks,errors};File.WriteAllText("Assets/Plan/Validation/equipment-animation-regression-modals.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
}

}
namespace EquipmentAnimationRegression {
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
public static class VerifyPlayerDeath
{
    public static async Task<object> Run()
    {
        if(!Application.isPlaying)throw new Exception("Requires task-owned Play Mode.");
        var player=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).First(p=>p.GetComponent<StarterAssets.ThirdPersonController>()!=null);
        int instance=player.GetInstanceID(); string scene=player.gameObject.scene.name;
        var controls=player.GetComponent<PlayerControlState>(); var motor=player.GetComponent<StarterAssets.ThirdPersonController>();
        player.TakeDamage(player.currentHealth+Mathf.Max(0,player.armor)+1000);
        controls.Synchronize();
        bool death=player.IsDead&&!controls.GameplayAllowed&&!motor.canMove;
        await Task.Delay(1800);
        var replacement=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).FirstOrDefault(p=>p.GetComponent<StarterAssets.ThirdPersonController>()!=null);
        bool restart=replacement!=null&&replacement.GetInstanceID()!=instance&&!replacement.IsDead&&replacement.currentHealth>0&&replacement.gameObject.scene.name==scene;
        var result=new{passed=death&&restart,deathBlocksInputAndMovement=death,reloadsCurrentSceneWithLivingPlayer=restart,scene};
        Directory.CreateDirectory("Assets/Plan/Validation");File.WriteAllText("Assets/Plan/Validation/equipment-animation-regression-death.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
}

}
