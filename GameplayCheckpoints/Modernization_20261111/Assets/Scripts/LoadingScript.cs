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
}
