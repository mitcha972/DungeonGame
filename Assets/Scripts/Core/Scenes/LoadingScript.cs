using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScript : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        //Debug.Log("We Loaded");
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        if (!IsValidDestination(GameController.goToScene))
        {
            Debug.LogError("Loading requires a gameplay destination enabled in Build Settings. Start from Main.", this);
            yield break;
        }
        AsyncOperation operation = SceneManager.LoadSceneAsync(GameController.goToScene);

        if (operation == null) yield break;
        while(!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress/ .9f);
            if (slider != null) slider.value = progress;
            //progressText.text = progress * 100f;
            yield return null; 
        }
       
    }

    public static bool IsValidDestination(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings) return false;
        string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        return !string.IsNullOrEmpty(path)
            && path != "Assets/Scenes/Game/Start.unity"
            && path != "Assets/Scenes/Game/Loading.unity";
    }
}
