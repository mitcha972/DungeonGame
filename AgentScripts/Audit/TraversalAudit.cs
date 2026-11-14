using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Newtonsoft.Json;

public static class TraversalAudit
{
    public static async Task<object> Run()
    {
        var g=UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>();
        if(!Application.isPlaying || g==null) throw new Exception("Main Play Mode required");
        var cc=g.player.GetComponent<CharacterController>();var movement=g.player.GetComponent<StarterAssets.ThirdPersonController>();
        var random=UnityEngine.Random.state;bool enabled=movement.enabled;var rows=new List<object>();bool passed=true;
        try
        {
            foreach(int seed in new[]{0,1,4,6})
            {
                UnityEngine.Random.InitState(seed);g.ReStart();double deadline=EditorApplication.timeSinceStartup+60;
                while(g.IsGenerating){if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Generation timeout");await Task.Delay(30);}
                if(!g.IsReady||g.LastFailure!=null)throw new Exception("Generation failed "+g.LastFailure);
                foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None))ai.enabled=false;
                foreach(var enemy in UnityEngine.Object.FindObjectsByType<EnemyStats>(FindObjectsSortMode.None))
                {var combat=enemy.GetComponent<EnemyCombat>();if(combat!=null)combat.enabled=false;foreach(var c in enemy.GetComponentsInChildren<Collider>())c.enabled=false;}
                movement.enabled=false;cc.enabled=false;g.player.transform.position=g.PlayerSpawnPosition+Vector3.up*.05f;cc.enabled=true;Physics.SyncTransforms();
                var exit=g.GetComponentInChildren<DungeonExit>();var surface=g.GetComponent<Unity.AI.Navigation.NavMeshSurface>();
                var filter=new NavMeshQueryFilter{agentTypeID=surface.agentTypeID,areaMask=NavMesh.AllAreas};
                NavMesh.SamplePosition(exit.transform.position,out var end,4,filter);var path=new NavMeshPath();NavMesh.CalculatePath(g.PlayerSpawnPosition,end.position,filter,path);
                int moves=0,jumps=0;bool stuck=false;string obstacle=null,firstObstruction=null;
                foreach(var corner in path.corners)
                {
                    int noProgress=0;
                    while(Vector2.Distance(new Vector2(g.player.transform.position.x,g.player.transform.position.z),new Vector2(corner.x,corner.z))>.12f&&moves<30000)
                    {
                        var before=g.player.transform.position;cc.Move(Vector3.ClampMagnitude(corner+Vector3.up*.05f-before,.2f));moves++;
                        if(Vector3.Distance(before,g.player.transform.position)<.002f)noProgress++;else noProgress=0;
                        if(noProgress>4)
                        {
                            if(firstObstruction==null)firstObstruction=string.Join(",",Physics.OverlapSphere(g.player.transform.position+Vector3.up,.8f).Select(c=>c.name));
                            // Test bounded clearance using the existing jump capability, without claiming a timed input jump.
                            var beforeJump=g.player.transform.position;
                            cc.Move(Vector3.up*Mathf.Min(.6f,movement.JumpHeight));jumps++;noProgress=0;
                            if(jumps>24 || g.player.transform.position.y-beforeJump.y<.05f){stuck=true;break;}
                        }
                    }
                    if(stuck)break;
                }
                float distance=Vector3.Distance(g.player.transform.position,exit.transform.position);bool ok=!stuck&&distance<g.player.GetComponent<PlayerController>().InteractionDistance;
                if(!ok)obstacle=string.Join(",",Physics.OverlapSphere(g.player.transform.position+Vector3.up,.8f).Select(c=>c.name));
                passed&=ok;rows.Add(new{seed,passed=ok,walkPassed=ok&&jumps==0,jumps,moves,distance,stuck,position=g.player.transform.position.ToString(),obstacle,firstObstruction});
            }
        }
        finally{movement.enabled=enabled;UnityEngine.Random.state=random;cc.enabled=false;g.player.transform.position=g.PlayerSpawnPosition+Vector3.up*.05f;cc.enabled=true;}
        var report=new{passed,rows,scope="Controlled CharacterController.Move substeps through real static colliders, with bounded upward clearance probes up to the existing jump height when blocked; enemy colliders disabled. Horizontal waypoint arrival ignores NavMesh vertical approximation. walkPassed distinguishes routes requiring clearance probes. Not timed hardware jumps, gravity/locomotion feel or combat navigation."};
        System.IO.File.WriteAllText("Assets/Plan/Validation/audit-traversal.json",JsonConvert.SerializeObject(report,Formatting.Indented));return report;
    }
}
