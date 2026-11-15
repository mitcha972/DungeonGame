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
    private static async Task Wait(int ms)=>await Task.Delay(ms);
    public static async Task<object> Run()
    {
        if(!Application.isPlaying) throw new Exception("Play Mode required.");
        checks.Clear(); var cleanup=new List<UnityEngine.Object>(); var errors=new List<string>(); NavMeshDataInstance navInstance=default;
        Application.LogCallback log=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception)errors.Add(m);}; Application.logMessageReceived+=log;
        var original=InputSystem.settings; var settings=UnityEngine.Object.Instantiate(original); settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus; settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; InputSystem.settings=settings;
        try
        {
            var p=UnityEngine.Object.FindObjectsByType<PlayerStats>(FindObjectsSortMode.None).First(x=>x.GetComponent<StarterAssets.ThirdPersonController>()!=null);
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
            var ea=enemy.GetComponentInChildren<Animator>(); Check(ea.GetCurrentAnimatorClipInfo(1).Any(c=>c.clip!=null&&c.clip.name.Contains("Punch")),"Shared CharacterAnimation selects the enemy punch clip");
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
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); InputSystem.Update(); InputSystem.settings=original; UnityEngine.Object.Destroy(settings);
            foreach(var item in cleanup)if(item!=null)UnityEngine.Object.Destroy(item); if(navInstance.valid)navInstance.Remove(); Application.logMessageReceived-=log;
        }
        var result=new{passed=checks.All(c=>(bool)c.GetType().GetProperty("pass").GetValue(c))&&errors.Count==0,checks,errors}; Directory.CreateDirectory("Assets/Plan/Validation");File.WriteAllText("Assets/Plan/Validation/equipment-animation-regression-integration.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
}
