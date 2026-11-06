using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageFlash : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private float fadeTime = 1f;

    private Color originalColor;

    private void Awake()
    {
        if (image == null)
        {
            image = GetComponent<Image>(); // Get the Image component if it is not set in the inspector
        }

        originalColor = image.color;
        originalColor.a = 0f; // Set the original alpha to 0
        image.color = originalColor;
    }

    public void DamageFlash(float damageIntensity)
    {
        if (!image.enabled)
        {
            StartCoroutine(FadeOut(damageIntensity));
        }
    }

    private IEnumerator FadeOut(float damageIntensity)
    {
        image.enabled = true; // Enable the image
        originalColor.a = 1f * Mathf.Sqrt(damageIntensity);
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Fade out to alpha = 0
        float startTime = Time.time;

        while (Time.time < startTime + fadeTime)
        {
            float t = (Time.time - startTime) / fadeTime;
            image.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }

        image.enabled = false; // Disable the image again when done fading out
        image.color = originalColor; // Reset the color to the original value
    }
}
