using System.Collections;
using UnityEngine;

public class BrokenObject : MonoBehaviour
{
    private Renderer[] parts;
    private Material[] fadeMaterials;
    private Material[] originalMaterials;

    private void Awake()
    {
        parts = new Renderer[transform.childCount];
        fadeMaterials = new Material[parts.Length];
        originalMaterials = new Material[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i] = transform.GetChild(i).GetComponent<MeshRenderer>();
            if (parts[i] == null || parts[i].sharedMaterial == null) continue;
            originalMaterials[i] = parts[i].sharedMaterial;
            fadeMaterials[i] = new Material(originalMaterials[i]);
            parts[i].sharedMaterial = fadeMaterials[i];
        }
        StartCoroutine(RemoveParts());
    }

    private IEnumerator RemoveParts()
    {
        yield return new WaitForSeconds(2f);
        float alpha = 1f;
        while (alpha > 0f)
        {
            alpha = Mathf.Max(0f, alpha - 0.4f * Time.deltaTime);
            foreach (Material material in fadeMaterials)
            {
                if (material == null) continue;
                Color color = material.color;
                color.a = alpha;
                material.color = color;
            }
            yield return null;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (fadeMaterials == null) return;
        for (int i = 0; i < fadeMaterials.Length; i++)
        {
            if (fadeMaterials[i] == null) continue;
            if (parts[i] != null && parts[i].sharedMaterial == fadeMaterials[i])
                parts[i].sharedMaterial = originalMaterials[i];
            Destroy(fadeMaterials[i]);
        }
    }
}
