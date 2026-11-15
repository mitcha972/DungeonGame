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
        Directory.CreateDirectory("Assets/Plan/Validation");File.WriteAllText("Assets/Plan/Validation/player-cleanup-death.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
}
