using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
public static class TestMap {
static void Check(bool ok,string label) {if(!ok) throw new System.Exception(label); Debug.Log("[Minimap validation] PASS "+label);}
public static async Task<string> Run() {
var map=Object.FindFirstObjectByType<MapController>();
Check(map!=null,"controller wired");
Check(map.mapCamera.targetTexture==map.display.texture && map.display.texture.width==512,"owned 512 output");
Check(map.mapCamera.transform.eulerAngles.y<.1f,"north up");
var target=map.target;var position=target.position;var rotation=target.rotation;
var cc=target.GetComponent<CharacterController>();bool enabled=cc!=null && cc.enabled;
try {
if(cc!=null)cc.enabled=false;
target.position+=new Vector3(5,0,7);target.rotation=Quaternion.Euler(0,123,0);
await Task.Delay(150);
Check(Vector2.Distance(new Vector2(map.mapCamera.transform.position.x,map.mapCamera.transform.position.z),new Vector2(target.position.x,target.position.z))<.01f,"follows world position");
Check(Mathf.Abs(Mathf.DeltaAngle(map.mapCamera.transform.eulerAngles.y,0))<.01f,"rotation independent of player");
Check(Mathf.Abs(Mathf.DeltaAngle(map.playerMarker.eulerAngles.z,-target.eulerAngles.y))<.01f,"heading marker");
var input=target.GetComponent<PlayerInput>();Check(input.actions.FindAction("Map")!=null,"M action exists");
map.SetExpanded(true);target.GetComponent<PlayerControlState>().Synchronize();
Check(!target.GetComponent<PlayerControlState>().GameplayAllowed && Time.timeScale>0,"expanded input gating without pause");
Check(map.mapCamera.orthographicSize==100 && map.mapPanel.sizeDelta.x>220,"expanded view and range");
map.SetExpanded(false);target.GetComponent<PlayerControlState>().Synchronize();
Check(map.mapCamera.orthographicSize==30 && map.mapPanel.sizeDelta.x==220,"collapse restores view");
return "All minimap runtime checks passed";
} finally {target.SetPositionAndRotation(position,rotation);if(cc!=null)cc.enabled=enabled;map.SetExpanded(false);}
}}
