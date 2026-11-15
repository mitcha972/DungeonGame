using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class InspectCharacterUnsavedScene
{
    public static void Main()
    {
        if(EditorApplication.isPlaying)throw new Exception("Edit Mode required.");
        var scene=SceneManager.GetActiveScene();
        Directory.CreateDirectory("Temp/PlayerCleanup");
        if(!EditorSceneManager.SaveScene(scene,"Temp/PlayerCleanup/Combat_Test.unsaved-snapshot.unity",true))throw new Exception("Could not snapshot unsaved scene.");
    }
}
