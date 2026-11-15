using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Newtonsoft.Json;
public static class InspectAnimation
{
    public static void Main()
    {
        var c = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Animators/Characters/Player Animator.controller");
        var list = new List<object>();
        foreach (var layer in c.layers) Walk(layer.stateMachine, layer.name, list);
        File.WriteAllText("Temp/PlayerCleanup/animation.json", JsonConvert.SerializeObject(new { parameters = c.parameters.Select(p=>p.name), layers = list }, Formatting.Indented));
    }
    private static void Walk(AnimatorStateMachine sm, string path, List<object> list)
    {
        list.Add(new { path, defaultState = sm.defaultState != null ? sm.defaultState.name : "", any = sm.anyStateTransitions.Select(T), states = sm.states.Select(s=> new { name=s.state.name, transitions=s.state.transitions.Select(T) }) });
        foreach (var sub in sm.stateMachines) Walk(sub.stateMachine, path+"/"+sub.stateMachine.name, list);
    }
    private static object T(AnimatorStateTransition t) => new { destination = t.destinationState != null ? t.destinationState.name : t.isExit ? "Exit" : "Submachine", exit = t.hasExitTime, conditions=t.conditions.Select(c=> new {c.parameter, mode=c.mode.ToString(), c.threshold}) };
}
