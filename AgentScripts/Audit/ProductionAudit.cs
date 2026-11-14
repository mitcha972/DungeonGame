using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;
using Newtonsoft.Json;

public static class ProductionAudit
{
    static readonly List<string> checks=new List<string>();
    static void Check(bool ok,string label){checks.Add((ok?"PASS ":"FAIL ")+label); Debug.Log("[ProductionAudit] "+checks.Last());}
    static async Task Until(Func<bool> predicate){double end=EditorApplication.timeSinceStartup+150;while(!predicate()){if(EditorApplication.timeSinceStartup>end)throw new Exception("Scene/initialization timeout");await Task.Delay(30);}}
    static async Task Press(Key key){InputSystem.EnableDevice(Keyboard.current);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(key));await Task.Delay(130);InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());await Task.Delay(130);}
    static void FreezeEnemies(){foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None))ai.enabled=false;foreach(var c in UnityEngine.Object.FindObjectsByType<EnemyCombat>(FindObjectsSortMode.None))c.enabled=false;}
    public static async Task<object> Run()
    {
        if(!Application.isPlaying)throw new Exception("Owned Play Mode required");
        checks.Clear();var errors=new List<string>();string error=null;
        Application.LogCallback logger=(message,stack,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);};
        var settings=InputSystem.settings; var bg=settings.backgroundBehavior;var input=settings.editorInputBehaviorInPlayMode;
        settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.logMessageReceived+=logger;
        try
        {
            var menu=UnityEngine.Object.FindFirstObjectByType<MenuController>(); if(menu==null)throw new Exception("Start from menu");
            var selected=AssetDatabase.FindAssets("t:Class").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Class>).First(c=>c!=null&&c!=menu.pClass);
            menu.NewClass(selected);menu.name="Audit Adventurer";GameController.floor=7;GameController.difficulty=3;menu.Play();
            await Until(()=>SceneManager.GetActiveScene().name=="Main"&&UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>()?.IsReady==true);
            FreezeEnemies(); await Task.Delay(200);
            var stats=UnityEngine.Object.FindFirstObjectByType<PlayerStats>();var controls=stats.GetComponent<PlayerControlState>();controls.SendMessage("OnApplicationFocus",true);
            Check(stats.name=="Audit Adventurer"&&stats.playerClass==selected,"Start Loading Main preserve chosen name and class");
            Check(stats.maxHealth==selected.health&&stats.currentHealth==selected.health,"selected class initializes full health");
            Check(selected.equipment.Where(e=>e!=null).All(e=>stats.equipmentManager.currentEquipment.TryGetValue(e.equipSlot,out var worn)&&worn==e),"selected class equips its defaults");
            Check(GameController.floor==7&&GameController.difficulty==3,"dungeon bootstrap preserves session floor and difficulty");
            int frame=Time.frameCount;await Task.Delay(200);Check(Time.frameCount>frame,"production gameplay frames advance");
            await Press(Key.Tab);Check(!controls.GameplayAllowed&&Time.timeScale==1,"production inventory owns input without pausing");await Press(Key.Tab);
            var map=stats.transform.root.GetComponentInChildren<MapController>(true);await Press(Key.M);Check(map.IsExpanded&&!controls.GameplayAllowed&&Time.timeScale==1,"production expanded map blocks gameplay");await Press(Key.Escape);Check(!map.IsExpanded&&controls.GameplayAllowed&&Time.timeScale==1,"map Escape closes without pausing");
            await Press(Key.J);Check(!controls.GameplayAllowed&&Time.timeScale==1,"production journal blocks gameplay");await Press(Key.Escape);Check(controls.GameplayAllowed,"journal close restores gameplay");
            await Press(Key.Escape);Check(Time.timeScale==0&&!controls.GameplayAllowed,"production Escape pauses");await Press(Key.Escape);Check(Time.timeScale==1&&controls.GameplayAllowed,"production Escape resumes");
            controls.SendMessage("OnApplicationFocus",false);Check(!controls.GameplayAllowed,"focus loss blocks production gameplay");controls.SendMessage("OnApplicationFocus",true);Check(controls.GameplayAllowed,"focus regain restores production gameplay");
            stats.TakeDamage(stats.currentHealth+Mathf.Max(0,stats.armor)+100,null,0);
            controls.Synchronize();
            Check(stats.IsDead&&!controls.GameplayAllowed,"lethal damage blocks gameplay");
            await Until(()=>UnityEngine.Object.FindFirstObjectByType<PlayerStats>()!=null&&UnityEngine.Object.FindFirstObjectByType<PlayerStats>()!=stats&&UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>()?.IsReady==true);
            FreezeEnemies();await Task.Delay(200);var replacement=UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
            Check(!replacement.IsDead&&replacement.currentHealth==selected.health&&replacement.name=="Audit Adventurer"&&replacement.playerClass==selected,"death reload recreates selected player alive");
            Check(GameController.floor==7&&GameController.difficulty==3&&Time.timeScale==1,"death reload preserves floor difficulty and time");
            var pause=UnityEngine.Object.FindFirstObjectByType<SettingsMenu>();pause.Pause();pause.Exit();await Until(()=>SceneManager.GetActiveScene().name=="Start");await Task.Delay(300);
            var gc=UnityEngine.Object.FindFirstObjectByType<GameController>();Check(GameController.floor==gc.m_floor&&GameController.difficulty==gc.m_difficulty&&Time.timeScale==1,"return to menu resets session defaults from pause");
            Check(UnityEngine.Object.FindFirstObjectByType<MenuController>()!=null,"return to menu restores class selection UI");
            Check(errors.Count==0,"production transitions emit no errors");
        }
        catch(Exception e){error=e.ToString();Check(false,error);}
        finally{Application.logMessageReceived-=logger;settings.backgroundBehavior=bg;settings.editorInputBehaviorInPlayMode=input;InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState());}
        var report=new{passed=error==null&&checks.All(c=>c.StartsWith("PASS")),checks,errors,error};System.IO.File.WriteAllText("Assets/Plan/Validation/audit-production.json",JsonConvert.SerializeObject(report,Formatting.Indented));return report;
    }
}
