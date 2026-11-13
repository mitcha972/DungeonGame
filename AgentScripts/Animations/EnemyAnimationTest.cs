using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Newtonsoft.Json.Linq;
public static class EnemyAnimationTest {
 public static async Task<object> Run(){if(!EditorApplication.isPlaying)throw new Exception("Play Mode required");var checks=new JArray();var errors=new JArray();Application.LogCallback logger=(m,s,t)=>{if(t==LogType.Error||t==LogType.Exception)errors.Add(m);};Application.logMessageReceived+=logger;GameObject enemy=null;
 try{var p=UnityEngine.Object.FindFirstObjectByType<PlayerCombat>();p.CancelAttack();var hp=p.myStats.currentHealth;p.myStats.currentHealth=1000;var cc=p.GetComponent<CharacterController>();var pos=p.transform.position;var rot=p.transform.rotation;
 // Move only this runtime fixture into a clear, isolated space.
 cc.enabled=false;p.transform.position=new Vector3(150,10,150);cc.enabled=true;
 enemy=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Enemy_Main.prefab"),p.transform.position+Vector3.forward*.8f,Quaternion.LookRotation(Vector3.back));var ai=enemy.GetComponent<EnemyAi>();var combat=enemy.GetComponent<CharacterCombat>();var nav=enemy.GetComponent<NavMeshAgent>();nav.enabled=false;ai.target=p.transform;await Task.Delay(300);combat.CancelAttack();var ca=enemy.GetComponent<CharacterAnimator>();ca.SetBusy(false);ai.target=p.transform;enemy.transform.position=p.transform.position+Vector3.forward*.8f;enemy.transform.rotation=Quaternion.LookRotation(p.transform.position-enemy.transform.position);int count=0;combat.OnAttack+=h=>count++;combat.Attack(false);bool accepted=combat.IsExecutingAttack;await Task.Delay(220);var a=enemy.GetComponentInChildren<Animator>();var clips=a.GetCurrentAnimatorClipInfo(1).Select(c=>c.clip.name).ToArray();checks.Add(new JObject{["check"]="Real enemy combat accepts an in-range visible target",["pass"]=accepted||count>0});checks.Add(new JObject{["check"]="Enemy child Animator plays native selected punch",["pass"]=clips.Contains("Punch - Right"),["clips"]=new JArray(clips)});ai.enabled=false;combat.CancelAttack();await Task.Delay(100);checks.Add(new JObject{["check"]="Cancellation clears attack and busy",["pass"]=!combat.IsExecutingAttack&&!ca.AnimationIsBusy()});
 cc.enabled=false;p.transform.position=pos;p.transform.rotation=rot;cc.enabled=true;p.myStats.currentHealth=hp;
 }catch(Exception ex){errors.Add(ex.ToString());}finally{if(enemy!=null)UnityEngine.Object.Destroy(enemy);Application.logMessageReceived-=logger;}
 var r=new JObject{["checks"]=checks,["errors"]=errors,["passed"]=checks.All(x=>(bool)x["pass"])&&errors.Count==0};File.WriteAllText("Assets/Plan/Validation/animation-enemy.json",r.ToString());return r;}
}
