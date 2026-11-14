using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Newtonsoft.Json;

public static class WorldAudit
{
    static readonly List<string> checks = new List<string>();
    static void Check(bool condition, string label) { checks.Add((condition ? "PASS " : "FAIL ") + label); Debug.Log("[WorldAudit] " + checks.Last()); }
    static async Task Ready(DungeonGenerator g)
    {
        double deadline = EditorApplication.timeSinceStartup + 35;
        while (g.IsGenerating) { if (EditorApplication.timeSinceStartup > deadline) throw new Exception("Generation timeout"); await Task.Delay(20); }
        await Task.Delay(150);
        foreach (var ai in UnityEngine.Object.FindObjectsByType<EnemyAi>(FindObjectsSortMode.None)) ai.enabled = false;
        foreach (var combat in UnityEngine.Object.FindObjectsByType<EnemyCombat>(FindObjectsSortMode.None)) combat.enabled = false;
    }
    public static object Inspect()
    {
        var g = UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>();
        var surface = g.GetComponent<Unity.AI.Navigation.NavMeshSurface>();
        var exit = g.GetComponentInChildren<DungeonExit>();
        var filter = new NavMeshQueryFilter { agentTypeID = surface.agentTypeID, areaMask = NavMesh.AllAreas };
        var path = new NavMeshPath();
        bool sampled = exit != null && NavMesh.SamplePosition(exit.transform.position, out _, 4, filter);
        NavMesh.SamplePosition(exit == null ? Vector3.zero : exit.transform.position, out var hit, 4, filter);
        bool calculated = sampled && NavMesh.CalculatePath(g.PlayerSpawnPosition, hit.position, filter, path);
        var rooms = g.GetComponentsInChildren<RoomBehaviour>().Select(r => {
            bool on = NavMesh.SamplePosition(r.transform.position, out var h, 8, filter);
            var p = new NavMeshPath(); bool ok = on && NavMesh.CalculatePath(g.PlayerSpawnPosition, h.position, filter, p);
            return new { r.name, position = r.transform.position.ToString(), sample = h.position.ToString(), on, calculated = ok, status = p.status.ToString() };
        }).ToArray();
        var report = new { g.IsReady, g.LastFailure, size = g.size.ToString(), offset = g.offset.ToString(), spawn = g.PlayerSpawnPosition.ToString(), exit = exit == null ? null : exit.transform.position.ToString(), sampled, calculated, status = path.status.ToString(), corners = path.corners.Select(c => c.ToString()).ToArray(), rooms,
            surface = new { surface.agentTypeID, surface.collectObjects, surface.useGeometry, mask = surface.layerMask.value, surface.overrideVoxelSize, surface.voxelSize },
            doors = g.GetComponentsInChildren<DoorScript>().Select(d => new { d.name, pos = d.transform.position.ToString(), d.isOpen, bounds = d.GetComponentsInChildren<Collider>().Select(c => new { c.name, c.enabled, c.isTrigger, center = c.bounds.center.ToString(), size = c.bounds.size.ToString() }).ToArray() }).ToArray() };
        System.IO.File.WriteAllText("Temp/AuditRestore/world-inspect.json", JsonConvert.SerializeObject(report, Formatting.Indented));
        return report;
    }
    public static async Task<object> Run(int cycles)
    {
        if (!Application.isPlaying) throw new Exception("Play Mode required");
        checks.Clear(); string error = null;
        var random = UnityEngine.Random.state;
        var g = UnityEngine.Object.FindFirstObjectByType<DungeonGenerator>();
        var surface = g.GetComponent<Unity.AI.Navigation.NavMeshSurface>();
        try
        {
            var stats = g.player.GetComponent<PlayerStats>(); stats.maxHealth = stats.currentHealth = 100000;
            int frame = Time.frameCount; await Task.Delay(200); Check(Time.frameCount > frame, "frames advance");
            await Ready(g);
            int nativeData = Resources.FindObjectsOfTypeAll<NavMeshData>().Count(d => d.name == g.name);
            var directions = new HashSet<int>();
            for (int seed = 0; seed < cycles; seed++)
            {
                UnityEngine.Random.InitState(seed);
                g.ReStart(); await Ready(g);
                Check(g.IsReady && g.LastFailure == null, "seed " + seed + " generated");
                var filter = new NavMeshQueryFilter { agentTypeID = surface.agentTypeID, areaMask = NavMesh.AllAreas };
                int reachable = 0, rooms = 0;
                foreach (var room in g.GetComponentsInChildren<RoomBehaviour>())
                {
                    rooms++; var p = new NavMeshPath();
                    if (NavMesh.SamplePosition(room.transform.position, out var hit, 8, filter) && NavMesh.CalculatePath(g.PlayerSpawnPosition, hit.position, filter, p) && p.status == NavMeshPathStatus.PathComplete) reachable++;
                }
                Check(reachable == rooms, "seed " + seed + " room centers reachable " + reachable + "/" + rooms);
                var exit = g.GetComponentInChildren<DungeonExit>(); var path = new NavMeshPath();
                bool sampled = exit != null && NavMesh.SamplePosition(exit.transform.position, out _, 4, filter);
                NavMesh.SamplePosition(exit == null ? Vector3.zero : exit.transform.position, out var end, 4, filter);
                bool complete = sampled && NavMesh.CalculatePath(g.PlayerSpawnPosition, end.position, filter, path) && path.status == NavMeshPathStatus.PathComplete;
                Check(complete, "seed " + seed + " exit interaction path " + path.status);
                var cell=g.Cells.First(c=>c.endCell); for(int d=0;d<4;d++) if(cell.status[d]) directions.Add(d);
            }
            Check(directions.Count==4,"all four end-room entrance directions exercised");
            Check(Resources.FindObjectsOfTypeAll<NavMeshData>().Count(d=>d.name==g.name)<=nativeData,"repeated floors do not accumulate native NavMeshData");
            var originalEnd=g.endRoom; var originalSize=g.size;
            var originalBoard=g.Cells; var originalData=surface.navMeshData;
            var originalPosition=g.player.transform.position; int originalFloor=GameController.floor;
            var cc=g.player.GetComponent<CharacterController>(); var movement=g.player.GetComponent<StarterAssets.ThirdPersonController>();
            g.size=Vector2Int.one; g.AdvanceFloor(); await Ready(g);
            Check(g.IsReady && g.LastFailure!=null && GameController.floor==originalFloor && surface.navMeshData==originalData && g.Cells==originalBoard,"invalid configuration preserves previous floor and navigation");
            g.size=originalSize;
            g.endRoom=g.startRoom; g.AdvanceFloor(); await Ready(g);
            Check(g.IsReady && g.LastFailure!=null && GameController.floor==originalFloor && surface.navMeshData==originalData && g.Cells==originalBoard,"missing exit rolls back partially assembled hierarchy");
            g.endRoom=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dungeons/Rooms/DBig.prefab"); g.AdvanceFloor(); await Ready(g);
            Check(g.IsReady && g.LastFailure!=null && GameController.floor==originalFloor && surface.navMeshData==originalData && g.Cells==originalBoard,"unreachable exit rolls back newly built native mesh");
            g.endRoom=originalEnd;
            for(int attempt=0;attempt<3;attempt++)
            {
                g.AdvanceFloor(); Check(g.IsGenerating,"cancellation begins replacement "+attempt); g.enabled=false;
                Check(!g.IsGenerating && g.IsReady && GameController.floor==originalFloor && surface.navMeshData==originalData && g.Cells==originalBoard && cc.enabled && movement.canMove && Vector3.Distance(g.player.transform.position,originalPosition)<.1f,"disable cancellation restores mesh floor player and movement "+attempt);
                g.enabled=true; await Task.Delay(100);
            }
            g.GetComponentInChildren<DungeonExit>().Interact(g.player.transform); g.AdvanceFloor(); await Ready(g);
            Check(g.IsReady && g.LastFailure==null && GameController.floor==originalFloor+1,"exit advances exactly one floor after rollback and duplicate request");
            var failed=new GameObject("Audit initial failure"); failed.SetActive(false);
            try {var empty=failed.AddComponent<DungeonGenerator>(); var iterator=empty.TT(); while(iterator.MoveNext()){} Check(!empty.IsReady&&!empty.IsGenerating&&empty.LastFailure!=null,"initial configuration failure terminates without a ready floor");}
            finally {UnityEngine.Object.Destroy(failed);}
        }
        catch (Exception e) { error = e.ToString(); Check(false, error); }
        finally { UnityEngine.Random.state = random; }
        var report = new { passed = error == null && checks.All(c => c.StartsWith("PASS")), checks, frame = Time.frameCount, error };
        System.IO.Directory.CreateDirectory("Assets/Plan/Validation");
        System.IO.File.WriteAllText("Assets/Plan/Validation/audit-world.json", JsonConvert.SerializeObject(report, Formatting.Indented));
        return report;
    }
}
