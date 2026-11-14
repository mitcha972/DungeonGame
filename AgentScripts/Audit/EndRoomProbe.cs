using System; using System.Linq; using System.Collections.Generic; using UnityEngine; using UnityEngine.AI; using UnityEditor;
public static class EndRoomProbe { public static object Run() {
if(!Application.isPlaying)throw new Exception("Owned Play Mode required");
var root=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dungeons/Rooms/DEnd.prefab"),new Vector3(3000,0,0),Quaternion.identity);
var room=root.GetComponent<RoomBehaviour>();room.UpdateRoom(new[]{true,false,false,false});
foreach(var s in root.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>(true))s.enabled=false;
foreach(var s in root.GetComponentsInChildren<WallCollision>(true))s.enabled=false;
var surface=root.AddComponent<Unity.AI.Navigation.NavMeshSurface>();surface.collectObjects=Unity.AI.Navigation.CollectObjects.Children;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.overrideVoxelSize=true;surface.voxelSize=.08f;
try {Physics.SyncTransforms();surface.BuildNavMesh();var rows=new List<object>();
for(int i=0;i<=26;i+=2){var p=root.transform.position+Vector3.forward*i;bool on=NavMesh.SamplePosition(p,out var hit,.25f,NavMesh.AllAreas);bool floor=Physics.Raycast(p+Vector3.up,Vector3.down,out var ray,2,~0,QueryTriggerInteraction.Ignore);rows.Add(new{i,on,floor,collider=floor?ray.collider.name:null});}
NavMesh.SamplePosition(root.transform.position+Vector3.forward*24,out var start,2,NavMesh.AllAreas);NavMesh.SamplePosition(root.transform.position,out var end,2,NavMesh.AllAreas);var path=new NavMeshPath();NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path);
var blockers=Physics.OverlapBox(root.transform.position+new Vector3(0,1,16),new Vector3(2,1,1),Quaternion.identity).Select(c=>new{c.name,center=(c.bounds.center-root.transform.position).ToString(),size=c.bounds.size.ToString(),parent=c.transform.parent.name}).ToArray();return new {blockers,rows,status=path.status.ToString(),corners=path.corners.Select(p=>(p-root.transform.position).ToString()).ToArray()};
}finally{surface.RemoveData();UnityEngine.Object.Destroy(surface.navMeshData);UnityEngine.Object.Destroy(root);}
}}
