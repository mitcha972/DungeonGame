using System.Collections;
using UnityEngine;
using TMPro;

public class HealthTextDisplay : MonoBehaviour
{
    public float speed = 1.0f;
    public float popDuration = 0.5f;
    public float disappearDuration = 0.5f;

    private TextMeshPro textMesh;
    private Vector3 originalScale;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        transform.localScale = Vector3.zero;
       
    }

    public void Setup(int damageText)
    {
        // Set text mesh
        textMesh.text = damageText.ToString();

        // Enable the textMesh
        textMesh.enabled = true;

        StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        float timer = 0.0f;

        // Pop animation
        while (timer < popDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(0.0f, 1.0f, timer / popDuration);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        // Wait a bit
        yield return new WaitForSeconds(0.5f);

        // Disappear animation
        timer = 0.0f;

        while (timer < disappearDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(1.0f, 0.0f, timer / disappearDuration);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}
