using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
public static class InspectWorldAssets
{
    public static string Run()
    {
        var result = new[] { "DEnd", "DBig", "DHall", "DMain" }.Select(name => {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dungeons/Rooms/"+name+".prefab");
            var room = root.GetComponent<RoomBehaviour>();
            Func<GameObject,object> describe = o => new {o.name, pos=o.transform.localPosition.ToString(), scale=o.transform.localScale.ToString(), scripts=o.GetComponentsInChildren<MonoBehaviour>(true).Where(c=>c!=null).Select(c=>c.GetType().Name).ToArray(), colliders=o.GetComponentsInChildren<Collider>(true).Select(c=>new {c.name,c.enabled,c.isTrigger,active=c.gameObject.activeSelf,type=c.GetType().Name,pos=c.transform.localPosition.ToString()}).ToArray()};
            return new {name, walls=room.walls.Select(describe).ToArray(), doors=room.doors.Select(describe).ToArray()};
        }).ToArray();
        string json=JsonConvert.SerializeObject(result,Formatting.Indented); System.IO.File.WriteAllText("Temp/AuditRestore/room-assets.json",json); return json;
    }
}
